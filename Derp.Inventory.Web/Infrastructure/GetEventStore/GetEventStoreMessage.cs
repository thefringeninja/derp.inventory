using EventStore.ClientAPI;

namespace Derp.Inventory.Web.Infrastructure.GetEventStore
{
    public class GetEventStoreMessage<T>
    {
        public readonly T Message;
        public readonly Position Position;

        public GetEventStoreMessage(T message, Position position)
        {
            Message = message;
            Position = position;
        }
    }
}
