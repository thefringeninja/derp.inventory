using System;
using Derp.Inventory.Messages;

namespace Derp.Inventory.Domain
{
    public partial class WarehouseItem : AggregateRoot
    {
        private bool counting;
        private Guid id;
        private int quantityOnHand;

        protected WarehouseItem()
        {
        }

        public WarehouseItem(Guid warehouseItemId, Guid inventoryItemId, Guid warehouseId, string sku,
                             string description)
        {
            ApplyChange(new ItemTracked(warehouseItemId, inventoryItemId, warehouseId, sku, description));
        }

        public override Guid Id
        {
            get { return id; }
        }

        private void ShouldNotBeCounting()
        {
            Guard.Against(counting, "Cycle count for this item has begun.");
        }


        public void AdjustQuantity(int quantity)
        {
            ShouldNotBeCounting();
            ApplyChange(new ItemQuantityAdjusted(id, quantity));
        }

        public void Relocate(string location)
        {
            ShouldNotBeCounting();
            ApplyChange(new ItemRelocated(id, location));
        }

        public void StartCycleCount()
        {
            ApplyChange(new CycleCountStarted(id, quantityOnHand));
        }

        public void CompleteCycleCount(int quantityFound)
        {
            ApplyChange(new CycleCountCompleted(id, quantityFound));
            ApplyChange(new ItemQuantityAdjusted(id, quantityFound - quantityOnHand));
        }

        public void Pick(int quantity)
        {
            ShouldNotBeCounting();
            Guard.Against<ArgumentOutOfRangeException>(quantity == 0, "quantity", "Tried to pick 0 quantity.");
            Guard.Against<ArgumentOutOfRangeException>(quantity < 0, "quantity",
                                                       "You tried to pick a negative quantity. Did you mean to receive or make an adjustment?");
            ApplyChange(new ItemPicked(id, quantity));
        }

        public void Receive(int quantity)
        {
            ShouldNotBeCounting();
            Guard.Against<ArgumentOutOfRangeException>(quantity == 0, "quantity", "Tried to receive 0 quantity.");
            Guard.Against<ArgumentOutOfRangeException>(quantity < 0, "quantity",
                                                       "You tried to receive a negative quantity. Did you mean to pick or make an adjustment?");
            ApplyChange(new ItemReceived(id, quantity));
        }

        public void Liquidate(int quantity)
        {
            ShouldNotBeCounting();
            Guard.Against<ArgumentOutOfRangeException>(quantity == 0, "quantity", "Tried to liquidate 0 quantity.");
            Guard.Against<ArgumentOutOfRangeException>(quantity < 0, "quantity",
                                                       "You tried to liquidate a negative quantity. Did you mean to receive or make an adjustment?");
            ApplyChange(new ItemLiquidated(id, quantity));
        }
    }
}