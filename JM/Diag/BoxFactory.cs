using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace JM.Diag
{
    public class BoxFactory
    {
        private static readonly BoxFactory instance = new BoxFactory();
        private Dictionary<BoxVersion, ICommbox> commboxes;
        private BoxVersion version;
        private StreamType streamType;

        private BoxFactory()
        {
            commboxes = new Dictionary<BoxVersion, ICommbox>();
            version = BoxVersion.C168;
            streamType = Diag.StreamType.SerialPort;
        }

        public static BoxFactory Instance
        {
            get { return instance; }
        }

        public StreamType StreamType
        {
            get { return streamType; }
            set
            {
                streamType = value;
            }
        }

        public BoxVersion Version
        {
            get { return version; }
            set
            {
                version = value;
                if (commboxes.ContainsKey(version))
                {
                    return;
                }

                switch (version)
                {
                    case BoxVersion.C168:
                        {
                            SerialPort port = new SerialPort();
                            SerialPortStream stream = new SerialPortStream(port);
                            commboxes[version] = new C168.Commbox<SerialPortStream>(stream);
                        }
                        break;
                    case BoxVersion.W80:
                        {
                            SerialPort port = new SerialPort();
                            SerialPortStream stream = new SerialPortStream(port);
                            commboxes[version] = new W80.Commbox<SerialPortStream>(stream);
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public ICommbox Commbox
        {
            get
            {
                if (commboxes.ContainsKey(Version))
                    return commboxes[Version];
                return null;
            }
        }
    }
}