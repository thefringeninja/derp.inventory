using System.Collections.Generic;
using System.Linq;
using System.Text;
using Derp.Inventory.Tests.Fakes;
using Derp.Inventory.Web;
using Nancy.Testing;

namespace Derp.Inventory.Tests.Fixtures
{
    public class App
    {
        public App(BrowserResponse response, FakeCommandSender commandSender = null)
        {
            if (commandSender != null)
            {
                Dispatched = new List<object>(commandSender.SentCommands).AsReadOnly();
            }
            Response = response;
        }

        public IEnumerable<object> Dispatched { get; private set; }

        public BrowserResponse Response { get; private set; }

        public override string ToString()
        {
            var responseBuilder =
                new StringBuilder().Append("HTTP/1.1 ")
                                   .Append((int) Response.StatusCode)
                                   .Append(' ')
                                   .Append(Response.StatusCode.ToString().Titleize())
                                   .AppendLine();
            responseBuilder = Response.Headers.Aggregate(responseBuilder,
                                                         (builder, header) =>
                                                         builder.Append(header.Key)
                                                                .Append(": ")
                                                                .Append(header.Value)
                                                                .AppendLine());
            return responseBuilder.ToString();
        }
    }
}