using System;

namespace JM.Diag.W80
{
    internal class Commbox<T> : JM.Diag.Commbox<T>, ICommbox, V1.ICommbox where T : SerialPortStream
    {
        private byte buffID;
        private byte lastError;

        public byte LastError
        {
            get
            {
                return lastError;
            }
        }

        private ushort boxVer;
        private Box box;
        private int startpos;
        private Core.Timer reqByteToByte;
        private Core.Timer reqWaitTime;
        private Core.Timer resByteToByte;
        private Core.Timer resWaitTime;
        private ConnectorType connector;

        public Commbox(T stream)
            : base(stream)
        {
            lastError = 0;
            boxVer = 0;
            box = new Box();
            startpos = 0;
            reqByteToByte = new Core.Timer();
            reqWaitTime = new Core.Timer();
            reqByteToByte = new Core.Timer();
            resWaitTime = new Core.Timer();
        }

        public ushort BoxVer
        {
            get { return boxVer; }
            private set { boxVer = value; }
        }

        public byte BuffID
        {
            get { return buffID; }
            set { buffID = value; }
        }

        public bool CheckIdle()
        {
            int avail = Stream.BytesToRead;
            if (avail > 20)
            {
                Stream.Clear();
                return true;
            }

            byte[] buffer = new byte[1];
            buffer[0] = Constant.READY;
            Stream.ReadTimeout = Core.Timer.FromMilliseconds(200);
            while (Stream.BytesToRead != 0)
                Stream.Read(buffer, 0, 1);
            if (buffer[0] == Constant.READY || buffer[0] == Constant.ERROR)
                return true;
            return false;
        }

        private bool CheckSend()
        {
            Stream.ReadTimeout = Core.Timer.FromMilliseconds(200);
            byte[] buffer = new byte[1];
            buffer[0] = 0;
            if (Stream.Read(buffer, 0, 1) != 1)
                return false;
            if (buffer[0] == Constant.RECV_OK)
                return true;
            return false;
        }

        public bool CheckResult(Core.Timer time)
        {
            Stream.ReadTimeout = time;
            byte[] rb = new byte[1];
            rb[0] = 0;
            if (Stream.Read(rb, 0, 1) != 1)
            {
                return false;
            }
            if (rb[0] == Constant.READY || rb[0] == Constant.ERROR)
            {
                Stream.Clear();
                return true;
            }
            return false;
        }

        private bool SendCmd(byte cmd)
        {
            return SendCmd(cmd, null, 0, 0);
        }

        private bool SendCmd(byte cmd, byte[] buffer, int offset, int length)
        {
            byte cs = cmd;
            byte[] data = new byte[length + 2];

            data[0] = (byte)(cmd + box.RunFlag);
            if (buffer != null)
            {
                for (int i = 0; i < length; i++)
                {
                    cs += buffer[offset + i];
                }
                Array.Copy(buffer, offset, data, 1, length);
            }
            data[data.Length - 1] = cs;
            for (int i = 0; i < 3; i++)
            {
                if (!CheckIdle())
                    continue;
                if (Stream.Write(data, 0, data.Length) != data.Length)
                    continue;
                if (CheckSend())
                    return true;
            }
            return false;
        }

        public int ReadData(byte[] buffer, int offset, int length, Core.Timer time)
        {
            Stream.ReadTimeout = time;
            int len = Stream.Read(buffer, offset, length);
            if (len < length)
            {
                int avail = Stream.BytesToRead;
                if (avail > 0)
                {
                    if (avail <= (length - len))
                    {
                        len += Stream.Read(buffer, offset + len, avail);
                    }
                    else
                    {
                        len += Stream.Read(buffer, offset + len, length - len);
                    }
                }
            }
            return len;
        }

        private int RecvBytes(byte[] buffer, int offset, int length)
        {
            return ReadData(
                buffer,
                offset,
                length,
                Core.Timer.FromMilliseconds(500)
            );
        }

