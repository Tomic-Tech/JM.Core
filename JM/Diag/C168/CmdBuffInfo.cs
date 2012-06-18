using System;

namespace JM.Diag.C168
{
    internal class CmdBuffInfo
    {
        private byte cmdBuffID;
        private byte cmdUsedNum;
        private byte[] cmdBuffAdd;
        private byte[] cmdBuffUsed;

        public CmdBuffInfo()
        {
            cmdBuffID = 0;
            cmdUsedNum = 0;
            cmdBuffAdd = new byte[Constant.MAXIM_BLOCK + 2];
            cmdBuffUsed = new byte[Constant.MAXIM_BLOCK];
        }

        public byte CmdBuffID
        {
            get
            {
                return cmdBuffID;
            }
            set
            {
                cmdBuffID = value;
            }
        }

        public byte CmdUsedNum
        {
            get
            {
                return cmdUsedNum;
            }
            set
            {
                cmdUsedNum = value;
            }
        }

        public byte[] CmdBuffAdd
        {
            get
            {
                return cmdBuffAdd;
            }
        }

        public byte[] CmdBuffUsed
        {
            get
            {
                return cmdBuffUsed;
            }
        }
    }
}