using System;

namespace Derp.Inventory.Web.ViewModels
{
    public class ItemSearchResultViewModel
    {
        private string description;
        private string sku;

        public ItemSearchResultViewModel(Guid warehouseItemId, Guid warehouseId)
        {
            SearchTerm = String.Empty;
            WarehouseItemId = warehouseItemId;
            WarehouseId = warehouseId;
        }

        public string Sku
        {
            get { return sku; }
            set
            {
                sku = value;

                SetSearchTerm();
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                description = value;

                SetSearchTerm();
            }
        }

        public Guid WarehouseItemId { get; private set; }
        public Guid WarehouseId { get; private set; }
        public string SearchTerm { get; set; }

        private void SetSearchTerm()
        {
            SearchTerm = (Sku + " " + Description).Trim().ToLowerInvariant();
        }
    }
}