using System;

namespace Derp.Inventory.Messages
{
    public class ItemReceived : Event
    {
        public readonly int Quantity;
        public readonly Guid WarehouseItemId;

        public ItemReceived(Guid warehouseItemId, int quantity)
        {
            WarehouseItemId = warehouseItemId;
            Quantity = quantity;
        }

        public override string ToString()
        {
            return string.Format("Received {0}.", Quantity);
        }
    }
}