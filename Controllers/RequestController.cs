using System;
using System.Collections.Generic;
using System.Linq;
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
    public class RequestController : Controller
    {
        [HttpPost]
        public void Post([FromBody] JObject content)
        {
            JObject config = Settings.LoadConfig();

            Console.WriteLine("Hi there, we are connecting to the bunq API...\n");

            var apiContext = ApiContext.Restore();
            BunqContext.LoadApiContext(apiContext);
            Console.WriteLine(" -- Connected as: " + BunqContext.UserContext.UserPerson.DisplayName + " (" + BunqContext.UserContext.UserId + ")\n");


            var amount = content["amount"].ToString();
            string recipient = content["recipient"].ToString();
            string description = content["description"].ToString();

            Console.WriteLine("Todo:");
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine("Description:           " + description);
            Console.WriteLine("Send to:               " + recipient);
            Console.WriteLine("Amount:                € " + amount);
            Console.WriteLine("----------------------------------------------------------");

            Console.WriteLine("Executing...");
            RequestInquiry.Create(new Amount(amount, "EUR"), new Pointer("EMAIL", recipient), description, true);
            Console.WriteLine("Yeah, this one is completed!");
            Console.WriteLine("----------------------------------------------------------\n");
        }
    }
}
