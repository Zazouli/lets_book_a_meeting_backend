﻿using meetspace.room.management.module.Application.DTOs;
using meetspace.room.management.module.Core.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace meetspace.web.Controllers
{
    [Route("api/")]
    [ApiController]
    public class RoomManagementController : ControllerBase
    {
        private readonly IRoomManagementService _roomManagementService;
        public RoomManagementController(IRoomManagementService roomManagementService)
        {
            _roomManagementService = roomManagementService;
        }
        // GET: api/<RoomManagementController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<RoomManagementController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<RoomManagementController>
        [HttpPost("roommanagement")]
        public async Task<ActionResult> Post([FromBody] RoomDetailsDTO roomDetail)
        {
            if(await _roomManagementService.CreateRoom(roomDetail))
            {
                return Ok();
            }
            return BadRequest();
        }

        // PUT api/<RoomManagementController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<RoomManagementController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
