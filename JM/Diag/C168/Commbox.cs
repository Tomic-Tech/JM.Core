using System;
using JM.Core;
using System.IO;
using System.Threading;

namespace JM.Diag.C168
{
    internal class Commbox : ICommbox
    {
        public const int REPLAYTIMES = 3; //错误运行次数
        ///////////////////////////////////////////////////////
        //设置Commbox宏定义区
        ////////////////////////////////////////////////////////
        public const int TIMEVALUE = 1000000; //万分之一秒微妙
        public const int COMMBOXINFOLEN = 18; //共有18个数据需从COMMBOX得到
        public const int VERSIONLEN = 2;
        public const int MINITIMELEN = 3;
        public const int COMMBOXPORTNUM = 4;
        public const int COMMBOXIDLEN = 10;
        public const int MAXIM_BLOCK = 0x40; //命令缓从区的最大数
        public const int LINKBLOCK = MAXIM_BLOCK; //链路保持的命令缓冲区
        ///////////////////////////////////////////////////////
        // CommBox 固定信息 宏定义表
        ///////////////////////////////////////////////////////
        public const byte NULLADD = 0xFF;					//表示此块无使用
        public const byte SWAPBLOCK = MAXIM_BLOCK + 1;		//数据交换区的块表识

        public const int START_BAUD = 57600;   //上位机同下位机通信在复位或上电时波特率为57600
        public const int CMD_DATALEN = 4;		//非发送命令最大长度


        ///////////////////////////////////////////////////////
        /*
        //	P1口为通讯口
        public const byte DH		0x80						//高电平输出,1为关闭,0为打开
        public const byte DL2		0x40						//低电平输出,1为关闭,0为打开,正逻辑发送通讯线
        public const byte DL1		0x20						//低电平输出,1为关闭,0为打开,正逻辑发送通讯线,带接受控制
        public const byte DL0		0x10						//低电平输出,1为关闭,0为打开,正逻辑发送通讯线,带接受控制
        public const byte PWMS	0x08						//PWM发送线
        public const byte COMS	0x02						//标准发送通讯线路
        public const byte SET_NULL	0x00					//不选择任何设置

        //P2口为通讯物理控制口
        public const byte PWC		0x80						//通讯电平控制,1为5伏,0为12伏
        public const byte REFC	0x40						//通讯比较电平控制,1为通讯电平1/5,0为比较电平控制1/2
        public const byte CK		0x20						//K线控制开关,1为双线通讯,0为单线通讯
        public const byte SZFC	0x10						//发送逻辑控制,1为负逻辑,0为正逻辑
        public const byte RZFC	0x08						//接受逻辑控制,1为负逻辑,0为正逻辑
        public const byte DLC1	0x04						//DLC1接受控制,1为接受关闭,0为接受打开
        public const byte DLC0	0x02						//DLC0接受控制,1为接受关闭,0为接受打开
        public const byte SLC		0x01						//线选地址锁存器控制线(待用)
        //   P0口选线控制
        public const byte CLOSEALL  0x08

        //	 通讯控制字设定
        public const byte RS_232		0x00					//通讯控制字1
        public const byte EXRS_232	0x20					//通讯控制字1
        public const byte	SET_VPW     0x40					//通讯控制字1
        public const byte SET_PWM     0x60					//通讯控制字1
        public const byte BIT9_SPACE  0x00					//通讯控制字1
        public const byte BIT9_MARK   0x01					//通讯控制字1
        public const byte BIT9_EVEN   0x02					//通讯控制字1
        public const byte BIT9_ODD    0x03					//通讯控制字1
        public const byte SEL_SL		0x00					//通讯控制字1
        public const byte SEL_DL0     0x08					//通讯控制字1
        public const byte SEL_DL1     0x10					//通讯控制字1
        public const byte SEL_DL2     0x18					//通讯控制字1
        public const byte SET_DB20    0x04					//通讯控制字1
        public const byte UN_DB20     0x00					//通讯控制字1
        public const byte ONEBYONE    0x80					//通讯控制字3
        public const byte INVERTBYTE  0x40					//通讯控制字3
        public const byte ORIGNALBYTE 0X00					//通讯控制字3
        */
        /***************************************************************************
            命令定义区:
               命令分为四类:
                  1、写入命令缓冲区命令：
                     将以整理好的批处理命令写入缓冲区：格式如下
                     命令字 WR_DATA	 0xD0?+ 长度（数据[N]+地址） +写入缓冲区地址+命令1+ 命令2。。。+命令N+校验。
                     其中命令N：为不含校验的命令，校验方法：为校验和
                     命令区存放格式为：长度（数据[N]+地址） +写入缓冲区地址+命令1+ 命令2。。。+命令N
                  2、单字节命令：（大于写入命令缓冲区命令字 WR_DATA	 0xD0，皆为单字节命令区）
                     简称快速命令：格式如下
                     命令字+校验和：
                     非缓冲区命令：
                     其中中断命令2个：停止执行，停止接受
                     软件复位，得到命令缓冲区数据，得到链路保持数据，得到上次缓冲区命令的数据
                     缓冲区命令：
                        1、缓冲区数据操作命令。
                        2、开关命令
                        3、链路保持命令
                        4、接受命令
                  3、多字节命令：（命令空间 0x30-0xCF）
                     格式如下：命令字（6BIT）+长度（数据长度-1；2BIT）+数据[N]+校验和
                     1、设置命令
                     2、数据操作命令

                  4、发送命令：（命令空间 0x00-0x2F）
                     格式如下：
                        长度（数据[N]+1）+数据[N]+校验和
                    发送命令在写入缓从区时长度可以有0x2F，有0x30个数据，但不写入缓冲区直接发送，追多不超过4个
                  5、中断命令2个：停止执行，停止接受
                        发送命令字，无校验，仅为一个字节，无运行返回，以等待运行结果标志返回。

        ***************************************************************************/
        //  1、写入命令缓冲区命令：
        public const byte WR_DATA = 0xD0;   				//写缓冲区命令字,写入数据到命令缓冲区
        public const byte WR_LINK = 0xFF;   				//若写入命令的地址为WR_LINK ，写入数据到链路保持区
        //链路保持区存放在命令缓冲区最后,存放次序:按地址从低到高
        public const int SEND_LEN = 0xA8;   				//一次发送数据的数据长度,0X70个数据
        public const byte SEND_DATA = 0x00;

        //  2、单字节命令：（大于写入命令缓冲区命令字 WR_DATA	 0xD0，皆为单字节命令区）

        //  非缓冲区命令
        public byte RESET = 0xF3; 			//软件复位命令  清除所有缓冲区和寄存器内容。
        public byte GETINFO = 0xF4;
        /*
                    得到CPU速度		F9         返回CPU的指令执行时间（按纳秒计，数值传递，3个字节）
                    和时间控制参数             返回时间控制的指令执行数：
                                               其他控制（1byte）（DB20）
                                               长等待控制（1byte）（DB200）
                                               缓冲区长度（1byte）
                                               产品序号（10byte）
                    和版本信息				   返回Commbox的硬件版本号。
                    等待接受5字节密码：（第五个字节为校验和）同公钥循环与或的校验和，返回命令增值。

        */
        /*
        public const byte GET_TIME    	0xF5			//得到时间设定	DD 返回字节时间、等待发送时间、链路保持时间、字节超时时间、接受超时时间
        public const byte GET_SET	    	0xF6			//得到链路设定  DE 返回链路控制字(3字节)、通讯波特率
        public const byte GET_PORT    	0xF7			//得到端口设置  DF 返回端口p0，p1，p2，p3
        public const byte GET_LINKDATA	0xF8		    //得到链路数据 FC 返回链路保持命令块中的所有内容 (中断命令)
        public const byte GET_BUFFDATA    0xF9			//得到缓冲器数据 FD 返回整个缓冲区数据 (中断命令)
        public const byte GET_CMMAND      0xFA			//得到命令数据 FE 返回上一执行命令。 (中断命令)
        */
        //中断命令定义
        public const byte STOP_REC = 0xFB; 			//中断接受命令  强行退出当前接受命令，不返回错误。(中断命令)
        public const byte STOP_EXECUTE = 0xFC; 			//中断批处理命令	在当前执行时，通过该命令停止当前接受操作，返回错误。(中断命令)

