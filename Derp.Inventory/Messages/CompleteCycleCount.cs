using System;

namespace Derp.Inventory.Messages
{
    public class CompleteCycleCount : Command
    {
        public readonly int QuantityFound;
        public readonly Guid WarehouseItemId;

        private CompleteCycleCount()
        {
        }

        public CompleteCycleCount(Guid warehouseItemId, int quantityFound)
        {
            WarehouseItemId = warehouseItemId;
            QuantityFound = quantityFound;
        }

        public override string ToString()
        {
            return string.Format("Completing cycle count. {0} found on hand.", QuantityFound);
        }
    }
}