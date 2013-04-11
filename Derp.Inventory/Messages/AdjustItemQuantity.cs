using System;

namespace Derp.Inventory.Messages
{
    public class AdjustItemQuantity : Command
    {
        public readonly int Quantity;
        public readonly Guid WarehouseItemId;

        private AdjustItemQuantity()
        {
        }

        public AdjustItemQuantity(Guid warehouseItemId, int quantity)
        {
            WarehouseItemId = warehouseItemId;
            Quantity = quantity;
        }

        public override string ToString()
        {
            return "Adjusting quantity by " + Quantity;
        }
    }
}