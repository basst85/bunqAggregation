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
    public class TestController : Controller
    {
        [HttpPost]
        [Route("setup")]
        public IActionResult Post()
        {
            bool channel_key = (Environment.GetEnvironmentVariable("IFTTT_CHANNEL_KEY").ToString() == Request.Headers["IFTTT-Channel-Key"]);
            bool service_key = (Environment.GetEnvironmentVariable("IFTTT_SERVICE_KEY").ToString() == Request.Headers["IFTTT-Service-Key"]);

            if (channel_key && service_key)
            {
                JObject response = new JObject
                {
                    {"data", new JObject{
                        {"samples", new JObject{
                            {"actions",new JObject{
                                {"request", new JObject{
                                    {"recipient", "Your BFF"},
                                    {"description", "You owe me money!"},
                                    {"amount", "0.01"},
                                    {"email", "online@duijvelshoff.com"}
                                }},
                                {"transfer_defined_amount", new JObject{
                                    {"recipient", "Your BFF"},
                                    {"description", "Here is some of my money!"},
                                    {"amount", "0.01"},
                                    {"from_iban", "NL00BUNQ0000000000"},
                                    {"to_iban", "NL00BUNQ0000000000"}
                                }},
                                {"transfer_defined_percentage", new JObject{
                                    {"recipient", "Your BFF"},
                                    {"description", "Here is half of my money!"},
                                    {"percentage", "50"},
                                    {"from_iban", "NL00BUNQ0000000000"},
                                    {"to_iban", "NL00BUNQ0000000000"}
                                }},
                                {"transfer_full_saldo", new JObject{
                                    {"recipient", "Your wifes name"},
                                    {"description", "Here is all of my money!"},
                                    {"from_iban", "NL00BUNQ0000000000"},
                                    {"to_iban", "NL00BUNQ0000000000"}
                                }}
                            }}
                        }}
                    }}
                };
                return StatusCode(200, response);
            }
            else
            {
                return StatusCode(401, "Operational, but invalid channel or service key.");
            }
        }
    }
}
