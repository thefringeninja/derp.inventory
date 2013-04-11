using System;

namespace Derp.Inventory.Messages
{
    public class TrackItem : Command
    {
        public readonly string Description;
        public readonly Guid InventoryItemId;
        public readonly string Sku;
        public readonly Guid WarehouseId;
        public readonly Guid WarehouseItemId;

        private TrackItem()
        {
        }

        public TrackItem(Guid warehouseItemId, Guid inventoryItemId, Guid warehouseId, string sku, string description)
        {
            WarehouseItemId = warehouseItemId;
            InventoryItemId = inventoryItemId;
            WarehouseId = warehouseId;
            Sku = sku;
            Description = description;
        }

        public override string ToString()
        {
            return "Tracking a new item.";
        }
    }
}