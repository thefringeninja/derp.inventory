using System;
using System.Collections.Generic;
using Derp.Inventory.Infrastructure;

namespace Derp.Inventory.Web.Services
{
    public class Bus : CommandSender, EventPublisher, Registration
    {
        private readonly Dictionary<Type, List<Action<object>>> handlersByType =
            new Dictionary<Type, List<Action<object>>>();

        #region CommandSender Members

        public void Send<T>(T command) where T : class
        {
            List<Action<object>> handlers;

            if (false == handlersByType.TryGetValue(typeof (T), out handlers))
            {
                throw new InvalidOperationException("no handler registered");
            }

            if (handlers.Count != 1)
            {
                throw new InvalidOperationException("cannot send to more than one handler");
            }

            handlers[0](command);
        }

        #endregion

        #region EventPublisher Members

        public void Publish<TEvent>(TEvent @event)
        {
            var eventType = @event.GetType();
            while (eventType != null)
            {
                DispatchByType(@event, eventType);
                eventType = @eventType.BaseType;
            }
        }

        #endregion

        #region Registration Members

        public void Register<T>(Handles<T> handler) where T : class
        {
            List<Action<object>> handlers;
            if (!handlersByType.TryGetValue(typeof (T), out handlers))
            {
                handlers = new List<Action<object>>();
                handlersByType.Add(typeof (T), handlers);
            }
            handlers.Add(DelegateAdjuster.CastArgument<object, T>(x => handler.Handle(x)));
        }

        #endregion

        private void DispatchByType(object @event, Type publishAs)
        {
            List<Action<object>> handlers;
            if (false == handlersByType.TryGetValue(publishAs, out handlers))
            {
                return;
            }
            handlers.ForEach(handle => handle(@event));
        }
    }
}