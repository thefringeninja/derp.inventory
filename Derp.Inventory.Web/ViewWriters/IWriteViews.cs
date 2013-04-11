using System;
using EventStore.ClientAPI;

namespace Derp.Inventory.Web.ViewWriters
{
    public interface IWriteViews<in TKey, TView> where TView : class
    {
        void AddOrUpdate(TKey key, Position? position, Func<TView> add, Action<TView> update = null);

        void TryUpdate(TKey key, Position? position, Action<TView> update);

        void TryDelete(TKey key, Position? position);
    }
}