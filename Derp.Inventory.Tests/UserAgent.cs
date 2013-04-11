using System;
using System.Linq;
using System.Text;
using Nancy.Helpers;
using Nancy.Testing;

namespace Derp.Inventory.Tests
{
    public abstract class UserAgent
    {
        protected readonly Action<BrowserContext> OnContext;
        protected readonly string Path;

        protected UserAgent(string path, Action<BrowserContext> onContext)
        {
            Path = path;
            OnContext = onContext;
        }

        public static UserAgent Post(string path, Action<BrowserContext> onContext)
        {
            return new PostRequest(path, onContext);
        }

        public static UserAgent Get(string path, Action<BrowserContext> onContext)
        {
            return new GetRequest(path, onContext);
        }

        public abstract BrowserResponse Execute(Browser browser);

        public override string ToString()
        {
            var context = new BrowserContext();
            OnContext(context);

            IBrowserContextValues values = context;

            var builder = new StringBuilder()
                .Append(GetMethod()).Append(" ").Append(Path);

            if (false == String.IsNullOrEmpty(values.QueryString))
            {
                builder.Append('?').Append(values.QueryString);
            }

            if (false == String.IsNullOrEmpty(values.FormValues))
            {
                var body = HttpUtility.ParseQueryString(values.FormValues);
                builder = body.AllKeys.Aggregate(
                    builder.AppendLine(), (sb, key) => builder.Append(key).Append(": ").Append(body[key]).AppendLine());
            }

            return builder.AppendLine().ToString();
        }

        private string GetMethod()
        {
            var name = GetType().Name;
            var method = name.Substring(0, name.Length - "Request".Length).ToUpper();
            return method;
        }

        #region Nested type: GetRequest

        private class GetRequest : UserAgent
        {
            public GetRequest(string path, Action<BrowserContext> onContext)
                : base(path, onContext)
            {
            }

            public override BrowserResponse Execute(Browser browser)
            {
                return browser.Get(Path, OnContext);
            }
        }

        #endregion

        #region Nested type: PostRequest

        private class PostRequest : UserAgent
        {
            public PostRequest(string path, Action<BrowserContext> onContext) : base(path, onContext)
            {
            }

            public override BrowserResponse Execute(Browser browser)
            {
                return browser.Post(Path, OnContext);
            }
        }

        #endregion
    }
}