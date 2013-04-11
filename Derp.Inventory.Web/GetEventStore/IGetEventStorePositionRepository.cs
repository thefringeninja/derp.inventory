using EventStore.ClientAPI;

namespace Derp.Inventory.Web.GetEventStore
{
    public interface IGetEventStorePositionRepository
    {
        Position? GetLastProcessedPosition();
    }
}