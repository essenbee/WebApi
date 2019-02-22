using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Authorize(Policy = "readonly")]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class LaunchController : ControllerBase
    {
        // GET api/launch
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(Launch))]
        [ProducesResponseType(401)]
        public ActionResult<Launch> Get(string id)
        {
            return Ok(GetTestData());
        }

        private Launch GetTestData()
        {
            return new Launch
            {
                Id = "1",
                LaunchProvider = "ULA",
                LaunchVehicle = "Delta 5",
                Mission = "Some Sat",
                LaunchDate = new DateTime(2019, 02, 23)
            };
        }
    }
}
