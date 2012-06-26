namespace JM.Diag
{
    public class CanbusOptions
    {
        private int targetID;
        private CanbusBaud baud;

        public CanbusBaud Baud
        {
            get
            {
                return baud;
            }
            set
            {
                baud = value;
            }
        }

        private CanbusIDMode idMode;

        public CanbusIDMode IdMode
        {
            get
            {
                return idMode;
            }
            set
            {
                idMode = value;
            }
        }

        private CanbusFilterMask filterMask;

        public CanbusFilterMask FilterMask
        {
            get
            {
                return filterMask;
            }
            set
            {
                filterMask = value;
            }
        }

        private CanbusFrameType frameType;

        public CanbusFrameType FrameType
        {
            get
            {
                return frameType;
            }
            set
            {
                frameType = value;
            }
        }

        private int highPin;

        public int HighPin
        {
            get
            {
                return highPin;
            }
            set
            {
                highPin = value;
            }
        }

        private int lowPin;

        public int LowPin
        {
            get
            {
                return lowPin;
            }
            set
            {
                lowPin = value;
            }
        }

        private int[] idVector;

        public int[] IdVector
        {
            get
            {
                return idVector;
            }
            set
            {
                idVector = value;
            }
        }

        public int TargetID
        {
            get
            {
                return targetID;
            }

            set
            {
                targetID = value;
            }
        }
    }
}