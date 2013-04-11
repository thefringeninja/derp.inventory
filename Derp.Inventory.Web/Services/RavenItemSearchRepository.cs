using System;
using System.Linq;
using Derp.Inventory.Web.Raven;
using Derp.Inventory.Web.ViewModels;
using Raven.Client;

namespace Derp.Inventory.Web.Services
{
    public class RavenItemSearchRepository : IItemSearchRepository
    {
        private readonly IDocumentSession session;

        public RavenItemSearchRepository(IDocumentSession session)
        {
            this.session = session;
        }

        #region IItemSearchRepository Members

        public ItemSearchResultsViewModel Search(string term, Guid warehouseId)
        {
            return new ItemSearchResultsViewModel(
                session.Query<ItemSearchResultViewModel, ItemSearch>()
                       .Search(vm => vm.SearchTerm, term)
                       .Where(vm => vm.WarehouseId == warehouseId)
                       .ToList());
        }

        #endregion
    }
}