using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Derp.Inventory.Tests.Fakes;
using Derp.Inventory.Tests.Fixtures;
using Derp.Inventory.Web.Modules;
using Nancy.Testing;
using Simple.Testing.ClientFramework;

namespace Derp.Inventory.Tests.Templates
{
    public class CommandModuleSpecification<TCommand> :
        TypedSpecification<App> where TCommand : class
    {
        private readonly FakeCommandSender commandSender;
        public Action Before;
        public Action<ConfigurableBootstrapper.ConfigurableBootstrapperConfigurator> Bootstrap = with => { };
        public List<Expression<Func<App, bool>>> Expect = new List<Expression<Func<App, bool>>>();
        public Action Finally;
        public string Name;


        public Action<BrowserContext> OnContext = context => { };

        public Func<Action<BrowserContext>, UserAgent> When;

        public CommandModuleSpecification()
        {
            commandSender = new FakeCommandSender();
        }

        #region TypedSpecification<App> Members

        public string GetName()
        {
            return Name;
        }

        public Action GetBefore()
        {
            return Before;
        }

        public Delegate GetOn()
        {
            return new Func<UserAgent>(() => When(OnContext));
        }

        public Delegate GetWhen()
        {
            return new Func<UserAgent, App>(
                userAgent => new App(
                                 userAgent.Execute(
                                     new Browser(with => Bootstrap(
                                         with.Module<CommandModule<TCommand>>()
                                             .Dependency<CommandSender>(commandSender)))), commandSender));
        }

        public IEnumerable<Expression<Func<App, bool>>> GetAssertions()
        {
            return Expect;
        }

        public Action GetFinally()
        {
            return Finally;
        }

        #endregion
    }
}