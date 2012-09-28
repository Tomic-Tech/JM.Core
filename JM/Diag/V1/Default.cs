using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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

        public int SendOneFrame(byte[] buffer, int offset, int length, Diag.IPack pack, bool needRecv)
        {
            byte[] sendBuff = pack.Pack(buffer, offset, length);
            commbox.BuffID = 0;
            byte receive = 0;

            if (commbox.GetType() == typeof(Diag.W80.Commbox<SerialPortStream>))
            {
                receive = W80.Constant.RECEIVE;
            }
            else
            {
                receive = C168.Constant.RECEIVE;
            }

            if (!commbox.NewBatch(commbox.BuffID))
            {
                //throw new IOException(Core.SysDB.GetText("Communication Fail"));
                return 0;
            }

            if (length <= 0)
            {
                //throw new IOException(Core.SysDB.GetText("Communication Fail"));
                return 0;
            }

            if (needRecv)
            {
                if (!commbox.SendOutData(sendBuff, 0, sendBuff.Length) ||
                    !commbox.RunReceive(receive) ||
                    !commbox.EndBatch() ||
                    !commbox.RunBatch(new byte[] { commbox.BuffID }, 1, false))
                {
                    //throw new IOException(Core.SysDB.GetText("Communication Fail"));
                    return 0;
                }
            }
            else
            {
                if (!commbox.SendOutData(sendBuff, 0, sendBuff.Length) ||
                    !commbox.EndBatch() ||
                    !commbox.RunBatch(new byte[] { commbox.BuffID }, 1, false))
                {
                    //throw new IOException(Core.SysDB.GetText("Communication Fail"));
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
                    continue;
                byte[] result = protocol.ReadFrames(pack);
                if (result == null)
                    continue;
                return result;
            }
            return null;
        }

        public bool SetKeepLink(byte[] data, int offset, int length)
        {
            byte linkBlock = 0;
            if (commbox.GetType() == typeof(Diag.W80.Commbox<SerialPortStream>))
            {
                linkBlock = W80.Constant.LINKBLOCK;
            }
            else
            {
                linkBlock = C168.Constant.LINKBLOCK;
            }
            if (!commbox.NewBatch(linkBlock))
            {
                //throw new System.IO.IOException();
                return false;
            }

            if (!commbox.SendOutData(data, offset, length) ||
                !commbox.EndBatch())
            {
                //throw new System.IO.IOException(Core.SysDB.GetText("KeepLink Fail"));
                return false;
            }
            return true;
        }

        public bool StartKeepLink(bool isRun)
        {
            if (!commbox.KeepLink(isRun))
            {
                //throw new System.IO.IOException(Core.SysDB.GetText("KeepLink Fail"));
                return false;
            }
            return true;
        }

        public bool SetTimeout(long txB2B, long rxB2B, long txF2F, long rxF2F, long total)
        {
            return true;
        }
    }
}
