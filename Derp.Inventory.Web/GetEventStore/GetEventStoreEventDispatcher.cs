using System;
using System.Collections.Generic;
using Derp.Inventory.Web.Messages;
using Derp.Inventory.Web.Services;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Derp.Inventory.Web.GetEventStore
{
    public class GetEventStoreEventDispatcher
    {
        // Credit algorithm to Szymon Pobiega
        // http://simon-says-architecture.com/2013/02/02/mechanics-of-durable-subscription/#comments
        // 1. The subscriber always starts with pull assuming there were some messages generated while it was offline
        // 2. The subscriber pulls messages until there’s nothing left to pull (it is up to date with the stream)
        // 3. Push subscription is started  but arriving messages are not processed immediately but temporarily redirected to a buffer
        // 4. One last pull is done to ensure nothing happened between step 2 and 3
        // 5. Messages from this last pull are processed
        // 6. Processing messages from push buffer is started. While messages are processed, they are checked against IDs of messages processed in step 5 to ensure there’s no duplicates.
        // 7. System works in push model until subscriber is killed or subscription is dropped by publisher drops push subscription.

        //Credit to Andrii Nakryiko
        //If data is written to storage at such a speed, that between the moment you did your last 
        //pull read and the moment you subscribed to push notifications more data (events) were 
        //generated, than you request in one pull request, you would need to repeat steps 4-5 few 
        //times until you get a pull message which position is >= subscription position 
        //(EventStore provides you with those positions).

        private readonly EventStoreConnection connection;

        private readonly IGetEventStorePositionRepository positions;
        private readonly EventPublisher publisher;
        private readonly JsonSerializerSettings serializerSettings;

        private readonly IDictionary<Type, List<Action<object, Position?>>> subscriptions =
            new Dictionary<Type, List<Action<object, Position?>>>();

        private bool stopRequested;
        private EventStoreAllCatchUpSubscription subscription;

        public GetEventStoreEventDispatcher(EventStoreConnection connection,
                                            JsonSerializerSettings serializerSettings,
                                            IGetEventStorePositionRepository positions,
                                            EventPublisher publisher)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            this.connection = connection;
            this.serializerSettings = serializerSettings;
            this.positions = positions;
            this.publisher = publisher;
        }


        public void StartDispatching()
        {
            stopRequested = false;
            RecoverSubscription();
        }

        public void StopDispatching()
        {
            stopRequested = true;
            if (subscription != null)
                subscription.Stop(TimeSpan.FromSeconds(2));
        }

        private void RecoverSubscription()
        {
            var livePosition = connection.ReadAllEventsBackward(Position.End, 1, true).FromPosition;
            var catchingUp = true;
            var lastProcessedPosition = positions.GetLastProcessedPosition();
            subscription = connection.SubscribeToAllFrom(
                lastProcessedPosition, false,
                (_, resolvedEvent) =>
                {
                    List<Action<object, Position?>> subscribers;

                    if (!resolvedEvent.OriginalPosition.HasValue)
                        throw new ArgumentException(
                            "ResolvedEvent didn't come off a subscription to all (has no position).");

                    var @event = ProcessRawEvent(resolvedEvent);

                    if (@event != null && subscriptions.TryGetValue(@event.GetType(), out subscribers))
                    {
                        subscribers.ForEach(handler => handler(@event, resolvedEvent.OriginalPosition));
                    }

                    if (false == catchingUp || resolvedEvent.OriginalPosition.Value < livePosition) return;

                    catchingUp = false;
                    publisher.Publish(new CaughtUp());
                },
                (_, reason, error) =>
                {
                    if (stopRequested)
                        return;

                    RecoverSubscription();
                });
        }

        private Event ProcessRawEvent(ResolvedEvent rawEvent)
        {
            var @event = rawEvent.OriginalEvent.DeserializeEventAsync(
                serializerSettings).Result as Event;

            return @event;
        }

        public void Subscribe<TEvent>(Action<TEvent, Position?> handler)
        {
            List<Action<object, Position?>> subscribers;
            if (false == subscriptions.TryGetValue(typeof (TEvent), out subscribers))
            {
                subscribers = subscriptions[typeof (TEvent)] = new List<Action<object, Position?>>();
            }

            subscribers.Add((message, position) => handler((TEvent) message, position));
        }
    }
}