using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;

namespace Derp.Inventory.Web.Modules
{
    public class CommandModule<TCommand> : CommandModule<TCommand, TCommand> where TCommand : Command
    {
        public CommandModule(CommandSender bus)
            : base(bus)
        {
        }
    }
    public class CommandModule<TCommand, TCommandDto> : NancyModule where TCommand : Command
    {
        private readonly CommandSender bus;

        public CommandModule(
            CommandSender bus)
            : base(typeof (TCommand).GetDispatcherResource())
        {
            this.bus = bus;
            Get["/"] = p =>
            {
                var useCase = this.Bind<TCommandDto>();
                return Negotiate.WithModel(useCase);
            };
            Post["/"] = p =>
            {
                var commandDto = this.BindAndValidate<TCommandDto>();

                if (false == ModelValidationResult.IsValid)
                {
                    return 400;
                }

                TCommand command = (dynamic) commandDto;

                DispatchCommand(command);

                return OnHandled(command);
            };
        }

        protected virtual Negotiator OnHandled(TCommand command)
        {
            return Negotiate.WithStatusCode(200);
        }

        protected virtual void DispatchCommand(TCommand command)
        {
            bus.Send(command);
        }
    }
}