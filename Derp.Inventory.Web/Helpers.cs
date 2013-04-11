using System;
using System.Linq.Expressions;

namespace Derp.Inventory.Web
{
    public static class Helpers
    {
        public static string GetDispatcherResource(this Type type)
        {
            return "/dispatcher/" + type.Name.Underscore().Dasherize();
        }

        public static string Named<TModel>(this TModel model, Expression<Func<TModel, string>> expression)
        {
            return (expression.Body as MemberExpression).Member.Name.Underscore().Pascalize();
        }

        public static string GetInputType(this Type type)
        {
            if (type == typeof (DateTime)) return "datetime";
            if (type == typeof (bool)) return "checkbox";
            return "text";
        }
    }
}