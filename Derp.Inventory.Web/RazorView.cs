using Nancy.Extensions;
using Nancy.ViewEngines;
using Nancy.ViewEngines.Razor;

namespace Derp.Inventory.Web
{
    public abstract class RazorView<TModel>
        : NancyRazorViewBase<TModel> where TModel : class
    {
        public FormHelper<TModel> Form { get; private set; }

        public override void Initialize(RazorViewEngine engine, IRenderContext renderContext, object model)
        {
            base.Initialize(engine, renderContext, model);

            Layout = renderContext.Context.Request.IsAjaxRequest()
                         ? "shared/_ajaxlayout.cshtml"
                         : "shared/_layout.cshtml";

            Form = new FormHelper<TModel>(Html);
        }
    }
}