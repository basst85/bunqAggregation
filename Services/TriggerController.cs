using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using bunqAggregation.Common;

namespace bunqAggregation.Services
{
    [Route("api/[controller]")]
    public class TriggerController : Controller
    {
        [HttpGet]
        public void Get()
        {
            JObject config = Settings.LoadConfig();
            foreach (JObject rule in config["rules"])
            {
                if (rule["if"]["type"].ToString() == "trigger")
                {
                    foreach (JObject action in rule["then"])
                    {
                        if (action.ContainsKey("email"))
                        {
                            Console.WriteLine(action);
                            //TODO: Create Mail on trigger!
                        }
                        if (action.ContainsKey("payment"))
                        {
                            Payment.Execute(action);
                        }
                    }
                }
            }
        }
    }
}
