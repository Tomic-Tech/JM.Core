using System;
using System.Collections.Generic;
using System.Text;

namespace JM.Diag.V1
{
    internal abstract class Protocol
    {
        private ICommbox box;

        public Protocol(ICommbox box)
        {
            this.box = box;
        }

        public ICommbox Box
        {
            get { return box; }
        }

        public byte PWC
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.PWC;
                }
                else
                {
                    return C168.Constant.PWC;
                }
            }
        }

        public byte RZFC
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.RZFC;
                }
                else
                {
                    return C168.Constant.RZFC;
                }
            }
        }

        public byte CK
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.CK;
                }
                else
                {
                    return C168.Constant.CK;
                }
            }
        }

        public byte REFC
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.REFC;
                }
                else
                {
                    return C168.Constant.REFC;
                }
            }
        }

        public byte SET_NULL
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SET_NULL;
                }
                else
                {
                    return C168.Constant.SET_NULL;
                }
            }
        }

        public byte SK_NO
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SK_NO;
                }
                else
                {
                    return C168.Constant.SK_NO;
                }
            }
        }

        public byte SK0
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SK0;
                }
                else
                {
                    return C168.Constant.SK0;
                }
            }
        }

        public byte SK1
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SK1;
                }
                else
                {
                    return C168.Constant.SK1;
                }
            }
        }

        public byte SK2
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SK2;
                }
                else
                {
                    return C168.Constant.SK2;
                }
            }
        }

        public byte SK3
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SK3;
                }
                else
                {
                    return C168.Constant.SK3;
                }
            }
        }

        public byte SK4
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SK4;
                }
                else
                {
                    return C168.Constant.SK4;
                }
            }
        }

        public byte SK5
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SK5;
                }
                else
                {
                    return C168.Constant.SK5;
                }
            }
        }

        public byte SK6
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SK6;
                }
                else
                {
                    return C168.Constant.SK6;
                }
            }
        }

        public byte SK7
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SK7;
                }
                else
                {
                    return C168.Constant.SK7;
                }
            }
        }

        public byte RK_NO
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.RK_NO;
                }
                else
                {
                    return C168.Constant.RK_NO;
                }
            }
        }

        public byte RK0
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.RK0;
                }
                else
                {
                    return C168.Constant.RK0;
                }
            }
        }

        public byte RK1
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.RK1;
                }
                else
                {
                    return C168.Constant.RK1;
                }
            }
        }

        public byte RK2
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.RK2;
                }
                else
                {
                    return C168.Constant.RK2;
                }
            }
        }

        public byte RK3
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.RK3;
                }
                else
                {
                    return C168.Constant.RK3;
                }
            }
        }

        public byte RK4
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.RK4;
                }
                else
                {
                    return C168.Constant.RK4;
                }
            }
        }

        public byte RK5
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.RK5;
                }
                else
                {
                    return C168.Constant.RK5;
                }
            }
        }

        public byte RK6
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.RK6;
                }
                else
                {
                    return C168.Constant.RK6;
                }
            }
        }

        public byte RK7
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.RK7;
                }
                else
                {
                    return C168.Constant.RK7;
                }
            }
        }

        public byte RS_232
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.RS_232;
                }
                else
                {
                    return C168.Constant.RS_232;
                }
            }
        }

        public byte BIT9_MARK
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.BIT9_MARK;
                }
                else
                {
                    return C168.Constant.BIT9_MARK;
                }
            }
        }

        public byte BIT9_EVEN
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.BIT9_EVEN;
                }
                else
                {
                    return C168.Constant.BIT9_EVEN;
                }
            }
        }

        public byte BIT9_ODD
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.BIT9_ODD;
                }
                else
                {
                    return C168.Constant.BIT9_ODD;
                }
            }
        }

        public byte BIT9_SPACE
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.BIT9_SPACE;
                }
                else
                {
                    return C168.Constant.BIT9_SPACE;
                }
            }
        }

        public byte SEL_SL
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SEL_SL;
                }
                else
                {
                    return C168.Constant.SEL_SL;
                }
            }
        }

        public byte UN_DB20
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.UN_DB20;
                }
                else
                {
                    return C168.Constant.UN_DB20;
                }
            }
        }

        public byte SETBYTETIME
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SETBYTETIME;
                }
                else
                {
                    return C168.Constant.SETBYTETIME;
                }
            }
        }

        public byte SETWAITTIME
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SETWAITTIME;
                }
                else
                {
                    return C168.Constant.SETWAITTIME;
                }
            }
        }

        public byte SETRECBBOUT
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SETRECBBOUT;
                }
                else
                {
                    return C168.Constant.SETRECBBOUT;
                }
            }
        }

        public byte SETRECFROUT
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SETRECFROUT;
                }
                else
                {
                    return C168.Constant.SETRECFROUT;
                }
            }
        }

        public byte SETLINKTIME
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SETLINKTIME;
                }
                else
                {
                    return C168.Constant.SETLINKTIME;
                }
            }
        }

        public byte COMS
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.COMS;
                }
                else
                {
                    return C168.Constant.COMS;
                }
            }
        }

        public byte REC_FR
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.REC_FR;
                }
                else
                {
                    return C168.Constant.REC_FR;
                }
            }
        }

        public byte REC_LEN_1
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.REC_LEN_1;
                }
                else
                {
                    return C168.Constant.REC_LEN_1;
                }
            }
        }

        public byte SET55_BAUD
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SET55_BAUD;
                }
                else
                {
                    return C168.Constant.SET55_BAUD;
                }
            }
        }

        public byte SET_DB20
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.SET_DB20;
                }
                else
                {
                    return C168.Constant.SET_DB20;
                }
            }
        }

        public byte INVERTBYTE
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.INVERTBYTE;
                }
                else
                {
                    return C168.Constant.INVERTBYTE;
                }
            }
        }

        public abstract void FinishExecute(bool isFinish);
    }
}
