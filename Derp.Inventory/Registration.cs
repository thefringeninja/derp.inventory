namespace Derp.Inventory
{
    public interface Registration
// ReSharper restore InconsistentNaming
    {
        void Register<T>(Handles<T> handler) where T : class;
    }
}