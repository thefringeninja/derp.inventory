using System;

namespace Derp.Inventory
{
    public class Command : Message
    {
        public readonly Guid Id = Guid.NewGuid();
    }
}