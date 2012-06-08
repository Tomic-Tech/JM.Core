using System;

namespace JM.Diag.W80
{
    internal class Commbox : ICommbox, V1.ICommbox
    {
        public const int BOXINFO_LEN = 12;
        public const int MAXPORT_NUM = 4;
        public const int MAXBUFF_NUM = 4;
        public const int MAXBUFF_LEN = 0xA8;
        public const int LINKBLOCK = 0x40;

        //批处理执行次数
        public const byte RUN_ONCE = 0x00;
        public const byte RUN_MORE = 0x01;

        //通讯校验和方式
        public const byte CHECK_SUM = 0x01;
        public const byte CHECK_REVSUM = 0x02;
        public const byte CHECK_CRC = 0x03;

        ///////////////////////////////////////////////////////////////////////////////
        //  通讯口 PORT
        ///////////////////////////////////////////////////////////////////////////////
        public const byte DH = 0x80; //高电平输出,1为关闭,0为打开
        public const byte DL2 = 0x40; //低电平输出,1为关闭,0为打开,正逻辑发送通讯线
        public const byte DL1 = 0x20; //低电平输出,1为关闭,0为打开,正逻辑发送通讯线,带接受控制
        public const byte DL0 = 0x10; //低电平输出,1为关闭,0为打开,正逻辑发送通讯线,带接受控制
        public const byte PWMS = 0x08; //PWM发送线
        public const byte PWMR = 0x04;
        public const byte COMS = 0x02; //标准发送通讯线路
        public const byte COMR = 0x01;
        public const byte SET_NULL = 0x00; //不选择任何设置

        ///////////////////////////////////////////////////////////////////////////////
        //  通讯物理控制口
        ///////////////////////////////////////////////////////////////////////////////
        public const byte PWC = 0x80; //通讯电平控制,1为5伏,0为12伏
        public const byte REFC = 0x40; //通讯比较电平控制,1为通讯电平1/5,0为比较电平控制1/2
        public const byte CK = 0x20; //K线控制开关,1为双线通讯,0为单线通讯
        public const byte SZFC = 0x10; //发送逻辑控制,1为负逻辑,0为正逻辑
        public const byte RZFC = 0x08; //接受逻辑控制,1为负逻辑,0为正逻辑
        public const byte DLC0 = 0x04; //DLC1接受控制,1为接受关闭,0为接受打开
        public const byte DLC1 = 0x02; //DLC0接受控制,1为接受关闭,0为接受打开
        public const byte SLC = 0x01; //线选地址锁存器控制线(待用)
        public const byte CLOSEALL = 0x08; //关闭所有发送口线，和接受口线

        ///////////////////////////////////////////////////////////////////////////////
        //  通讯控制字1设定
        ///////////////////////////////////////////////////////////////////////////////
        public const byte RS_232 = 0x00;
        public const byte EXRS_232 = 0x20;
        public const byte SET_VPW = 0x40;
        public const byte SET_PWM = 0x60;
        public const byte BIT9_SPACE = 0x00;
        public const byte BIT9_MARK = 0x01;
        public const byte BIT9_EVEN = 0x02;
        public const byte BIT9_ODD = 0x03;
        public const byte SEL_SL = 0x00;
        public const byte SEL_DL0 = 0x08;
        public const byte SEL_DL1 = 0x10;
        public const byte SEL_DL2 = 0x18;
        public const byte SET_DB20 = 0x04;
        public const byte UN_DB20 = 0x00;

        ///////////////////////////////////////////////////////////////////////////////
        //  通讯控制字3设定
        ///////////////////////////////////////////////////////////////////////////////
        public const byte ONEBYONE = 0x80;
        public const byte INVERTBYTE = 0x40;
        public const byte ORIGNALBYTE = 0x00;

