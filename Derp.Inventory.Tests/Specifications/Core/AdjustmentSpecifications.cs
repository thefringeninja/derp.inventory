using System;
using System.Linq;
using Derp.Inventory.Application;
using Derp.Inventory.Domain;
using Derp.Inventory.Messages;
using Derp.Inventory.Tests.Templates;
using Simple.Testing.ClientFramework;

namespace Derp.Inventory.Tests.Specifications.Core
{
    public class AdjustmentSpecifications : WarehouseItemSpecifications
    {
        public Specification adjusting_quantities_on_hand()
        {
            return new CommandSpecification<WarehouseItem, AdjustItemQuantity>
            {
                AggregateId = AggregateId,
                OnHandler = repository => new InventoryHandlers(repository),
                Given =
                {
                    new ItemTracked(AggregateId, InventoryItemId, WarehouseId, "12345", "Abraxo Cleaner"),
                },
                When = new AdjustItemQuantity(AggregateId, 10),
                Expect =
                {
                    result => result.Decisions.OfType<ItemQuantityAdjusted>(typeof (ItemQuantityAdjusted))
                                    .Count().Equals(1),
                    result => result.Decisions.OfType<ItemQuantityAdjusted>(typeof (ItemQuantityAdjusted))
                                    .Single().AdjustmentQuantity.Equals(10)
                }
            };
        }

        public Specification adjusting_quantities_on_hand_after_some_already_on_hand()
        {
            return new CommandSpecification<WarehouseItem, AdjustItemQuantity>
            {
                AggregateId = AggregateId,
                OnHandler = repository => new InventoryHandlers(repository),
                Given =
                {
                    new ItemTracked(AggregateId, InventoryItemId, WarehouseId, "12345", "Abraxo Cleaner"),
                    new ItemQuantityAdjusted(AggregateId, 100)
                },
                When = new AdjustItemQuantity(AggregateId, 10),
                Expect =
                {
                    result => result.Decisions.OfType<ItemQuantityAdjusted>(typeof (ItemQuantityAdjusted))
                                    .Count().Equals(1),
                    result => result.Decisions.OfType<ItemQuantityAdjusted>(typeof (ItemQuantityAdjusted))
                                    .Single().AdjustmentQuantity.Equals(10)
                }
            };
        }

        public Specification negative_quantities_on_hand_are_ok()
        {
            return new CommandSpecification<WarehouseItem, AdjustItemQuantity>
            {
                AggregateId = AggregateId,
                OnHandler = repository => new InventoryHandlers(repository),
                Given =
                {
                    new ItemTracked(AggregateId, InventoryItemId, WarehouseId, "12345", "Abraxo Cleaner"),
                    new ItemQuantityAdjusted(AggregateId, 100)
                },
                When = new AdjustItemQuantity(AggregateId, -150),
                Expect =
                {
                    result => result.Decisions.OfType<ItemQuantityAdjusted>(typeof (ItemQuantityAdjusted))
                                    .Count().Equals(1),
                    result => result.Decisions.OfType<ItemQuantityAdjusted>(typeof (ItemQuantityAdjusted))
                                    .Single().AdjustmentQuantity.Equals(-150)
                }
            };
        }


        public Specification relocating_during_cycle_counting_throws_exception()
        {
            return new CommandSpecification<WarehouseItem, RelocateItem>
            {
                AggregateId = AggregateId,
                OnHandler = repository => new InventoryHandlers(repository),
                Given =
                {
                    new ItemTracked(AggregateId, InventoryItemId, WarehouseId, "12345", "Abraxo Cleaner"),
                    new CycleCountStarted(AggregateId, 0)
                },
                When = new RelocateItem(AggregateId, "SH A1"),
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