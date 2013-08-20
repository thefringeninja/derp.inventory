using System;
using Derp.Inventory.Messages;
using Derp.Inventory.Web.Infrastructure.GetEventStore;
using Derp.Inventory.Web.ViewModels;
using EventStore.ClientAPI;

namespace Derp.Inventory.Web.Projections
{
    public class ItemDetailResultProjection 
        : Handles<GetEventStoreMessage<ItemTracked>>,
        Handles<GetEventStoreMessage<ItemPicked>>,
        Handles<GetEventStoreMessage<ItemLiquidated>>,
        Handles<GetEventStoreMessage<ItemQuantityAdjusted>>,
        Handles<GetEventStoreMessage<ItemReceived>>,
        Handles<GetEventStoreMessage<CycleCountStarted>>,
        Handles<GetEventStoreMessage<CycleCountCompleted>>
    {
        private readonly IWriteViews<Guid, ItemDetailViewModel> writer;

        public ItemDetailResultProjection(IWriteViews<Guid, ItemDetailViewModel> writer)
        {
            this.writer = writer;
        }

        #region ProjectionHandles<GetEventStoreMessage<CycleCountCompleted>> Members

        public void Handle(GetEventStoreMessage<CycleCountCompleted> message)
        {
            writer.TryUpdate(message.Message.WarehouseItemId, message.Position, vm => vm.Counting = false);
        }

        #endregion

        #region ProjectionHandles<GetEventStoreMessage<CycleCountStarted>> Members

        public void Handle(GetEventStoreMessage<CycleCountStarted> message)
        {
            writer.TryUpdate(message.Message.WarehouseItemId, message.Position, vm => vm.Counting = true);
        }

        #endregion

        #region ProjectionHandles<GetEventStoreMessage<ItemLiquidated>> Members

        public void Handle(GetEventStoreMessage<ItemLiquidated> message)
        {
            ChangeQuantity(message.Message.WarehouseItemId, message.Position, -message.Message.Quantity);
        }

        #endregion

        #region ProjectionHandles<GetEventStoreMessage<ItemPicked>> Members

        public void Handle(GetEventStoreMessage<ItemPicked> message)
        {
            ChangeQuantity(message.Message.WarehouseItemId, message.Position, -message.Message.Quantity);
        }

        #endregion

        #region ProjectionHandles<GetEventStoreMessage<ItemQuantityAdjusted>> Members

        public void Handle(GetEventStoreMessage<ItemQuantityAdjusted> message)
        {
            ChangeQuantity(message.Message.WarehouseItemId, message.Position, message.Message.AdjustmentQuantity);
        }

        #endregion

        #region ProjectionHandles<GetEventStoreMessage<ItemReceived>> Members

        public void Handle(GetEventStoreMessage<ItemReceived> message)
        {
            ChangeQuantity(message.Message.WarehouseItemId, message.Position, message.Message.Quantity);
        }

        #endregion

        #region ProjectionHandles<GetEventStoreMessage<ItemTracked>> Members

        public void Handle(GetEventStoreMessage<ItemTracked> message)
        {
            writer.AddOrUpdate(
                message.Message.WarehouseItemId, message.Position,
                () => new ItemDetailViewModel(message.Message.WarehouseItemId),
                vm =>
                {
                    vm.Sku = message.Message.Sku;
                    vm.Description = message.Message.Description;
                });
        }

        #endregion

        private void ChangeQuantity(Guid warehouseItemId, Position position, int quantity)
        {
            writer.TryUpdate(warehouseItemId, position, vm => vm.QuantityOnHand += quantity);
        }

   }
}