        ///////////////////////////////////////////////////////////////////////////////
        //  接受命令类型定义
        ///////////////////////////////////////////////////////////////////////////////
        public const byte WR_DATA = 0x00;
        public const byte WR_LINK = 0xFF;
        public const byte STOP_REC = 0x04;
        public const byte STOP_EXECUTE = 0x08;
        public const byte SET_UPBAUD = 0x0C;
        public const byte UP_9600BPS = 0x00;
        public const byte UP_19200BPS = 0x01;
        public const byte UP_38400BPS = 0x02;
        public const byte UP_57600BPS = 0x03;
        public const byte UP_115200BPS = 0x04;
        public const byte RESET = 0x10;
        public const byte GET_CPU = 0x14;
        public const byte GET_TIME = 0x18;
        public const byte GET_SET = 0x1C;
        public const byte GET_LINK = 0x20;
        public const byte GET_BUF = 0x24;
        public const byte GET_CMD = 0x28;
        public const byte GET_PORT = 0x2C;
        public const byte GET_BOXID = 0x30;

        public const byte DO_BAT_C = 0x34;
        public const byte DO_BAT_CN = 0x38;
        public const byte DO_BAT_L = 0x3C;
        public const byte DO_BAT_LN = 0x40;

        public const byte SET55_BAUD = 0x44;
        public const byte SET_ONEBYONE = 0x48;
        public const byte SET_BAUD = 0x4C;
        public const byte RUN_LINK = 0x50;
        public const byte STOP_LINK = 0x54;
        public const byte CLEAR_LINK = 0x58;
        public const byte GET_PORT1 = 0x5C;

        public const byte SEND_DATA = 0x60;
        public const byte SET_CTRL = 0x64;
        public const byte SET_PORT0 = 0x68;
        public const byte SET_PORT1 = 0x6C;
        public const byte SET_PORT2 = 0x70;
        public const byte SET_PORT3 = 0x74;
        public const byte DELAYSHORT = 0x78;
        public const byte DELAYTIME = 0x7C;
        public const byte DELAYDWORD = 0x80;

        public const byte SETBYTETIME = 0x88;
        public const byte SETVPWSTART = 0x08; //最终要将SETVPWSTART转换成SETBYTETIME
        public const byte SETWAITTIME = 0x8C;
        public const byte SETLINKTIME = 0x90;
        public const byte SETRECBBOUT = 0x94;
        public const byte SETRECFROUT = 0x98;
        public const byte SETVPWRECS = 0x14; //最终要将SETVPWRECS转换成SETRECBBOUT

        public const byte COPY_BYTE = 0x9C;
        public const byte UPDATE_BYTE = 0xA0;
        public const byte INC_BYTE = 0xA4;
        public const byte DEC_BYTE = 0xA8;
        public const byte ADD_BYTE = 0xAC;
        public const byte SUB_BYTE = 0xB0;
        public const byte INVERT_BYTE = 0xB4;

        public const byte REC_FR = 0xE0;
        public const byte REC_LEN_1 = 0xE1;
        public const byte REC_LEN_2 = 0xE2;
        public const byte REC_LEN_3 = 0xE3;
        public const byte REC_LEN_4 = 0xE4;
        public const byte REC_LEN_5 = 0xE5;
        public const byte REC_LEN_6 = 0xE6;
        public const byte REC_LEN_7 = 0xE7;
        public const byte REC_LEN_8 = 0xE8;
        public const byte REC_LEN_9 = 0xE9;
        public const byte REC_LEN_10 = 0xEA;
        public const byte REC_LEN_11 = 0xEB;
        public const byte REC_LEN_12 = 0xEC;
        public const byte REC_LEN_13 = 0xED;
        public const byte REC_LEN_14 = 0xEE;
        public const byte REC_LEN_15 = 0xEF;
        public const byte RECEIVE = 0xF0;

        public const byte RECV_ERR = 0xAA; //接收错误
        public const byte RECV_OK = 0x55; //接收正确
        public const byte BUSY = 0xBB; //开始执行
        public const byte READY = 0xDD; //执行结束
        public const byte ERROR = 0xEE; //执行错误

        //RF多对一的设定接口,最多16个
        public const byte RF_RESET = 0xD0;
        public const byte RF_SETDTR_L = 0xD1;
        public const byte RF_SETDTR_H = 0xD2;
        public const byte RF_SET_BAUD = 0xD3;
        public const byte RF_SET_ADDR = 0xD8;