        //  单字节缓冲区命令
        //public const byte GET_PORT1    0xD8				//等到通讯口的当前状态

        public const byte SET_ONEBYONE = 0xD9;           	//将原有的接受一个发送一个的标志翻转
        /*
        public const byte SET55_BAUD   0xDA				//计算0x55的波特率
        public const byte REC_FR       0xE0				//接受一帧命令  E0 开始时回传开始接受信号，然后长期等待接受，接到数据实时回传，
                                                //待中断当前命令和中断处理命令，当接受的字节超过字节间的最大时间，自动正常退出。
                                                //若设定了长度接受,超时最长等待时间,自动返回.
        public const byte REC_LEN      0xE1				//接受长度数据 	E1-EF 开始时回传开始接受信号，接受命令字节低四位为长度的数据自动退出，
                                                //接到数据实时回传，待中断当前命令和中断处理命令，接受一个字节超过最长等待时间,正常退出.
        public const byte RECIEVE      0xF0				//连续接受	F0	开始时回传开始接受信号，然后长期等待接受，接到数据实时回传，
                                                //直到接受中断当前命令和中断处理命令。
        */
        public const byte RUNLINK = 0xF1;				//启动链路保持   F1 启动链路保持，定时执行链路保持内容，在每次执行前回传链路保持
        //开始信号，结束时回传链路保持结束信号。
        public const byte STOPLINK = 0xF2;				//中断链路保持   F2 结束链路保持执行。
        public const byte CLR_LINK = 0xDE;   			//清除链路保持缓冲区

        public const byte DO_BAT_00 = 0xDF;				//批处理命令，执行一次命令缓冲区00地址的命令区

        // 3、多字节命令：（命令空间 0x30-0xCF）
        public const byte D0_BAT = 0x78;				//批处理命令，连续执行一次最多4块命令缓冲区的地址命令区；数据最多为4个命令区的首地址
        public const byte D0_BAT_FOR = 0x7c;				//批处理命令，连续执行无数次最多4块命令缓冲区的地址命令区；数据最多为4个命令区的首地址		

        //多字节命令
        public const byte SETTING = 0x80;		 		//下位机通讯链路状态字设定：设定3个通讯控制字，无用设定或没有设定都自动清零
        public const byte SETBAUD = 0x84;         		//通讯波特率设定，只用2个数据位，单字节为低字节，双字节高字节在前，低字节在后。
        /*
        public const byte SETBYTETIME 0x88		 		//字节间时间设定 db20?（vpw为指令数） ，只用2个数据位，单字节为低字节，双字节高字节在前，低字节在后。
        public const byte SETWAITTIME 0x8c         		//空闲等待时间设定 db20?，只用2个数据位，单字节为低字节，双字节高字节在前，低字节在后。
        public const byte SETLINKTIME 0x90		 		//链路保持时建设定 db20?，只用2个数据位，单字节为低字节，双字节高字节在前，低字节在后。
        public const byte SETRECBBOUT 0x94		 		//接受字节超时错误判断 db20（vpw为指令数） ，只用2个数据位，单字节为低字节，双字节高字节在前，低字节在后。
        public const byte SETRECFROUT 0x98		 		//接受一帧超时错误判断?db20?，只用2个数据位，单字节为低字节，双字节高字节在前，低字节在后。
        */
        public const byte ECHO = 0x9c;         		//回传指定数据，按序回传数据

        public const byte SETPORT0 = 0xa0;			    //只有一个字节的数据，设定端口0
        public const byte SETPORT1 = 0xa4;			    //只有一个字节的数据，设定端口1
        public const byte SETPORT2 = 0xa8;			    //只有一个字节的数据，设定端口2
        public const byte SETPORT3 = 0xac;			    //只有一个字节的数据，设定端口3
        //已删除public const byte SETALLPORT  0x6F			    //只有四个字节的数据，设定端口0，1，2，3

        public const byte SET_UPBAUD = 0xb0;				//设置上位机的通讯波特率 ,仅有数据位1位,定义如下:其他非法
        /*
        public const byte UP_9600BPS   0x00
        public const byte UP_19200BPS  0x01
        public const byte UP_38400BPS  0x02
        public const byte UP_57600BPS  0x03
        public const byte UP_115200BPS 0x04
        */
        public const byte DELAYSHORT = 0xb4;				//设定延时时间 (DB20)只用2个数据位，单字节为低字节，双字节高字节在前，低字节在后。
        public const byte DELAYTIME = 0xb8;				//设定延时时间 (DB20)只用2个数据位，单字节为低字节，双字节高字节在前，低字节在后。
        public const byte DELAYLONG = 0xbC;				//设定延时时间 (DB200) 只用2个数据位，单字节为低字节，双字节高字节在前，低字节在后。

        //Operat Buff CMD
        //指定修改
        /*
        public const byte UPDATE_1BYTE 0xc1				//81 结果地址  数据1 					结果地址=数据1  
        public const byte UPDATE_2BYTE 0xc3   			//83 结果地址1 数据1 结果地址2 数据2	结果地址1=数据1 结果地址2=数据2			
        //数据拷贝
        public const byte COPY_DATA    0xcA				//8A 结果地址1  操作地址1 长度			COPY 操作地址1 TO 结果地址1 FOR 长度 字节
        //自增命令
        public const byte DEC_DATA     0xc4				//84 结果地址							结果地址=结果地址-1
        public const byte INC_DATA     0xc0				//80 结果地址 							结果地址=结果地址+1
        public const byte INC_2DATA    0xc5				//85 结果地址1 结果地址2				结果地址1=结果地址1+1 结果地址2=结果地址2+1
        //加法命令
        public const byte ADD_1BYTE    0xc2				//82 结果地址  操作地址1 数据1   		结果地址=操作地址1+数据1
        public const byte ADD_2BYTE	 0xc7				//87 结果地址1 结果地址2 数据1  数据2	结果地址1=结果地址1+数据1 结果地址2=结果地址2+数据2
        public const byte ADD_DATA     0xc6 				//86 结果地址1 操作地址1 操作地址2		结果地址1=操作地址1+操作地址2
        //减法命令
        public const byte SUB_DATA	 0xce  		    //8E 结果地址1 操作地址1 操作地址2		结果地址1=操作地址1-操作地址2
        public const byte SUB_BYTE     0xcD				//8D 结果地址1 数据1 					结果地址1=数据1-结果地址1
        public const byte INVERT_DATA  0xcC				//8C 结果地址1							结果地址1=~结果地址
        //取数据
        public const byte GET_NDATA    0xc9				//88 地址								返回数据缓冲区指定的数据


        public const byte UPDATE_1BYTE_A	0xc0			//81 结果地址  数据1 					结果地址=数据1  
        public const byte UPDATE_2BYTE_A	0xc0   			//83 结果地址1 数据1 结果地址2 数据2	结果地址1=数据1 结果地址2=数据2			
        //自增命令
        public const byte DEC_DATA_A		0xc4			//84 结果地址							结果地址=结果地址-1
        public const byte INC_DATA_A		0xc0			//80 结果地址 							结果地址=结果地址+1
        public const byte INC_2DATA_A		0xc4			//85 结果地址1 结果地址2				结果地址1=结果地址1+1 结果地址2=结果地址2+1
        //加法命令
        public const byte ADD_1BYTE_A		0xc0			//82 结果地址  操作地址1 数据1   		结果地址=操作地址1+数据1
        public const byte ADD_2BYTE_A		0xc4			//87 结果地址1 结果地址2 数据1  数据2	结果地址1=结果地址1+数据1 结果地址2=结果地址2+数据2
        public const byte ADD_DATA_A		0xc4 			//86 结果地址1 操作地址1 操作地址2		结果地址1=操作地址1+操作地址2
        //减法命令
        public const byte SUB_DATA_A		0xcc  		    //8E 结果地址1 操作地址1 操作地址2		结果地址1=操作地址1-操作地址2
        public const byte SUB_BYTE_A		0xcc			//8D 结果地址1 数据1 					结果地址1=数据1-结果地址1
        public const byte INVERT_DATA_A	0xcC			//8C 结果地址1							结果地址1=~结果地址
        */
        //取数据
        public const byte GET_DATA = 0xc8;			//88 地址								返回数据缓冲区指定的数据

