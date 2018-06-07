using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Bunq.Sdk.Context;
using Bunq.Sdk.Model.Generated.Endpoint;
using Bunq.Sdk.Model.Generated.Object;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BunqAggregation
{
    [Route("api/[controller]")]
    public class TriggerController : Controller
    {
        [HttpGet]
        public void Get()
        {
            JObject config = Settings.LoadConfig();

            Console.WriteLine("Hi there, we are connecting to the bunq API...\n");

            var apiContext = ApiContext.Restore();
            BunqContext.LoadApiContext(apiContext);
            Console.WriteLine(" -- Connected as: " + BunqContext.UserContext.UserPerson.DisplayName + " (" + BunqContext.UserContext.UserId + ")\n");

            foreach (JObject rule in config["rules"])
            {
                if (rule["if"]["type"].ToString() == "trigger"){
                    foreach (JObject action in rule["then"])
                    {
                        if (action.ContainsKey("email")){
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
                            Console.WriteLine("Description:           " + payment_desc );
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
                }

            }

        }
    }
}
