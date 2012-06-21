using System;
using System.Collections.Generic;
using System.Text;

namespace JM.Diag.V1
{
    internal class KWP2000 : Diag.V1.Protocol, Diag.IProtocol
    {
        private delegate void StartCommunication();

        private Default<KWP2000> func;
        private KWPOptions options;
        private Dictionary<KWPStartType, StartCommunication> startComms;
        private byte sendLine;
        private byte recvLine;

        public KWP2000(ICommbox box)
            : base(box)
        {
            this.func = new Default<KWP2000>(box, this);
            StartCommunicationInit();
            sendLine = SK_NO;
            recvLine = RK_NO;
        }

        private void StartCommunicationInit()
        {
            startComms = new Dictionary<KWPStartType, StartCommunication>();
            startComms [KWPStartType.Fast] = () =>
            {
                byte valueOpen = 0;
                if (options.LLine)
                {
                    valueOpen = (byte)(PWC | RZFC | CK);
                } else
                {
                    valueOpen = (byte)(PWC | RZFC | CK);
                }

                if (!Box.SetCommCtrl(valueOpen, SET_NULL) ||
                    !Box.SetCommLine(sendLine, recvLine) ||
                    !Box.SetCommLink(
                    (byte)(RS_232 | BIT9_MARK | SEL_SL | UN_DB20),
                    SET_NULL,
                    SET_NULL
                ) ||
                    !Box.SetCommBaud(options.Baudrate) ||
                    !Box.SetCommTime(
                    SETBYTETIME,
                    Core.Timer.FromMilliseconds(5)
                ) ||
                    !Box.SetCommTime(
                    SETWAITTIME,
                    Core.Timer.FromMilliseconds(0)
                ) ||
                    !Box.SetCommTime(
                    SETRECBBOUT,
                    Core.Timer.FromMilliseconds(400)
                ) ||
                    !Box.SetCommTime(
                    SETRECFROUT,
                    Core.Timer.FromMilliseconds(500)
                ) ||
                    !Box.SetCommTime(
                    SETLINKTIME,
                    Core.Timer.FromMilliseconds(500)
                ))
                {
                    throw new System.IO.IOException();
                }

                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                Box.BuffID = 0;

                if (!Box.NewBatch(Box.BuffID))
                {
                    throw new System.IO.IOException();
                }

                if (!Box.SetLineLevel(COMS, SET_NULL) ||
                    !Box.CommboxDelay(Core.Timer.FromMilliseconds(25)) ||
                    !Box.SetLineLevel(SET_NULL, COMS) ||
                    !Box.CommboxDelay(Core.Timer.FromMilliseconds(25)) ||
                    !Box.SendOutData(
                    options.FastCmd,
                    0,
                    options.FastCmd.Length
                ) ||
                    !Box.RunReceive(REC_FR) ||
                    !Box.EndBatch())
                {
                    Box.DelBatch(Box.BuffID);
                    throw new System.IO.IOException();
                }

                if (!Box.RunBatch(new byte[] { Box.BuffID }, 1, false))
                {
                    throw new System.IO.IOException();
                }

                byte[] recv = ReadOneFrame(new NoPack());
                if (recv == null)
                {
                    throw new System.IO.IOException();
                }

                Box.SetCommTime(SETWAITTIME, Core.Timer.FromMilliseconds(55));
            };

            startComms [KWPStartType.AddressCode] = () =>
            {
                if (!Box.SetCommCtrl((byte)(PWC | REFC | RZFC | CK), SET_NULL) ||
                    !Box.SetCommBaud(5) ||
                    !Box.SetCommTime(
                    SETBYTETIME,
                    Core.Timer.FromMilliseconds(5)
                ) ||
                    !Box.SetCommTime(
                    SETWAITTIME,
                    Core.Timer.FromMilliseconds(12)
                ) ||
                    !Box.SetCommTime(
                    SETRECBBOUT,
                    Core.Timer.FromMilliseconds(400)
                ) ||
                    !Box.SetCommTime(
                    SETRECFROUT,
                    Core.Timer.FromMilliseconds(500)
                ) ||
                    !Box.SetCommTime(
                    SETLINKTIME,
                    Core.Timer.FromMilliseconds(500)
                ))
                {
                    throw new System.IO.IOException();
                }

                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));

                Box.BuffID = 0;

                if (!Box.NewBatch(Box.BuffID))
                {
                    throw new System.IO.IOException();
                }

                if (!Box.SendOutData(new byte[] { options.AddrCode }, 0, 1) ||
                    !Box.SetCommLine(
                    (recvLine == RK_NO) ? sendLine : SK_NO,
                    recvLine
                ) ||
                    !Box.RunReceive(SET55_BAUD) ||
                    !Box.RunReceive(REC_LEN_1) ||
                    !Box.TurnOverOneByOne() ||
                    !Box.RunReceive(REC_LEN_1) ||
                    !Box.TurnOverOneByOne() ||
                    !Box.RunReceive(REC_LEN_1) ||
                    !Box.EndBatch())
                {
                    Box.DelBatch(Box.BuffID);
                    throw new System.IO.IOException();
                }

