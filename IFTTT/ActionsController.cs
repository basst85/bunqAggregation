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

        [HttpPost]
        [Route("transfer_defined_amount")]
        public IActionResult TransferDefinedAmount([FromBody] JObject content)
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
                var from_iban = action_details["from_iban"];
                var to_iban = action_details["to_iban"];
                var amount = action_details["amount"];
                var description = action_details["description"];

                if (
                    recipient == null ||
                    from_iban == null ||
                    to_iban == null ||
                    amount == null ||
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
                    {"origin", new JObject {
                            {"iban", from_iban.ToString()}
                        }},
                    {"destination", new JObject {
                            {"iban", to_iban.ToString()},
                            {"name", recipient.ToString()}
                        }},
                    {"description", description.ToString()},
                    {"amount", new JObject {
                            {"type", "exact"},
                            {"value", amount.ToString()}
                        }}
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
                    Payment.Execute(details);
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
        [HttpPost]
        [Route("transfer_full_saldo")]
        public IActionResult TransferFullSaldo([FromBody] JObject content)
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
                var from_iban = action_details["from_iban"];
                var to_iban = action_details["to_iban"];
                var description = action_details["description"];

                if (
                    recipient == null ||
                    from_iban == null ||
                    to_iban == null ||
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
                    {"origin", new JObject {
                            {"iban", from_iban.ToString()}
                        }},
                    {"destination", new JObject {
                            {"iban", to_iban.ToString()},
                            {"name", recipient.ToString()}
                        }},
                    {"description", description.ToString()},
                    {"amount", new JObject {
                            {"type", "precent"},
                            {"value", "100"}
                        }}
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
                    Payment.Execute(details);
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