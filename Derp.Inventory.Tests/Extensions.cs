using System;
using System.Collections.Generic;
using System.Linq;

namespace Derp.Inventory.Tests
{
    public static class Extensions
    {
        public static IEnumerable<T> OfType<T>(this IEnumerable<object> source, Type type)
        {
            if (type != typeof (T)) throw new ArgumentException("type");
            return source.OfType<T>();
        }
    }
}