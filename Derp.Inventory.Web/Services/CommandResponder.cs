using Nancy;

namespace Derp.Inventory.Web.Services
{
    public interface CommandResponder<in TCommand> where TCommand : class
    {
        Response OnHandled(NancyContext context, TCommand command);
    }
}