                byte[] temp = new byte[3];
                if (!Box.RunBatch(new byte[] { Box.BuffID }, 1, false) ||
                    (Box.ReadData(
                    temp,
                    0,
                    temp.Length,
                    Core.Timer.FromSeconds(5)
                ) <= 0) ||
                    !Box.CheckResult(Core.Timer.FromSeconds(5)))
                {
                    Box.DelBatch(Box.BuffID);
                    throw new System.IO.IOException();
                }

                if (!Box.DelBatch(Box.BuffID) ||
                    !Box.SetCommTime(
                    SETWAITTIME,
                    Core.Timer.FromMilliseconds(55)
                ))
                {
                    throw new System.IO.IOException();
                }

                if (temp [2] != 0)
                {
                    throw new System.IO.IOException();
                }

            };
        }

        public override void FinishExecute(bool isFinish)
        {
            if (isFinish)
            {
                Box.StopNow(true);
                Box.DelBatch(Box.BuffID);
                Box.CheckResult(Core.Timer.FromMilliseconds(500));
            }
        }

        private byte[] ReadOneFrame(IPack pack, bool isFinish)
        {
            byte[] temp = new byte[3];
            byte[] result = new byte[255];
            int frameLength = 0;
            int length = 0;
            byte checksum = 0;
            int i;

            length = Box.ReadBytes(temp, 0, temp.Length);

            if (length <= 0)
            {
                FinishExecute(isFinish);
                return null;
            }

            if (temp [1] == options.SourceAddress)
            {
                if (temp [0] == 0x80)
                {
                    byte[] b = new byte[1];
                    length = Box.ReadBytes(b, 0, 1);
                    if (length <= 0)
                    {
                        FinishExecute(isFinish);
                        return null;
                    }

                    length = Convert.ToInt32(b [0]);
                    if (length <= 0)
                    {
                        FinishExecute(isFinish);
                        return null;
                    }

                    Array.Copy(result, 0, temp, 0, temp.Length);
                    frameLength += temp.Length;
                    Array.Copy(result, frameLength, b, 0, 1);
                    ++frameLength;
                    length = Box.ReadBytes(
                        result,
                        KWPPack.KWP80_HEADER_LENGTH,
                        length + KWPPack.KWP_CHECKSUM_LENGTH
                    );
                    frameLength += length;
                } else
                {
                    length = Convert.ToInt32(temp [0] - 0x80);
                    if (length <= 0)
                    {
                        FinishExecute(isFinish);
                        return null;
                    }

                    Array.Copy(result, 0, temp, 0, temp.Length);
                    frameLength += temp.Length;
                    length = Box.ReadBytes(
                        result,
                        temp.Length,
                        length + KWPPack.KWP_CHECKSUM_LENGTH
                    );
                    frameLength += length;
                }
            } else
            {
                if (temp [0] == 0x00)
                {
                    length = Convert.ToInt32(temp [1]);
                    if (length <= 0)
                    {
                        FinishExecute(isFinish);
                        return null;
                    }
                    Array.Copy(result, 0, temp, 0, temp.Length);
                    frameLength += temp.Length;
                    length = Box.ReadBytes(result, temp.Length, length);
                    frameLength += length;
                } else
                {
                    length = Convert.ToInt32(temp [0]);
                    if (length <= 0)
                    {
                        FinishExecute(isFinish);
                        return null;
                    }

                    Array.Copy(result, 0, temp, 0, temp.Length);
                    frameLength += temp.Length;
                    length = Box.ReadBytes(
                        result,
                        temp.Length,
                        length - KWPPack.KWP_CHECKSUM_LENGTH
                    );
                    frameLength += length;
                }
            }

            FinishExecute(isFinish);
            if (frameLength <= 0)
            {
                return null;
            }

            for (i = 0; i < frameLength - 1; i++)
            {
                checksum += result [i];
            }

            if (checksum != result [frameLength - 1])
            {
                return null;
            }

            return pack.Unpack(result, 0, frameLength);
        }

        public int SendOneFrame(byte[] data, int offset, int count, IPack pack)
        {
            return func.SendOneFrame(data, offset, count, pack, false);
        }

        public int SendFrames(byte[] data, int offset, int count, IPack pack)
        {
            return SendOneFrame(data, offset, count, pack);
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

        public void StartKeepLink(bool run)
        {
            func.StartKeepLink(run);
        }

        public void SetKeepLink(byte[] data, int offset, int count, IPack pack)
        {
            byte[] buff = pack.Pack(data, offset, count);
            func.SetKeepLink(buff, 0, buff.Length);
        }

        public void SetTimeout(int txB2B, int rxB2B, int txF2F, int rxF2F, int total)
        {
            func.SetTimeout(txB2B, rxB2B, txF2F, rxF2F, total);
        }

        private void ConfigLines()
        {
            switch (options.ComLine)
            {
                case 7:
                    sendLine = SK1;
                    recvLine = RK1;
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        public void Config(object options)
        {
            if (options is KWPOptions)
            {
                this.options = (KWPOptions)options;
                ConfigLines();
                startComms [this.options.StartType]();
            } else
            {
                throw new ArgumentException();
            }
        }
    }
}
