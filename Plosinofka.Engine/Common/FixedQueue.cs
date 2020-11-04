using System.Collections.Generic;

namespace Ujeby.Plosinofka.Engine.Common
{
    public class FixedQueue<T>
    {
        public Queue<T> Queue { get; private set; } = new Queue<T>();

        private readonly object LockObject = new object();

        public FixedQueue(int limit)
        {
            Limit = limit;
            Queue = new Queue<T>();
        }

        public int Limit { get; protected set; }

        public void Add(T item)
        {
            lock (LockObject)
            {
                if (Queue.Count == Limit)
                    Queue.Dequeue();

                Queue.Enqueue(item);
            }
        }
	}
}
