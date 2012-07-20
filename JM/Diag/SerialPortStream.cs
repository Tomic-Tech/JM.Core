using System;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;

namespace JM.Diag
{
    internal class SerialPortStream : Stream
    {
        private SerialPort serialPort;
        private Core.Timer readTimeout;

        public SerialPortStream(SerialPort serialPort)
        {
            this.serialPort = serialPort;
            this.readTimeout = new Core.Timer(-1);
        }

        public SerialPort SerialPort
        {
            get { return serialPort; }
        }

        public override bool Clear()
        {
            try
            {
                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private int ReadImmediately(byte[] buffer, int offset, int count)
        {
            try
            {
                int ret = serialPort.Read(buffer, offset, count);
                Core.Log.Write("Recv", buffer, offset, count);
                return ret;
            }
            catch
            {
                return 0;
            }
        }

        private int ReadWithoutTimeout(byte[] buffer, int offset, int count)
        {
            try
            {
                while (BytesToRead < count)
                    ;
                return ReadImmediately(buffer, offset, count);
            }
            catch
            {
                return 0;
            }
        }

        private int ReadWithTimeout(byte[] buffer, int offset, int count)
        {
            try
            {
                int len = 0;
                Stopwatch watch = Stopwatch.StartNew();
                while (watch.ElapsedTicks < ReadTimeout.Ticks)
                {
                    int size = BytesToRead;
                    if (size >= count)
                    {
                        serialPort.Read(buffer, len + offset, count);
                        len += count;
                        break;
                    }

                    if (size != 0)
                    {
                        serialPort.Read(buffer, len + offset, size);
                        len += size;
                        count -= size;
                    }
                }
                watch.Stop();
                Core.Log.Write("Recv", buffer, offset, len);
                return len;
            }
            catch
            {
                return 0;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count <= BytesToRead)
            {
                return ReadImmediately(buffer, offset, count);
            }
            if (ReadTimeout.Ticks <= 0)
            {
                return ReadWithoutTimeout(buffer, offset, count);
            }

            return ReadWithTimeout(buffer, offset, count);
        }

        public override int Write(byte[] buffer, int offset, int count)
        {
            try
            {
                Core.Log.Write("Send", buffer, offset, count);
                serialPort.Write(buffer, offset, count);
                return count;
            }
            catch
            {
                return 0;
            }
        }

        public override int BytesToRead
        {
            get
            {
                try
                {
                    return serialPort.BytesToRead;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public override Core.Timer ReadTimeout
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
    }
}