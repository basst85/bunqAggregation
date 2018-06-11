using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace bunqAggregation.Intergration.IFTTT
{
    [Route("ifttt/v1/[controller]")]
    public class StatusController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            bool channel_key = (Environment.GetEnvironmentVariable("IFTTT_CHANNEL_KEY").ToString() == Request.Headers["IFTTT-Channel-Key"]);
            bool service_key = (Environment.GetEnvironmentVariable("IFTTT_SERVICE_KEY").ToString() == Request.Headers["IFTTT-Service-Key"]);

            if (channel_key && service_key)
            {
                return StatusCode(200,"Operational!");
            }
            else
            {
                return StatusCode(401, "Operational, but invalid channel or service key.");
            }
        }
    }
}