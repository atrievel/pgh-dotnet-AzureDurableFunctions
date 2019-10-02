using DurableFunctionsDemo.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DurableFunctionsDemo
{
    public static class ProcessOrdersOrchestrator
    {
        [FunctionName("func-process-order-orchestrator")]
        public static async Task<OrderResult> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context,
            ILogger log)
        {
            var order = context.GetInput<Order>();
            var defaultRetryOptions = new RetryOptions(TimeSpan.FromSeconds(10), 5);

            try
            {
                // if the order is new, generate order id, calculate the total, and set status to InProgress
                if (order.Id == Guid.Empty)
                {
                    order.Id = context.NewGuid();

                    order.Total = await context.CallActivityWithRetryAsync<float>(
                        "func-calc-order-total",
                        defaultRetryOptions,
                        order);

                    order.OrderStatus = OrderStatus.InProgress;
                    order.OrchestrationId = context.InstanceId;
                }

                // use this if statement for logging to prevent duplicate messages
                if (!context.IsReplaying)
                {
                    log.LogInformation($"Processing order {order.Id.ToString()}");
                }

                // only charge the customer when the order was fully created
                if (order.OrderStatus == OrderStatus.InProgress)
                {
                    if (!context.IsReplaying)
                    {
                        log.LogInformation($"Charging customer {order.Customer.Name} ${order.Total} on order {order.Id.ToString()}");
                    }

                    // call service to charge card, which is known to be buggy/long running 
                    // we will cancel an order if it takes 10 seconds to charge the card (assuming the card cannot be processed, basically)
                    using (var cts = new CancellationTokenSource())
                    {
                        TimeSpan timeout = TimeSpan.FromSeconds(10);
                        DateTime deadline = context.CurrentUtcDateTime.Add(timeout);
                        Task timer = context.CreateTimer(deadline, cts.Token);
                        Task chargeCustomer = context.CallActivityAsync(
                            "func-charge-customer",
                            (order.Customer, order.Total));

                        Task firstDone = await Task.WhenAny(timer, chargeCustomer);

                        if (firstDone == chargeCustomer)
                        {
                            // we could drop this order on a queue for the DC to ship now, but we will just simulate this out of laziness
                            order.OrderStatus = OrderStatus.Charged;
                        }
                        else
                        {
                            throw new Exception("Cannot process card");
                        }
                    }
                }

                // check if the order can be shipped or not from an external event
                // in this case, we need a human to say the order was picked, packed, and shipped 
                // but we will simulate through an HTTP Trigger for simplicity
                if (order.OrderStatus == OrderStatus.Charged)
                {
                    if (!context.IsReplaying)
                    {
                        log.LogInformation($"Shipping order {order.Id.ToString()}");
                    }

                    bool shipped = await context.WaitForExternalEvent("func-ship-order", TimeSpan.FromMinutes(30), false);

                    if (shipped)
                    {
                        order.OrderStatus = OrderStatus.Shipped;
                    }
                    else
                    {
                        throw new Exception("Order is not shippable");
                    }

                }

                // fan-out on shipping confirmation emails and posting sale data to accounting
                if (order.OrderStatus == OrderStatus.Shipped)
                {
                    if (!context.IsReplaying)
                    {
                        log.LogInformation($"Whoop whoop, the order {order.Id.ToString()} was shipped. Cleaning up now...");
                    }

                    await Task.WhenAll(new List<Task>
                    {
                        context.CallActivityWithRetryAsync("func-send-ship-confirmation", defaultRetryOptions, order),
                        context.CallActivityWithRetryAsync("func-send-accounting-details", defaultRetryOptions, order)
                    });

                    return new OrderResult { OrderId = order.Id, OrderStatus = order.OrderStatus, OrderTotal = order.Total };
                }

                return new OrderResult { OrderId = order.Id, OrderStatus = order.OrderStatus, OrderTotal = order.Total };
            }
            catch (Exception ex)
            {
                if (!context.IsReplaying)
                {
                    log.LogError($"Could not process order {order.Id.ToString()}");
                    log.LogError($"Reason: {ex.ToString()}");
                }

                // if the order processing fails, cancel (and refund) the order, and then retry processing the order 
                order.OrderStatus = OrderStatus.New;
                context.ContinueAsNew(order);

                return new OrderResult { OrderId = order.Id, OrderStatus = order.OrderStatus, OrderTotal = 0f };
            }
        }
    }
}