using System;

namespace Derp.Inventory.Web.ViewModels
{
    public class WarehouseNameViewModel
    {
        public WarehouseNameViewModel(string name, Guid warehouseId)
        {
            Name = name;
            WarehouseId = warehouseId;
        }

        public string Name { get; private set; }
        public Guid WarehouseId { get; private set; }
    }
}