        public int ReadBytes(byte[] buffer, int offset, int length)
        {
            return ReadData(buffer, offset, length, resWaitTime);
        }

        private bool GetCmdData(byte[] buffer, int offset, int maxlen)
        {
            byte[] len = new byte[1];
            len[0] = 0;
            if (RecvBytes(buffer, 0, 1) != 1)
                return false;
            if (RecvBytes(len, 0, 1) != 1)
                return false;
            if (len[0] > maxlen)
                len[0] = Convert.ToByte(maxlen);
            if (RecvBytes(buffer, 0, len[0]) != len[0])
                return false;
            byte[] cs = new byte[1];
            cs[0] = 0;
            if (RecvBytes(cs, 0, 1) != 1)
                return false;
            return len[0] > 0;
        }

        private bool DoCmd(byte cmd)
        {
            return DoCmd(cmd, null, 0, 0);
        }

        private bool DoCmd(byte cmd, byte[] buffer, int offset, int length)
        {
            byte[] tmp = null;
            startpos = 0;
            if (cmd != Constant.WR_DATA && cmd != Constant.SEND_DATA)
                cmd |= (byte)length;  //加上长度位
            if (box.IsDoNow)
            {
                //发送到BOX执行
                switch (cmd)
                {
                    case Constant.WR_DATA:
                        if (length == 0)
                            return false;
                        tmp = new byte[2 + length];
                        if (box.IsLink)
                            tmp[0] = 0xFF; //写链路保持
                        else
                            tmp[0] = 0x00; //写通讯命令
                        tmp[1] = (byte)length;
                        Array.Copy(buffer, offset, tmp, 2, length);
                        return SendCmd(Constant.WR_DATA, tmp, 0, tmp.Length);

                    case Constant.SEND_DATA:
                        if (length == 0)
                            return false;
                        tmp = new byte[4 + length];
                        tmp[0] = 0; //写入位置
                        tmp[1] = (byte)(length + 2); //数据包长度
                        tmp[2] = Constant.SEND_DATA; //命令
                        tmp[3] = (byte)(length - 1); //命令长度-1
                        Array.Copy(buffer, offset, tmp, 4, length);
                        if (!SendCmd(Constant.WR_DATA, tmp, 0, tmp.Length))
                            return false;

                        return SendCmd(Constant.DO_BAT_C);
                    default:
                        return SendCmd(cmd, buffer, offset, length);
                }
            }
            else
            {
                //写命令到缓冲区
                box.Buf[box.Pos++] = cmd;
                if (cmd == Constant.SEND_DATA)
                    box.Buf[box.Pos++] = (byte)(length - 1);
                startpos = box.Pos;
                if (length > 0)
                {
                    Array.Copy(buffer, offset, box.Buf, box.Pos, length);
                    box.Pos += length;
                }
                return true;
            }
        }

        private bool DoSet(byte cmd)
        {
            return DoSet(cmd, null, 0, 0);
        }

        private bool DoSet(byte cmd, byte[] buffer, int offset, int length)
        {
            bool result = DoCmd(cmd, buffer, offset, length);
            if (result && box.IsDoNow)
                result = CheckResult(Core.Timer.FromMilliseconds(150));
            return result;
        }

        private bool GetBuffData(byte addr, byte[] buffer, int offset, int length)
        {
            //Addr相对AUTOBUFF_0的位置
            byte[] tmp = new byte[2];
            tmp[0] = addr;
            tmp[1] = (byte)length;
            if (!DoCmd(Constant.GET_BUF, tmp, 0, 2))
                return false;
            return GetCmdData(buffer, offset, length);
        }

