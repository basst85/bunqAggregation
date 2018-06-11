using Microsoft.AspNetCore.Mvc;

namespace bunqAggregation.Services
{
    [Route("api/[controller]")]
    public class StatusController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Operational");
        }
    }
}