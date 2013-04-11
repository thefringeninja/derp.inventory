using System;
using Derp.Inventory.Messages;
using Derp.Inventory.Web.ViewModels;
using Derp.Inventory.Web.ViewWriters;
using EventStore.ClientAPI;

namespace Derp.Inventory.Web.Projections
{
    public class ItemDetailResultProjection :
        ProjectionHandles<ItemTracked>,
        ProjectionHandles<ItemPicked>,
        ProjectionHandles<ItemLiquidated>,
        ProjectionHandles<ItemQuantityAdjusted>,
        ProjectionHandles<ItemReceived>,
        ProjectionHandles<CycleCountStarted>,
        ProjectionHandles<CycleCountCompleted>
    {
        private readonly IWriteViews<Guid, ItemDetailViewModel> writer;

        public ItemDetailResultProjection(IWriteViews<Guid, ItemDetailViewModel> writer)
        {
            this.writer = writer;
        }

        #region ProjectionHandles<CycleCountCompleted> Members

        public void Handle(CycleCountCompleted message, Position? position)
        {
            writer.TryUpdate(message.WarehouseItemId, position, vm => vm.Counting = false);
        }

        #endregion

        #region ProjectionHandles<CycleCountStarted> Members

        public void Handle(CycleCountStarted message, Position? position)
        {
            writer.TryUpdate(message.WarehouseItemId, position, vm => vm.Counting = true);
        }

        #endregion

        #region ProjectionHandles<ItemLiquidated> Members

        public void Handle(ItemLiquidated message, Position? position)
        {
            ChangeQuantity(message.WarehouseItemId, position, -message.Quantity);
        }

        #endregion

        #region ProjectionHandles<ItemPicked> Members

        public void Handle(ItemPicked message, Position? position)
        {
            ChangeQuantity(message.WarehouseItemId, position, -message.Quantity);
        }

        #endregion

        #region ProjectionHandles<ItemQuantityAdjusted> Members

        public void Handle(ItemQuantityAdjusted message, Position? position)
        {
            ChangeQuantity(message.WarehouseItemId, position, message.AdjustmentQuantity);
        }

        #endregion

        #region ProjectionHandles<ItemReceived> Members

        public void Handle(ItemReceived message, Position? position)
        {
            ChangeQuantity(message.WarehouseItemId, position, message.Quantity);
        }

        #endregion

        #region ProjectionHandles<ItemTracked> Members

        public void Handle(ItemTracked message, Position? position)
        {
            writer.AddOrUpdate(
                message.WarehouseItemId, position,
                () => new ItemDetailViewModel(message.WarehouseItemId),
                vm =>
                {
                    vm.Sku = message.Sku;
                    vm.Description = message.Description;
                });
        }

        #endregion

        private void ChangeQuantity(Guid warehouseItemId, Position? position, int quantity)
        {
            writer.TryUpdate(warehouseItemId, position, vm => vm.QuantityOnHand += quantity);
        }
    }
}