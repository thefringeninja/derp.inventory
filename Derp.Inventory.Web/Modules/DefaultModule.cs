using Derp.Inventory.Web.ViewModels;
using Nancy;

namespace Derp.Inventory.Web.Modules
{
    public class DefaultModule : NancyModule
    {
        public DefaultModule()
        {
            Get["/"] = _ => Negotiate
                                .WithModel(WarehouseListViewModel.Instance)
                                .WithView("home");
        }
    }
}