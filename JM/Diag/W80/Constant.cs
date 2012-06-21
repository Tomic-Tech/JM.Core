using System;

namespace JM.Diag.W80
{
    internal static class Constant
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
    }
}