        /***************************************************************************
            返回命令定义区:
               返回命令分为两类:
               1 单字节返回:无长度和校验,仅返回单字节
                    1 错误,成功信息:
                    2 接受的数据:(接受数据,通讯端口数据)
                    使用于缓冲区命令
               2 多字节返回:
                    1 格式:接受的命令字 + 长度 + 数据 + 校验和
                    长度：仅包含数据个数
                    使用于非缓冲区命令
               3 中断命令不返回：以执行结果返回

        ***************************************************************************/
        /*
        // 1 单字节返回:无长度和校验,仅返回单字节
                                            //接受返回错误信息定义
        public const byte UP_TIMEOUT 0xC0   			//接受命令超时错误
        public const byte UP_DATAEER 0xC1   			//接受命令数据错误
        public const byte OVER_BUFF  0xC2   			//批处理缓冲区溢出,不判断链路保持数据是否会破坏缓冲区数据,
                                            //仅判断数据长度+数据地址>链路保持的开始位置成立溢出.
        public const byte ERROR_REC  0xC3   			//其他接受错误

                                            //执行操作错误
        public const byte SUCCESS    0xAA   			//执行成功
        public const byte RUN_ERR    0xC4   			//运行启动检测错误
        */

        //批处理执行次数
        public const byte RUN_ONCE = 0x00;
        public const byte RUN_MORE = 0x01;
        //通讯校验和方式
        public const byte CHECK_SUM = 0x01;
        public const byte CHECK_REVSUM = 0x02;
        public const byte CHECK_CRC = 0X03;

        //RF多对一的设定接口
        public const byte SETDTR_L = 0x02;
        public const byte SETDTR_H = 0x03;

        public const byte MAX_RFADD = 0x2F;						//0x00-0x2F间的0x30个地址
        public const byte SETADD = 0x10;						//切换无线通讯设备到新地址
        public const byte CHANGADD = 0x40;						//改变当前与之通讯的无线设备的地址

        public const byte SETRFBAUD = 0x04;						//改变无线串口通讯波特率
        public const byte RESET_RF = 0x00;						//复位无线通讯主设备，该命令需在9600波特率下实现




        ///////////////////////////////////////////////////////////////////////////////
        //  通讯口 PORT
        ///////////////////////////////////////////////////////////////////////////////
        public const byte DH = 0x80;					//高电平输出,1为关闭,0为打开
        public const byte DL2 = 0x40;					//低电平输出,1为关闭,0为打开,正逻辑发送通讯线
        public const byte DL1 = 0x20;					//低电平输出,1为关闭,0为打开,正逻辑发送通讯线,带接受控制
        public const byte DL0 = 0x10;					//低电平输出,1为关闭,0为打开,正逻辑发送通讯线,带接受控制
        public const byte PWMS = 0x08;					//PWM发送线
        public const byte PWMR = 0x04;
        public const byte COMS = 0x02;					//标准发送通讯线路
        public const byte COMR = 0x01;
        public const byte SET_NULL = 0x00;					//不选择任何设置

        ///////////////////////////////////////////////////////////////////////////////
        //  通讯物理控制口
        ///////////////////////////////////////////////////////////////////////////////
        public const byte PWC = 0x80;						//通讯电平控制,1为5伏,0为12伏
        public const byte REFC = 0x40;						//通讯比较电平控制,1为通讯电平1/5,0为比较电平控制1/2
        public const byte CK = 0x20;						//K线控制开关,1为双线通讯,0为单线通讯
        public const byte SZFC = 0x10;						//发送逻辑控制,1为负逻辑,0为正逻辑
        public const byte RZFC = 0x08;						//接受逻辑控制,1为负逻辑,0为正逻辑
        public const byte DLC0 = 0x04;						//DLC1接受控制,1为接受关闭,0为接受打开
        public const byte DLC1 = 0x02;						//DLC0接受控制,1为接受关闭,0为接受打开
        public const byte SLC = 0x01;						//线选地址锁存器控制线(待用)
        public const byte CLOSEALL = 0x08;						//关闭所有发送口线，和接受口线

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
        public const byte ORIGNALBYTE = 0X00;

        ///////////////////////////////////////////////////////////////////////////////
        //  通讯设置参数时间
        ///////////////////////////////////////////////////////////////////////////////

        public const byte SETBYTETIME = 0x88;	 		//字节间时间设定 db20? ，只用2个数据位，单字节为低字节，双字节高字节在前，低字节在后。
        public const byte SETVPWSTART = 0x08;            //设置vpw发送数据时需发送0的时间。
        public const byte SETWAITTIME = 0x8c;       		//空闲等待时间设定 db20?，只用2个数据位，单字节为低字节，双字节高字节在前，低字节在后。
        public const byte SETLINKTIME = 0x90;	 		//链路保持时建设定 db20?，只用2个数据位，单字节为低字节，双字节高字节在前，低字节在后。
        public const byte SETRECBBOUT = 0x94;	 		//接受字节超时错误判断 db20（vpw为指令数） ，只用2个数据位，单字节为低字节，双字节高字节在前，低字节在后。
        public const byte SETRECFROUT = 0x98;	 		//接受一帧超时错误判断?db20?，只用2个数据位，单字节为低字节，双字节高字节在前，低字节在后。
        public const byte SETVPWRECS = 0X14;

        ///////////////////////////////////////////////////////////////////////////////
        //  上下位机通讯波特率
        ///////////////////////////////////////////////////////////////////////////////

        public const byte UP_9600BPS = 0x00;
        public const byte UP_19200BPS = 0x01;
        public const byte UP_38400BPS = 0x02;
        public const byte UP_57600BPS = 0x03;
        public const byte UP_115200BPS = 0x04;