        private bool InitBox()
        {
            const int PASS_LEN = 10;
            byte[] password = new byte[10];
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

            box.IsDoNow = true;
            box.RunFlag = 0;

            byte[] buf = new byte[32];
            Random rand = new Random();

            int i;
            for (i = 1; i < 4; i++)
                buf[i] = (byte)rand.Next();

            byte run = 0;
            for (i = 0; i < PASS_LEN; i++)
                run += (byte)(password[i] ^ buf[i % 3 + 1]);
            if (run == 0)
                run = 0x55;

            if (!DoCmd(Constant.GET_CPU, buf, 1, 3))
                return false;

            if (!GetCmdData(buf, 0, 32))
                return false;

            box.RunFlag = 0;
            box.BoxTimeUnit = 0;

            for (i = 0; i < 3; i++)
                box.BoxTimeUnit = box.BoxTimeUnit * 256 + buf[i];
            box.TimeBaseDB = buf[i++];
            box.TimeExternB = buf[i++];

            for (i = 0; i < Constant.MAXPORT_NUM; i++)
                box.Port[i] = 0xFF;
            box.Pos = 0;
            box.IsDB20 = false;
            return true;
        }

        private bool CheckBox()
        {
            byte[] buff = new byte[32];
            if (!DoCmd(Constant.GET_BOXID))
                return false;
            if (!GetCmdData(buff, 0, 32))
                return false;
            boxVer = (ushort)(buff[10] << 8 | buff[11]);
            return true;
        }

        public bool SetLineLevel(byte valueLow, byte valueHigh)
        {
            //设定端口1
            box.Port[1] &= (byte)(~valueLow);
            box.Port[1] |= valueHigh;
            return DoSet(Constant.SET_PORT1, box.Port, 1, 1);
        }

        public bool SetCommCtrl(byte valueOpen, byte valueClose)
        {
            //设定端口2
            box.Port[2] &= (byte)(~valueOpen);
            box.Port[2] |= valueClose;
            return DoSet(Constant.SET_PORT2, box.Port, 2, 1);
        }

        public bool SetCommLine(byte sendLine, byte recvLine)
        {
            //设定端口0
            if (sendLine > 7)
                sendLine = 0x0F;
            if (recvLine > 7)
                recvLine = 0x0F;
            box.Port[0] = (byte)(sendLine | (recvLine << 4));
            return DoSet(Constant.SET_PORT0, box.Port, 0, 1);
        }

        public bool TurnOverOneByOne()
        {
            //将原有的接受一个发送一个的标志翻转
            return DoSet(Constant.SET_ONEBYONE);
        }

        public bool KeepLink(bool isRunLink)
        {
            return DoSet(isRunLink ? Constant.RUN_LINK : Constant.STOP_LINK);
        }

        public bool SetCommLink(byte ctrlWord1, byte ctrlWord2, byte ctrlWord3)
        {
            byte[] ctrlWord = new byte[3];
            byte modeControl = (byte)(ctrlWord1 & 0xE0);
            int length = 3;
            ctrlWord[0] = ctrlWord1;
            if ((ctrlWord1 & 0x04) != 0)
                box.IsDB20 = true;
            else
                box.IsDB20 = false;
            if (modeControl == Constant.SET_VPW || modeControl == Constant.SET_PWM)
                return DoSet(Constant.SET_CTRL, ctrlWord, 0, 1);
            ctrlWord[1] = ctrlWord2;
            ctrlWord[2] = ctrlWord3;
            if (ctrlWord3 == 0)
            {
                length--;
                if (ctrlWord2 == 0)
                    length--;
            }
            if (modeControl == Constant.EXRS_232 && length < 2)
                return false;
            return DoSet(Constant.SET_CTRL, ctrlWord, 0, length);
        }

