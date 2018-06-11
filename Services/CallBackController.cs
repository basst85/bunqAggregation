using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using bunqAggregation.Common;

namespace bunqAggregation.Services
{
    [Route("api/[controller]")]
    public class CallBackController : Controller
    {
        [HttpPost]
        public void Post([FromBody] JObject content)
        {
            JObject config = Settings.LoadConfig();

            string amount = content["NotificationUrl"]["object"]["Payment"]["amount"]["value"].ToString();
            string description = content["NotificationUrl"]["object"]["Payment"]["description"].ToString();
            string to_iban = content["NotificationUrl"]["object"]["Payment"]["alias"]["iban"].ToString();
            string from_iban = content["NotificationUrl"]["object"]["Payment"]["counterparty_alias"]["iban"].ToString();
            bool match = false;

            // Check for matching rules and actions!
            foreach (JObject rule in config["rules"])
            {
                if (!match)
                {
                    if (rule["if"]["type"].ToString() == "mutation")
                    {

                        Regex descRegex = new Regex(rule["if"]["description"].ToString());
                        if (
                            from_iban == rule["if"]["origin"]["iban"].ToString() &
                            to_iban == rule["if"]["destination"]["iban"].ToString() &
                            descRegex.IsMatch(description)
                        )
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
                                    JObject metadata = new JObject {
                                        {"callback", new JObject{
                                                {"amount", amount.ToString()}
                                            }}
                                    };
                                    action.Add("metadata", metadata);
                                    Payment.Execute(action);
                                }
                            }
                            match = true;
                        }
                    }
                }
            }         
        }
    }
}