        ///////////////////////////////////////////////////////////////////////////////
        //  操作数据缓冲区
        ///////////////////////////////////////////////////////////////////////////////
        //数据拷贝
        public const byte COPY_DATA = 0xcA;				//8A 结果地址1  操作地址1 长度			COPY 操作地址1 TO 结果地址1 FOR 长度 字节
        //修改数据
        public const byte UPDATE_1BYTE = 0xc1;				//81 结果地址  数据1 					结果地址=数据1  
        public const byte UPDATE_2BYTE = 0xc3;   			//83 结果地址1 数据1 结果地址2 数据2	结果地址1=数据1 结果地址2=数据2			
        //自增命令
        public const byte DEC_DATA = 0xc4;				//84 结果地址							结果地址=结果地址-1
        public const byte INC_DATA = 0xc0;				//80 结果地址 							结果地址=结果地址+1
        public const byte INC_2DATA = 0xc5;				//85 结果地址1 结果地址2				结果地址1=结果地址1+1 结果地址2=结果地址2+1
        //加法命令
        public const byte ADD_1BYTE = 0xc2;				//82 结果地址  操作地址1 数据1   		结果地址=操作地址1+数据1
        public const byte ADD_2BYTE = 0xc7;				//87 结果地址1 数据1  结果地址2 数据2	结果地址1=结果地址1+数据1 结果地址2=结果地址2+数据2
        public const byte ADD_DATA = 0xc6; 				//86 结果地址1 操作地址1 操作地址2		结果地址1=操作地址1+操作地址2
        //减法命令
        public const byte SUB_DATA = 0xce;  		    //8E 结果地址1 操作地址1 操作地址2		结果地址1=操作地址1-操作地址2
        public const byte SUB_BYTE = 0xcD;				//8D 结果地址1 数据1 					结果地址1=数据1-结果地址1
        public const byte INVERT_DATA = 0xcC;				//8C 结果地址1							结果地址1=~结果地址


        ///////////////////////////////////////////////////////////////////////////////
        //  接受命令类型定义
        ///////////////////////////////////////////////////////////////////////////////

        public const byte GET_PORT1 = 0xD8;				//等到通讯口的当前状态
        public const byte SET55_BAUD = 0xDA;				//计算0x55的波特率
        public const byte REC_FR = 0xE0;				//接受一帧命令  E0 开始时回传开始接受信号，然后长期等待接受，接到数据实时回传，
        public const byte REC_LEN_1 = 0xE1;				//接受1个数据，返回
        public const byte REC_LEN_2 = 0xE2;				//接受2个数据，返回
        public const byte REC_LEN_3 = 0xE3;				//接受3个数据，返回
        public const byte REC_LEN_4 = 0xE4;				//接受4个数据，返回
        public const byte REC_LEN_5 = 0xE5;				//接受5个数据，返回
        public const byte REC_LEN_6 = 0xE6;				//接受6个数据，返回
        public const byte REC_LEN_7 = 0xE7;				//接受7个数据，返回
        public const byte REC_LEN_8 = 0xE8;				//接受8个数据，返回
        public const byte REC_LEN_9 = 0xE9;				//接受9个数据，返回
        public const byte REC_LEN_10 = 0xEA;				//接受10个数据，返回
        public const byte REC_LEN_11 = 0xEB;				//接受11个数据，返回
        public const byte REC_LEN_12 = 0xEC;				//接受12个数据，返回
        public const byte REC_LEN_13 = 0xED;				//接受13个数据，返回
        public const byte REC_LEN_14 = 0xEE;				//接受14个数据，返回
        public const byte REC_LEN_15 = 0xEF;				//接受15个数据，返回
        public const byte RECEIVE = 0xF0;				//连续接受	F0	开始时回传开始接受信号，然后长期等待接受，接到数据实时回传，

        ///////////////////////////////////////////////////////////////////////////////
        //  ComBox记录信息和当前状态种类定义
        ///////////////////////////////////////////////////////////////////////////////

        public const byte GET_TIME = 0xF5;			//得到时间设定	DD 返回字节时间、等待发送时间、链路保持时间、字节超时时间、接受超时时间
        public const byte GET_SET = 0xF6;			//得到链路设定  DE 返回链路控制字(3字节)、通讯波特率
        public const byte GET_PORT = 0xF7;			//得到端口设置  DF 返回端口p0，p1，p2，p3
        public const byte GET_LINKDATA = 0xF8;		    //得到链路数据 FC 返回链路保持命令块中的所有内容 (中断命令)
        public const byte GET_BUFFDATA = 0xF9;			//得到缓冲器数据 FD 返回整个缓冲区数据 (中断命令)
        public const byte GET_CMMAND = 0xFA;			//得到命令数据 FE 返回上一执行命令。 (中断命令)

        ///////////////////////////////////////////////////////////////////////////////
        // 返回失败时，可根据Error_Record的值查找错误表定义
        ///////////////////////////////////////////////////////////////////////////////

        public const byte ILLIGICAL_LEN = 0xFF;				//设置命令数据非法超长
        public const byte NOBUFF_TOSEND = 0xFE;				//无交换缓冲区用于发送数据存放
        public const byte SENDDATA_ERROR = 0xFD;				//上位机发送数据异常
        public const byte CHECKSUM_ERROR = 0xFC;				//接受命令回复校验和出错
        public const byte TIMEOUT_ERROR = 0xFB;				//接受数据超时错误
        public const byte LOST_VERSIONDATA = 0xFA;			//读到的Commbox数据长度不够.
        public const byte ILLIGICAL_CMD = 0xF9;			//无此操作功能,没有定义.
        public const byte DISCONNECT_COMM = 0xF8;			//没有连接上串口
        public const byte DISCONNECT_COMBOX = 0xF7;			//没有连接上COMMBOX设备
        public const byte NODEFINE_BUFF = 0xF6;			//没有此命令块存在,未定义
        public const byte APPLICATION_NOW = 0xF5;			//现有缓冲区申请,未取消,不能再此申请
        public const byte BUFFBUSING = 0xF4;			//此缓冲区有数据未被撤销,不能使用,需删除此缓冲区,方可使用
        public const byte BUFFFLOW = 0xF3;			//整个缓冲区无可使用的空间,不能申请,需删除缓冲区释放空间,方可使用
        public const byte NOAPPLICATBUFF = 0xF2;			//未申请错误,需先申请,方可使用
        public const byte UNBUFF_CMD = 0xF1;			//不是缓冲区命令,不能加载
        public const byte NOUSED_BUFF = 0xF0;			//该缓冲区现没有使用,删除无效
        public const byte KEEPLINK_ERROR = 0xEF;			//链路保持已断线
        public const byte UNDEFINE_CMD = 0xEE;			//无效命令,未曾定义
        public const byte UNSET_EXRSBIT = 0xED;			//没有设定扩展RS232的接受数据位个数
        public const byte COMMBAUD_OUT = 0xEC;			//按照定义和倍增标志计算通讯波特率超出范围
        public const byte COMMTIME_OUT = 0xEB;			//按照定义和倍增标志计算通讯时间超出范围
        public const byte OUTADDINBUFF = 0xEA;			//缓冲区寻址越界
        public const byte COMMTIME_ZERO = 0xE9;			//commbox时间基数为零
        public const byte SETTIME_ERROR = 0xE8;			//延时时间为零
        public const byte NOADDDATA = 0xE7;			//没有向申请的缓冲区填入命令,申请的缓冲区被撤销
        public const byte TESTNOLINK = 0xE6;			//选择的线路没有连通
        public const byte PORTLEVELIDLE = 0xE5;			//端口电平为常态
        public const byte COMMBOXID_ERR = 0xE4;			//COMMBOX ID错误

        public const byte UP_TIMEOUT = 0xC0;   			//COMMBOX接受命令超时错误
        public const byte UP_DATAEER = 0xC1;   			//COMMBOX接受命令数据错误
        public const byte OVER_BUFF = 0xC2;   			//COMMBOX批处理缓冲区溢出,不判断链路保持数据是否会破坏缓冲区数据,
        //仅判断数据长度+数据地址>链路保持的开始位置成立溢出.
        public const byte ERROR_REC = 0xC3;   			//COMMBOX其他接受错误
        //COMMBOX执行操作错误
        public const byte SUCCESS = 0xAA;   			//COMMBOX执行成功
        public const byte SEND_OK = 0x55;
        public const byte RF_ERR = 0xC8;

