using System;
using JM.Core;
using System.IO;
using System.Threading;

namespace JM.Diag.C168
{
    internal class Commbox<T> : JM.Diag.Commbox<T>, ICommbox, V1.ICommbox where T : SerialPortStream
    {
        private CommboxInfo commboxInfo; //CommBox 有关信息数据
        private CmdBuffInfo cmdBuffInfo; //维护COMMBOX数据缓冲区
        private byte[] cmdTemp; //写入命令缓冲区
        private byte lastError; //提供错误查询
        private bool isDB20;
        private bool isDoNow;
        private byte buffID;
        byte[] password;
        private int position;
        private Core.Timer reqByteToByte;
        private Core.Timer reqWaitTime;
        private Core.Timer resByteToByte;
        private Core.Timer resWaitTime;
        private ConnectorType connector;

        public Commbox(T stream)
            : base(stream)
        {
            commboxInfo = new CommboxInfo();
            cmdBuffInfo = new CmdBuffInfo();
            cmdTemp = new byte[256];
            lastError = 0;
            isDB20 = false;
            isDoNow = true;
            password = new byte[10];
            password[0] = 0x0C;
            password[1] = 0x22;
            password[2] = 0x17;
            password[3] = 0x41;
            password[4] = 0x57;
            password[5] = 0x2D;
            password[6] = 0x43;
            password[7] = 0x17;
            password[8] = 0x2D;
            password[9] = 0x4D;
            position = 0;
            reqByteToByte = new Core.Timer();
            reqWaitTime = new Core.Timer();
            resByteToByte = new Core.Timer();
            resWaitTime = new Core.Timer();

        }

        public bool CheckIdle()
        {
            int avail = Stream.BytesToRead;
            if (avail > 240)
            {
                Stream.Clear();
                return true;
            }

            byte[] receiveBuffer = new byte[1];
            receiveBuffer[0] = Constant.SUCCESS;
            Stream.ReadTimeout = Core.Timer.FromMilliseconds(200);
            while (Stream.Read(receiveBuffer, 0, 1) != 0)
                ;
            if (receiveBuffer[0] == Constant.SUCCESS)
                return true;
            lastError = Constant.KEEPLINK_ERROR;
            return false;
        }

        public bool SendOk(Core.Timer time)
        {
            Stream.ReadTimeout = time;
            byte[] receiveBuffer = new byte[1];
            receiveBuffer[0] = 0;
            if (Stream.Read(receiveBuffer, 0, 1) == 1)
            {
                if (receiveBuffer[0] == Constant.SEND_OK)
                {
                    return true;
                }
                else if (receiveBuffer[0] >= Constant.UP_TIMEOUT && receiveBuffer[0] <= Constant.ERROR_REC)
                {
                    lastError = Constant.SENDDATA_ERROR;
                    return false;
                }
            }
            lastError = Constant.TIMEOUT_ERROR;
            return false;
        }

        public byte BuffID
        {
            get { return buffID; }
            set { buffID = value; }
        }

        public uint BoxVer
        {
            get { return (uint)((commboxInfo.Version[0] << 8) | (commboxInfo.Version[1])); }
        }

        private bool CommboxCommand(byte commandWord, byte[] buff, int offset, int length)
        {
            byte checksum = (byte)(commandWord + length);
            if (commandWord < Constant.WR_DATA)
            {
                if (length == 0)
                {
                    lastError = Constant.ILLIGICAL_LEN;
                    return false;
                }
                checksum--;
            }
            else
            {
                if (length != 0)
                {
                    lastError = Constant.ILLIGICAL_LEN;
                    return false;
                }
            }

            int i;
            byte[] command = new byte[length + 2];
            command[0] = (byte)(checksum + commboxInfo.HeadPassword);
            for (i = 1; i <= length; i++)
            {
                command[i] = buff[offset + i - 1];
                checksum += command[i];
            }
            command[command.Length - 1] = checksum;
            for (i = 0; i < 3; i++)
            {
                if (commandWord != Constant.STOP_REC && commandWord != Constant.STOP_EXECUTE)
                {
                    if (!CheckIdle() || Stream.Write(
                        command,
                        0,
                        command.Length
                    ) != command.Length)
                    {
                        lastError = Constant.SENDDATA_ERROR;
                        continue;
                    }
                }
                else
                {
                    if (Stream.Write(command, 0, command.Length) != command.Length)
                    {
                        lastError = Constant.SENDDATA_ERROR;
                        continue;
                    }
                }

                if (SendOk(Core.Timer.FromMilliseconds(20 * (length + 3))))
                    break;
            }
            return (i < 3) ? true : false;
        }

        private bool CommboxEcuOld(byte commandWord, byte[] buff, int offset, int length)
        {
            if (cmdBuffInfo.CmdBuffAdd[Constant.LINKBLOCK] - cmdBuffInfo.CmdBuffAdd[Constant.SWAPBLOCK] < length + 1)
            {
                lastError = Constant.NOBUFF_TOSEND;
                return false;
            }
            byte[] command = new byte[5 + length];
            command[0] = (byte)(Constant.WR_DATA + commboxInfo.HeadPassword);
            command[1] = (byte)(length + 2);
            command[2] = cmdBuffInfo.CmdBuffAdd[Constant.SWAPBLOCK];
            command[3] = (byte)(length - 1);
            byte checksum = (byte)(Constant.WR_DATA + command[1] + command[2] + command[3]);

            for (int i = 0; i < length; i++)
            {
                command[i + 4] = buff[i];
                checksum += buff[i];
            }
            command[command.Length - 1] = checksum;
            for (int i = 0; i < 3; i++)
            {
                if (!CheckIdle() || Stream.Write(command, 0, command.Length) != command.Length)
                {
                    lastError = Constant.SENDDATA_ERROR;
                    continue;
                }
                if (SendOk(Core.Timer.FromMilliseconds(20 * command.Length)))
                    return true;
            }
            return false;
        }

