using System;
using Derp.Inventory.Web.Infrastructure.GetEventStore;
using EventStore.ClientAPI;
using Raven.Client;

namespace Derp.Inventory.Web.Projections.Raven
{
    public class BatchingDocumentSessionObserver<TView> : IObserver<RavenOperation>,
                                                          IGetEventStorePositionRepository
    {
        private readonly IDocumentStore documentStore;
        private IDocumentSession session;
        private Position lastProcessedPosition;

        public BatchingDocumentSessionObserver(IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
            session = OpenDocumentSession(documentStore);
        }

        #region IGetEventStorePositionRepository Members

        public Position GetLastProcessedPosition()
        {
            return lastProcessedPosition;
        }

        #endregion

        #region IObserver<RavenOperation> Members

        public void OnNext(RavenOperation operation)
        {
            operation.Execute(session);
            lastProcessedPosition = operation.Position;
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
            session.PersistPosition<TView>(lastProcessedPosition);
            session.SaveChanges();
            session.Dispose();
            session = OpenDocumentSession(documentStore);
        }
    }
}