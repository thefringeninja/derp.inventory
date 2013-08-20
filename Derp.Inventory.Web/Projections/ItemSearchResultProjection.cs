using System;
using Derp.Inventory.Messages;
using Derp.Inventory.Web.Infrastructure.GetEventStore;
using Derp.Inventory.Web.ViewModels;
using EventStore.ClientAPI;

namespace Derp.Inventory.Web.Projections
{
    public class ItemSearchResultProjection : Handles<GetEventStoreMessage<ItemTracked>>
    {
        private readonly IWriteViews<Guid, ItemSearchResultViewModel> writer;

        public ItemSearchResultProjection(IWriteViews<Guid, ItemSearchResultViewModel> writer)
        {
            this.writer = writer;
        }

        #region ProjectionHandles<ItemTracked> Members

        public void Handle(GetEventStoreMessage<ItemTracked> message)
        {
            writer.AddOrUpdate(
                message.Message.WarehouseItemId,
                message.Position,
                () => new ItemSearchResultViewModel(message.Message.WarehouseItemId, message.Message.WarehouseId)
                {
                    Description = message.Message.Description,
                    Sku = message.Message.Sku
                });
        }

        #endregion
    }
}