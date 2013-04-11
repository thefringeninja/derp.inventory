using System;
using System.Linq;
using System.Text;

namespace Derp.Inventory.Web.GetEventStore
{
    internal static class TypeUtilities
    {
        public static string ToPartiallyQualifiedName(this Type type)
        {
            if (type.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Open generic types are not allowed.", "type");
            }
            var sb = new StringBuilder();
            sb.Append(type.FullName);
            if (type.IsGenericType)
            {
                sb.Append("[");

                sb.Append(String.Join(", ",
                                      type.GetGenericArguments()
                                          .Select(g => "[" + ToPartiallyQualifiedName(g) + "]")
                                          .ToArray()));

                sb.Append("]");
            }
            sb.Append(", ").Append(type.Assembly.GetName().Name);
            return sb.ToString();
        }
    }
}