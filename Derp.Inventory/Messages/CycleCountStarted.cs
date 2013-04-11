using System;

namespace Derp.Inventory.Messages
{
    public class CycleCountStarted : Event
    {
        public readonly int QuantityOnHand;
        public readonly Guid WarehouseItemId;

        public CycleCountStarted(Guid warehouseItemId, int quantityOnHand)
        {
            WarehouseItemId = warehouseItemId;
            QuantityOnHand = quantityOnHand;
        }

        public override string ToString()
        {
            return string.Format("Cycle count started. {0} found on hand.", QuantityOnHand);
        }
    }
}