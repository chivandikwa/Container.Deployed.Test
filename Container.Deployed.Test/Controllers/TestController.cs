using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Container.Deployed.Test.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private ILogger<TestController> logger;

        public TestController(ILogger<TestController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            logger.LogInformation(Environment.GetEnvironmentVariable("DBCONNECTIONSTRING"));
            // Fetch a variable that should be resolved by env key
            return Environment.GetEnvironmentVariable("DBCONNECTIONSTRING");
        }
    }
}
