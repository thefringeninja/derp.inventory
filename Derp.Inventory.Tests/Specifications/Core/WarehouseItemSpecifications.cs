using System;

namespace Derp.Inventory.Tests.Specifications.Core
{
    public abstract class WarehouseItemSpecifications
    {
        protected readonly Guid AggregateId = Guid.NewGuid();
        protected readonly Guid InventoryItemId = Guid.NewGuid();
        protected readonly Guid WarehouseId = Guid.NewGuid();
    }
}