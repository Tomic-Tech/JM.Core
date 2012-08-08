using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace JM.Diag.V1
{
    internal class ISO9141 : Diag.V1.Protocol, Diag.IProtocol
    {
        private ISO9141Options options;
        private byte kLine;
        private byte lLine;
        private Default<ISO9141> func;

        public ISO9141(ICommbox box)
            : base(box)
        {
            options = null;
            kLine = SK_NO;
            lLine = RK_NO;
            func = new Default<ISO9141>(box, this);
        }

        #region implemented abstract members of JM.Diag.V1.Protocol
        public override void FinishExecute(bool isFinish)
        {
            if (isFinish)
            {
                Box.StopNow(false);
                Box.CheckResult(Core.Timer.FromMilliseconds(500));
                Box.DelBatch(Box.BuffID);
            }
        }
        #endregion

        #region IProtocol implementation
        public int SendOneFrame(byte[] data, int offset, int count, IPack pack)
        {
            return func.SendOneFrame(data, offset, count, pack, false);
        }

        public int SendFrames(byte[] data, int offset, int count, IPack pack)
        {
            return func.SendOneFrame(data, offset, count, pack, true);
        }

        public byte[] ReadOneFrame(IPack pack)
        {
            byte[] buff = new byte[128];
            int i;
            for (i = 0; i < buff.Length; i++)
            {
                if (Box.ReadBytes(buff, i, 1) != 1)
                    break;
            }

            if (i < 5 || i > 11)
            {
                return null;
            }

            return pack.Unpack(buff, 0, i);
        }

        public byte[] ReadFrames(IPack pack)
        {
            byte[] buff = new byte[128];
            int i;
            for (i = 0; i < buff.Length; i++)
            {
                if (Box.ReadBytes(buff, i, 1) != 1)
                    break;
            }

            if (i < 5)
            {
                FinishExecute(true);
                return null;
            }

            List<byte> result = new List<byte>();
            int j = 3;
            int k = 0;

            while (j < i) // Multi Frame
            {
                if ((buff[k] == buff[j]) &&
                    (buff[k + 1] == buff[j + 1]) &&
                    (buff[k + 2] == buff[j + 2]))
                {
                    result.AddRange(pack.Unpack(buff, k, j - k));
                    k = j;
                }
                j++;
            }

            result.AddRange(pack.Unpack(buff, k, j - k)); // Add last frame or it's a single frame
            FinishExecute(true);

            return result.ToArray();
        }

        public byte[] SendAndRecv(byte[] data, int offset, int count, IPack pack)
        {
            return func.SendAndRecv(data, offset, count, pack);
        }

        public bool KeepLink(bool run)
        {
            return func.StartKeepLink(run);
        }

        public bool SetKeepLink(byte[] data, int offset, int count, IPack pack)
        {
            byte[] buff = pack.Pack(data, offset, count);
            return func.SetKeepLink(buff, 0, buff.Length);
        }

        public bool SetTimeout(int txB2B, int rxB2B, int txF2F, int rxF2F, int total)
        {
            return func.SetTimeout(txB2B, rxB2B, txF2F, rxF2F, total);
        }

        private bool ConfigLines()
        {
            switch (options.ComLine)
            {
                case 7:
                    lLine = SK1;
                    kLine = RK1;
                    return true;
                default:
                    return false;
            }
        }

        public bool Config(object opts)
        {
            if (opts is ISO9141Options)
            {
                options = (ISO9141Options)opts;
                if (!ConfigLines())
                    return false;

                if (!Box.SetCommCtrl((byte)(PWC | RZFC | CK), SET_NULL) ||
                    !Box.SetCommLine(lLine, kLine) ||
                    !Box.SetCommLink((byte)(RS_232 | BIT9_MARK | SEL_SL | SET_DB20), SET_NULL, INVERTBYTE) ||
                    !Box.SetCommBaud(5) ||
                    !Box.SetCommTime(SETBYTETIME, Core.Timer.FromMilliseconds(5)) ||
                    !Box.SetCommTime(SETWAITTIME, Core.Timer.FromMilliseconds(25)) ||
                    !Box.SetCommTime(SETRECBBOUT, Core.Timer.FromMilliseconds(400)) ||
                    !Box.SetCommTime(SETRECFROUT, Core.Timer.FromMilliseconds(500)) ||
                    !Box.SetCommTime(SETLINKTIME, Core.Timer.FromMilliseconds(500)))
                    return false;

                Thread.Sleep(TimeSpan.FromSeconds(1));

                Box.BuffID = 0;
                if (!Box.NewBatch(Box.BuffID))
                    return false;

                if (!Box.SendOutData(new byte[] { options.AddrCode }, 0, 1) ||
                    !Box.SetCommLine((kLine == RK_NO ? lLine : SK_NO), kLine) ||
                    !Box.RunReceive(SET55_BAUD) ||
                    !Box.RunReceive(REC_LEN_1) ||
                    !Box.TurnOverOneByOne() ||
                    !Box.RunReceive(REC_LEN_1) ||
                    !Box.TurnOverOneByOne() ||
                    !Box.RunReceive(REC_LEN_1) ||
                    !Box.EndBatch())
                {
                    Box.DelBatch(Box.BuffID);
                    return false;
                }

                int tempLen = 0;
                byte[] tempBuff = new byte[3];

                if (!Box.RunBatch(new byte[] { Box.BuffID }, 1, false) ||
                    (tempLen = Box.ReadData(tempBuff, 0, 3, Core.Timer.FromSeconds(3))) != 3 ||
                    !Box.CheckResult(Core.Timer.FromMilliseconds(500)))
                {
                    Box.DelBatch(Box.BuffID);
                    return false;
                }

                if (!Box.DelBatch(Box.BuffID))
                    return false;

                //if (tempBuff[2] != 0)
                //{
                //    return false;
                //}

                if (!Box.SetCommTime(SETBYTETIME, Core.Timer.FromMilliseconds(5)) ||
                    !Box.SetCommTime(SETWAITTIME, Core.Timer.FromMilliseconds(15)) ||
                    !Box.SetCommTime(SETRECBBOUT, Core.Timer.FromMilliseconds(80)) ||
                    !Box.SetCommTime(SETRECFROUT, Core.Timer.FromMilliseconds(200)))
                    return false;

                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}

