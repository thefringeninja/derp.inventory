using System;
using System.Collections.Generic;
using Derp.Inventory.Infrastructure;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Derp.Inventory.Web.Infrastructure.GetEventStore
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

        private readonly Action caughtUp;
        private readonly IEventStoreConnection connection;

        private readonly IGetEventStorePositionRepository positions;
        private readonly JsonSerializerSettings serializerSettings;

        private readonly IDictionary<Type, List<Action<object>>> subscriptions =
            new Dictionary<Type, List<Action<object>>>();

        private EventStoreAllCatchUpSubscription subscription;

        public GetEventStoreEventDispatcher(
            IEventStoreConnection connection,
            JsonSerializerSettings serializerSettings,
            IGetEventStorePositionRepository positions,
            Action caughtUp)
        {
            if (connection == null)
                throw new ArgumentNullException("connection");

            this.connection = connection;
            this.serializerSettings = serializerSettings;
            this.positions = positions;
            this.caughtUp = caughtUp;
        }


        public void StartDispatching()
        {
            RecoverSubscription();
        }

        public void StopDispatching()
        {
            if (subscription != null)
                subscription.Stop(TimeSpan.FromSeconds(2));
        }

        private void EventAppeared(EventStoreCatchUpSubscription _, ResolvedEvent resolvedEvent)
        {
            List<Action<object>> subscribers;

            if (!resolvedEvent.OriginalPosition.HasValue)
                throw new ArgumentException(
                    "ResolvedEvent didn't come off a subscription to all (has no position).");

            if (resolvedEvent.OriginalEvent.EventType.StartsWith("$")
                || resolvedEvent.OriginalStreamId.StartsWith("$"))
                return;

            var @event = ProcessRawEvent(resolvedEvent);

            if (@event == null || !subscriptions.TryGetValue(@event.GetType(), out subscribers))
                return;

            var message = CreateEventStoreMessage(@event, resolvedEvent.OriginalPosition.Value);
            subscribers.ForEach(handler => handler(message));
        }

        private static object CreateEventStoreMessage(Event @event, Position position)
        {
            return Activator.CreateInstance(
                typeof (GetEventStoreMessage<>).MakeGenericType(@event.GetType()), @event, position);
        }

        private void RecoverSubscription()
        {
            var lastProcessedPosition = positions.GetLastProcessedPosition();
            subscription = connection.SubscribeToAllFrom(
                lastProcessedPosition, false,
                EventAppeared,
                LiveProcessingStarted,
                SubscriptionDropped);
        }

        private void SubscriptionDropped(
            EventStoreCatchUpSubscription _, SubscriptionDropReason reason, Exception exception)
        {
            if (reason == SubscriptionDropReason.UserInitiated)
                return;

            RecoverSubscription();
        }

        private void LiveProcessingStarted(EventStoreCatchUpSubscription _)
        {
            caughtUp();
        }

        private Event ProcessRawEvent(ResolvedEvent rawEvent)
        {
            var @event = rawEvent.OriginalEvent.DeserializeEventAsync(
                serializerSettings).Result as Event;

            return @event;
        }

        public void Subscribe<TEvent>(Action<GetEventStoreMessage<TEvent>> handler) where TEvent : Event
        {
            List<Action<object>> subscribers;
            if (false == subscriptions.TryGetValue(typeof (TEvent), out subscribers))
            {
                subscribers = subscriptions[typeof (TEvent)] = new List<Action<object>>();
            }

            subscribers.Add(
                DelegateAdjuster.CastArgument<object, GetEventStoreMessage<TEvent>>(message => handler(message)));
        }
    }
}