        public const byte COMMBOXID_ERR = 1;
        public const byte DISCONNECT_COMM = 2;
        public const byte DISCONNECT_COMMBOX = 3;
        public const byte OTHER_ERROR = 4;

        // 錯誤標識
        public const byte ERR_OPEN = 0x01; //OpenComm() 失敗
        public const byte ERR_CHECK = 0x02; //CheckEcm() 失敗

        //接頭標識定義
        public const byte OBDII_16 = 0x00;
        public const byte UNIVERSAL_3 = 0x01;
        public const byte BENZ_38 = 0x02;
        public const byte BMW_20 = 0x03;
        public const byte AUDI_4 = 0x04;
        public const byte FIAT_3 = 0x05;
        public const byte CITROEN_2 = 0x06;
        public const byte CHRYSLER_6 = 0x07;
        public const byte TOYOTA_17R = 0x20;
        public const byte TOYOTA_17F = 0x21;
        public const byte HONDA_3 = 0x22;
        public const byte MITSUBISHI = 0x23;
        public const byte HYUNDAI = 0x23;
        public const byte NISSAN = 0x24;
        public const byte SUZUKI_3 = 0x25;
        public const byte DAIHATSU_4 = 0x26;
        public const byte ISUZU_3 = 0x27;
        public const byte CANBUS_16 = 0x28;
        public const byte GM_12 = 0x29;
        public const byte KIA_20 = 0x30;

        //常量定義
        public const int TRYTIMES = 3;

        //通訊通道定義
        public const byte SK0 = 0;
        public const byte SK1 = 1;
        public const byte SK2 = 2;
        public const byte SK3 = 3;
        public const byte SK4 = 4;
        public const byte SK5 = 5;
        public const byte SK6 = 6;
        public const byte SK7 = 7;
        public const byte SK_NO = 0xFF;
        public const byte RK0 = 0;
        public const byte RK1 = 1;
        public const byte RK2 = 2;
        public const byte RK3 = 3;
        public const byte RK4 = 4;
        public const byte RK5 = 5;
        public const byte RK6 = 6;
        public const byte RK7 = 7;
        public const byte RK_NO = 0xFF;

        //協議常量標誌定義
        public const byte NO_PACK = 0x80; //發送的命令不需要打包
        public const byte UN_PACK = 0x08; //接收到的數據解包處理
        public const byte MFR_1 = 0x00;
        public const byte MFR_2 = 0x02;
        public const byte MFR_3 = 0x03;
        public const byte MFR_4 = 0x04;
        public const byte MFR_5 = 0x05;
        public const byte MFR_6 = 0x06;
        public const byte MFR_7 = 0x07;
        public const byte MFR_N = 0x01;

        private byte buffID;
        private byte lastError;
        private ushort boxVer;
        private Box box;
        private BoxStream bs;
        private VirtualStream vs;
        private int startpos;
        private Core.Timer reqByteToByte;
        private Core.Timer reqWaitTime;
        private Core.Timer resByteToByte;
        private Core.Timer resWaitTime;
        private ConnectorType connector;

        public Commbox()
        {
            lastError = 0;
            boxVer = 0;
            box = new Box();
            bs = BoxStream.Instance;
            vs = bs.VirtualStream;
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
            int avail = vs.BytesToRead;
            if (avail > 20)
            {
                vs.DiscardInBuffer();
                return true;
            }

            byte[] buffer = new byte[1];
            buffer[0] = READY;
            vs.ReadTimeout = TimeSpan.FromMilliseconds(200);
            while (vs.Read(buffer, 0, 1) == 1) ;
            if (buffer[0] == READY || buffer[0] == ERROR)
                return true;
            return false;
        }

        private bool CheckSend()
        {
            vs.ReadTimeout = TimeSpan.FromMilliseconds(200);
            byte[] buffer = new byte[1];
            buffer[0] = 0;
            if (vs.Read(buffer, 0, 1) != 1)
                return false;
            if (buffer[0] == RECV_OK)
                return true;
            return false;
        }

