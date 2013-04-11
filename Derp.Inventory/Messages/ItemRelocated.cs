using System;

namespace Derp.Inventory.Messages
{
    public class ItemRelocated : Event
    {
        public readonly string Location;
        public readonly Guid WarehouseItemid;

        public ItemRelocated(Guid warehouseItemid, string location)
        {
            WarehouseItemid = warehouseItemid;
            Location = location;
        }

        public override string ToString()
        {
            return "Relocated to " + Location;
        }
    }
}