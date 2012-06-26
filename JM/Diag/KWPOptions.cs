namespace JM.Diag
{
    public class KWPOptions
    {
        private KWPStartType startType;

        public KWPStartType StartType
        {
            get { return startType; }
            set { startType = value; }
        }

        private KWPMode linkMode;

        public KWPMode LinkMode
        {
            get { return linkMode; }
            set { linkMode = value; }
        }

        private KWPMode msgMode;

        public KWPMode MsgMode
        {
            get { return msgMode; }
            set { msgMode = value; }
        }

        private int baudrate;

        public int Baudrate
        {
            get { return baudrate; }
            set { baudrate = value; }
        }

        private int targetAddress;

        public int TargetAddress
        {
            get { return targetAddress; }
            set { targetAddress = value; }
        }

        private int sourceAddress;

        public int SourceAddress
        {
            get { return sourceAddress; }
            set { sourceAddress = value; }
        }

        private int comLine;

        public int ComLine
        {
            get { return comLine; }
            set { comLine = value; }
        }

        private bool lLine;

        public bool LLine
        {
            get { return lLine; }
            set { lLine = value; }
        }

        private byte[] fastCmd;

        public byte[] FastCmd
        {
            get { return fastCmd; }
            set { fastCmd = value; }
        }

        private byte addrCode;

        public byte AddrCode
        {
            get { return addrCode; }
            set { addrCode = value; }
        }

    }
}