using System;

namespace Derp.Inventory.Messages
{
    public class ReceiveItem : Command
    {
        public readonly int Quantity;
        public readonly Guid WarehouseItemId;

        private ReceiveItem()
        {
        }

        public ReceiveItem(Guid warehouseItemId, int quantity)
        {
            WarehouseItemId = warehouseItemId;
            Quantity = quantity;
        }

        public override string ToString()
        {
            return string.Format("Receiving {0}.", Quantity);
        }
    }
}