        public bool CheckResult(Core.Timer time)
        {
            vs.ReadTimeout = time.TimeSpan;
            byte[] rb = new byte[1];
            rb[0] = 0;
            if (vs.Read(rb, 0, 1) != 1)
            {
                return false;
            }
            if (rb[0] == READY || rb[0] == ERROR)
            {
                vs.DiscardInBuffer();
                return true;
            }
            return false;
        }

        private bool SendCmd(byte cmd)
        {
            return SendCmd(cmd, null, 0, 0, null, 0, 0);
        }

        private bool SendCmd(byte cmd, byte[] buffer, int offset, int length)
        {
            return SendCmd(cmd, buffer, offset, length, null, 0, 0);
        }

        private bool SendCmd(byte cmd, byte[] buffer1, int offset1, int length1, byte[] buffer2, int offset2, int length2)
        {
            cmd += box.RunFlag;
            byte cs = 0;
            for (int i = 0; i < length1; i++)
            {
                cs += buffer1[offset1 + i];
            }
            for (int i = 0; i < length2; i++)
            {
                cs += buffer2[offset2 + i];
            }

            for (int i = 0; i < 3; i++)
            {
                if (!CheckIdle())
                    continue;
                if (vs.Write(new byte[] { cmd }, 0, 1) != 1)
                    continue;
                if (vs.Write(buffer1, offset1, length1) != length1)
                    continue;
                if (vs.Write(buffer2, offset2, length2) != length2)
                    continue;
                if (vs.Write(new byte[] { cs }, 0, 1) != 1)
                    continue;
                if (CheckSend())
                    return true;
            }
            return false;
        }

        public int ReadData(byte[] buffer, int offset, int length, Core.Timer time)
        {
            vs.ReadTimeout = time.TimeSpan;
            int len = vs.Read(buffer, offset, length);
            if (len < length)
            {
                int avail = vs.BytesToRead;
                if (avail > 0)
                {
                    if (avail <= (length - len))
                    {
                        len += vs.Read(buffer, offset + len, avail);
                    }
                    else
                    {
                        len += vs.Read(buffer, offset + len, length - len);
                    }
                }
            }
            return len;
        }

