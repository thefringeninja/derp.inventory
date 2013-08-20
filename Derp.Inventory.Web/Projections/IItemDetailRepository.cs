using System;
using Derp.Inventory.Web.ViewModels;

namespace Derp.Inventory.Web.Projections
{
    public interface IItemDetailRepository
    {
        ItemDetailViewModel Get(Guid warehouseItemId);
    }
}