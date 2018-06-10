using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using bunqAggregation.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace bunqAggregation
{
    [Route("api/[controller]")]
    public class TriggerController : Controller
    {
        [HttpGet]
        public void Get()
        {
            JObject config = Settings.LoadConfig();
            Console.WriteLine("Trigger is starting:");
            Console.WriteLine("----------------------------------------------------------");
            foreach (JObject rule in config["rules"])
            {
                if (rule["if"]["type"].ToString() == "trigger")
                {
                    Console.WriteLine("Matching result:");
                    Console.WriteLine("----------------------------------------------------------");
                    Console.WriteLine("Rule Name:             " + rule["name"].ToString());
                    Console.WriteLine("----------------------------------------------------------");
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
