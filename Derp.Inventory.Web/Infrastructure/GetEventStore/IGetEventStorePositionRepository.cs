using EventStore.ClientAPI;

namespace Derp.Inventory.Web.Infrastructure.GetEventStore
{
    public interface IGetEventStorePositionRepository
    {
        Position GetLastProcessedPosition();
    }
}