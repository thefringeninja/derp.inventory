using System;
using Derp.Inventory.Web.GetEventStore;
using Derp.Inventory.Web.Messages;
using EventStore.ClientAPI;
using Raven.Client;

namespace Derp.Inventory.Web.ViewWriters
{
    public class CatchUpDocumentSessionObserver<TView> : IObserver<Action<IDocumentSession>>,
                                                         Handles<CaughtUp>,
                                                         IGetEventStorePositionRepository
    {
        private readonly IDocumentStore documentStore;
        private IObserver<Action<IDocumentSession>> inner;
        private IGetEventStorePositionRepository positions;

        public CatchUpDocumentSessionObserver(IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
            var observer = new BatchingDocumentSessionObserver<TView>(documentStore);
            positions = observer;
            inner = observer;
        }

        #region Handles<CaughtUp> Members

        public void Handle(CaughtUp message)
        {
            inner.OnCompleted();

            var observer = new ImmediateDocumentSessionObserver<TView>(documentStore);
            positions = observer;
            inner = observer;
        }

        #endregion

        #region IGetEventStorePositionRepository Members

        public Position? GetLastProcessedPosition()
        {
            return positions.GetLastProcessedPosition();
        }

        #endregion

        #region IObserver<Action<IDocumentSession>> Members

        public void OnNext(Action<IDocumentSession> value)
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
    }
}