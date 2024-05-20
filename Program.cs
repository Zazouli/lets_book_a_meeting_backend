using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using meetspace.web.Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;
using meetspace.room.management.module.Configuration;

var builder = WebApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.Development.json")
                 .Build();

var certificatePath = @$"{Directory.GetCurrentDirectory()}/myCertificate.pfx";
var certificatePassword = config["AzureAd:Password"];

// Ensure the file path is correct
if (!System.IO.File.Exists(certificatePath))
{
    throw new FileNotFoundException($"Certificate file not found at {certificatePath}");
}

var certificateDescription = CertificateDescription.FromPath(certificatePath, certificatePassword);

// AddDependencies
builder.Services.AddRoomManagementDependencies(config);

// Add services to the container.

builder.Services.AddControllers();
// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Configure Azure AD Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

// Configure the Microsoft Identity options with the certificate
builder.Services.Configure<MicrosoftIdentityOptions>(options =>
{
    options.ClientCertificates = new List<CertificateDescription> { certificateDescription };
});

builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
        {
            // Load the signing keys from Azure AD metadata
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{parameters.ValidIssuer}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever());

            var config = configurationManager.GetConfigurationAsync(CancellationToken.None).Result;
            return config.SigningKeys;
        },
        ValidIssuer = $"https://sts.windows.net/{config["AzureAd:TenantId"]}/",
        ValidAudience = config["AzureAd:Audiance"],
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(options =>
{
    options.AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod();
});
//app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    if (context.Request.Headers.ContainsKey("Authorization"))
    {
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        Console.WriteLine($"Received Token: {token}");
    }
    await next();
});
app.UseAuthorization();

app.MapControllers();


app.Run();
