using System.Collections.Generic;
using Derp.Inventory.Web.Services;
using Nancy;
using Nancy.ModelBinding;

namespace Derp.Inventory.Web.Modules
{
    public class CommandModule<TCommand> : NancyModule where TCommand : class
    {
        private readonly CommandSender bus;

        public CommandModule(
            CommandSender bus,
            CommandResponder<TCommand> responder,
            Parameters parameters)
            : base(typeof (TCommand).GetDispatcherResource())
        {
            this.bus = bus;
            var binder = new CommandBinder<TCommand>(parameters.TypeConverters, parameters.BodyDeserializers,
                                                     parameters.FieldNameConverter, new BindingDefaults());

            Get["/"] = p =>
            {
                var useCase = GetUseCaseFromQueryString(parameters.ContextFactory, binder);
                return Negotiate.WithModel(useCase);
            };
            Post["/"] = p =>
            {
                var command = (TCommand) binder.Bind(Context, typeof (TCommand), null, BindingConfig.Default);

                DispatchCommand(command);

                return responder.OnHandled(Context, command);
            };
        }

        protected virtual void DispatchCommand(TCommand command)
        {
            bus.Send(command);
        }

        private TCommand GetUseCaseFromQueryString(INancyContextFactory contextFactory, IBinder binder)
        {
            var context = contextFactory.Create(Request);

            foreach (var key in context.Request.Query)
            {
                context.Request.Form.Add(key, context.Request.Query[key]);
            }

            var useCase = (TCommand) binder.Bind(context, typeof (TCommand), null, BindingConfig.Default);

            return useCase;
        }

        #region Nested type: Parameters

        public class Parameters
        {
            private readonly IEnumerable<IBodyDeserializer> bodyDeserializers;
            private readonly INancyContextFactory contextFactory;
            private readonly IFieldNameConverter fieldNameConverter;
            private readonly IEnumerable<ITypeConverter> typeConverters;

            public Parameters(INancyContextFactory contextFactory, IEnumerable<ITypeConverter> typeConverters,
                              IEnumerable<IBodyDeserializer> bodyDeserializers, IFieldNameConverter fieldNameConverter)
            {
                this.contextFactory = contextFactory;
                this.typeConverters = typeConverters;
                this.bodyDeserializers = bodyDeserializers;
                this.fieldNameConverter = fieldNameConverter;
            }

            public INancyContextFactory ContextFactory
            {
                get { return contextFactory; }
            }

            public IEnumerable<ITypeConverter> TypeConverters
            {
                get { return typeConverters; }
            }

            public IEnumerable<IBodyDeserializer> BodyDeserializers
            {
                get { return bodyDeserializers; }
            }

            public IFieldNameConverter FieldNameConverter
            {
                get { return fieldNameConverter; }
            }
        }

        #endregion
    }
}