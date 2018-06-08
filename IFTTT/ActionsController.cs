using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BunqAggregation.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BunqAggregation.IFTTT
{
    [Route("ifttt/v1/[controller]")]
    public class ActionsController : Controller
    {
        [HttpPost]
        [Route("request")]
        public IActionResult MakeRequest([FromBody] JObject content)
        {
            bool channel_key = (Environment.GetEnvironmentVariable("IFTTT_CHANNEL_KEY").ToString() == Request.Headers["IFTTT-Channel-Key"]);
            bool service_key = (Environment.GetEnvironmentVariable("IFTTT_SERVICE_KEY").ToString() == Request.Headers["IFTTT-Service-Key"]);
            JObject response;

            if (channel_key && service_key)
            {
                var action_details = content["actionFields"];

                if (action_details == null)
                {
                    response = new JObject {
                        {"errors", new JArray {
                                new JObject {
                                    {"message", "Whoops, the actionFields are missing!"}
                                }
                            }
                        }
                    };
                    return StatusCode(400, response);
                }

                var recipient = action_details["recipient"];
                var amount = action_details["amount"];
                var email = action_details["email"];
                var description = action_details["description"];

                if (
                    recipient == null ||
                    amount == null ||
                    email == null ||
                    description == null
                )
                {
                    response = new JObject {
                        {"errors", new JArray {
                                new JObject {
                                    {"message", "Whoops, one of the mandatory fields are missing!"}
                                }
                            }
                        }
                    };
                    return StatusCode(400, response);
                }

                string identifier = Guid.NewGuid().ToString();

                JObject details = new JObject {
                    {"amount", amount.ToString() },
                    {"recipient", email.ToString() },
                    {"description", description.ToString() }
                };

                response = new JObject {
                    {"data", new JArray {
                            new JObject {
                                {"id", identifier}
                            }
                        }
                    }
                };
                
                if (Request.Headers["IFTTT-Test-Mode"].Equals("1"))
                {
                    return StatusCode(200, response);
                }
                else
                {
                    PaymentRequest.Send(details);
                    return StatusCode(200, response);
                }
            }
            else
            {
                response = new JObject {
                        {"errors", new JArray {
                                new JObject {
                                    {"message", "Invalid channel or service key."}
                                }
                            }
                        }
                    };
                return StatusCode(401, response);
            }
        }
    }
}