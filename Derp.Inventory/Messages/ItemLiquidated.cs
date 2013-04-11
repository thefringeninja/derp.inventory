using System;

namespace Derp.Inventory.Messages
{
    public class ItemLiquidated : Event
    {
        public readonly int Quantity;
        public readonly Guid WarehouseItemId;

        public ItemLiquidated(Guid warehouseItemId, int quantity)
        {
            WarehouseItemId = warehouseItemId;
            Quantity = quantity;
        }

        public override string ToString()
        {
            return string.Format("Liquidated {0}.", Quantity);
        }
    }
}