using DurableFunctionsDemo.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace DurableFunctionsDemo.Functions
{
    public static class OrderTotalService
    {
        private static readonly float FLAT_TAX_RATE = 1.07f;
        private static readonly float FLAT_SHIPPING_COST = 7.5f;

        [FunctionName("func-calc-order-total")]
        public static async Task<float> Run(
            [ActivityTrigger] Order order,
            ILogger log)
        {
            // simulate shipping and tax with a flat rate on a long running API
            await Task.Delay(2000);

            float total = order.PurchasedItems.Count >= 5 ? FLAT_SHIPPING_COST : 0f;
            return order.PurchasedItems.Sum(i => i.Price) * FLAT_TAX_RATE;
        }
    }
}
