using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Derp.Inventory.Web.Modules;
using Derp.Inventory.Web.Services;
using EventStore.ClientAPI;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Session;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace Derp.Inventory.Web.Bootstrap
{
    public partial class Bootstrapper : DefaultNancyBootstrapper
    {
        private readonly Bus bus;
        private readonly List<ModuleRegistration> commandModules = new List<ModuleRegistration>();

        private readonly Lazy<IDocumentStore> documentStoreLazy = new Lazy<IDocumentStore>(() =>
        {
            var documentStore = new DocumentStore();
            documentStore.ParseConnectionString("Url=http://localhost:8080/;Database=derp");
            documentStore.Initialize();

            IndexCreation.CreateIndexes(typeof(Bootstrapper).Assembly, documentStore);

            return documentStore;
        });

        private readonly Lazy<EventStoreConnection> eventStoreConnectionLazy = new Lazy<EventStoreConnection>(() =>
        {
            var settings = ConnectionSettings.Create()
                                             .KeepReconnecting()
                                             .KeepRetrying();

            var connection = EventStoreConnection.Create(settings);
            connection.Connect(new IPEndPoint(IPAddress.Loopback, 1113));
            return connection;
        });

        private readonly JsonSerializerSettings serializerSettings;

        public Bootstrapper()
        {
            bus = new Bus();

            serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None
            };
        }

        private EventStoreConnection EventStoreConnection
        {
            get { return eventStoreConnectionLazy.Value; }
        }

        private IDocumentStore DocumentStore
        {
            get { return documentStoreLazy.Value; }
        }

        // probably don't need these as we have to have them both on startup.

        protected override IEnumerable<ModuleRegistration> Modules
        {
            get
            {
                return base.Modules.Union(commandModules)
                           .Where(r => false == r.ModuleType.IsGenericTypeDefinition);
            }
        }

        private void Register<TCommand>(
            Handles<TCommand> handler,
            Func<NancyContext, TCommand, Response> onHandled = null)
            where TCommand : class
        {
            Register<TCommand, CommandModule<TCommand>>(handler, onHandled);
        }

        private void Register<TCommand, TCommandModule>(
            Handles<TCommand> handler,
            Func<NancyContext, TCommand, Response> onHandled = null)
            where TCommand : class
            where TCommandModule : CommandModule<TCommand>
        {
            onHandled = onHandled ?? ((_, __) => 200);
            bus.Register(handler);

            var moduleType = typeof (TCommandModule);
            commandModules.Add(
                new ModuleRegistration(
                    moduleType,
                    GetModuleKeyGenerator().GetKeyForModuleType(moduleType)));

            ApplicationContainer.Register<CommandResponder<TCommand>>(
                new DelegateCommandResponder<TCommand>(onHandled));
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);

            nancyConventions.ViewLocationConventions.Insert(
                0, (viewName, model, context) => "views" + context.ModulePath);
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            container.Register<CommandSender>(bus);
            container.Register<EventPublisher>(bus);

            base.ApplicationStartup(container, pipelines);

            RegisterInventoryHandlers();
            RegisterProjections(container);

            CookieBasedSessions.Enable(pipelines);
        }

        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);

            container.Register((_, n) => DocumentStore.OpenSession());
        }
    }
}