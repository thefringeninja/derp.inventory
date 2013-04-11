using System;
using EventStore.ClientAPI;
using Raven.Client;

namespace Derp.Inventory.Web.ViewWriters
{
    /// Big ups to Yves Reynhout for the idea of using an observer to control how we flush to underlying storage 
    public class RavenDbViewWriter<TKey, TView> : IWriteViews<TKey, TView>
        where TView : class

    {
        private readonly IObserver<Action<IDocumentSession>> raven;

        public RavenDbViewWriter(IObserver<Action<IDocumentSession>> raven)
        {
            this.raven = raven;
        }

        #region IWriteViews<TKey,TView> Members

        public void AddOrUpdate(TKey key, Position? position, Func<TView> add, Action<TView> update = null)
        {
            raven.OnNext(session =>
            {
                var documentKey = GetDocumentKey(key, session);

                var document = session.Load<TView>(documentKey) ?? AddInternal(documentKey, add, session);
                if (update != null)
                {
                    update(document);
                }
                session.PersistPosition<TView>(position ?? Position.Start);
            });
        }

        public void TryUpdate(TKey key, Position? position, Action<TView> update)
        {
            raven.OnNext(session =>
            {
                var documentKey = GetDocumentKey(key, session);
                var document = session.Load<TView>(documentKey);
                if (document != null)
                {
                    update(document);
                }
                session.PersistPosition<TView>(position ?? Position.Start);
            });
        }

        public void TryDelete(TKey key, Position? position)
        {
            raven.OnNext(session =>
            {
                var documentKey = GetDocumentKey(key, session);
                var document = session.Load<TView>(documentKey);
                if (document != null)
                {
                    session.Delete(document);
                }
                session.PersistPosition<TView>(position ?? Position.Start);
            });
        }

        #endregion

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