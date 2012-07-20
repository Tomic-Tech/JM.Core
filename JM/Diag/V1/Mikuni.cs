using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace JM.Diag.V1
{
    internal class Mikuni : Diag.V1.Protocol, Diag.IProtocol
    {
        private Default<Mikuni> func;
        private MikuniOptions options;

        public Mikuni(ICommbox box)
            : base(box)
        {
            this.func = new Default<Mikuni>(box, this);
        }

        public int SendOneFrame(byte[] data, int offset, int count, IPack pack)
        {
            return func.SendOneFrame(data, offset, count, pack, false);
        }

        public int SendFrames(byte[] data, int offset, int count, IPack pack)
        {
            return func.SendOneFrame(data, offset, count, pack, true);
        }

        private byte[] ReadOneFrame(IPack pack, bool isFinish)
        {
            byte[] result = new byte[100];
            int pos = 0;
            byte before = 0;
            while (Box.ReadBytes(result, pos++, 1) == 1)
            {
                if (before == 0x0D && (result[pos - 1] == 0x0A))
                {
                    break;
                }
                before = result[pos - 1];
            }

            if (before == 0x0D && result[pos - 1] == 0x0A)
            {
                // break normal
                result = pack.Unpack(result, 0, pos);
            }
            else
            {
                result = null;
            }
            FinishExecute(isFinish);
            return result;
        }

        public byte[] ReadOneFrame(IPack pack)
        {
            return ReadOneFrame(pack, true);
        }

        public byte[] ReadFrames(IPack pack)
        {
            return ReadOneFrame(pack);
        }

        public byte[] SendAndRecv(byte[] data, int offset, int count, IPack pack)
        {
            return func.SendAndRecv(data, offset, count, pack);
        }

        public bool KeepLink(bool isRun)
        {
            return func.StartKeepLink(isRun);
        }

        public bool SetKeepLink(byte[] data, int offset, int count, IPack pack)
        {
            byte[] packData = pack.Pack(data, offset, count);
            return func.SetKeepLink(packData, 0, packData.Length);
        }

        public bool SetTimeout(int txB2B, int rxB2B, int txF2F, int rxF2F, int total)
        {
            return func.SetTimeout(txB2B, rxB2B, txF2F, rxF2F, total);
        }

        public bool Config(object options)
        {
            if (!(options is MikuniOptions))
            {
                return false;
            }

            this.options = options as MikuniOptions;

            byte parity = 0;
            byte cmd2 = SET_NULL;
            byte cmd3 = SET_NULL;

            if (this.options.Parity == MikuniParity.None)
            {
                parity = BIT9_MARK;
                cmd2 = 0xFF;
                cmd3 = 0x02;
            }
            else
            {
                parity = BIT9_EVEN;
                cmd2 = 0xFF;
                cmd3 = 0x03;
            }
            
            if (!Box.SetCommCtrl((byte)(PWC | RZFC | CK | REFC), SET_NULL) ||
                !Box.SetCommLine(SK_NO, RK1) ||
                !Box.SetCommLink((byte)(RS_232 | parity | SEL_SL | UN_DB20), cmd2, cmd3) ||
                !Box.SetCommBaud(19200) ||
                !Box.SetCommTime(SETBYTETIME, Core.Timer.FromMilliseconds(5)) ||
                !Box.SetCommTime(SETWAITTIME, Core.Timer.FromMilliseconds(0)) ||
                !Box.SetCommTime(SETRECBBOUT, Core.Timer.FromMilliseconds(500)) ||
                !Box.SetCommTime(SETRECFROUT, Core.Timer.FromSeconds(2)) ||
                !Box.SetCommTime(SETLINKTIME, Core.Timer.FromMilliseconds(500)))
            {
                return false;
            }

            Thread.Sleep(TimeSpan.FromSeconds(1));
            return true;
        }

        public override void FinishExecute(bool isFinish)
        {
            if (isFinish)
            {
                Box.StopNow(true);
                Box.DelBatch(Box.BuffID);
                //Box.CheckResult(Core.Timer.FromMilliseconds(500));
            }
        }
    }
}
