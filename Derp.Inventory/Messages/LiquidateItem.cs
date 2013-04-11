using System;

namespace Derp.Inventory.Messages
{
    public class LiquidateItem : Command
    {
        public readonly int Quantity;
        public readonly Guid WarehouseItemId;

        private LiquidateItem()
        {
        }

        public LiquidateItem(Guid warehouseItemId, int quantity)
        {
            WarehouseItemId = warehouseItemId;
            Quantity = quantity;
        }

        public override string ToString()
        {
            return string.Format("Liquidating {0}.", Quantity);
        }
    }
}