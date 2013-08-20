using System;
using System.Collections.Generic;
using Derp.Inventory.Web.ViewModels;

namespace Derp.Inventory.Web.Projections.InMemory
{
    public class InMemoryItemDetailRepository : Dictionary<Guid, ItemDetailViewModel>, IItemDetailRepository
    {
        #region IItemDetailRepository Members

        public ItemDetailViewModel Get(Guid warehouseItemId)
        {
            ItemDetailViewModel value;
            TryGetValue(warehouseItemId, out value);
            return value;
        }

        #endregion
    }
}