using CsvHelper;
using DurableFunctionsDemo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DurableFunctionsDemo.Functions
{
    public static class InternalProcesses
    {
        [FunctionName("func-ship-order")]
        public static async Task<IActionResult> RunShipmentConf(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "approve/{orderID}")] HttpRequest req,
            [OrchestrationClient] DurableOrchestrationClientBase client,
            string orderID,
            ILogger log)
        {

            await Task.Run(() => log.LogTrace($"Shipping order with orchestractionId of ${orderID}"));
            await client.RaiseEventAsync(orderID, "func-ship-order", true);

            return new OkResult();
        }

        [FunctionName("func-send-ship-confirmation")]
        public static async Task RunSendShippingConfirmation(
            [ActivityTrigger] Order order,
            ILogger log)
        {
            await Task.Run(() => log.LogTrace($"Shipping email sent for order ${order.Id.ToString()}"));
        }

        [FunctionName("func-send-accounting-details")]
        public static async Task RunAccoutingReport(
            [ActivityTrigger] Order order,
            ILogger log)
        {
            // dumping the order to csv for accounting, would be better use to Blobs or something similar in production
            using (var writer = new StreamWriter($@"c:\data\accounting-report-{DateTime.Now.ToFileTime()}.csv"))
            using (var csvWriter = new CsvWriter(writer))
            {
                csvWriter.WriteRecord(order);
                await csvWriter.FlushAsync();
                await writer.WriteLineAsync($"\nORDER DATE: {DateTime.Now.ToString()}");
            }
        }
    }
}
