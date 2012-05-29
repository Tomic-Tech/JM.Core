using System;
using System.Threading;
using System.IO.Ports;

namespace JM.Diag
{
    public class BoxStream
    {
        private static BoxStream instance = null;
        private StreamType type;
        private Stream toCommbox = null;
        private Stream fromCommbox = null;
        private ManualResetEvent mre = null;
        private VirtualStream virtualStream = null;
        private RealStream<SerialPort> serialPortStream = null;

        public static BoxStream Instance
        {
            get
            {
                if (instance == null)
                    instance = new BoxStream();
                return instance;
            }
        }

        private BoxStream()
        {
            type = StreamType.Unknow;
        }

        public StreamType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                switch(type)
                {
                    case StreamType.SerialPort:
                        if (serialPortStream == null)
                            serialPortStream = new RealStream<SerialPort>(toCommbox, fromCommbox, mre);
                        serialPortStream.Start();
                        break;
                }
            }
        }

        public SerialPort SerialPort
        {
            get
            {
                if (type == StreamType.SerialPort)
                {
                    return serialPortStream.Real;
                }
                return null;
            }
        }

        public VirtualStream VirtualStream
        {
            get
            {
                return virtualStream;
            }
        }
    }
}