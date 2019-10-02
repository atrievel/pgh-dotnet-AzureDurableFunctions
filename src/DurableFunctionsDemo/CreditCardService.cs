using DurableFunctionsDemo.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DurableFunctionsDemo.Functions
{
    public static class CreditCardService
    {
        [FunctionName("func-charge-customer")]
        public static async Task Run(
            [ActivityTrigger] Tuple<Customer, float> requiredData,
            ILogger log)
        {
            Random random = new Random();
            int randomWaitTime = random.Next(0, 11) * 1000;

            // simulate our buggy/long waiting API by waiting for a random amount of time
            await Task.Delay(randomWaitTime);
        }
    }
}
