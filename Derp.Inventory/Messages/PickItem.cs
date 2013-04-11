using System;

namespace Derp.Inventory.Messages
{
    public class PickItem : Command
    {
        public readonly int Quantity;
        public readonly Guid WarehouseItemId;

        private PickItem()
        {
        }

        public PickItem(Guid warehouseItemId, int quantity)
        {
            WarehouseItemId = warehouseItemId;
            Quantity = quantity;
        }

        public override string ToString()
        {
            return string.Format("Picking {0}.", Quantity);
        }
    }
}