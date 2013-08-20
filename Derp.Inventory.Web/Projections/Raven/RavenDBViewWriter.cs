using System;
using EventStore.ClientAPI;
using Raven.Client;

namespace Derp.Inventory.Web.Projections.Raven
{
    /// Big ups to Yves Reynhout for the idea of using an observer to control how we flush to underlying storage 
    public class RavenDbViewWriter<TKey, TView> : IWriteViews<TKey, TView>
        where TView : class

    {
        private readonly IObserver<RavenOperation> raven;

        public RavenDbViewWriter(IObserver<RavenOperation> raven)
        {
            this.raven = raven;
        }

        public void AddOrUpdate(TKey key, Position position, Func<TView> add, Action<TView> update = null)
        {
            raven.OnNext(
                new RavenOperation(
                    session =>
                    {
                        var documentKey = GetDocumentKey(key, session);

                        var document = session.Load<TView>(documentKey) ?? AddInternal(documentKey, add, session);
                        if (update != null)
                        {
                            update(document);
                        }
                    }, position));
        }

        public void TryUpdate(TKey key, Position position, Action<TView> update)
        {
            raven.OnNext(
                new RavenOperation(
                    session =>
                    {
                        var documentKey = GetDocumentKey(key, session);
                        var document = session.Load<TView>(documentKey);
                        if (document != null)
                        {
                            update(document);
                        }
                    }, position));
        }

        public void TryDelete(TKey key, Position position)
        {
            raven.OnNext(
                new RavenOperation(
                    session =>
                    {
                        var documentKey = GetDocumentKey(key, session);
                        var document = session.Load<TView>(documentKey);
                        if (document != null)
                        {
                            session.Delete(document);
                        }
                    }, position));
        }

        private static string GetDocumentKey(TKey key, IDocumentSession session)
        {
            var documentKey = key as string
                              ?? session.Advanced.DocumentStore.Conventions
                                        .FindFullDocumentKeyFromNonStringIdentifier(
                                            key, typeof (TView), true);
            return documentKey;
        }

        private static TView AddInternal(string documentKey, Func<TView> add, IDocumentSession session)
        {
            var view = add();
            session.Store(view, documentKey);
            return view;
        }
    }
}
