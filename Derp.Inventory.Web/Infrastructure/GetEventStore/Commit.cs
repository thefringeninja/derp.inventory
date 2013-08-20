using System.Collections.Generic;
using EventStore.ClientAPI;

namespace Derp.Inventory.Web.Infrastructure.GetEventStore
{
    public class Commit
    {
        private readonly IList<EventData> commit;
        private readonly string streamId;
        private readonly int version;

        public Commit(string streamId, int version, IList<EventData> commit)
        {
            this.streamId = streamId;
            this.version = version;
            this.commit = commit;
        }

        public string StreamId
        {
            get { return streamId; }
        }

        public int Version
        {
            get { return version; }
        }

        public IList<EventData> Events
        {
            get { return commit; }
        }
    }
}