        private bool CommboxEcuNew(byte commandWord, byte[] buff, int offset, int length)
        {
            if (cmdBuffInfo.CmdBuffAdd[Constant.LINKBLOCK] - cmdBuffInfo.CmdBuffAdd[Constant.SWAPBLOCK] < length + 1)
            {
                lastError = Constant.NOBUFF_TOSEND;
                return false;
            }
            byte[] command = new byte[6 + length];
            command[0] = (byte)(Constant.WR_DATA + commboxInfo.HeadPassword);
            command[1] = (byte)(length + 3);
            command[2] = cmdBuffInfo.CmdBuffAdd[Constant.SWAPBLOCK];
            command[3] = Constant.SEND_CMD;
            command[4] = (byte)(length - 1);
            byte checksum = (byte)(Constant.WR_DATA + command[1] + command[2] + command[3] + command[4]);

            for (int i = 0; i < length; i++)
            {
                command[i + 5] = buff[i];
                checksum += buff[i];
            }
            command[command.Length - 1] = checksum;
            for (int i = 0; i < 3; i++)
            {
                if (!CheckIdle() || Stream.Write(command, 0, command.Length) != command.Length)
                {
                    lastError = Constant.SENDDATA_ERROR;
                    continue;
                }
                if (SendOk(Core.Timer.FromMilliseconds(20 * (command.Length + 7))))
                    return true;
            }
            return false;
        }

        public bool CommboxDo(byte commandWord, byte[] buff, int offset, int length)
        {
            if (length > Constant.CMD_DATALEN)
            {
                if (commandWord == Constant.SEND_DATA && length <= Constant.SEND_LEN)
                {
                    bool ret;
                    if (BoxVer > 0x400)
                    {
                        //增加发送长命令
                        ret = CommboxEcuNew(commandWord, buff, offset, length);
                    }
                    else
                    {
                        //保持与旧盒子兼容
                        ret = CommboxEcuOld(commandWord, buff, offset, length);
                    }
                    if (!ret)
                        return ret;
                    return CommboxDo(
                        Constant.D0_BAT,
                        cmdBuffInfo.CmdBuffAdd,
                        Constant.SWAPBLOCK,
                        1
                    );
                }
                else
                {
                    lastError = Constant.ILLIGICAL_LEN;
                    return false;
                }

            }
            else
            {
                return CommboxCommand(commandWord, buff, offset, length);
            }
        }

        private bool DoSet(byte commandWord, byte[] buff, int offset, int length)
        {
            int times = Constant.REPLAYTIMES;
            while ((times--) > 0)
            {
                if (!CommboxDo(commandWord, buff, offset, length))
                    continue;
                else if (CheckResult(Core.Timer.FromMicroseconds(50)))
                    return true;
                StopNow(true);
            }
            return false;
        }

        public bool CheckResult(Core.Timer time)
        {
            Stream.ReadTimeout = time;
            byte[] receiveBuffer = new byte[1];
            receiveBuffer[0] = 0;
            if (Stream.Read(receiveBuffer, 0, 1) != 1)
            {
                lastError = Constant.TIMEOUT_ERROR;
                return false;
            }
            if (receiveBuffer[0] == Constant.SUCCESS)
                return true;
            while (Stream.Read(receiveBuffer, 0, 1) == 1)
                ;
            lastError = receiveBuffer[0];
            return false;
        }

        public byte GetCmdData(byte command, byte[] receiveBuffer, int offset)
        {
            byte[] cmdInfo = new byte[2];
            int i = 0;
            byte checksum = command;
            if (ReadData(cmdInfo, 0, 2, Core.Timer.FromMilliseconds(150)) != 2)
                return 0;
            if (cmdInfo[0] != command)
            {
                lastError = cmdInfo[0];
                Stream.Clear();
                return 0;
            }
            if (ReadData(receiveBuffer, offset, cmdInfo[1], Core.Timer.FromMilliseconds(150)) != cmdInfo[1] ||
                ReadData(cmdInfo, 0, 1, Core.Timer.FromMilliseconds(150)) != 1)
                return 0;
            checksum += cmdInfo[1];
            for (i = 0; i < cmdInfo[1]; i++)
                checksum += receiveBuffer[i];
            if (checksum != cmdInfo[0])
            {
                lastError = Constant.CHECKSUM_ERROR;
                return 0;
            }
            return cmdInfo[1];
        }

        public int ReadData(byte[] receiveBuffer, int offset, int length, Core.Timer totalTime)
        {
            Stream.ReadTimeout = totalTime;
            return Stream.Read(receiveBuffer, offset, length);
        }

