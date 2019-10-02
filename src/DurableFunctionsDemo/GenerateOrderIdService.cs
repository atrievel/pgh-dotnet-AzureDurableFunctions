using DurableFunctionsDemo.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DurableFunctionsDemo.Activity.Functions
{
    public static class GenerateOrderIdService
    {
        [FunctionName("func-generate-order-id")]
        public static async Task<Guid> Run(
            [ActivityTrigger] Order order,
            ILogger log)
        {
            // simulate custom order id creation 
            return await Task.Run(() => { return new Guid(); });
        }
    }
}
