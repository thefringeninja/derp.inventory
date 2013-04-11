using System;
using System.Linq;
using Derp.Inventory.Application;
using Derp.Inventory.Domain;
using Derp.Inventory.Messages;
using Derp.Inventory.Tests.Templates;
using Simple.Testing.ClientFramework;

namespace Derp.Inventory.Tests.Specifications.Core
{
    public class LocationSpecifications : WarehouseItemSpecifications
    {
        public Specification relocation()
        {
            return new CommandSpecification<WarehouseItem, RelocateItem>
            {
                AggregateId = AggregateId,
                OnHandler = repository => new InventoryHandlers(repository),
                Given =
                {
                    new ItemTracked(AggregateId, InventoryItemId, WarehouseId, "12345", "Abraxo Cleaner")
                },
                When = new RelocateItem(AggregateId, "SH A1"),
                Expect =
                {
                    result => result.Decisions
                                    .OfType<ItemRelocated>(typeof (ItemRelocated))
                                    .Count().Equals(1),
                    result => result.Decisions
                                    .OfType<ItemRelocated>(typeof (ItemRelocated))
                                    .Single().Location.Equals("SH A1")
                }
            };
        }

        public Specification adjusting_during_cycle_counting_throws_exception()
        {
            return new CommandSpecification<WarehouseItem, AdjustItemQuantity>
            {
                AggregateId = AggregateId,
                OnHandler = repository => new InventoryHandlers(repository),
                Given =
                {
                    new ItemTracked(AggregateId, InventoryItemId, WarehouseId, "12345", "Abraxo Cleaner"),
                    new CycleCountStarted(AggregateId, 0)
                },
                When = new AdjustItemQuantity(AggregateId, 1),
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