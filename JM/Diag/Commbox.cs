using System;

namespace JM.Diag
{
    public abstract class Commbox<T> where T : Stream
    {
        private T stream;

        public Commbox(T stream)
        {
            this.stream = stream;
        }

        public T Stream
        {
            get { return stream; }
        }
    }
}