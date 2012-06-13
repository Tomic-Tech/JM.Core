using System;
using System.Threading;
using System.Threading.Tasks;

namespace JM.Diag
{
    public class RealStream<RealType> where RealType : System.IO.Ports.SerialPort, new()
    {
        public RealStream(Stream toCommbox, Stream fromCommbox, ManualResetEvent mre)
        {
            real = new RealType();
            writting = true;
            reading = true;
            this.toCommbox = toCommbox;
            this.fromCommbox = fromCommbox;
            inter = TimeSpan.FromMilliseconds(1);
            writeTask = new Task(WriteToCommbox);
            readTask = new Task(ReadFromCommbox);
            this.mre = mre;
        }

        ~RealStream()
        {
            stop();
        }

        public RealType Real
        {
            get
            {
                return real;
            }
        }

        public void Start()
        {
            writeTask.Start();
            readTask.Start();
        }

        public void stop()
        {
            writting = false;
            reading = false;
            writeTask.Wait();
            readTask.Wait();
        }

        private void WriteToCommbox()
        {
            writting = true;
            while (writting)
            {
                try
                {
                    int avail = toCommbox.BytesToRead;
                    if (avail <= 0)
                    {
                        Thread.Sleep(inter);
                        continue;
                    }

                    byte[] result = new byte[avail];
                    toCommbox.Read(result, 0, avail);
                    real.Write(result, 0, result.Length);
                }
                catch
                {
                    Thread.Sleep(inter);
                    continue;
                }
            }
        }

        private void ReadFromCommbox()
        {
            reading = true;
            while (reading)
            {
                try
                {
                    int avail = real.BytesToRead;
                    if (avail <= 0)
                    {
                        Thread.Sleep(inter);
                        continue;
                    }

                    byte[] cache = new byte[avail];
                    avail = real.Read(cache, 0, avail);
                    avail = fromCommbox.Write(cache, 0, avail);
                    mre.Set();
                }
                catch
                {
                    Thread.Sleep(inter);
                    continue;
                }
            }
        }

        private bool writting;
        private bool reading;
        private RealType real;
        private Stream toCommbox;
        private Stream fromCommbox;
        private ManualResetEvent mre;
        private TimeSpan inter;
        private Task writeTask;
        private Task readTask;
    }
}