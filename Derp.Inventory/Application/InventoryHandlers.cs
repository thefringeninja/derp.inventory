using Derp.Inventory.Domain;
using Derp.Inventory.Infrastructure;
using Derp.Inventory.Messages;

namespace Derp.Inventory.Application
{
    public class InventoryHandlers :
        Handles<TrackItem>,
        Handles<AdjustItemQuantity>,
        Handles<RelocateItem>,
        Handles<StartCycleCount>,
        Handles<CompleteCycleCount>,
        Handles<PickItem>,
        Handles<ReceiveItem>,
        Handles<LiquidateItem>
    {
        private readonly IRepository<WarehouseItem> repository;

        public InventoryHandlers(IRepository<WarehouseItem> repository)
        {
            this.repository = repository;
        }

        #region Handles<AdjustItemQuantity> Members

        public void Handle(AdjustItemQuantity message)
        {
            var item = repository.GetById(message.WarehouseItemId);
            item.AdjustQuantity(message.Quantity);
            repository.Save(item, message.Id);
        }

        #endregion

        #region Handles<CompleteCycleCount> Members

        public void Handle(CompleteCycleCount message)
        {
            var item = repository.GetById(message.WarehouseItemId);
            item.CompleteCycleCount(message.QuantityFound);
            repository.Save(item, message.Id);
        }

        #endregion

        #region Handles<LiquidateItem> Members

        public void Handle(LiquidateItem message)
        {
            var item = repository.GetById(message.WarehouseItemId);
            item.Liquidate(message.Quantity);
            repository.Save(item, message.Id);
        }

        #endregion

        #region Handles<PickItem> Members

        public void Handle(PickItem message)
        {
            var item = repository.GetById(message.WarehouseItemId);
            item.Pick(message.Quantity);
            repository.Save(item, message.Id);
        }

        #endregion

        #region Handles<ReceiveItem> Members

        public void Handle(ReceiveItem message)
        {
            var item = repository.GetById(message.WarehouseItemId);
            item.Receive(message.Quantity);
            repository.Save(item, message.Id);
        }

        #endregion

        #region Handles<RelocateItem> Members

        public void Handle(RelocateItem message)
        {
            var item = repository.GetById(message.WarehouseItemId);
            item.Relocate(message.Location);
            repository.Save(item, message.Id);
        }

        #endregion

        #region Handles<StartCycleCount> Members

        public void Handle(StartCycleCount message)
        {
            var item = repository.GetById(message.WarehouseItemId);
            item.StartCycleCount();
            repository.Save(item, message.Id);
        }

        #endregion

        #region Handles<TrackItem> Members

        public void Handle(TrackItem message)
        {
            var item = new WarehouseItem(message.WarehouseItemId, message.InventoryItemId, message.WarehouseId,
                                         message.Sku, message.Description);
            repository.Save(item, message.Id);
        }

        #endregion
    }
}