        private int RecvBytes(byte[] buffer, int offset, int length)
        {
            return ReadData(buffer, offset, length, Core.Timer.FromMilliseconds(500));
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
            byte[] tmp = new byte[4];
            startpos = 0;
            if (cmd != WR_DATA && cmd != SEND_DATA)
                cmd |= (byte)length;  //加上长度位
            if (box.IsDoNow)
            {
                //发送到BOX执行
                switch (cmd)
                {
                    case WR_DATA:
                        if (length == 0)
                            return false;
                        if (box.IsLink)
                            tmp[0] = 0xFF; //写链路保持
                        else
                            tmp[0] = 0x00; //写通讯命令
                        tmp[1] = (byte)length;
                        return SendCmd(WR_DATA, tmp, 0, 2, buffer, offset, length);

                    case SEND_DATA:
                        if (length == 0)
                            return false;
                        tmp[0] = 0; //写入位置
                        tmp[1] = (byte)(length + 2); //数据包长度
                        tmp[2] = SEND_DATA; //命令
                        tmp[3] = (byte)(length - 1); //命令长度-1
                        if (!SendCmd(WR_DATA, tmp, 0, 4, buffer, offset, length))
                            return false;

                        return SendCmd(DO_BAT_C);
                    default:
                        return SendCmd(cmd, buffer, offset, length);
                }
            }
            else
            {
                //写命令到缓冲区
                box.Buf[box.Pos++] = cmd;
                if (cmd == SEND_DATA)
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
            if (!DoCmd(GET_BUF, tmp, 0, 2))
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

            if (!DoCmd(GET_CPU, buf, 1, 3))
                return false;

            if (!GetCmdData(buf, 0, 32))
                return false;

            box.RunFlag = 0;
            box.BoxTimeUnit = 0;

            for (i = 0; i < 3; i++)
                box.BoxTimeUnit = box.BoxTimeUnit * 256 + buf[i];
            box.TimeBaseDB = buf[i++];
            box.TimeExternB = buf[i++];

            for (i = 0; i < MAXPORT_NUM; i++)
                box.Port[i] = 0xFF;
            box.Pos = 0;
            box.IsDB20 = false;
            return true;
        }

        private bool CheckBox()
        {
            byte[] buff = new byte[32];
            if (!DoCmd(GET_BOXID))
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
            return DoSet(SET_PORT1, box.Port, 1, 1);
        }

        public bool SetCommCtrl(byte valueOpen, byte valueClose)
        {
            //设定端口2
            box.Port[2] &= (byte)(~valueOpen);
            box.Port[2] |= valueClose;
            return DoSet(SET_PORT2, box.Port, 2, 1);
        }

        public bool SetCommLine(byte sendLine, byte recvLine)
        {
            //设定端口0
            if (sendLine > 7)
                sendLine = 0x0F;
            if (recvLine > 7)
                recvLine = 0x0F;
            box.Port[0] = (byte)(sendLine | (recvLine << 4));
            return DoSet(SET_PORT0, box.Port, 0, 1);
        }

        public bool TurnOverOneByOne()
        {
            //将原有的接受一个发送一个的标志翻转
            return DoSet(SET_ONEBYONE);
        }

        public bool KeepLink(bool isRunLink)
        {
            return DoSet(isRunLink ? RUN_LINK : STOP_LINK);
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
            if (modeControl == SET_VPW || modeControl == SET_PWM)
                return DoSet(SET_CTRL, ctrlWord, 0, 1);
            ctrlWord[1] = ctrlWord2;
            ctrlWord[2] = ctrlWord3;
            if (ctrlWord3 == 0)
            {
                length--;
                if (ctrlWord2 == 0)
                    length--;
            }
            if (modeControl == EXRS_232 && length < 2)
                return false;
            return DoSet(SET_CTRL, ctrlWord, 0, length);
        }

        public bool SetCommBaud(double baud)
        {
            byte[] baudTime = new byte[2];
            double instructNum = ((1000000.0 / (box.BoxTimeUnit)) * 1000000) / baud;
            if (box.IsDB20)
                instructNum /= 20;
            instructNum += 0.5;
            if (instructNum > 65535 || instructNum < 10)
                return false;
            baudTime[0] = (byte)(instructNum / 256);
            baudTime[1] = (byte)(instructNum % 256);

            if (baudTime[0] == 0)
                return DoSet(SET_BAUD, baudTime, 1, 1);
            return DoSet(SET_BAUD, baudTime, 0, 2);
        }

        private void GetLinkTime(byte type, Core.Timer time)
        {
            switch (type)
            {
                case SETBYTETIME:
                    reqByteToByte = time;
                    break;
                case SETWAITTIME:
                    reqWaitTime = time;
                    break;
                case SETRECBBOUT:
                    resByteToByte = time;
                    break;
                case SETRECFROUT:
                    resWaitTime = time;
                    break;
            }
        }

        public bool SetCommTime(byte type, Core.Timer time)
        {
            byte[] timeBuff = new byte[2];
            GetLinkTime(type, time);
            ulong microTime = (ulong)time.Microseconds;
            if (type == SETVPWSTART || type == SETVPWRECS)
            {
                if (type == SETVPWRECS)
                    microTime = (microTime * 2) / 3;
                type = (byte)(type + (SETBYTETIME & 0xF0));
                microTime = (ulong)(microTime / (box.BoxTimeUnit / 1000000.0));
            }
            else
            {
                microTime = (ulong)((microTime / box.TimeBaseDB) / (box.BoxTimeUnit / 1000000.0));
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
            byte delayWord = DELAYSHORT;
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
                    delayWord = DELAYDWORD;
                }
                else
                {
                    delayWord = DELAYTIME;
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
            return DoSet(SEND_DATA, buffer, offset, length);
        }

        public bool RunReceive(byte type)
        {
            if (type == GET_PORT1)
                box.IsDB20 = false;
            return DoCmd(type);
        }

        public bool StopNow(bool isStopExecute)
        {
            byte cmd = isStopExecute ? STOP_EXECUTE : STOP_REC;
            for (int i = 0; i < 3; i++)
            {
                if (vs.Write(new byte[] { cmd }, 0, 1) != 1)
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

        public void Open()
        {
            if (bs.Type == StreamType.SerialPort)
            {
                lastError = DISCONNECT_COMM;
                string[] ports = System.IO.Ports.SerialPort.GetPortNames();

                foreach (string portName in ports)
                {
                    try
                    {
                        bs.SerialPort.Close();
                        bs.SerialPort.BaudRate = 115200;
                        bs.SerialPort.StopBits = System.IO.Ports.StopBits.Two;
                        bs.SerialPort.Parity = System.IO.Ports.Parity.None;
                        bs.SerialPort.Handshake = System.IO.Ports.Handshake.None;
                        bs.SerialPort.DataBits = 8;
                        bs.SerialPort.PortName = portName;
                        bs.SerialPort.Open();
                        bs.SerialPort.DtrEnable = true;
                        if (InitBox() && CheckBox())
                        {
                            vs.DiscardInBuffer();
                            return;
                        }
                        bs.SerialPort.Close();
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            throw new System.IO.IOException(Core.SysDB.GetText("Open Commbox Fail!"));
        }

        public void Close()
        {
            try
            {
                Reset();
                bs.SerialPort.Close();
            }
            catch (Exception)
            {
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
                case INC_BYTE:
                case DEC_BYTE:
                case INVERT_BYTE:
                    len = 1;
                    break;
                case UPDATE_BYTE:
                case ADD_BYTE:
                case SUB_BYTE:
                    len = 2;
                    break;
                case COPY_BYTE:
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
            return DoSet(COPY_BYTE, buf, 0, 3);
        }

        public bool NewBatch(byte buffID)
        {
            box.Pos = 0;
            box.IsLink = (buffID == LINKBLOCK ? true : false);
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
                        case COPY_BYTE:
                            box.Buf[i + 3] += (byte)(MAXBUFF_LEN - box.Pos);
                            box.Buf[i + 2] += (byte)(MAXBUFF_LEN - box.Pos);
                            box.Buf[i + 1] += (byte)(MAXBUFF_LEN - box.Pos);
                            break;
                        case SUB_BYTE:
                            box.Buf[i + 2] += (byte)(MAXBUFF_LEN - box.Pos);
                            box.Buf[i + 1] += (byte)(MAXBUFF_LEN - box.Pos);
                            break;
                        case UPDATE_BYTE:
                        case INVERT_BYTE:
                        case ADD_BYTE:
                        case DEC_BYTE:
                        case INC_BYTE:
                            box.Buf[i + 1] += (byte)(MAXBUFF_LEN - box.Pos);
                            break;
                    }

                    if (box.Buf[i] == SEND_DATA)
                        i += (1 + (box.Buf[i + 1] + 1) + 1);
                    else if (box.Buf[i] >= REC_LEN_1 && box.Buf[i] <= REC_LEN_15)
                        i += 1; //特殊
                    else
                        i += (box.Buf[i] & 0x03) + 1;
                }
            }

            return DoCmd(WR_DATA, box.Buf, 0, box.Pos);
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
            if (buffID[0] == LINKBLOCK)
                cmd = isRunMore ? DO_BAT_LN : DO_BAT_L;
            else
                cmd = isRunMore ? DO_BAT_CN : DO_BAT_C;
            return DoCmd(cmd);
        }

        private bool Reset()
        {
            StopNow(true);
            vs.DiscardInBuffer();
            for (int i = 0; i < MAXPORT_NUM; i++)
                box.Port[i] = 0xFF;
            return DoCmd(RESET);
        }

        public ConnectorType Connector
        {
            get { return connector; }
            set { connector = value; }
        }

        public void SetConnector(ConnectorType cnn)
        {
            Connector = cnn;
        }

        public IProtocol CreateProtocol(ProtocolType type)
        {
            switch (type)
            {
                case ProtocolType.MIKUNI:
                    return new V1.Mikuni(this);
                case ProtocolType.ISO14230:
                    return new V1.KWP2000(this);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}