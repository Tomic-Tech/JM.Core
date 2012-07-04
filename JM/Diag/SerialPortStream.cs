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

        public override void Clear()
        {
            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
        }

        private int ReadImmediately(byte[] buffer, int offset, int count)
        {
            return serialPort.Read(buffer, offset, count);
        }

        private int ReadWithoutTimeout(byte[] buffer, int offset, int count)
        {
            while (BytesToRead < count)
                ;
            return ReadImmediately(buffer, offset, count);
        }

        private int ReadWithTimeout(byte[] buffer, int offset, int count)
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
            return len;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
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
            catch
            {
            }
            finally
            {
            }
            return 0;
        }

        public override int Write(byte[] buffer, int offset, int count)
        {
            try
            {
                serialPort.Write(buffer, offset, count);
                return count;
            }
            catch
            {
            }
            finally
            {
            }
            return 0;
        }

        public override int BytesToRead
        {
            get { return serialPort.BytesToRead; }
        }

        public override Core.Timer ReadTimeout
        {
            get { return readTimeout; }
            set { readTimeout = value; }
        }
    }
}