using System;
using System.Collections.Generic;

namespace Derp.Inventory.Infrastructure
{
    public interface IRepository<TAggregate> where TAggregate : AggregateRoot
    {
        TAggregate GetById(Guid id, Int32 version = Int32.MaxValue);
        void Save(TAggregate aggregate, Guid commitId, Action<IDictionary<string, object>> updateHeaders = null);
    }
}