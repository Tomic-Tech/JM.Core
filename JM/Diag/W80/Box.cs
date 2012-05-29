using System;

namespace JM.Diag.W80
{
    internal class Box
    {
        private uint boxTimeUnit; //万分之一微妙

        public uint BoxTimeUnit
        {
            get { return boxTimeUnit; }
            set { boxTimeUnit = value; }
        }
        private byte timeBaseDB; //标准时间的倍数

        public byte TimeBaseDB
        {
            get { return timeBaseDB; }
            set { timeBaseDB = value; }
        }
        private byte timeExternB; //扩展时间的倍数

        public byte TimeExternB
        {
            get { return timeExternB; }
            set { timeExternB = value; }
        }
        private byte[] port = new byte[Commbox.MAXBUFF_NUM]; //端口

        public byte[] Port
        {
            get { return port; }
        }
        private bool isDB20;

        public bool IsDB20
        {
            get { return isDB20; }
            set { isDB20 = value; }
        }
        private bool isDoNow;

        public bool IsDoNow
        {
            get { return isDoNow; }
            set { isDoNow = value; }
        }
        private byte[] buf = new byte[Commbox.MAXBUFF_LEN]; //缓冲区

        public byte[] Buf
        {
            get { return buf; }
        }
        private int pos;

        public int Pos
        {
            get { return pos; }
            set { pos = value; }
        }
        private bool isLink; //是否是链路保持块

        public bool IsLink
        {
            get { return isLink; }
            set { isLink = value; }
        }
        private byte runFlag;

        public byte RunFlag
        {
            get { return runFlag; }
            set { runFlag = value; }
        }
    }
}