        public bool SetCommBaud(double baud)
        {
            byte[] baudTime = new byte[2];
            double instructNum = 1000000000000.0 / (box.BoxTimeUnit * baud);
            if (box.IsDB20)
                instructNum /= 20;
            instructNum += 0.5;
            if (instructNum > 65535 || instructNum < 10)
                return false;
            baudTime[0] = (byte)(instructNum / 256);
            baudTime[1] = (byte)(instructNum % 256);

            if (baudTime[0] == 0)
                return DoSet(Constant.SET_BAUD, baudTime, 1, 1);
            return DoSet(Constant.SET_BAUD, baudTime, 0, 2);
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
                if (type == Constant.SETVPWRECS)
                    microTime = (microTime * 2) / 3;
                type = (byte)(type + (Constant.SETBYTETIME & 0xF0));
                microTime = (ulong)((microTime * 1000000.0) / box.BoxTimeUnit);
            }
            else
            {
                microTime = (ulong)((microTime * 1000000.0) / (box.TimeBaseDB * box.BoxTimeUnit));
            }

            timeBuff[0] = (byte)(microTime / 256);
            timeBuff[1] = (byte)(microTime % 256);

            if (timeBuff[0] == 0)
                return DoSet(type, timeBuff, 1, 1);
            return DoSet(type, timeBuff, 0, 2);
        }

        public bool CommboxDelay(Core.Timer time)
        {
            byte[] timeBuff = new byte[2];
            byte delayWord = Constant.DELAYSHORT;
            ulong microTime = (ulong)(time.Microseconds / (box.BoxTimeUnit / 1000000.0));

            if (microTime == 0)
                return false;
            if (microTime > 65535)
            {
                microTime = microTime / box.TimeBaseDB;
                if (microTime > 65535)
                {
                    microTime = (microTime * box.TimeBaseDB) / box.TimeExternB;
                    if (microTime > 65535)
                        return false;
                    delayWord = Constant.DELAYDWORD;
                }
                else
                {
                    delayWord = Constant.DELAYTIME;
                }
            }

            timeBuff[0] = (byte)(microTime / 256);
            timeBuff[1] = (byte)(microTime % 256);

            if (timeBuff[0] == 0)
                return DoSet(delayWord, timeBuff, 1, 1);
            return DoSet(delayWord, timeBuff, 0, 2);
        }

        public bool SendOutData(byte[] buffer, int offset, int length)
        {
            return DoSet(Constant.SEND_DATA, buffer, offset, length);
        }

        public bool RunReceive(byte type)
        {
            if (type == Constant.GET_PORT1)
                box.IsDB20 = false;
            return DoCmd(type);
        }

        public bool StopNow(bool isStopExecute)
        {
            byte cmd = isStopExecute ? Constant.STOP_EXECUTE : Constant.STOP_REC;
            for (int i = 0; i < 3; i++)
            {
                if (Stream.Write(new byte[] { cmd }, 0, 1) != 1)
                    continue;
                if (CheckSend())
                {
                    if (isStopExecute && !CheckResult(Core.Timer.FromMilliseconds(200)))
                        continue;
                    return true;
                }
            }
            return false;
        }

