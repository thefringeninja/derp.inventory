using System.Collections.Concurrent;

namespace Derp.Inventory.Infrastructure
{
    public class FixedSizedQueue<T> : ConcurrentQueue<T>
    {
        private readonly int size;

        public FixedSizedQueue(int size)
        {
            this.size = size;
        }

        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);

            if (Count < size) return;

            lock (this)
            {
                while (Count > size)
                {
                    T outObj;
                    TryDequeue(out outObj);
                }
            }
        }
    }
}