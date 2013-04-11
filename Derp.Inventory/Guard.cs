using System;

namespace Derp.Inventory
{
    public static class Guard
    {
        public static void Against(bool condition, params object[] args)
        {
            Against<InvalidOperationException>(condition, args);
        }

        public static void Against<TException>(bool condition, params object[] args)
            where TException : Exception
        {
            if (condition)
            {
                throw (TException) Activator.CreateInstance(typeof (TException), args);
            }
        }
    }
}