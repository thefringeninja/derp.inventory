using System;
using Derp.Inventory.Web.Services;
using Nancy;

namespace Derp.Inventory.Web.Modules
{
    public class InventoryModule : NancyModule
    {
        public InventoryModule(IItemDetailRepository items)
            : base("/inventory")
        {
            Get["/{warehouseItemId}"] = p =>
            {
                Guid warehouseItemId = p.warehouseItemId;

                return items.Get(warehouseItemId);
            };
        }
    }
}