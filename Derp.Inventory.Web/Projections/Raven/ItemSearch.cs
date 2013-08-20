using System.Linq;
using Derp.Inventory.Web.ViewModels;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Derp.Inventory.Web.Projections.Raven
{
    public class ItemSearch : AbstractIndexCreationTask<ItemSearchResultViewModel>
    {
        public ItemSearch()
        {
            Map = searchResults => from searchResult in searchResults
                                   select new
                                   {
                                       searchResult.WarehouseId,
                                       searchResult.SearchTerm
                                   };

            Indexes.Add(result => result.SearchTerm, FieldIndexing.Analyzed);
        }
    }
}