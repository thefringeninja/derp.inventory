using System;
using EventStore.ClientAPI;
using Raven.Client;

namespace Derp.Inventory.Web.Projections.Raven
{
    public class RavenOperation
    {
        public readonly Action<IDocumentSession> Execute;
        public readonly Position Position;

        public RavenOperation(Action<IDocumentSession> execute, Position position)
        {
            Execute = execute;
            Position = position;
        }
    }
}