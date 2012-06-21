using System;
using System.Collections.Generic;
using System.Text;

namespace JM.Diag.V1
{
    internal class Default<T> where T : Diag.V1.Protocol, Diag.IProtocol 
    {
        private Diag.V1.ICommbox commbox;
        private T protocol;

        public Default(Diag.V1.ICommbox commbox, T protocol)
        {
            this.commbox = commbox;
            this.protocol = protocol;
        }

        public int SendOneFrame(byte [] buffer, int offset, int length, Diag.IPack pack, bool needRecv)
        {
            byte[] sendBuff = pack.Pack(buffer, offset, length);
            commbox.BuffID = 0;
            byte receive = 0;

            if (commbox.GetType() == typeof(Diag.W80.Commbox<SerialPortStream>))
            {
                receive = W80.Constant.RECEIVE;
            }
            
            if (!commbox.NewBatch(commbox.BuffID))
            {
                return 0;
            }

            if (length <= 0)
            {
                return 0;
            }

            if (needRecv)
            {
                if (!commbox.SendOutData(sendBuff, 0, sendBuff.Length) ||
                    !commbox.RunReceive(receive) ||
                    !commbox.EndBatch() ||
                    !commbox.RunBatch(new byte[] { commbox.BuffID }, 1, false))
                {
                    return 0;
                }
            }
            else
            {
                if (!commbox.SendOutData(sendBuff, 0, sendBuff.Length) ||
                    !commbox.EndBatch() ||
                    !commbox.RunBatch(new byte[] { commbox.BuffID }, 1, false))
                {
                    return 0;
                }
            }

            protocol.FinishExecute(!needRecv);
            return length;
        }

        public byte[] SendAndRecv(byte[] data, int offset, int count, IPack pack)
        {
            int times = 3;
            while (times-- != 0)
            {
                if (protocol.SendFrames(data, offset, count, pack) != count)
                {
                    return null;
                }
                return protocol.ReadFrames(pack);
            }
            return null;
        }

        public void SetKeepLink(byte[] data, int offset, int length)
        {
            byte linkBlock = 0;
            if (commbox.GetType() == typeof(Diag.W80.Commbox<SerialPortStream>))
            {
                linkBlock = W80.Constant.LINKBLOCK;
            }
            if (!commbox.NewBatch(linkBlock))
            {
                throw new System.IO.IOException();
            }

            if (!commbox.SendOutData(data, offset, length) ||
                !commbox.EndBatch())
            {
                throw new System.IO.IOException();
            }
        }

        public void StartKeepLink(bool isRun)
        {
            if (!commbox.KeepLink(isRun))
            {
                throw new System.IO.IOException();
            }
        }

        public void SetTimeout(long txB2B, long rxB2B, long txF2F, long rxF2F, long total)
        {

        }
    }
}
