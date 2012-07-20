using System;

namespace JM.Diag
{
    public abstract class Stream
    {
        public abstract int BytesToRead
        {
            get;
        }

        public abstract Core.Timer ReadTimeout
        {
            get;
            set;
        }

        public abstract bool Clear();

        public abstract int Read(byte[] buffer, int offset, int count);

        public abstract int Write(byte[] buffer, int offset, int count);
    }
}