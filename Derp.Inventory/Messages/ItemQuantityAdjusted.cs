using System;

namespace Derp.Inventory.Messages
{
    public class ItemQuantityAdjusted : Event
    {
        public readonly int AdjustmentQuantity;
        public readonly Guid WarehouseItemId;

        public ItemQuantityAdjusted(Guid warehouseItemId, int adjustmentQuantity)
        {
            WarehouseItemId = warehouseItemId;
            AdjustmentQuantity = adjustmentQuantity;
        }

        public override string ToString()
        {
            return string.Format("Quantity adjusted by {0}.",
                                 AdjustmentQuantity);
        }
    }
}