using DurableFunctionsDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DurableFunctionsDemo
{
    public static class CreateOrderClient
    {
        [FunctionName("func-create-order")]
        public static async Task<IActionResult> RunClient(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClientBase client,
            ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                Order order = JsonConvert.DeserializeObject<Order>(requestBody);
                var orchestrantionId = await client.StartNewAsync("func-process-order-orchestrator", order);

                log.LogInformation($"Processing order for {order.Customer.Name} with {order.PurchasedItems.Count} items");

                return new OkObjectResult(new { orchestrantionId });
            }
            catch (Exception ex)
            {
                // error handling here...
                log.LogError(ex.ToString());
                return new BadRequestResult();
            }
        }
    }
}
