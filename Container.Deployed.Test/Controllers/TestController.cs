using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Container.Deployed.Test.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            // Fetch a variable that should be resolved by env key
            return Environment.GetEnvironmentVariable("DBCONNECTIONSTRING");
        }
    }
}