        private bool CheckBox()
        {
            byte checksum = 0;
            int i = 0;
            int len = 0;
            cmdTemp[4] = 0;
            Random rand = new Random();
            while (i < 4)
            {
                cmdTemp[i] = (byte)rand.Next();
                cmdTemp[4] += cmdTemp[i++];
            }

            if (Stream.Write(cmdTemp, 0, 5) != 5)
            {
                lastError = Constant.SENDDATA_ERROR;
                return false;
            }
            len = password.Length;
            i = 0;
            checksum = (byte)(cmdTemp[4] + cmdTemp[4]);
            while (i < len)
            {
                checksum += (byte)(password[i] ^ cmdTemp[i % 5]);
                i++;
            }

            System.Threading.Thread.Sleep(20);
            if (GetCmdData(Constant.GETINFO, cmdTemp, 0) == 0)
                return false;
            commboxInfo.HeadPassword = cmdTemp[0];

            if (checksum != commboxInfo.HeadPassword)
            {
                lastError = Constant.CHECKSUM_ERROR;
                return false;
            }
            if (commboxInfo.HeadPassword == 0)
            {
                commboxInfo.HeadPassword = 0x55;
            }
            return true;
        }

        private bool InitBox()
        {
            commboxInfo.HeadPassword = 0x00;
            isDB20 = false;

            if (!CommboxDo(Constant.GETINFO, null, 0, 0))
                return false;

            byte length = GetCmdData(Constant.GETINFO, cmdTemp, 0);
            if (length <= 0)
                return false;
            if (length < Constant.COMMBOXINFOLEN)
            {
                lastError = Constant.LOST_VERSIONDATA;
                return false;
            }
            commboxInfo.CommboxTimeUnit = 0;
            int pos = 0;
            for (int i = 0; i < Constant.MINITIMELEN; i++)
                commboxInfo.CommboxTimeUnit = commboxInfo.CommboxTimeUnit * 256 + cmdTemp[pos++];
            commboxInfo.TimeBaseDB = cmdTemp[pos++];
            commboxInfo.TimeExternDB = cmdTemp[pos++];
            commboxInfo.CmdBuffLen = cmdTemp[pos++];
            if (commboxInfo.TimeBaseDB == 0 || commboxInfo.CommboxTimeUnit == 0 || commboxInfo.CmdBuffLen == 0)
            {
                lastError = Constant.COMMTIME_ZERO;
                return false;
            }

            for (int i = 0; i < Constant.COMMBOXIDLEN; i++)
                commboxInfo.CommboxID[i] = cmdTemp[pos++];
            for (int i = 0; i < Constant.VERSIONLEN; i++)
                commboxInfo.Version[i] = cmdTemp[pos++];
            commboxInfo.CommboxPort[0] = Constant.NULLADD;
            commboxInfo.CommboxPort[1] = Constant.NULLADD;
            commboxInfo.CommboxPort[2] = Constant.NULLADD;
            commboxInfo.CommboxPort[3] = Constant.NULLADD;

            cmdBuffInfo.CmdBuffID = Constant.NULLADD;
            cmdBuffInfo.CmdUsedNum = 0;
            for (int i = 0; i < Constant.MAXIM_BLOCK; i++)
                cmdBuffInfo.CmdBuffAdd[i] = Constant.NULLADD;
            cmdBuffInfo.CmdBuffAdd[Constant.LINKBLOCK] = commboxInfo.CmdBuffLen;
            cmdBuffInfo.CmdBuffAdd[Constant.SWAPBLOCK] = 0;

            return true;
        }

