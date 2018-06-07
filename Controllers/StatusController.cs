using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BunqAggregation.Controllers
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
