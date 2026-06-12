using Microsoft.AspNetCore.Mvc;

namespace MyWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Hello from API!", timestamp = DateTime.Now });
        }

        [HttpPost]
        public IActionResult Post([FromBody] dynamic data)
        {
            return Created("", new { received = data, status = "Processed" });
        }
    }
}
