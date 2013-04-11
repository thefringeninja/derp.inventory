using System.Linq;
using Derp.Inventory.Application;
using Derp.Inventory.Domain;
using Derp.Inventory.Messages;
using Derp.Inventory.Tests.Templates;
using Simple.Testing.ClientFramework;

namespace Derp.Inventory.Tests.Specifications.Core
{
    public class CycleCountingSpecifications : WarehouseItemSpecifications
    {
        public Specification cycle_counting()
        {
            return new CommandSpecification<WarehouseItem, StartCycleCount>
            {
                AggregateId = AggregateId,
                OnHandler = repository => new InventoryHandlers(repository),
                Given =
                {
                    new ItemTracked(AggregateId, InventoryItemId, WarehouseId, "12345", "Abraxo Cleaner")
                },
                When = new StartCycleCount(AggregateId),
                Expect =
                {
                    result => result.Decisions
                                    .OfType<CycleCountStarted>(typeof (CycleCountStarted))
                                    .Count().Equals(1)
                }
            };
        }

        public Specification cycle_counting_after_activity()
        {
            return new CommandSpecification<WarehouseItem, StartCycleCount>
            {
                AggregateId = AggregateId,
                OnHandler = repository => new InventoryHandlers(repository),
                Given =
                {
                    new ItemTracked(AggregateId, InventoryItemId, WarehouseId, "12345", "Abraxo Cleaner"),
                    new ItemReceived(AggregateId, 1246),
                    new ItemPicked(AggregateId, 100),
                    new ItemQuantityAdjusted(AggregateId, -1),
                    new ItemQuantityAdjusted(AggregateId, 3),
                    new ItemLiquidated(AggregateId, 10)
                },
                When = new StartCycleCount(AggregateId),
                Expect =
                {
                    result => result.Decisions
                                    .OfType<CycleCountStarted>(typeof (CycleCountStarted))
                                    .Count().Equals(1),
                    result => result.Decisions
                                    .OfType<CycleCountStarted>(typeof (CycleCountStarted))
                                    .Single().QuantityOnHand.Equals(1138),
                }
            };
        }

        public Specification completion_of_cycle_counting()
        {
            return new CommandSpecification<WarehouseItem, CompleteCycleCount>
            {
                AggregateId = AggregateId,
                OnHandler = repository => new InventoryHandlers(repository),
                Given =
                {
                    new ItemTracked(AggregateId, InventoryItemId, WarehouseId, "12345", "Abraxo Cleaner"),
                    new ItemQuantityAdjusted(AggregateId, 30)
                },
                When = new CompleteCycleCount(AggregateId, 20),
                Expect =
                {
                    result => result.Decisions.OfType<CycleCountCompleted>(typeof (CycleCountCompleted))
                                    .Count().Equals(1),
                    result => result.Decisions.OfType<CycleCountCompleted>(typeof (CycleCountCompleted))
                                    .Single().QuantityFound.Equals(20),
                    result => result.Decisions.OfType<ItemQuantityAdjusted>(typeof (ItemQuantityAdjusted))
                                    .Single().AdjustmentQuantity.Equals(-10)
                }
            };
        }
    }
}