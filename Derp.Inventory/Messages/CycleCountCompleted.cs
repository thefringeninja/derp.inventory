using System;

namespace Derp.Inventory.Messages
{
    public class CycleCountCompleted : Event
    {
        public readonly int QuantityFound;
        public readonly Guid WarehouseItemId;

        public CycleCountCompleted(Guid warehouseItemId, int quantityFound)
        {
            WarehouseItemId = warehouseItemId;
            QuantityFound = quantityFound;
        }

        public override string ToString()
        {
            return string.Format("Cycle count completed. {0} on hand.", QuantityFound);
        }
    }
}