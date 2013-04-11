using System;
using Derp.Inventory.Messages;
using Derp.Inventory.Web.ViewModels;
using Derp.Inventory.Web.ViewWriters;
using EventStore.ClientAPI;

namespace Derp.Inventory.Web.Projections
{
    public class ItemSearchResultProjection : ProjectionHandles<ItemTracked>
    {
        private readonly IWriteViews<Guid, ItemSearchResultViewModel> writer;

        public ItemSearchResultProjection(IWriteViews<Guid, ItemSearchResultViewModel> writer)
        {
            this.writer = writer;
        }

        #region ProjectionHandles<ItemTracked> Members

        public void Handle(ItemTracked message, Position? position)
        {
            writer.AddOrUpdate(
                message.WarehouseItemId,
                position,
                () => new ItemSearchResultViewModel(message.WarehouseItemId, message.WarehouseId)
                {
                    Description = message.Description,
                    Sku = message.Sku
                });
        }

        #endregion
    }
}