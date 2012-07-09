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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
            }
        }

        public byte SETLINKTIME
        {
            get
            {
                if (box is W80.Commbox<SerialPortStream>)
                {
                    return W80.Constant.LINKBLOCK;
                }
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
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
                return 0;
            }
        }

        public abstract void FinishExecute(bool isFinish);
    }
}
