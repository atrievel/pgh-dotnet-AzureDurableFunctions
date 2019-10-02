using DurableFunctionsDemo.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace SampleDataGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Generating data, please wait...");

            var prodGen = new Bogus.Faker<Product>()
                .RuleFor(p => p.Id, f => f.Commerce.Ean13())
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Price, f => Convert.ToSingle(f.Commerce.Price()));

            var custGen = new Bogus.Faker<Customer>()
                .RuleFor(c => c.Name, f => f.Name.FullName())
                .RuleFor(c => c.Address, f => f.Address.StreetAddress())
                .RuleFor(c => c.Country, f => f.Address.Country())
                .RuleFor(c => c.Email, f => f.Internet.Email());

            var customers = custGen.Generate(25);

            var orderGen = new Bogus.Faker<Order>()
                .RuleFor(o => o.Customer, f => customers.ElementAt(f.Random.Int(min: 0, max: customers.Count - 1)))
                .RuleFor(o => o.PurchasedItems, f => prodGen.Generate(f.Random.Int(min: 1, max: 10)));

            using (StreamWriter file = File.CreateText(@"c:\data\sample-orders.json"))
            {
                file.Write(JsonConvert.SerializeObject(orderGen.Generate(1)));
            }

            Console.WriteLine("Done.");
        }
    }
}
