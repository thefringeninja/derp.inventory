using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Derp.Inventory.Web.Modules;
using Derp.Inventory.Web.Services;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
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

        private readonly Lazy<IEventStoreConnection> EventStoreLazy = new Lazy<IEventStoreConnection>(() =>
        {
            var connection = EventStoreConnection.Create(
                ConnectionSettings.Create().SetDefaultUserCredentials(
                    new UserCredentials("admin", "changeit")), new IPEndPoint(IPAddress.Loopback, 1113));
            connection.Connect();
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

        private IEventStoreConnection EventStore
        {
            get { return EventStoreLazy.Value; }
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
            Handles<TCommand> handler)
            where TCommand : Command
        {
            Register<TCommand, CommandModule<TCommand>>(handler);
        }

        private void Register<TCommand, TCommandModule>(
            Handles<TCommand> handler)
            where TCommand : Command where TCommandModule : CommandModule<TCommand>
        {
            bus.Register(handler);

            var moduleType = typeof(TCommandModule);
            commandModules.Add(new ModuleRegistration(moduleType));
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);

            nancyConventions.ViewLocationConventions.Add(
                (viewName, model, context) => "views" + context.ModulePath);
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