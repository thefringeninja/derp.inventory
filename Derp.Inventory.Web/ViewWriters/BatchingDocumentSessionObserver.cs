using System;
using Derp.Inventory.Web.GetEventStore;
using EventStore.ClientAPI;
using Raven.Client;

namespace Derp.Inventory.Web.ViewWriters
{
    public class BatchingDocumentSessionObserver<TView> : IObserver<Action<IDocumentSession>>,
                                                          IGetEventStorePositionRepository
    {
        private readonly IDocumentStore documentStore;
        private IDocumentSession session;

        public BatchingDocumentSessionObserver(IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
            session = OpenDocumentSession(documentStore);
        }

        #region IGetEventStorePositionRepository Members

        public Position? GetLastProcessedPosition()
        {
            return session.GetPositionFromRaven<TView>();
        }

        #endregion

        #region IObserver<Action<IDocumentSession>> Members

        public void OnNext(Action<IDocumentSession> value)
        {
            value(session);
            FlushIfNecessary();
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
            Flush();
            session.Dispose();
            session = null;
        }

        #endregion

        private static IDocumentSession OpenDocumentSession(IDocumentStore documentStore)
        {
            var session = documentStore.OpenSession();
            session.Advanced.MaxNumberOfRequestsPerSession = 1024;
            return session;
        }

        private void FlushIfNecessary()
        {
            if (session.Advanced.NumberOfRequests < 1000) return;
            Flush();
        }

        private void Flush()
        {
            session.SaveChanges();
            session.Dispose();
            session = OpenDocumentSession(documentStore);
        }
    }
}