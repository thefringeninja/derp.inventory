using System;
using Derp.Inventory.Web.Infrastructure.GetEventStore;
using EventStore.ClientAPI;
using Raven.Client;

namespace Derp.Inventory.Web.Projections.Raven
{
    public class CatchUpDocumentSessionObserver<TView> : IObserver<RavenOperation>,
                                                         IGetEventStorePositionRepository
    {
        private readonly IDocumentStore documentStore;
        private IObserver<RavenOperation> inner;
        private IGetEventStorePositionRepository positions;

        public CatchUpDocumentSessionObserver(IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
            var observer = new BatchingDocumentSessionObserver<TView>(documentStore);
            positions = observer;
            inner = observer;
        }

        #region IGetEventStorePositionRepository Members

        public Position GetLastProcessedPosition()
        {
            return positions.GetLastProcessedPosition();
        }

        #endregion

        #region IObserver<RavenOperation> Members

        public void OnNext(RavenOperation value)
        {
            inner.OnNext(value);
        }

        public void OnError(Exception error)
        {
            inner.OnError(error);
        }

        public void OnCompleted()
        {
            inner.OnCompleted();
        }

        #endregion

        public void CaughtUp()
        {
            inner.OnCompleted();

            var observer = new ImmediateDocumentSessionObserver<TView>(documentStore);
            positions = observer;
            inner = observer;
        }
    }
}