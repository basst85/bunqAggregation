using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using bunqAggregation;
using Bunq.Sdk.Context;
using Bunq.Sdk.Model.Generated.Endpoint;
using Bunq.Sdk.Model.Generated.Object;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace bunqAggregation.Controllers
{
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        [HttpPost]
        public void Post([FromBody] JObject content)
        {
            Console.WriteLine(content.ToString());

            JObject config = Settings.LoadConfig();

            // Load and connect to bunq API
            Console.WriteLine("Hi there, we are connecting to the bunq API...\n");

            var apiContext = ApiContext.Restore();
            BunqContext.LoadApiContext(apiContext);

            Console.WriteLine(" -- Connected as: " + BunqContext.UserContext.UserPerson.DisplayName + " (" + BunqContext.UserContext.UserId + ")\n");

            // Getting account ID and balance
            var AllMonetaryAccounts = MonetaryAccountBank.List().Value;
            int MonetaryAccountId = 0;
            string MonetaryAccountBalance = null;

            foreach (var MonetaryAccount in AllMonetaryAccounts)
            {
                foreach (var Alias in MonetaryAccount.Alias)
                {
                    if (Alias.Value == content["payment"]["origin"]["iban"].ToString())
                    {
                        MonetaryAccountId = MonetaryAccount.Id.Value;
                        MonetaryAccountBalance = MonetaryAccount.Balance.Value;
                    }
                }
            }

            // Define transaction amount
            double AmountToTransfer = 0;
            string DefinedTransaction = null;
            string SuggestedTransaction = null;
            var amount = content["metadata"]["callback"]["amount"];
       
            switch (content["payment"]["amount"]["type"].ToString())
            {
                case "exact":
                    if (Double.Parse(MonetaryAccountBalance) >= Double.Parse(content["payment"]["amount"]["value"].ToString()))
                    {
                        AmountToTransfer = Double.Parse(content["payment"]["amount"]["value"].ToString());
                        DefinedTransaction = "€" + content["payment"]["amount"]["value"].ToString() + " (Exact amount)";
                        SuggestedTransaction = AmountToTransfer.ToString("0.00") + " - Reason: Due to insufficied balance on the account.";
                    }
                    else
                    {
                        AmountToTransfer = Double.Parse(MonetaryAccountBalance);
                        DefinedTransaction = "€ " + content["payment"]["amount"]["value"].ToString() + " (Exact amount)";
                        SuggestedTransaction = AmountToTransfer.ToString("0.00");
                    }
                    break;
                case "percent":
                    AmountToTransfer = (Double.Parse(MonetaryAccountBalance) * (Double.Parse(content["payment"]["amount"]["value"].ToString()) / 100));
                    DefinedTransaction = content["payment"]["amount"]["value"].ToString() + "% of € " + MonetaryAccountBalance;
                    SuggestedTransaction = AmountToTransfer.ToString("0.00");
                    break;
                case "differance":
                    if (amount != null){
                        AmountToTransfer = (Double.Parse(MonetaryAccountBalance) - Double.Parse(amount.ToString()));
                        DefinedTransaction = "The differance between: € " + MonetaryAccountBalance + " and € " + amount.ToString();
                        SuggestedTransaction = AmountToTransfer.ToString("0.00");
                    }
                    break;
                default:
                    break;
            }

            // Set description with additional variables.
            DateTime now = DateTime.Today;
            string month = now.ToString("MMMM", new CultureInfo("nl-NL"));
            string year = now.ToString("yyyy");
            string payment_desc = String.Format(content["payment"]["description"].ToString(), month, year);

            // Show details of transaction.
            Console.WriteLine("Todo:");
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("Description:           " + payment_desc);
            Console.WriteLine("Origin Account:        " + content["payment"]["origin"]["iban"].ToString() + " (" + MonetaryAccountId.ToString() + ")");
            Console.WriteLine("Destination Account:   " + content["payment"]["destination"]["iban"].ToString());
            Console.WriteLine("Current Balance:       € " + MonetaryAccountBalance);
            Console.WriteLine("Defined Transaction:   " + DefinedTransaction);
            Console.WriteLine("Suggested Transaction: € " + SuggestedTransaction);
            Console.WriteLine("----------------------------------------------------------");

            // Execute transaction.
            if (AmountToTransfer > 0)
            {
                var Recipient = new Pointer("IBAN", content["payment"]["destination"]["iban"].ToString());
                Recipient.Name = content["payment"]["destination"]["name"].ToString();
                Console.WriteLine("Executing...");
                var PaymentID = Payment.Create(new Amount(AmountToTransfer.ToString("0.00"), "EUR"), Recipient, payment_desc, MonetaryAccountId).Value;
                Console.WriteLine("Yeah, this one is completed!");
            }
            else
            {
                Console.WriteLine("Bummer, nothing to do!");
            }
            Console.WriteLine("----------------------------------------------------------\n");
        }
    }
}
