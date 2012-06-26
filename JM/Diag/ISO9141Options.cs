using System;

namespace JM.Diag
{
    public class ISO9141Options
    {
        private byte header;

        public byte Header
        {
            get
            {
                return header;
            }
            set
            {
                header = value;
            }
        }

        private byte targetAddress;

        public byte TargetAddress
        {
            get
            {
                return targetAddress;
            }
            set
            {
                targetAddress = value;
            }
        }

        private byte sourceAddress;

        public byte SourceAddress
        {
            get
            {
                return sourceAddress;
            }
            set
            {
                sourceAddress = value;
            }
        }

        private int comLine;

        public int ComLine
        {
            get
            {
                return comLine;
            }
            set
            {
                comLine = value;
            }
        }

        private bool lLine;

        public bool LLine
        {
            get
            {
                return lLine;
            }
            set
            {
                lLine = value;
            }
        }

        private byte addrCode;

        public byte AddrCode
        {
            get
            {
                return addrCode;
            }
            set
            {
                addrCode = value;
            }
        }

        public ISO9141Options()
        {
            header = 0;
            targetAddress = 0;
            sourceAddress = 0;
            comLine = 0;
            lLine = false;
            addrCode = 0;
        }

    }
}

