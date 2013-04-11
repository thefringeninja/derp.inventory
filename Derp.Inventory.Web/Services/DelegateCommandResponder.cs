using System;
using Nancy;

namespace Derp.Inventory.Web.Services
{
    public class DelegateCommandResponder<TCommand> : CommandResponder<TCommand> where TCommand : class
    {
        private readonly Func<NancyContext, TCommand, Response> onHandled;

        public DelegateCommandResponder(Func<NancyContext, TCommand, Response> onHandled)
        {
            this.onHandled = onHandled;
        }

        #region CommandResponder<TCommand> Members

        public Response OnHandled(NancyContext context, TCommand command)
        {
            return onHandled(context, command);
        }

        #endregion
    }
}