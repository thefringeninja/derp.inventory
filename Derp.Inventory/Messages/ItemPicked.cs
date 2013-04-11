using System;

namespace Derp.Inventory.Messages
{
    public class ItemPicked : Event
    {
        public readonly int Quantity;
        public readonly Guid WarehouseItemId;

        public ItemPicked(Guid warehouseItemId, int quantity)
        {
            WarehouseItemId = warehouseItemId;
            Quantity = quantity;
        }

        public override string ToString()
        {
            return string.Format("Picked {0}.", Quantity);
        }
    }
}