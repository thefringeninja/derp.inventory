namespace Derp.Inventory
{
// ReSharper disable InconsistentNaming
    public interface CommandSender
// ReSharper restore InconsistentNaming
    {
        void Send<TCommand>(TCommand command) where TCommand : class;
    }

// ReSharper disable InconsistentNaming
}