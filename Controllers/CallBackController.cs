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
using BunqAggregation;
using Bunq.Sdk.Context;
using Bunq.Sdk.Model.Generated.Endpoint;
using Bunq.Sdk.Model.Generated.Object;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BunqAggregation.Controllers
{
    [Route("api/[controller]")]
    public class CallBackController : Controller
    {
        [HttpPost]
        public void Post([FromBody] JObject content)
        {
            JObject config = Settings.LoadConfig();

            Console.WriteLine("Hi there, we are connecting to the bunq API...\n");

            var apiContext = ApiContext.Restore();
            BunqContext.LoadApiContext(apiContext);
            Console.WriteLine(" -- Connected as: " + BunqContext.UserContext.UserPerson.DisplayName + " (" + BunqContext.UserContext.UserId + ")\n");

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
                            foreach (JObject action in rule["then"])
                            {
                                if (action.ContainsKey("email"))
                                {
                                    Console.WriteLine(action);
                                    //TODO: Create Mail on trigger!
                                }
                                if (action.ContainsKey("payment"))
                                {
                                    var AllMonetaryAccounts = MonetaryAccountBank.List().Value;
                                    int MonetaryAccountId = 0;
                                    string MonetaryAccountBalance = null;

                                    foreach (var MonetaryAccount in AllMonetaryAccounts)
                                    {
                                        foreach (var Alias in MonetaryAccount.Alias)
                                        {
                                            if (Alias.Value == action["payment"]["origin"]["iban"].ToString())
                                            {
                                                MonetaryAccountId = MonetaryAccount.Id.Value;
                                                MonetaryAccountBalance = MonetaryAccount.Balance.Value;
                                            }
                                        }
                                    }

                                    double AmountToTransfer = 0;
                                    string DefinedTransaction = null;
                                    string SuggestedTransaction = null;

                                    switch (action["payment"]["amount"]["type"].ToString())
                                    {
                                        case "exact":
                                            if (Double.Parse(MonetaryAccountBalance) >= Double.Parse(action["payment"]["amount"]["value"].ToString()))
                                            {
                                                AmountToTransfer = Double.Parse(action["payment"]["amount"]["value"].ToString());
                                                DefinedTransaction = "€" + action["payment"]["amount"]["value"].ToString() + " (Exact amount)";
                                                SuggestedTransaction = AmountToTransfer.ToString("0.00") + " - Reason: Due to insufficied balance on the account.";
                                            }
                                            else
                                            {
                                                AmountToTransfer = Double.Parse(MonetaryAccountBalance);
                                                DefinedTransaction = "€ " + action["payment"]["amount"]["value"].ToString() + " (Exact amount)";
                                                SuggestedTransaction = AmountToTransfer.ToString("0.00");
                                            }
                                            break;
                                        case "percent":
                                            AmountToTransfer = (Double.Parse(MonetaryAccountBalance) * (Double.Parse(action["payment"]["amount"]["value"].ToString()) / 100));
                                            DefinedTransaction = action["payment"]["amount"]["value"].ToString() + "% of € " + MonetaryAccountBalance;
                                            SuggestedTransaction = AmountToTransfer.ToString("0.00");
                                            break;
                                        case "differance":
                                            AmountToTransfer = (Double.Parse(MonetaryAccountBalance) - Double.Parse(amount));
                                            DefinedTransaction = "The differance between: € " + MonetaryAccountBalance + " and € " + amount;
                                            SuggestedTransaction = AmountToTransfer.ToString("0.00");
                                            break;
                                        default:
                                            break;
                                    }

                                    DateTime now = DateTime.Today;
                                    string month = now.ToString("MMMM", new CultureInfo("nl-NL"));
                                    string year = now.ToString("yyyy");
                                    string payment_desc = String.Format(action["payment"]["description"].ToString(), month, year);

                                    Console.WriteLine("Todo:");
                                    Console.WriteLine("----------------------------------------------------------");
                                    Console.WriteLine("Rule Name:             " + rule["name"].ToString());
                                    Console.WriteLine("Description:           " + payment_desc);
                                    Console.WriteLine("Origin Account:        " + action["payment"]["origin"]["iban"].ToString() + " (" + MonetaryAccountId.ToString() + ")");
                                    Console.WriteLine("Destination Account:   " + action["payment"]["destination"]["iban"].ToString());
                                    Console.WriteLine("Current Balance:       € " + MonetaryAccountBalance);
                                    Console.WriteLine("Defined Transaction:   " + DefinedTransaction);
                                    Console.WriteLine("Suggested Transaction: € " + SuggestedTransaction);
                                    Console.WriteLine("----------------------------------------------------------");

                                    var Recipient = new Pointer("IBAN", action["payment"]["destination"]["iban"].ToString());
                                    Recipient.Name = action["payment"]["destination"]["name"].ToString();
                                    Console.WriteLine("Executing...");
                                    var PaymentID = Payment.Create(new Amount(AmountToTransfer.ToString("0.00"), "EUR"), Recipient, payment_desc, MonetaryAccountId).Value;
                                    Console.WriteLine("Yeah, this one is completed!");
                                    Console.WriteLine("----------------------------------------------------------\n");
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
            }           
        }
    }
}
