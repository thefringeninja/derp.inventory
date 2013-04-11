using System;
using System.Linq;
using CsQuery;
using Derp.Inventory.Infrastructure;
using Derp.Inventory.Web;
using Nancy.Testing;

namespace Derp.Inventory.Tests.Specifications.Web
{
    public static class BodyExtensions
    {
        public static bool HasAListOfWarehouses(this BrowserResponseBodyWrapper body)
        {
            return body.ListOfWarehouses().Any();
        }

        public static QueryWrapper ListOfWarehouses(this BrowserResponseBodyWrapper body)
        {
            return body["ul.warehouses > li"];
        }

        public static bool HasAListOfTasks(this BrowserResponseBodyWrapper body)
        {
            return body.ListOfTasks().Count().Equals(1);
        }

        public static QueryWrapper ListOfTasks(this BrowserResponseBodyWrapper body)
        {
            return body["ul.tasks"];
        }

        public static bool Can(this QueryWrapper html, Type commandType)
        {
            return html["a[href='" + commandType.GetDispatcherResource() + "']"].Any();
        }

        public static string Text(this QueryWrapper html)
        {
            CQ document = html.AsDynamic().document;
            return document.Text();
        }
    }
}