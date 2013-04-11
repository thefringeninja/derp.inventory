using Derp.Inventory.Messages;

namespace Derp.Inventory.Domain
{
    partial class WarehouseItem
    {
        // ReSharper disable UnusedMember.Local
        private void Apply(ItemTracked e)
        {
            id = e.WarehouseItemId;
        }

        private void Apply(ItemQuantityAdjusted e)
        {
            quantityOnHand += e.AdjustmentQuantity;
        }

        private void Apply(ItemPicked e)
        {
            quantityOnHand -= e.Quantity;
        }

        private void Apply(ItemLiquidated e)
        {
            quantityOnHand -= e.Quantity;
        }

        private void Apply(ItemReceived e)
        {
            quantityOnHand += e.Quantity;
        }


        private void Apply(CycleCountStarted e)
        {
            counting = true;
        }

        // ReSharper restore UnusedMember.Local
    }
}