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
using bunqAggregation;
using bunqAggregation.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace bunqAggregation.Controllers
{
    [Route("api/[controller]")]
    public class CallBackController : Controller
    {
        [HttpPost]
        public void Post([FromBody] JObject content)
        {
            JObject config = Settings.LoadConfig();
            Console.WriteLine("Callback is starting:");
            Console.WriteLine("----------------------------------------------------------");

            // Log all notifications to ElasticSearch
            string identifier = Guid.NewGuid().ToString();
            var http = new HttpClient();
            var body = new StringContent(content.ToString(), Encoding.UTF8, "application/json");
            string url = Environment.GetEnvironmentVariable("ELASTIC_BASEURL").ToString();
            string username = Environment.GetEnvironmentVariable("ELASTIC_USERNAME").ToString();
            string password = Environment.GetEnvironmentVariable("ELASTIC_PASSWORD").ToString();
            string credentials = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            http.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials);

            http.PutAsync(url + "aggregation/transactions/" + identifier, body);

            Console.WriteLine(content.ToString());

            string category = content["NotificationUrl"]["category"].ToString();
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
            if (!match)
            {
                Console.WriteLine("None of the conditions match!");
                Console.WriteLine("----------------------------------------------------------");
            }           
        }
    }
}
