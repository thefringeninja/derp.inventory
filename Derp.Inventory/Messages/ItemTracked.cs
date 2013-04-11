using System;

namespace Derp.Inventory.Messages
{
    public class ItemTracked : Event
    {
        public readonly string Description;
        public readonly Guid InventoryItemId;
        public readonly string Sku;
        public readonly Guid WarehouseId;
        public readonly Guid WarehouseItemId;

        public ItemTracked(Guid warehouseItemId, Guid inventoryItemId, Guid warehouseId, string sku, string description)
        {
            WarehouseItemId = warehouseItemId;
            InventoryItemId = inventoryItemId;
            WarehouseId = warehouseId;
            Sku = sku;
            Description = description;
        }

        public override string ToString()
        {
            return string.Format(
                "Tracking inventory item {0} ({1}).", Sku, Description);
        }
    }
}