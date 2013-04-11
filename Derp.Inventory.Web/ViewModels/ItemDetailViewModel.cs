using System;

namespace Derp.Inventory.Web.ViewModels
{
    public class ItemDetailViewModel
    {
        public ItemDetailViewModel(Guid warehouseItemId)
        {
            WarehouseItemId = warehouseItemId;
        }

        public Guid WarehouseItemId { get; private set; }
        public string Sku { get; set; }
        public string Description { get; set; }
        public int QuantityOnHand { get; set; }
        public bool Counting { get; set; }
    }
}