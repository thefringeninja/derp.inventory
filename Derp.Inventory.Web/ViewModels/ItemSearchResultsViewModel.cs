using System.Collections;
using System.Collections.Generic;

namespace Derp.Inventory.Web.ViewModels
{
    public class ItemSearchResultsViewModel : IEnumerable<ItemSearchResultViewModel>
    {
        private readonly IEnumerable<ItemSearchResultViewModel> results;

        public ItemSearchResultsViewModel(IEnumerable<ItemSearchResultViewModel> results)
        {
            this.results = results;
        }

        #region IEnumerable<ItemSearchResultViewModel> Members

        public IEnumerator<ItemSearchResultViewModel> GetEnumerator()
        {
            return results.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}