        public const byte RUN_ERR = 0xC4;   			//COMMBOX运行启动检测错误
        public const byte SEND_CMD = 0x01;

        private CommboxInfo commboxInfo; //CommBox 有关信息数据
        private CmdBuffInfo cmdBuffInfo; //维护COMMBOX数据缓冲区
        private byte[] cmdTemp; //写入命令缓冲区
        private byte lastError; //提供错误查询
        private bool isDB20;
        private bool isDoNow;
        private BoxStream bs;
        private VirtualStream vs;
        byte[] password;
        private int position;
        private Core.Timer reqByteToByte;
        private Core.Timer reqWaitTime;
        private Core.Timer resByteToByte;
        private Core.Timer resWaitTime;

        public Commbox()
        {
            commboxInfo = new CommboxInfo();
            cmdBuffInfo = new CmdBuffInfo();
            cmdTemp = new byte[256];
            lastError = 0;
            isDB20 = false;
            isDoNow = true;
            bs = BoxStream.Instance;
            vs = bs.VirtualStream;
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
            if (BoxStream.Instance.SerialPort == null)
                return false;

            int avail = vs.BytesToRead;
            if (avail > 240)
            {
                bs.VirtualStream.DiscardInBuffer();
                return true;
            }

            byte[] receiveBuffer = new byte[1];
            receiveBuffer[0] = SUCCESS;
            vs.ReadTimeout = TimeSpan.FromMilliseconds(200);
            while (vs.Read(receiveBuffer, 0, 1) != 0) ;
            if (receiveBuffer[0] == SUCCESS)
                return true;
            lastError = KEEPLINK_ERROR;
            return false;
        }

        public bool SendOk(Core.Timer time)
        {
            vs.ReadTimeout = time.TimeSpan;
            byte[] receiveBuffer = new byte[1];
            receiveBuffer[0] = 0;
            if (vs.Read(receiveBuffer, 0, 1) == 1)
            {
                if (receiveBuffer[0] == SEND_OK)
                {
                    return true;
                }
                else if (receiveBuffer[0] >= UP_TIMEOUT && receiveBuffer[0] <= ERROR_REC)
                {
                    lastError = SENDDATA_ERROR;
                    return false;
                }
            }
            lastError = TIMEOUT_ERROR;
            return false;
        }

        public uint GetBoxVer()
        {
            return (uint)((commboxInfo.Version[0] << 8) | (commboxInfo.Version[1]));
        }

