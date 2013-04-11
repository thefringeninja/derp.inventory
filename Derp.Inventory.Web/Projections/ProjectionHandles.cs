using EventStore.ClientAPI;

namespace Derp.Inventory.Web.Projections
{
    public interface ProjectionHandles<in T>
    {
        void Handle(T message, Position? position);
    }
}