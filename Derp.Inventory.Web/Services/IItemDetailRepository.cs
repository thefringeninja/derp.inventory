using System;
using Derp.Inventory.Web.ViewModels;

namespace Derp.Inventory.Web.Services
{
    public interface IItemDetailRepository
    {
        ItemDetailViewModel Get(Guid warehouseItemId);
    }
}