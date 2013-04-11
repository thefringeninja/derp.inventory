namespace Derp.Inventory
{
    public interface EventPublisher
    {
        void Publish<TEvent>(TEvent @event);
    }
}