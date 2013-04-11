using Derp.Inventory.Tests.Templates;
using Derp.Inventory.Web.Modules;
using Nancy;
using Simple.Testing.ClientFramework;

namespace Derp.Inventory.Tests.Specifications.Web
{
    public class DefaultModuleSpecifications
    {
        public Specification when_i_am_on_the_home_page()
        {
            return new ModuleSpecification<DefaultModule>
            {
                OnContext = context => context.Header("Accept", "text/html"),
                When = onContext => UserAgent.Get("/", onContext),
                Expect =
                {
                    app => app.Response.StatusCode.Equals(HttpStatusCode.OK),
                    app => app.Response.Body.HasAListOfWarehouses(),
                    app => app.Response.Body.ListOfWarehouses()
                              .Text().Contains("Lilburn")
                }
            };
        }
    }
}