        public bool Open()
        {
            lastError = Constant.DISCONNECT_COMM;
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();

            foreach (string portName in ports)
            {
                try
                {
                    Stream.SerialPort.Close();
                    Stream.SerialPort.BaudRate = 115200;
                    Stream.SerialPort.StopBits = System.IO.Ports.StopBits.Two;
                    Stream.SerialPort.Parity = System.IO.Ports.Parity.None;
                    Stream.SerialPort.Handshake = System.IO.Ports.Handshake.None;
                    Stream.SerialPort.DataBits = 8;
                    Stream.SerialPort.PortName = portName;
                    Stream.SerialPort.Open();
                    Stream.Reset();
                    if (InitBox() && CheckBox())
                    {
                        Stream.Clear();
                        return true;
                    }
                    Stream.SerialPort.Close();
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
            return false;
        }

        public bool Close()
        {
            try
            {
                if (Stream.SerialPort.IsOpen)
                {
                    Reset();
                    Stream.SerialPort.DtrEnable = false;
                    Stream.SerialPort.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool UpdateBuff(byte type, byte addr, byte data)
        {
            byte[] buf = new byte[3];
            int len = 0;
            buf[0] = addr;
            buf[1] = data;
            switch (type)
            {
                case Constant.INC_BYTE:
                case Constant.DEC_BYTE:
                case Constant.INVERT_BYTE:
                    len = 1;
                    break;
                case Constant.UPDATE_BYTE:
                case Constant.ADD_BYTE:
                case Constant.SUB_BYTE:
                    len = 2;
                    break;
                case Constant.COPY_BYTE:
                    len = 3;
                    break;
            }
            return DoSet(type, buf, 0, len);
        }

        private bool CopyBuff(byte dest, byte src, byte len)
        {
            byte[] buf = new byte[3];
            buf[0] = dest;
            buf[1] = src;
            buf[2] = len;
            return DoSet(Constant.COPY_BYTE, buf, 0, 3);
        }

        public bool NewBatch(byte buffID)
        {
            box.Pos = 0;
            box.IsLink = (buffID == Constant.LINKBLOCK ? true : false);
            box.IsDoNow = false;
            return true;
        }

        public bool EndBatch()
        {
            int i = 0;
            box.IsDoNow = true;
            box.Buf[box.Pos++] = 0;  //命令块以0x00标记结束
            if (box.IsLink)
            {
                //修改UpdateBuff使用到的地址
                while (box.Buf[i] != 0)
                {
                    switch (box.Buf[i] & 0xFC)
                    {
                        case Constant.COPY_BYTE:
                            box.Buf[i + 3] += (byte)(Constant.MAXBUFF_LEN - box.Pos);
                            box.Buf[i + 2] += (byte)(Constant.MAXBUFF_LEN - box.Pos);
                            box.Buf[i + 1] += (byte)(Constant.MAXBUFF_LEN - box.Pos);
                            break;
                        case Constant.SUB_BYTE:
                            box.Buf[i + 2] += (byte)(Constant.MAXBUFF_LEN - box.Pos);
                            box.Buf[i + 1] += (byte)(Constant.MAXBUFF_LEN - box.Pos);
                            break;
                        case Constant.UPDATE_BYTE:
                        case Constant.INVERT_BYTE:
                        case Constant.ADD_BYTE:
                        case Constant.DEC_BYTE:
                        case Constant.INC_BYTE:
                            box.Buf[i + 1] += (byte)(Constant.MAXBUFF_LEN - box.Pos);
                            break;
                    }

                    if (box.Buf[i] == Constant.SEND_DATA)
                        i += (1 + (box.Buf[i + 1] + 1) + 1);
                    else if (box.Buf[i] >= Constant.REC_LEN_1 && box.Buf[i] <= Constant.REC_LEN_15)
                        i += 1; //特殊
                    else
                        i += (box.Buf[i] & 0x03) + 1;
                }
            }

            return DoCmd(Constant.WR_DATA, box.Buf, 0, box.Pos);
        }

        public bool DelBatch(byte buffID)
        {
            box.IsDoNow = true;
            box.Pos = 0;
            return true;
        }

        public bool RunBatch(byte[] buffID, int length, bool isRunMore)
        {
            byte cmd;
            if (buffID[0] == Constant.LINKBLOCK)
                cmd = isRunMore ? Constant.DO_BAT_LN : Constant.DO_BAT_L;
            else
                cmd = isRunMore ? Constant.DO_BAT_CN : Constant.DO_BAT_C;
            return DoCmd(cmd);
        }

        private bool Reset()
        {
            StopNow(true);
            Stream.Clear();
            for (int i = 0; i < Constant.MAXPORT_NUM; i++)
                box.Port[i] = 0xFF;
            return DoCmd(Constant.RESET);
        }

        public ConnectorType Connector
        {
            get { return connector; }
            set { connector = value; }
        }

        public bool SetConnector(ConnectorType cnn)
        {
            Connector = cnn;
            return true;
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