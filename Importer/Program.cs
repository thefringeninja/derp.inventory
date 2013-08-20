using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using CsQuery;
using Derp.Inventory;
using Derp.Inventory.Application;
using Derp.Inventory.Domain;
using Derp.Inventory.Infrastructure;
using Derp.Inventory.Messages;
using Derp.Inventory.Web.Infrastructure.GetEventStore;
using Derp.Inventory.Web.ViewModels;
using EventStore.ClientAPI;

namespace Importer
{
    public class Program
    {
        private static InventoryHandlers handlers;
        static Random random = new Random();
        public static void Main(string[] args)
        {
            var connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
            connection.Connect();
            var storage = new ConcurrentDictionary<Guid, List<Event>>();
            
            handlers = new InventoryHandlers(new InMemoryRepository<WarehouseItem>(storage));
            
            var warehouseId = WarehouseListViewModel.Instance.Single().WarehouseId;
            int i = 1000;
            var products = GetAllProducts();
            var tracking = from name in products
                           let sku = name.First() + name.Last() + "-" + ++i
                           let id = Guid.NewGuid()
                           select new TrackItem(id, id, warehouseId, sku, name);
            tracking.ForEachAsync(TrackItemAndDummyUpUsage).Wait();
            var repository = new GetEventStoreRepository<WarehouseItem>(connection, "inventory");

            var items = storage.Values.ToList().Select(events =>
            {
                var item = (WarehouseItem) Activator.CreateInstance(typeof (WarehouseItem), true);
                foreach (var @event in events)
                {
                    item.AsDynamic().ApplyChange(@event);
                }
                return item;
            }).ToList();
            Console.WriteLine("Saving");
            items.ForEach(item => repository.Save(item, Guid.NewGuid()));
        }

        static void TrackItemAndDummyUpUsage(TrackItem command)
        {
            handlers.Handle(command);
            for (var i = 0; i < random.Next(50); i++)
            {
                handlers.Handle(new AdjustItemQuantity(command.WarehouseItemId, random.Next(100)));
            }
        }

        private static IObservable<string> GetAllProducts()
        {
            var alphabet = Enumerable.Range(0x41, 26)
                .Select(Convert.ToChar).Union(new[]{'0'});

            var links = alphabet.Select(letter => new Uri("http://householdproducts.nlm.nih.gov/cgi-bin/household/list?tbl=TblBrands&alpha=" + letter));

            var productNames = links.Select(GetPage).Merge(2)
                .Select(reader => new CQ(reader))
                .SelectMany(document => document["ul > li > a"].Select(e => e.InnerText).Distinct());

            return productNames;
        } 

        private static IObservable<TextReader> GetPage(Uri link)
        {
            var response = WebRequest.Create(link).GetResponseAsync();
            return response.ContinueWith(task => new StreamReader(task.Result.GetResponseStream())).ToObservable();
        }
    }
}
