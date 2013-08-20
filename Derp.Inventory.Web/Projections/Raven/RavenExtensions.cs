using EventStore.ClientAPI;
using Raven.Client;

namespace Derp.Inventory.Web.Projections.Raven
{
    public static class RavenExtensions
    {
        public static Position GetPositionFromRaven<T>(this IDocumentStore store)
        {
            using (var session = store.OpenSession())
            {
                return session.GetPositionFromRaven<T>();
            }
        }

        public static Position GetPositionFromRaven<T>(this IDocumentSession session)
        {
            var conventions = session.Advanced.DocumentStore.Conventions;
            var tag = conventions.GetTypeTagName(typeof (T));
            var id = conventions.FindFullDocumentKeyFromNonStringIdentifier(tag, typeof (PositionOfView), true);
            return session.Load<PositionOfView>(id);
        }


        public static void PersistPosition<T>(this IDocumentSession session, Position position)
        {
            var conventions = session.Advanced.DocumentStore.Conventions;

            var tag = conventions.GetTypeTagName(typeof (T));
            var id = conventions.FindFullDocumentKeyFromNonStringIdentifier(tag, typeof (PositionOfView), true);

            var positionOfView = session.Load<PositionOfView>(id) ?? new PositionOfView
            {
                Tag = tag,
                Id = id
            };

            positionOfView.CommitPosition = position.CommitPosition;
            positionOfView.PreparePosition = position.PreparePosition;
            session.Store(positionOfView);
        }
    }
}