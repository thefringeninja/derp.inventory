using System;
using Derp.Inventory.Web.Infrastructure.GetEventStore;
using EventStore.ClientAPI;
using Raven.Client;

namespace Derp.Inventory.Web.Projections.Raven
{
    public class ImmediateDocumentSessionObserver<TView> : IObserver<RavenOperation>,
                                                           IGetEventStorePositionRepository
    {
        private readonly IDocumentStore documentStore;

        public ImmediateDocumentSessionObserver(IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
        }

        #region IGetEventStorePositionRepository Members

        public Position GetLastProcessedPosition()
        {
            return documentStore.GetPositionFromRaven<TView>();
        }

        #endregion

        #region IObserver<RavenOperation> Members

        public void OnNext(RavenOperation operation)
        {
            using (var session = documentStore.OpenSession())
            {
                operation.Execute(session);
                session.PersistPosition<TView>(operation.Position);
                session.SaveChanges();
            }
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        #endregion
    }
}