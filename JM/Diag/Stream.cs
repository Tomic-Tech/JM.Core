using System;
using System.Collections.Generic;

namespace JM.Diag
{
    public class Stream
    {
        private object lockForQueue;
        private Queue<byte> queue;

        public Stream()
        {
            lockForQueue = new object();
            queue = new Queue<byte>();
        }

        public int BytesToRead
        {
            get
            {
                lock (lockForQueue)
                {
                    return queue.Count;
                }
            }
        }

        public void Clear()
        {
            lock (lockForQueue)
            {
                queue.Clear();
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            lock (lockForQueue)
            {
                if (queue.Count == 0)
                {
                    return 0;
                }

                int realCount = count > queue.Count ? queue.Count : count;

                for (int i = 0; i < realCount; i++)
                {
                    buffer[i + offset] = queue.Dequeue();
                }
                return realCount;
            }
        }

        public int Write(byte[] data, int offset, int count)
        {
            lock (lockForQueue)
            {
                for (int i = 0; i < count; i++)
                {
                    queue.Enqueue(data[offset + i]);
                }
            }
            return count;
        }
    }
}