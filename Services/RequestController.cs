using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Bunq.Sdk.Context;
using Bunq.Sdk.Model.Generated.Endpoint;
using Bunq.Sdk.Model.Generated.Object;

namespace bunqAggregation.Services
{
    [Route("api/[controller]")]
    public class RequestController : Controller
    {
        [HttpPost]
        public void Post([FromBody] JObject content)
        {
            // Load and connect to bunq API
            var apiContext = ApiContext.Restore();
            BunqContext.LoadApiContext(apiContext);

            // Setting variables
            var amount = content["amount"].ToString();
            string recipient = content["recipient"].ToString();
            string description = content["description"].ToString();

            // Execute request.
            if (Convert.ToDouble(amount) > 0)
            {
                RequestInquiry.Create(new Amount(amount, "EUR"), new Pointer("EMAIL", recipient), description, true);
            }
        }
    }
}
