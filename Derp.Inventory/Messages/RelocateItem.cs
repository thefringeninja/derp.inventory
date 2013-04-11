using System;

namespace Derp.Inventory.Messages
{
    public class RelocateItem : Command
    {
        public readonly string Location;
        public readonly Guid WarehouseItemId;

        private RelocateItem()
        {
        }

        public RelocateItem(Guid warehouseItemId, string location)
        {
            WarehouseItemId = warehouseItemId;
            Location = location;
        }

        public override string ToString()
        {
            return "Putting item in " + Location;
        }
    }
}