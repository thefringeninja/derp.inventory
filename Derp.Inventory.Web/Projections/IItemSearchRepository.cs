using System;
using Derp.Inventory.Web.ViewModels;

namespace Derp.Inventory.Web.Projections
{
    public interface IItemSearchRepository
    {
        ItemSearchResultsViewModel Search(string term, Guid warehouseId);
    }
}