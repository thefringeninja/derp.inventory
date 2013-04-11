using System;
using System.Collections.Generic;

namespace Derp.Inventory.Tests.Fakes
{
    public class FakeCommandSender : CommandSender
    {
        private readonly List<object> sentCommands = new List<object>();

        public IEnumerable<object> SentCommands
        {
            get { return sentCommands; }
        }

        #region CommandSender Members

        public void Send<T>(T command) where T : class
        {
            sentCommands.Add(command);
        }

        #endregion

        public void Register<T>(Action<T> handler) where T : class
        {
        }

        public void Publish<T>(T @event) where T : class
        {
        }
    }
}