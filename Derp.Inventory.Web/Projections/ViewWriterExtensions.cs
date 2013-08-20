using System;
using EventStore.ClientAPI;

namespace Derp.Inventory.Web.Projections
{
    public static class ViewWriterExtensions
    {
        public static void AddOrUpdate<TKey, TView>(this IWriteViews<TKey, TView> writer, TKey key, Position position,
                                                    Action<TView> update)
            where TView : class, new()
        {
            writer.AddOrUpdate(key, position, () => new TView(), update);
        }
    }
}