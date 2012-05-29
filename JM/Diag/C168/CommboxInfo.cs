namespace JM.Diag.C168
{
    internal class CommboxInfo
    {
        private long commboxTimeUnit;

        public long CommboxTimeUnit
        {
            get { return commboxTimeUnit; }
            set { commboxTimeUnit = value; }
        }
        private byte timeBaseDB;

        public byte TimeBaseDB
        {
            get { return timeBaseDB; }
            set { timeBaseDB = value; }
        }
        private byte timeExternDB;

        public byte TimeExternDB
        {
            get { return timeExternDB; }
            set { timeExternDB = value; }
        }
        private byte cmdBuffLen;

        public byte CmdBuffLen
        {
            get { return cmdBuffLen; }
            set { cmdBuffLen = value; }
        }
        private byte[] version;

        public byte[] Version
        {
            get { return version; }
        }
        private byte[] commboxID;

        public byte[] CommboxID
        {
            get { return commboxID; }
        }
        private byte[] commboxPort;

        public byte[] CommboxPort
        {
            get { return commboxPort; }
        }
        private byte headPassword;

        public byte HeadPassword
        {
            get { return headPassword; }
            set { headPassword = value; }
        }

        public CommboxInfo()
        {
            commboxTimeUnit = 0;
            timeBaseDB = 0;
            timeExternDB = 0;
            cmdBuffLen = 0;
            version = new byte[Commbox.VERSIONLEN];
            commboxID = new byte[Commbox.COMMBOXIDLEN];
            commboxPort = new byte[Commbox.COMMBOXPORTNUM];
            headPassword = 0;
        }
    }
}