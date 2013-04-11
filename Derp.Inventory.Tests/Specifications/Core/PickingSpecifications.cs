using System;
using System.Linq;
using Derp.Inventory.Application;
using Derp.Inventory.Domain;
using Derp.Inventory.Messages;
using Derp.Inventory.Tests.Templates;
using Simple.Testing.ClientFramework;

namespace Derp.Inventory.Tests.Specifications.Core
{
    public class PickingSpecifications : WarehouseItemSpecifications
    {
        public Specification picking_reduces_the_quantity()
        {
            return new CommandSpecification<WarehouseItem, PickItem>
            {
                AggregateId = AggregateId,
                OnHandler = repository => new InventoryHandlers(repository),
                Given =
                {
                    new ItemTracked(AggregateId, InventoryItemId, WarehouseId, "12345", "Abraxo Cleaner"),
                },
                When = new PickItem(AggregateId, 10),
                Expect =
                {
                    result => result.Decisions.OfType<ItemPicked>(typeof (ItemPicked)).Count().Equals(1),
                    result => result.Decisions.OfType<ItemPicked>(typeof (ItemPicked)).Single().Quantity.Equals(10)
                }
            };
        }

        public Specification picking_a_negative_quantity_throws_an_exception()
        {
            return new CommandSpecification<WarehouseItem, PickItem>
            {
                AggregateId = AggregateId,
                OnHandler = repository => new InventoryHandlers(repository),
                Given =
                {
                    new ItemTracked(AggregateId, InventoryItemId, WarehouseId, "12345", "Abraxo Cleaner"),
                },
                When = new PickItem(AggregateId, -10),
                Expect =
                {
                    result => result.ThrewAnException,
                    result => result.Exception is ArgumentOutOfRangeException,
                    result =>
                    result.Exception.Message.Equals(
                        "You tried to pick a negative quantity. Did you mean to receive or make an adjustment?\r\nParameter name: quantity")
                }
            };
        }

        public Specification picking_zero_throws_an_exception()
        {
            return new CommandSpecification<WarehouseItem, PickItem>
            {
                AggregateId = AggregateId,
                OnHandler = repository => new InventoryHandlers(repository),
                Given =
                {
                    new ItemTracked(AggregateId, InventoryItemId, WarehouseId, "12345", "Abraxo Cleaner"),
                },
                When = new PickItem(AggregateId, 0),
                Expect =
                {
                    result => result.ThrewAnException,
                    result => result.Exception is ArgumentOutOfRangeException,
                    result => result.Exception.Message.Equals("Tried to pick 0 quantity.\r\nParameter name: quantity")
                }
            };
        }

        public Specification picking_during_cycle_counting_throws_exception()
        {
            return new CommandSpecification<WarehouseItem, PickItem>
            {
                AggregateId = AggregateId,
                OnHandler = repository => new InventoryHandlers(repository),
                Given =
                {
                    new ItemTracked(AggregateId, InventoryItemId, WarehouseId, "12345", "Abraxo Cleaner"),
                    new CycleCountStarted(AggregateId, 0)
                },
                When = new PickItem(AggregateId, 1),
                Expect =
                {
                    result => result.ThrewAnException,
                    result => result.Exception is InvalidOperationException,
                    result => result.Exception.Message.Equals("Cycle count for this item has begun.")
                }
            };
        }
    }
}