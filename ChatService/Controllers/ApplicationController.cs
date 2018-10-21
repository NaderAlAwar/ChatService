using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ChatService.Controllers
{
    [Route("api/[controller]")]
    public class ApplicationController : Controller
    {
        private readonly IConfiguration configuration;

        public ApplicationController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpGet("version")]
        public IActionResult GetVersion()
        {
            string version = configuration.GetValue<string>("Application:Version");
            return Ok(version);
        }

        [HttpGet("environment")]
        public IActionResult GetEnvironmentName()
        {
            string version = configuration.GetValue<string>("Application:Environment");
            return Ok(version);
        }
    }
}
