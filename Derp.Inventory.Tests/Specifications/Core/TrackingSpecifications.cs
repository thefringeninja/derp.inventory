using System.Linq;
using Derp.Inventory.Application;
using Derp.Inventory.Domain;
using Derp.Inventory.Messages;
using Derp.Inventory.Tests.Templates;
using Simple.Testing.ClientFramework;

namespace Derp.Inventory.Tests.Specifications.Core
{
    public class TrackingSpecifications : WarehouseItemSpecifications
    {
        public Specification tracking_a_new_item()
        {
            return new CommandSpecification<WarehouseItem, TrackItem>
            {
                AggregateId = AggregateId,
                OnHandler = repository => new InventoryHandlers(repository),
                When =
                    new TrackItem(AggregateId, InventoryItemId, WarehouseId, "12345", "Abraxo Cleaner"),
                Expect =
                {
                    result => result.Decisions
                                    .OfType<ItemTracked>(typeof (ItemTracked))
                                    .Count().Equals(1)
                }
            };
        }
    }
}