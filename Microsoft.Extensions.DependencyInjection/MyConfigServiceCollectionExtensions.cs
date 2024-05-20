using meetspace.room.management.module.Core.Services;
using meetspace.room.management.module.Infrastructor.CosmosDB;
using meetspace.room.management.module.Infrastructor.Repositories;

namespace meetspace.web.Microsoft.Extensions.DependencyInjection
{
    public static class MyConfigServiceCollectionExtensions
    {
        public static IServiceCollection AddMyDependencyGroup(
             this IServiceCollection services)
        {
            services.AddScoped<ICosmosDBClientFactory, CosmosDBClientFactory>();
            services.AddScoped<IRoomManagementRepository, RoomManagementRepository>();
            services.AddScoped<IRoomManagementService, RoomManagementService>();

            return services;
        }
    }
}