        private bool OpenBox(string port, int baud)
        {
            try
            {
                Stream.SerialPort.PortName = port;
                Stream.SerialPort.BaudRate = baud;
                Stream.SerialPort.DataBits = 8;
                Stream.SerialPort.StopBits = System.IO.Ports.StopBits.One;
                Stream.SerialPort.Parity = System.IO.Ports.Parity.None;
                Stream.SerialPort.Handshake = System.IO.Ports.Handshake.None;
                Stream.SerialPort.ReadTimeout = 500;
                Stream.SerialPort.WriteTimeout = 500;
                Stream.SerialPort.Open();
                System.Threading.Thread.Sleep(50);
                Stream.SerialPort.DtrEnable = true;
                System.Threading.Thread.Sleep(50);
                for (int i = 0; i < 3; i++)
                {
                    SetRF(Constant.RESET_RF, 0);
                    SetRF(Constant.SETDTR_L, 0);
                    if (InitBox() && CheckBox())
                    {
                        Stream.SerialPort.DiscardInBuffer();
                        Stream.SerialPort.DiscardOutBuffer();
                        if (SetPCBaud(Constant.UP_57600BPS))
                            return true;
                    }
                }
                Stream.SerialPort.Close();
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool Open()
        {
            string[] portNames = System.IO.Ports.SerialPort.GetPortNames();
            foreach (string portName in portNames)
            {
                if (!OpenBox(portName, 9600))
                {
                    continue;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public bool Close()
        {
            if (Stream.SerialPort.IsOpen)
            {
                try
                {
                    StopNow(true);
                    DoSet(Constant.RESET, null, 0, 0);
                    SetRF(Constant.RESET_RF, 0);
                    Stream.SerialPort.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        private bool DoSetPCBaud(byte baud)
        {
            try
            {
                lastError = 0;
                if (!CommboxDo(Constant.SET_UPBAUD, new byte[] { baud }, 0, 1))
                    return false;
                Thread.Sleep(50);
                CheckResult(Core.Timer.FromMilliseconds(50));
                SetRF(Constant.SETRFBAUD, baud);
                CheckResult(Core.Timer.FromMilliseconds(50));
                switch (baud)
                {
                    case Constant.UP_9600BPS:
                        Stream.SerialPort.BaudRate = 9600;
                        break;
                    case Constant.UP_38400BPS:
                        Stream.SerialPort.BaudRate = 19200;
                        break;
                    case Constant.UP_57600BPS:
                        Stream.SerialPort.BaudRate = 57600;
                        break;
                    case Constant.UP_115200BPS:
                        Stream.SerialPort.BaudRate = 115200;
                        break;
                    default:
                        lastError = Constant.ILLIGICAL_CMD;
                        return false;
                }

                SetRF(Constant.SETRFBAUD, baud);
                if (!CommboxDo(Constant.SET_UPBAUD, new byte[] { baud }, 0, 1))
                    return false;
                if (!CheckResult(Core.Timer.FromMilliseconds(100)))
                    return false;
                Stream.SerialPort.DiscardInBuffer();
                return true;
            }
            catch
            {
                lastError = Constant.DISCONNECT_COMM;
                return false;
            }
        }

        private bool SetPCBaud(byte baud)
        {
            int times = Constant.REPLAYTIMES;
            while (times > 0)
            {
                if (DoSetPCBaud(baud))
                    return true;
                times--;
            }
            return false;
        }

        public bool NewBatch(byte buffID)
        {
            if (buffID > Constant.MAXIM_BLOCK)
            {
                lastError = Constant.NODEFINE_BUFF;
                return false;
            }

            if (cmdBuffInfo.CmdBuffID != Constant.NULLADD)
            {
                lastError = Constant.APPLICATION_NOW;
                return false;
            }
            if (cmdBuffInfo.CmdBuffAdd[buffID] != Constant.NULLADD && buffID != Constant.LINKBLOCK && !DelBatch(buffID))
                return false;
            cmdTemp[0] = Constant.WR_DATA;
            cmdTemp[1] = 0x01;
            if (buffID == Constant.LINKBLOCK)
            {
                cmdTemp[2] = 0xFF;
                cmdBuffInfo.CmdBuffAdd[Constant.LINKBLOCK] = commboxInfo.CmdBuffLen;
            }
            else
            {
                cmdTemp[2] = cmdBuffInfo.CmdBuffAdd[Constant.SWAPBLOCK];
            }
            if ((cmdBuffInfo.CmdBuffAdd[Constant.LINKBLOCK] - cmdBuffInfo.CmdBuffAdd[Constant.SWAPBLOCK]) <= 1)
            {
                lastError = Constant.BUFFFLOW;
                return false;
            }
            cmdTemp[3] = (byte)(Constant.WR_DATA + 0x01 + cmdTemp[2]);
            cmdTemp[0] += commboxInfo.HeadPassword;
            cmdBuffInfo.CmdBuffID = buffID;
            isDoNow = false;
            return true;
        }

        public bool AddToBuff(byte commandWord, byte[] data, int offset, int length)
        {
            byte checksum = cmdTemp[cmdTemp[1] + 2];
            position = cmdTemp[1] + length + 1;
            if (cmdBuffInfo.CmdBuffID == Constant.NULLADD)
            {
                //数据块标识登记是否有申请?
                lastError = Constant.NOAPPLICATBUFF;
                isDoNow = true;
                return false;
            }

            if ((cmdBuffInfo.CmdBuffAdd[Constant.LINKBLOCK] - cmdBuffInfo.CmdBuffAdd[Constant.SWAPBLOCK]) < position)
            {
                //检查是否有足够的空间存储?
                isDoNow = true;
                return false;
            }

            if (commandWord < Constant.RESET && commandWord != Constant.CLR_LINK && commandWord != Constant.DO_BAT_00 && commandWord != Constant.D0_BAT && commandWord != Constant.D0_BAT_FOR && commandWord != Constant.WR_DATA)
            {
                //是否为缓冲区命令?
                if (length <= Constant.CMD_DATALEN || (commandWord == Constant.SEND_DATA && length < Constant.SEND_LEN))
                {
                    //是否合法命令?
                    if (commandWord == Constant.SEND_DATA && BoxVer > 0x400)
                    {
                        //增加发送长命令
                        cmdTemp[cmdTemp[1] + 2] = Constant.SEND_CMD;
                        checksum += Constant.SEND_CMD;
                        cmdTemp[1]++;
                        cmdTemp[cmdTemp[1] + 2] = (byte)(commandWord + length);
                        if (length != 0)
                            cmdTemp[cmdTemp[1] + 2]--;
                        checksum += cmdTemp[cmdTemp[1] + 2];
                        cmdTemp[1]++;
                        for (int i = 0; i < length; i++, cmdTemp[1]++)
                        {
                            cmdTemp[cmdTemp[1] + 2] = data[i + offset];
                            checksum += data[i + offset];
                        }
                        cmdTemp[cmdTemp[1] + 2] = (byte)(checksum + length + 2);
                        position++;
                    }
                    else
                    {
                        cmdTemp[cmdTemp[1] + 2] = (byte)(commandWord + length);
                        if (length != 0)
                            cmdTemp[cmdTemp[1] + 2]--;
                        checksum += cmdTemp[cmdTemp[1] + 2];
                        cmdTemp[1]++;
                        for (int i = 0; i < length; i++, cmdTemp[1]++)
                        {
                            cmdTemp[cmdTemp[1] + 2] = data[i + offset];
                            checksum += data[i + offset];
                        }
                        cmdTemp[cmdTemp[1] + 2] = (byte)(checksum + length + 1);
                        position++;
                    }
                    return true;
                }
                lastError = Constant.ILLIGICAL_LEN;
                isDoNow = true;
                return false;
            }
            lastError = Constant.UNBUFF_CMD;
            isDoNow = true;
            return false;
        }

        public bool EndBatch()
        {
            int times = Constant.REPLAYTIMES;
            isDoNow = true;
            if (cmdBuffInfo.CmdBuffID == Constant.NULLADD)
            {
                //数据块标识登记是否有申请?
                lastError = Constant.NOAPPLICATBUFF;
                return false;
            }

            if (cmdTemp[1] == 0x01)
            {
                cmdBuffInfo.CmdBuffID = Constant.NULLADD;
                lastError = Constant.NOADDDATA;
                return false;
            }

            while (times != 0)
            {
                if (!CheckIdle() || Stream.Write(cmdTemp, 0, cmdTemp[1] + 3) != cmdTemp[1] + 3)
                    continue;
                else if (SendOk(Core.Timer.FromMilliseconds(20 * (cmdTemp[1] + 10))))
                    break;
                if (!StopNow(true))
                {
                    cmdBuffInfo.CmdBuffID = Constant.NULLADD;
                    return false;
                }
            }
            if (times == 0)
            {
                cmdBuffInfo.CmdBuffID = Constant.NULLADD;
                return false;
            }
            if (cmdBuffInfo.CmdBuffID == Constant.LINKBLOCK)
                cmdBuffInfo.CmdBuffAdd[Constant.LINKBLOCK] = (byte)(commboxInfo.CmdBuffLen - cmdTemp[1]);
            else
            {
                cmdBuffInfo.CmdBuffAdd[cmdBuffInfo.CmdBuffID] = cmdBuffInfo.CmdBuffAdd[Constant.SWAPBLOCK];
                cmdBuffInfo.CmdBuffUsed[cmdBuffInfo.CmdUsedNum] = cmdBuffInfo.CmdBuffID;
                cmdBuffInfo.CmdUsedNum++;
                cmdBuffInfo.CmdBuffAdd[Constant.SWAPBLOCK] += cmdTemp[1];
            }
            cmdBuffInfo.CmdBuffID = Constant.NULLADD;
            return true;
        }

        public bool DelBatch(byte buffID)
        {
            if (buffID > Constant.LINKBLOCK)
            {
                //数据块不存在
                lastError = Constant.NODEFINE_BUFF;
                return false;
            }
            if (cmdBuffInfo.CmdBuffID == buffID)
            {
                cmdBuffInfo.CmdBuffID = Constant.NULLADD;
                return true;
            }

            if (cmdBuffInfo.CmdBuffAdd[buffID] == Constant.NULLADD)
            {
                //数据块标识登记是否有申请?
                lastError = Constant.NOUSED_BUFF;
                return false;
            }

            if (buffID == Constant.LINKBLOCK)
                cmdBuffInfo.CmdBuffAdd[Constant.LINKBLOCK] = commboxInfo.CmdBuffLen;
            else
            {
                int i;
                for (i = 0; i < cmdBuffInfo.CmdUsedNum; i++)
                    if (cmdBuffInfo.CmdBuffUsed[i] == buffID)
                        break;
                byte[] data = new byte[3];
                data[0] = cmdBuffInfo.CmdBuffAdd[buffID];
                if (i < cmdBuffInfo.CmdUsedNum - 1)
                {
                    data[1] = cmdBuffInfo.CmdBuffAdd[cmdBuffInfo.CmdBuffUsed[i + 1]];
                    data[2] = (byte)(cmdBuffInfo.CmdBuffAdd[Constant.SWAPBLOCK] - data[1]);
                    if (!DoSet(
                        (byte)(Constant.COPY_DATA - Constant.COPY_DATA % 4),
                        data,
                        0,
                        3
                    ))
                        return false;
                }
                else
                {
                    data[1] = cmdBuffInfo.CmdBuffAdd[Constant.SWAPBLOCK];
                }
                int deleteBuffLen = data[1] - data[0];
                for (i = i + 1; i < cmdBuffInfo.CmdUsedNum; i++)
                {
                    cmdBuffInfo.CmdBuffUsed[i - 1] = cmdBuffInfo.CmdBuffUsed[i];
                    cmdBuffInfo.CmdBuffAdd[cmdBuffInfo.CmdBuffUsed[i]] -= (byte)deleteBuffLen;
                }
                cmdBuffInfo.CmdUsedNum--;
                cmdBuffInfo.CmdBuffAdd[Constant.SWAPBLOCK] -= (byte)deleteBuffLen;
                cmdBuffInfo.CmdBuffAdd[buffID] = Constant.NULLADD;
            }
            return true;
        }

        private bool SendCmdToBox(byte command, byte[] buffer, int offset, int length)
        {
            if (isDoNow)
            {
                return DoSet(command, buffer, offset, length);
            }
            else
            {
                return AddToBuff(command, buffer, offset, length);
            }
        }

        private bool SendCmdToBox(byte command)
        {
            return SendCmdToBox(command, null, 0, 0);
        }

        public bool SetLineLevel(byte valueLow, byte valueHigh)
        {
            //只有一个字节的数据，设定端口1
            commboxInfo.CommboxPort[1] &= (byte)(~valueLow);
            commboxInfo.CommboxPort[1] |= valueHigh;
            return SendCmdToBox(
                Constant.SETPORT1,
                commboxInfo.CommboxPort,
                1,
                1
            );
        }

        public bool SetCommCtrl(byte valueOpen, byte valueClose)
        {
            //只有一个字节的数据，设定端口2
            commboxInfo.CommboxPort[2] &= (byte)(~valueOpen);
            commboxInfo.CommboxPort[2] |= valueClose;
            return SendCmdToBox(
                Constant.SETPORT2,
                commboxInfo.CommboxPort,
                2,
                1
            );
        }

        public bool SetCommLine(byte sendLine, byte recLine)
        {
            //只有一个字节的数据，设定端口0
            if (sendLine > 7)
                sendLine = 0x0F;
            if (recLine > 7)
                recLine = 0x0F;
            commboxInfo.CommboxPort[0] = (byte)(sendLine + recLine * 16);
            return SendCmdToBox(
                Constant.SETPORT0,
                commboxInfo.CommboxPort,
                0,
                1
            );
        }

        public bool TurnOverOneByOne()
        {
            //将原有的接受一个发送一个的标志翻转
            return SendCmdToBox(Constant.SET_ONEBYONE);
        }

        public bool SetEchoData(byte[] echoBuff, int offset, int length)
        {
            if (length == 0 || length > 4)
            {
                lastError = Constant.ILLIGICAL_LEN;
                return false;
            }

            if (isDoNow)
            {
                byte[] buff = new byte[6];
                if (!CommboxDo(Constant.ECHO, echoBuff, offset, length) ||
                    ReadData(buff, 0, length, Core.Timer.FromMilliseconds(100)) != length)
                    return false;
                while (length-- > 0)
                {
                    if (buff[length] != echoBuff[length])
                    {
                        lastError = Constant.CHECKSUM_ERROR;
                        return false;
                    }
                }
                return CheckResult(Core.Timer.FromMilliseconds(100));
            }
            else
            {
                return AddToBuff(Constant.ECHO, echoBuff, 0, length);
            }
        }

        public bool KeepLink(bool isRunLink)
        {
            if (isRunLink)
            {
                return SendCmdToBox(Constant.RUNLINK);
            }
            else
            {
                return SendCmdToBox(Constant.STOPLINK);
            }
        }

        public bool SetCommLink(byte ctrlWord1, byte ctrlWord2, byte ctrlWord3)
        {
            byte[] ctrlWord = new byte[3]; //通讯控制字3
            byte modeControl = (byte)(ctrlWord1 & 0xE0);
            int length = 3;
            ctrlWord[0] = ctrlWord1;
            if ((ctrlWord1 & 0x04) != 0)
                isDB20 = true;
            else
                isDB20 = false;

            if (modeControl == Constant.SET_VPW || modeControl == Constant.SET_PWM)
            {
                return SendCmdToBox(Constant.SETTING, ctrlWord, 0, 1);
            }
            else
            {
                ctrlWord[1] = ctrlWord2;
                ctrlWord[2] = ctrlWord3;
                if (ctrlWord3 == 0)
                {
                    length--;
                    if (ctrlWord2 == 0)
                        length--;
                }
                if (modeControl == Constant.EXRS_232 && length < 2)
                {
                    lastError = Constant.UNSET_EXRSBIT;
                    return false;
                }
                return SendCmdToBox(Constant.SETTING, ctrlWord, 0, length);
            }
        }

        public bool SetCommBaud(double baud)
        {
            byte[] baudTime = new byte[2];
            double instructNum = ((1000000.0 / (commboxInfo.CommboxTimeUnit)) * 1000000) / baud;
            if (isDB20)
                instructNum /= 20;
            instructNum += 0.5;

            if (instructNum > 65535 || instructNum < 10)
            {
                lastError = Constant.COMMBAUD_OUT;
                return false;
            }
            baudTime[0] = (byte)(instructNum / 256);
            baudTime[1] = (byte)(instructNum % 256);

            if (baudTime[0] == 0)
            {
                return SendCmdToBox(Constant.SETBAUD, baudTime, 1, 1);
            }
            else
            {
                return SendCmdToBox(Constant.SETBAUD, baudTime, 0, 2);
            }
        }

        private void GetLinkTime(byte type, Core.Timer time)
        {
            switch (type)
            {
                case Constant.SETBYTETIME:
                    reqByteToByte = time;
                    break;
                case Constant.SETWAITTIME:
                    reqWaitTime = time;
                    break;
                case Constant.SETRECBBOUT:
                    resByteToByte = time;
                    break;
                case Constant.SETRECFROUT:
                    resWaitTime = time;
                    break;
            }
        }

        public bool SetCommTime(byte type, Core.Timer time)
        {
            byte[] timeBuff = new byte[2];
            GetLinkTime(type, time);
            ulong microTime = (ulong)time.Microseconds;

            if (type == Constant.SETVPWSTART || type == Constant.SETVPWRECS)
            {
                if (Constant.SETVPWRECS == type)
                    microTime = (microTime * 2) / 3;
                type = (byte)(type + (Constant.SETBYTETIME & 0xF0));
                microTime = (ulong)((microTime * 1000000.0) / commboxInfo.CommboxTimeUnit);
            }
            else
            {
                microTime = (ulong)((microTime * 1000000.0) / (commboxInfo.TimeBaseDB * commboxInfo.CommboxTimeUnit));
               // microTime = (microTime / commboxInfo.TimeBaseDB) / (commboxInfo.CommboxTimeUnit / 1000000.0);
            }

            if (microTime > 65535)
            {
                lastError = Constant.COMMTIME_OUT;
                return false;
            }

            if (type == Constant.SETBYTETIME || type == Constant.SETWAITTIME || type == Constant.SETRECBBOUT || type == Constant.SETRECFROUT || type == Constant.SETLINKTIME)
            {
                timeBuff[0] = (byte)(microTime / 256);
                timeBuff[1] = (byte)(microTime % 256);
                if (timeBuff[0] == 0)
                {
                    return SendCmdToBox(type, timeBuff, 1, 1);
                }
                else
                {
                    return SendCmdToBox(type, timeBuff, 0, 2);
                }
            }
            
            lastError = Constant.UNDEFINE_CMD;
            return false;
        }

        public bool RunReceive(byte type)
        {
            if (type == Constant.GET_PORT1)
                isDB20 = false;
            if (type == Constant.GET_PORT1 || type == Constant.SET55_BAUD || (type >= Constant.REC_FR && type <= Constant.RECEIVE))
            {
                if (isDoNow)
                {
                    return CommboxDo(type, null, 0, 0);
                }
                else
                {
                    return AddToBuff(type, null, 0, 0);
                }
            }
            lastError = Constant.UNDEFINE_CMD;
            return false;
        }

        public bool GetAbsAdd(byte buffID, ref byte buffAdd)
        {
            int length;
            byte startAdd;
            if (cmdBuffInfo.CmdBuffID != buffID)
            {
                if (cmdBuffInfo.CmdBuffAdd[buffID] == Constant.NULLADD)
                {
                    lastError = Constant.NOUSED_BUFF;
                    return false;
                }

                if (buffID == Constant.LINKBLOCK)
                {
                    length = commboxInfo.CmdBuffLen - cmdBuffInfo.CmdBuffAdd[Constant.LINKBLOCK];
                }
                else
                {
                    int i;
                    for (i = 0; i < cmdBuffInfo.CmdUsedNum; i++)
                        if (cmdBuffInfo.CmdBuffUsed[i] == buffID)
                            break;
                    if (i == cmdBuffInfo.CmdUsedNum - 1)
                        length = cmdBuffInfo.CmdBuffAdd[Constant.SWAPBLOCK] - cmdBuffInfo.CmdBuffAdd[buffID];
                    else
                        length = cmdBuffInfo.CmdBuffAdd[buffID + 1] - cmdBuffInfo.CmdBuffAdd[buffID];
                }
                startAdd = cmdBuffInfo.CmdBuffAdd[buffID];
            }
            else
            {
                length = cmdBuffInfo.CmdBuffAdd[Constant.LINKBLOCK] - cmdBuffInfo.CmdBuffAdd[Constant.SWAPBLOCK];
                startAdd = cmdBuffInfo.CmdBuffAdd[Constant.SWAPBLOCK];
            }

            if (buffAdd < length)
            {
                buffAdd += startAdd;
                return true;
            }
            else
            {
                lastError = Constant.OUTADDINBUFF;
                return false;
            }
        }

        public bool UpdateBuff(byte type, byte[] buffer)
        {
            byte[] temp = new byte[4];
            lastError = 0;
            temp[0] = buffer[1];
            if (!GetAbsAdd(buffer[0], ref temp[0]))
                return false;
            switch (type)
            {
                case Constant.INVERT_DATA: //add
                case Constant.DEC_DATA:
                case Constant.INC_DATA:
                    break;
                case Constant.UPDATE_1BYTE: //add + data
                case Constant.SUB_BYTE:
                    temp[1] = buffer[2];
                    break;
                case Constant.INC_2DATA: //add + add
                    temp[1] = buffer[3];
                    if (!GetAbsAdd(buffer[2], ref cmdTemp[1]))
                        return false;
                    break;
                case Constant.COPY_DATA: // add + add + data
                case Constant.ADD_1BYTE:
                    temp[1] = buffer[3];
                    if (!GetAbsAdd(buffer[2], ref temp[1]))
                        return false;
                    temp[2] = buffer[4];
                    break;
                case Constant.UPDATE_2BYTE: // add + data + add + data
                case Constant.ADD_2BYTE:
                    temp[1] = buffer[2];
                    temp[2] = buffer[4];
                    if (!GetAbsAdd(buffer[3], ref temp[2]))
                        return false;
                    temp[3] = buffer[5];
                    break;
                case Constant.ADD_DATA: // add + add + add
                case Constant.SUB_DATA:
                    temp[1] = buffer[3];
                    if (!GetAbsAdd(buffer[2], ref temp[1]))
                        return false;
                    temp[2] = buffer[5];
                    if (!GetAbsAdd(buffer[4], ref temp[2]))
                        return false;
                    break;
                default:
                    lastError = Constant.UNDEFINE_CMD;
                    return false;
            }

            return SendCmdToBox((byte)(type - type % 4), temp, 0, type % 4 + 1);
        }

        public bool CommboxDelay(Core.Timer time)
        {
            byte[] timeBuff = new byte[2];
            byte delayWord = Constant.DELAYSHORT;
            double microTime = time.Microseconds;
            microTime = microTime / (commboxInfo.CommboxTimeUnit / 1000000.0);
            if (microTime == 0)
            {
                lastError = Constant.SETTIME_ERROR;
                return false;
            }

            if (microTime > 65535)
            {
                microTime = microTime / commboxInfo.TimeBaseDB;
                if (microTime > 65535)
                {
                    microTime = (microTime * commboxInfo.TimeBaseDB) / commboxInfo.TimeExternDB;
                    if (microTime > 65535)
                    {
                        lastError = Constant.COMMTIME_OUT;
                        return false;
                    }
                    delayWord = Constant.DELAYLONG;
                }
                else
                {
                    delayWord = Constant.DELAYTIME;
                }
            }

            timeBuff[0] = (byte)(microTime / 256);
            timeBuff[1] = (byte)(microTime % 256);

            if (timeBuff[0] == 0)
            {
                if (isDoNow)
                {
                    return CommboxDo(delayWord, timeBuff, 1, 1);
                }
                else
                {
                    return AddToBuff(delayWord, timeBuff, 1, 1);
                }
            }
            else
            {
                if (isDoNow)
                {
                    return CommboxDo(delayWord, timeBuff, 0, 2);
                }
                else
                {
                    return AddToBuff(delayWord, timeBuff, 0, 2);
                }
            }
        }

        public bool SendOutData(byte[] buffer, int offset, int length)
        {
            if (length == 0 || buffer == null)
            {
                lastError = Constant.ILLIGICAL_LEN;
                return false;
            }
            if (isDoNow)
            {
                return CommboxDo(Constant.SEND_DATA, buffer, offset, length);
            }
            else
            {
                return AddToBuff(Constant.SEND_DATA, buffer, offset, length);
            }
        }

        public bool StopNow(bool isStopExecute)
        {
            if (isStopExecute)
            {
                byte[] receiveBuffer = new byte[1];
                int times = Constant.REPLAYTIMES;
                while (times-- != 0)
                {
                    if (!CommboxDo(Constant.STOP_EXECUTE, null, 0, 0))
                        return false;
                    else
                    {
                        Stream.ReadTimeout = Core.Timer.FromMilliseconds(600);
                        if ((Stream.Read(receiveBuffer, 0, 1) != 1) || receiveBuffer[0] != Constant.RUN_ERR)
                        {
                            lastError = Constant.TIMEOUT_ERROR;
                            return false;
                        }
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return CommboxDo(Constant.STOP_REC, null, 0, 0);
            }
        }

        public bool RunBatch(byte[] buffID, int length, bool repeat)
        {
            if (length > buffID.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            for (int i = 0; i < length; i++)
            {
                if (cmdBuffInfo.CmdBuffAdd[buffID[i]] == Constant.NULLADD)
                {
                    lastError = Constant.NOUSED_BUFF;
                    return false;
                }
                buffID[i] = cmdBuffInfo.CmdBuffAdd[buffID[i]];
            }
            byte commandWord = Constant.D0_BAT;
            if (repeat)
                commandWord = Constant.D0_BAT_FOR;
            if (commandWord == Constant.D0_BAT && buffID[0] == cmdBuffInfo.CmdBuffUsed[0])
            {
                length = 0;
                commandWord = Constant.DO_BAT_00;
            }
            return CommboxDo(commandWord, buffID, 0, length);
        }

        private bool SetRF(byte cmd, byte cmdInfo)
        {
            int times = Constant.REPLAYTIMES;
            cmdInfo = (byte)(cmd + cmdInfo);
            if (cmd == Constant.SETRFBAUD)
                times = 2;
            Thread.Sleep(6);
            while (times-- != 0)
            {
                if (CheckIdle() && Stream.Write(new byte[] { cmdInfo }, 0, 1) == 1)
                {
                    if (!SendOk(Core.Timer.FromMilliseconds(50)))
                        continue;
                    if (Stream.Write(new byte[] { cmdInfo }, 0, 1) != 1 || !CheckResult(Core.Timer.FromMilliseconds(150)))
                        continue;
                    Thread.Sleep(100);
                    return true;
                }
            }
            return false;
        }

        public int ReadBytes(byte[] buffer, int offset, int length)
        {
            return ReadData(buffer, offset, length, resWaitTime);
        }

        public ConnectorType Connector
        {
            get { return connector; }
            set { connector = value; }
        }
        
        public IProtocol CreateProtocol(ProtocolType type)
        {
            switch (type)
            {
                case ProtocolType.MIKUNI:
                    return new V1.Mikuni(this);
                case ProtocolType.ISO14230:
                    return new V1.KWP2000(this);
                case ProtocolType.ISO9141_2:
                    return new V1.ISO9141(this);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}