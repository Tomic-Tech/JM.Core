using System;
using System.Threading;

namespace JM.Diag
{
    public class VirtualStream
    {
        private Stream toCommbox;
        private Stream fromCommbox;
        private ManualResetEvent mre;
        private TimeSpan readTimeout;

        public VirtualStream(Stream toCommbox, Stream fromCommbox, ManualResetEvent mre)
        {
            this.toCommbox = toCommbox;
            this.fromCommbox = fromCommbox;
            this.mre = mre;
            readTimeout = TimeSpan.FromMilliseconds(500);
        }

        public TimeSpan ReadTimeout
        {
            get
            {
                return readTimeout;
            }
            set
            {
                readTimeout = value;
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (count <= fromCommbox.BytesToRead)
            {
                return ReadImmediately(buffer, offset, count);
            }

            if (readTimeout.TotalMilliseconds <= 0)
            {
                return ReadWithoutTimeout(buffer, offset, count);
            }

            return ReadWithTimeout(buffer, offset, count);
        }

        public int Write(byte[] buffer, int offset, int count)
        {
            return toCommbox.Write(buffer, offset, count);
        }

        private int ReadImmediately(byte[] buffer, int offset, int count)
        {
            return fromCommbox.Read(buffer, offset, count);
        }

        private int ReadWithoutTimeout(byte[] buffer, int offset, int count)
        {
            while (fromCommbox.BytesToRead < count)
            {
                mre.WaitOne();
            }

            return ReadImmediately(buffer, offset, count);
        }

        private int ReadWithTimeout(byte[] buffer, int offset, int count)
        {
            int len = 0;
            while (mre.WaitOne(readTimeout))
            {
                int size = fromCommbox.BytesToRead;

                if (size >= count)
                {
                    fromCommbox.Read(buffer, offset + len, count);
                    len += count;
                    break;
                }

                fromCommbox.Read(buffer, len + offset, size);
                len += size;
                count -= size;
            }
            return len;
        }

        public int BytesToRead
        {
            get
            {
                return fromCommbox.BytesToRead;
            }
        }

        public void DiscardOutBuffer()
        {
            toCommbox.Clear();
        }

        public void DiscardInBuffer()
        {
            fromCommbox.Clear();
        }
    }
}