        private bool CommboxCommand(byte commandWord, byte[] buff, int offset, int length)
        {
            byte checksum = (byte)(commandWord + length);
            if (commandWord < WR_DATA)
            {
                if (length == 0)
                {
                    lastError = ILLIGICAL_LEN;
                    return false;
                }
                checksum--;
            }
            else
            {
                if (length != 0)
                {
                    lastError = ILLIGICAL_LEN;
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
                if (commandWord != STOP_REC && commandWord != STOP_EXECUTE)
                {
                    if (!CheckIdle() || vs.Write(command, 0, command.Length) != command.Length)
                    {
                        lastError = SENDDATA_ERROR;
                        continue;
                    }
                }
                else
                {
                    if (vs.Write(command, 0, command.Length) != command.Length)
                    {
                        lastError = SENDDATA_ERROR;
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
            if (cmdBuffInfo.CmdBuffAdd[LINKBLOCK] - cmdBuffInfo.CmdBuffAdd[SWAPBLOCK] < length + 1)
            {
                lastError = NOBUFF_TOSEND;
                return false;
            }
            byte[] command = new byte[5 + length];
            command[0] = (byte)(WR_DATA + commboxInfo.HeadPassword);
            command[1] = (byte)(length + 2);
            command[2] = cmdBuffInfo.CmdBuffAdd[SWAPBLOCK];
            command[3] = (byte)(length - 1);
            byte checksum = (byte)(WR_DATA + command[1] + command[2] + command[3]);

            for (int i = 0; i < length; i++)
            {
                command[i + 4] = buff[i];
                checksum += buff[i];
            }
            command[command.Length - 1] = checksum;
            for (int i = 0; i < 3; i++)
            {
                if (!CheckIdle() || vs.Write(command, 0, command.Length) != command.Length)
                {
                    lastError = SENDDATA_ERROR;
                    continue;
                }
                if (SendOk(Core.Timer.FromMilliseconds(20 * command.Length)))
                    return true;
            }
            return false;
        }

        private bool CommboxEcuNew(byte commandWord, byte[] buff, int offset, int length)
        {
            if (cmdBuffInfo.CmdBuffAdd[LINKBLOCK] - cmdBuffInfo.CmdBuffAdd[SWAPBLOCK] < length + 1)
            {
                lastError = NOBUFF_TOSEND;
                return false;
            }
            byte[] command = new byte[6 + length];
            command[0] = (byte)(WR_DATA + commboxInfo.HeadPassword);
            command[1] = (byte)(length + 3);
            command[2] = cmdBuffInfo.CmdBuffAdd[SWAPBLOCK];
            command[3] = SEND_CMD;
            command[4] = (byte)(length - 1);
            byte checksum = (byte)(WR_DATA + command[1] + command[2] + command[3] + command[4]);

            for (int i = 0; i < length; i++)
            {
                command[i + 5] = buff[i];
                checksum += buff[i];
            }
            command[command.Length - 1] = checksum;
            for (int i = 0; i < 3; i++)
            {
                if (!CheckIdle() || vs.Write(command, 0, command.Length) != command.Length)
                {
                    lastError = SENDDATA_ERROR;
                    continue;
                }
                if (SendOk(Core.Timer.FromMilliseconds(20 * (command.Length + 7))))
                    return true;
            }
            return false;
        }

        public bool CommboxDo(byte commandWord, byte[] buff, int offset, int length)
        {
            if (length > CMD_DATALEN)
            {
                if (commandWord == SEND_DATA && length <= SEND_LEN)
                {
                    bool ret;
                    if (GetBoxVer() > 0x400)
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
                    return CommboxDo(D0_BAT, cmdBuffInfo.CmdBuffAdd, SWAPBLOCK, 1);
                }
                else
                {
                    lastError = ILLIGICAL_LEN;
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
            int times = REPLAYTIMES;
            while (times > 0)
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
            vs.ReadTimeout = time.TimeSpan;
            byte[] receiveBuffer = new byte[1];
            receiveBuffer[0] = 0;
            if (vs.Read(receiveBuffer, 0, 1) != 1)
            {
                lastError = TIMEOUT_ERROR;
                return false;
            }
            if (receiveBuffer[0] == SUCCESS)
                return true;
            while (vs.Read(receiveBuffer, 0, 1) == 1) ;
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
                vs.DiscardInBuffer();
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
                lastError = CHECKSUM_ERROR;
                return 0;
            }
            return cmdInfo[1];
        }

        public int ReadData(byte[] receiveBuffer, int offset, int length, Core.Timer totalTime)
        {
            vs.ReadTimeout = totalTime.TimeSpan;
            return vs.Read(receiveBuffer, 0, length);
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

            if (vs.Write(cmdTemp, 0, 5) != 5)
            {
                lastError = SENDDATA_ERROR;
                return false;
            }
            len = password.Length - 1;
            i = 0;
            checksum = (byte)(cmdTemp[4] + cmdTemp[4]);
            while (i < len)
            {
                checksum += (byte)(password[i] ^ cmdTemp[i % 5]);
                i++;
            }
            System.Threading.Thread.Sleep(20);
            if (GetCmdData(GETINFO, cmdTemp, 0) == 0)
                return false;
            commboxInfo.HeadPassword = cmdTemp[0];

            if (checksum != commboxInfo.HeadPassword)
            {
                lastError = CHECKSUM_ERROR;
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

            if (!CommboxDo(GETINFO, null, 0, 0))
                return false;

            byte length = GetCmdData(GETINFO, cmdTemp, 0);
            if (length <= 0)
                return false;
            if (length < COMMBOXINFOLEN)
            {
                lastError = LOST_VERSIONDATA;
                return false;
            }
            commboxInfo.CommboxTimeUnit = 0;
            int pos = 0;
            for (int i = 0; i < MINITIMELEN; i++)
                commboxInfo.CommboxTimeUnit = commboxInfo.CommboxTimeUnit * 256 + cmdTemp[pos++];
            commboxInfo.TimeBaseDB = cmdTemp[pos++];
            commboxInfo.TimeExternDB = cmdTemp[pos++];
            commboxInfo.CmdBuffLen = cmdTemp[pos++];
            if (commboxInfo.TimeBaseDB == 0 || commboxInfo.CommboxTimeUnit == 0 || commboxInfo.CmdBuffLen == 0)
            {
                lastError = COMMTIME_ZERO;
                return false;
            }

            for (int i = 0; i < COMMBOXIDLEN; i++)
                commboxInfo.CommboxID[i] = cmdTemp[pos++];
            for (int i = 0; i < VERSIONLEN; i++)
                commboxInfo.Version[i] = cmdTemp[pos++];
            commboxInfo.CommboxPort[0] = NULLADD;
            commboxInfo.CommboxPort[1] = NULLADD;
            commboxInfo.CommboxPort[2] = NULLADD;
            commboxInfo.CommboxPort[3] = NULLADD;

            cmdBuffInfo.CmdBuffID = NULLADD;
            cmdBuffInfo.CmdUsedNum = 0;
            for (int i = 0; i < MAXIM_BLOCK; i++)
                cmdBuffInfo.CmdBuffAdd[i] = NULLADD;
            cmdBuffInfo.CmdBuffAdd[LINKBLOCK] = commboxInfo.CmdBuffLen;
            cmdBuffInfo.CmdBuffAdd[SWAPBLOCK] = 0;

            return true;
        }

        private bool OpenBox(string port, int baud)
        {
            if (bs.Type == StreamType.SerialPort)
            {
                bs.SerialPort.PortName = port;
                bs.SerialPort.BaudRate = baud;
                bs.SerialPort.DataBits = 8;
                bs.SerialPort.StopBits = System.IO.Ports.StopBits.One;
                bs.SerialPort.Parity = System.IO.Ports.Parity.None;
                bs.SerialPort.Handshake = System.IO.Ports.Handshake.None;
                bs.SerialPort.ReadTimeout = 500;
                bs.SerialPort.WriteTimeout = 500;
                try
                {
                    bs.SerialPort.Open();
                    System.Threading.Thread.Sleep(50);
                    bs.SerialPort.DtrEnable = true;
                    System.Threading.Thread.Sleep(50);
                    for (int i = 0; i < 3; i++)
                    {
                        SetRF(RESET_RF, 0);
                        SetRF(SETDTR_L, 0);
                        if (InitBox() && CheckBox())
                        {
                            bs.SerialPort.DiscardInBuffer();
                            bs.SerialPort.DiscardOutBuffer();
                            if (SetPCBaud(UP_57600BPS))
                                return true;
                        }
                    }
                    bs.SerialPort.Close();
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        public void Open()
        {
            if (bs.Type == StreamType.SerialPort)
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
                        return;
                    }
                }
            }
            throw new IOException();
        }

        public void Close()
        {
            if (bs.Type == StreamType.SerialPort)
            {
                if (bs.SerialPort.IsOpen)
                {
                    StopNow(true);
                    DoSet(RESET, null, 0, 0);
                    SetRF(RESET_RF, 0);
                    bs.SerialPort.Close();
                    return;
                }
            }
            throw new IOException();
        }

        private bool DoSetPCBaud(byte baud)
        {
            try
            {
                lastError = 0;
                if (!CommboxDo(SET_UPBAUD, new byte[] { baud }, 0, 1))
                    return false;
                Thread.Sleep(50);
                CheckResult(Core.Timer.FromMilliseconds(50));
                SetRF(SETRFBAUD, baud);
                CheckResult(Core.Timer.FromMilliseconds(50));
                switch (baud)
                {
                    case UP_9600BPS:
                        bs.SerialPort.BaudRate = 9600;
                        break;
                    case UP_38400BPS:
                        bs.SerialPort.BaudRate = 19200;
                        break;
                    case UP_57600BPS:
                        bs.SerialPort.BaudRate = 38400;
                        break;
                    case UP_115200BPS:
                        bs.SerialPort.BaudRate = 115200;
                        break;
                    default:
                        lastError = ILLIGICAL_CMD;
                        return false;
                }

                SetRF(SETRFBAUD, baud);
                if (!CommboxDo(SET_UPBAUD, new byte[] { baud }, 0, 1))
                    return false;
                if (!CheckResult(Core.Timer.FromMilliseconds(100)))
                    return false;
                bs.SerialPort.DiscardInBuffer();
                return true;
            }
            catch (Exception)
            {
                lastError = DISCONNECT_COMM;
                return false;
            }
        }

        private bool SetPCBaud(byte baud)
        {
            int times = REPLAYTIMES;
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
            if (buffID > MAXIM_BLOCK)
            {
                lastError = NODEFINE_BUFF;
                return false;
            }

            if (cmdBuffInfo.CmdBuffID != NULLADD)
            {
                lastError = APPLICATION_NOW;
                return false;
            }
            if (cmdBuffInfo.CmdBuffAdd[buffID] != NULLADD && buffID != LINKBLOCK && !DelBatch(buffID))
                return false;
            cmdTemp[0] = WR_DATA;
            cmdTemp[1] = 0x01;
            if (buffID == LINKBLOCK)
            {
                cmdTemp[2] = 0xFF;
                cmdBuffInfo.CmdBuffAdd[LINKBLOCK] = commboxInfo.CmdBuffLen;
            }
            else
            {
                cmdTemp[2] = cmdBuffInfo.CmdBuffAdd[SWAPBLOCK];
            }
            if ((cmdBuffInfo.CmdBuffAdd[LINKBLOCK] - cmdBuffInfo.CmdBuffAdd[SWAPBLOCK]) <= 1)
            {
                lastError = BUFFFLOW;
                return false;
            }
            cmdTemp[3] = (byte)(WR_DATA + 0x01 + cmdTemp[2]);
            cmdTemp[0] += commboxInfo.HeadPassword;
            cmdBuffInfo.CmdBuffID = buffID;
            isDoNow = false;
            return true;
        }

        public bool AddToBuff(byte commandWord, byte[] data, int offset, int length)
        {
            byte checksum = cmdTemp[cmdTemp[1] + 2];
            position = cmdTemp[1] + length + 1;
            if (cmdBuffInfo.CmdBuffID == NULLADD)
            {
                //数据块标识登记是否有申请?
                lastError = NOAPPLICATBUFF;
                isDoNow = true;
                return false;
            }

            if ((cmdBuffInfo.CmdBuffAdd[LINKBLOCK] - cmdBuffInfo.CmdBuffAdd[SWAPBLOCK]) < position)
            {
                //检查是否有足够的空间存储?
                isDoNow = true;
                return false;
            }

            if (commandWord < RESET && commandWord != CLR_LINK && commandWord != DO_BAT_00 && commandWord != D0_BAT && commandWord != D0_BAT_FOR && commandWord != WR_DATA)
            {
                //是否为缓冲区命令?
                if (length <= CMD_DATALEN || (commandWord == SEND_DATA && length < SEND_LEN))
                {
                    //是否合法命令?
                    if (commandWord == SEND_DATA && GetBoxVer() > 0x400)
                    {
                        //增加发送长命令
                        cmdTemp[cmdTemp[1] + 2] = SEND_CMD;
                        checksum += SEND_CMD;
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
                lastError = ILLIGICAL_LEN;
                isDoNow = true;
                return false;
            }
            lastError = UNBUFF_CMD;
            isDoNow = true;
            return false;
        }

        public bool EndBatch()
        {
            int times = REPLAYTIMES;
            isDoNow = true;
            if (cmdBuffInfo.CmdBuffID == NULLADD)
            {
                //数据块标识登记是否有申请?
                lastError = NOAPPLICATBUFF;
                return false;
            }

            if (cmdTemp[1] == 0x01)
            {
                cmdBuffInfo.CmdBuffID = NULLADD;
                lastError = NOADDDATA;
                return false;
            }

            while (times != 0)
            {
                if (!CheckIdle() || vs.Write(cmdTemp, 0, cmdTemp[1] + 3) != cmdTemp[1] + 3)
                    continue;
                else if (SendOk(Core.Timer.FromMilliseconds(20 * (cmdTemp[1] + 10))))
                    break;
                if (!StopNow(true))
                {
                    cmdBuffInfo.CmdBuffID = NULLADD;
                    return false;
                }
            }
            if (times == 0)
            {
                cmdBuffInfo.CmdBuffID = NULLADD;
                return false;
            }
            if (cmdBuffInfo.CmdBuffID == LINKBLOCK)
                cmdBuffInfo.CmdBuffAdd[LINKBLOCK] = (byte)(commboxInfo.CmdBuffLen - cmdTemp[1]);
            else
            {
                cmdBuffInfo.CmdBuffAdd[cmdBuffInfo.CmdBuffID] = cmdBuffInfo.CmdBuffAdd[SWAPBLOCK];
                cmdBuffInfo.CmdBuffUsed[cmdBuffInfo.CmdUsedNum] = cmdBuffInfo.CmdBuffID;
                cmdBuffInfo.CmdUsedNum++;
                cmdBuffInfo.CmdBuffAdd[SWAPBLOCK] += cmdTemp[1];
            }
            cmdBuffInfo.CmdBuffID = NULLADD;
            return true;
        }

        public bool DelBatch(byte buffID)
        {
            if (buffID > LINKBLOCK)
            {
                //数据块不存在
                lastError = NODEFINE_BUFF;
                return false;
            }
            if (cmdBuffInfo.CmdBuffID == buffID)
            {
                cmdBuffInfo.CmdBuffID = NULLADD;
                return true;
            }

            if (cmdBuffInfo.CmdBuffAdd[buffID] == NULLADD)
            {
                //数据块标识登记是否有申请?
                lastError = NOUSED_BUFF;
                return false;
            }

            if (buffID == LINKBLOCK)
                cmdBuffInfo.CmdBuffAdd[LINKBLOCK] = commboxInfo.CmdBuffLen;
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
                    data[2] = (byte)(cmdBuffInfo.CmdBuffAdd[SWAPBLOCK] - data[1]);
                    if (!DoSet(COPY_DATA - COPY_DATA % 4, data, 0, 3))
                        return false;
                }
                else
                {
                    data[1] = cmdBuffInfo.CmdBuffAdd[SWAPBLOCK];
                }
                int deleteBuffLen = data[1] - data[0];
                for (i = i + 1; i < cmdBuffInfo.CmdUsedNum; i++)
                {
                    cmdBuffInfo.CmdBuffUsed[i - 1] = cmdBuffInfo.CmdBuffUsed[i];
                    cmdBuffInfo.CmdBuffAdd[cmdBuffInfo.CmdBuffUsed[i]] -= (byte)deleteBuffLen;
                }
                cmdBuffInfo.CmdUsedNum--;
                cmdBuffInfo.CmdBuffAdd[SWAPBLOCK] -= (byte)deleteBuffLen;
                cmdBuffInfo.CmdBuffAdd[buffID] = NULLADD;
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
            return SendCmdToBox(SETPORT1, commboxInfo.CommboxPort, 1, 1);
        }

        public bool SetCommCtrl(byte valueOpen, byte valueClose)
        {
            //只有一个字节的数据，设定端口2
            commboxInfo.CommboxPort[2] &= (byte)(~valueOpen);
            commboxInfo.CommboxPort[2] |= valueClose;
            return SendCmdToBox(SETPORT2, commboxInfo.CommboxPort, 2, 1);
        }

        public bool SetCommLine(byte sendLine, byte recLine)
        {
            //只有一个字节的数据，设定端口0
            if (sendLine > 7)
                sendLine = 0x0F;
            if (recLine > 7)
                recLine = 0x0F;
            commboxInfo.CommboxPort[0] = (byte)(sendLine + recLine * 16);
            return SendCmdToBox(SETPORT0, commboxInfo.CommboxPort, 0, 1);
        }

        public bool TurnOverOneByOne()
        {
            //将原有的接受一个发送一个的标志翻转
            return SendCmdToBox(SET_ONEBYONE);
        }

        public bool SetEchoData(byte[] echoBuff, int offset, int length)
        {
            if (length == 0 || length > 4)
            {
                lastError = ILLIGICAL_LEN;
                return false;
            }

            if (isDoNow)
            {
                byte[] buff = new byte[6];
                if (!CommboxDo(ECHO, echoBuff, offset, length) ||
                    ReadData(buff, 0, length, Core.Timer.FromMilliseconds(100)) != length)
                    return false;
                while (length-- > 0)
                {
                    if (buff[length] != echoBuff[length])
                    {
                        lastError = CHECKSUM_ERROR;
                        return false;
                    }
                }
                return CheckResult(Core.Timer.FromMilliseconds(100));
            }
            else
            {
                return AddToBuff(ECHO, echoBuff, 0, length);
            }
        }

        public bool KeepLink(bool isRunLink)
        {
            if (isRunLink)
            {
                return SendCmdToBox(RUNLINK);
            }
            else
            {
                return SendCmdToBox(STOPLINK);
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

            if (modeControl == SET_VPW || modeControl == SET_PWM)
            {
                return SendCmdToBox(SETTING, ctrlWord, 0, 1);
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
                if (modeControl == EXRS_232 && length < 2)
                {
                    lastError = UNSET_EXRSBIT;
                    return false;
                }
                return SendCmdToBox(SETTING, ctrlWord, 0, length);
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
                lastError = COMMBAUD_OUT;
                return false;
            }
            baudTime[0] = (byte)(instructNum / 256);
            baudTime[1] = (byte)(instructNum % 256);

            if (baudTime[0] == 0)
            {
                return SendCmdToBox(SETBAUD, baudTime, 1, 1);
            }
            else
            {
                return SendCmdToBox(SETBAUD, baudTime, 0, 2);
            }
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
            double microTime = time.Microseconds;

            if (type == SETVPWSTART || type == SETVPWRECS)
            {
                if (SETVPWRECS == type)
                    microTime = (microTime * 2) / 3;
                type = (byte)(type + (SETBYTETIME & 0xF0));
                microTime = (microTime / (commboxInfo.CommboxTimeUnit / 1000000.0));
            }
            else
            {
                microTime = (microTime / commboxInfo.TimeBaseDB) / (commboxInfo.CommboxTimeUnit / 1000000.0);
            }

            if (microTime > 65535)
            {
                lastError = COMMTIME_OUT;
                return false;
            }

            if (type == SETBYTETIME || type == SETWAITTIME || type == SETRECBBOUT || type == SETRECFROUT || type == SETLINKTIME)
            {
                timeBuff[0] = (byte)(microTime / 256);
                timeBuff[1] = (byte)(microTime % 256);
                if (timeBuff[0] == 0)
                {
                    return SendCmdToBox(type, timeBuff, 1, 1);
                }
            }
            else
            {
                return SendCmdToBox(type, timeBuff, 0, 2);
            }
            lastError = UNDEFINE_CMD;
            return false;
        }

        public bool RunReceive(byte type)
        {
            if (type == GET_PORT1)
                isDB20 = false;
            if (type == GET_PORT1 || type == SET55_BAUD || (type >= REC_FR && type <= RECEIVE))
            {
                if (isDoNow)
                {
                    return CommboxDo(type, null, 0, 0);
                }
            }
            lastError = UNDEFINE_CMD;
            return false;
        }

        public bool GetAbsAdd(byte buffID, ref byte buffAdd)
        {
            int length;
            byte startAdd;
            if (cmdBuffInfo.CmdBuffID != buffID)
            {
                if (cmdBuffInfo.CmdBuffAdd[buffID] == NULLADD)
                {
                    lastError = NOUSED_BUFF;
                    return false;
                }

                if (buffID == LINKBLOCK)
                {
                    length = commboxInfo.CmdBuffLen - cmdBuffInfo.CmdBuffAdd[LINKBLOCK];
                }
                else
                {
                    int i;
                    for (i = 0; i < cmdBuffInfo.CmdUsedNum; i++)
                        if (cmdBuffInfo.CmdBuffUsed[i] == buffID)
                            break;
                    if (i == cmdBuffInfo.CmdUsedNum - 1)
                        length = cmdBuffInfo.CmdBuffAdd[SWAPBLOCK] - cmdBuffInfo.CmdBuffAdd[buffID];
                    else
                        length = cmdBuffInfo.CmdBuffAdd[buffID + 1] - cmdBuffInfo.CmdBuffAdd[buffID];
                }
                startAdd = cmdBuffInfo.CmdBuffAdd[buffID];
            }
            else
            {
                length = cmdBuffInfo.CmdBuffAdd[LINKBLOCK] - cmdBuffInfo.CmdBuffAdd[SWAPBLOCK];
                startAdd = cmdBuffInfo.CmdBuffAdd[SWAPBLOCK];
            }

            if (buffAdd < length)
            {
                buffAdd += startAdd;
                return true;
            }
            else
            {
                lastError = OUTADDINBUFF;
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
                case INVERT_DATA: //add
                case DEC_DATA:
                case INC_DATA:
                    break;
                case UPDATE_1BYTE: //add + data
                case SUB_BYTE:
                    temp[1] = buffer[2];
                    break;
                case INC_2DATA: //add + add
                    temp[1] = buffer[3];
                    if (!GetAbsAdd(buffer[2], ref cmdTemp[1]))
                        return false;
                    break;
                case COPY_DATA: // add + add + data
                case ADD_1BYTE:
                    temp[1] = buffer[3];
                    if (!GetAbsAdd(buffer[2], ref temp[1]))
                        return false;
                    temp[2] = buffer[4];
                    break;
                case UPDATE_2BYTE: // add + data + add + data
                case ADD_2BYTE:
                    temp[1] = buffer[2];
                    temp[2] = buffer[4];
                    if (!GetAbsAdd(buffer[3], ref temp[2]))
                        return false;
                    temp[3] = buffer[5];
                    break;
                case ADD_DATA: // add + add + add
                case SUB_DATA:
                    temp[1] = buffer[3];
                    if (!GetAbsAdd(buffer[2], ref temp[1]))
                        return false;
                    temp[2] = buffer[5];
                    if (!GetAbsAdd(buffer[4], ref temp[2]))
                        return false;
                    break;
                default:
                    lastError = UNDEFINE_CMD;
                    return false;
            }

            return SendCmdToBox((byte)(type - type % 4), temp, 0, type % 4 + 1);
        }

        public bool CommboxDelay(Core.Timer time)
        {
            byte[] timeBuff = new byte[2];
            byte delayWord = DELAYSHORT;
            double microTime = time.Microseconds;
            microTime = microTime / (commboxInfo.CommboxTimeUnit / 1000000.0);
            if (microTime == 0)
            {
                lastError = SETTIME_ERROR;
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
                        lastError = COMMTIME_OUT;
                        return false;
                    }
                    delayWord = DELAYLONG;
                }
                else
                {
                    delayWord = DELAYTIME;
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
                lastError = ILLIGICAL_LEN;
                return false;
            }
            if (isDoNow)
            {
                return CommboxDo(SEND_DATA, buffer, offset, length);
            }
            else
            {
                return AddToBuff(SEND_DATA, buffer, offset, length);
            }
        }

        public bool StopNow(bool isStopExecute)
        {
            if (isStopExecute)
            {
                byte[] receiveBuffer = new byte[1];
                int times = REPLAYTIMES;
                while (times-- != 0)
                {
                    if (!CommboxDo(STOP_EXECUTE, null, 0, 0))
                        return false;
                    else
                    {
                        vs.ReadTimeout = TimeSpan.FromMilliseconds(600);
                        if ((vs.Read(receiveBuffer, 0, 1) != 1) || receiveBuffer[0] != RUN_ERR)
                        {
                            lastError = TIMEOUT_ERROR;
                            return false;
                        }
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return CommboxDo(STOP_REC, null, 0, 0);
            }
        }

        public bool RunBatch(byte[] buffID, bool repeat)
        {
            int length = buffID.Length;
            for (int i = 0; i < buffID.Length; i++)
            {
                if (cmdBuffInfo.CmdBuffAdd[buffID[i]] == NULLADD)
                {
                    lastError = NOUSED_BUFF;
                    return false;
                }
                buffID[i] = cmdBuffInfo.CmdBuffAdd[buffID[i]];
            }
            byte commandWord = D0_BAT;
            if (repeat)
                commandWord = D0_BAT_FOR;
            if (commandWord == D0_BAT && buffID[0] == cmdBuffInfo.CmdBuffUsed[0])
            {
                length = 0;
                commandWord = DO_BAT_00;
            }
            return CommboxDo(commandWord, buffID, 0, length);
        }

        private bool SetRF(byte cmd, byte cmdInfo)
        {
            int times = REPLAYTIMES;
            cmdInfo = (byte)(cmd + cmdInfo);
            if (cmd == SETRFBAUD)
                times = 2;
            Thread.Sleep(6);
            while (times-- != 0)
            {
                if (CheckIdle() && vs.Write(new byte[] { cmdInfo }, 0, 1) == 1)
                {
                    if (!SendOk(Core.Timer.FromMilliseconds(50)))
                        continue;
                    if (vs.Write(new byte[] { cmdInfo }, 0, 1) != 1 || !CheckResult(Core.Timer.FromMilliseconds(150)))
                        continue;
                    Thread.Sleep(100);
                    return true;
                }
            }
            return false;
        }

        public void SetConnector(ConnectorType cnn)
        {
            throw new NotImplementedException();
        }

        public ILink<IProtocol> CreateProtocol(ProtocolType type)
        {
            throw new NotImplementedException();
        }
    }
}