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
                if (box is W80.Commbox)
                {
                    return W80.Commbox.PWC;
                }
                return 0;
            }
        }

        public byte RZFC
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.RZFC;
                }
                return 0;
            }
        }

        public byte CK
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.CK;
                }
                return 0;
            }
        }

        public byte REFC
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.REFC;
                }
                return 0;
            }
        }

        public byte SET_NULL
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.SET_NULL;
                }
                return 0;
            }
        }

        public byte SK_NO
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.SK_NO;
                }
                return 0;
            }
        }

        public byte SK0
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.SK0;
                }
                return 0;
            }
        }

        public byte SK1
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.SK1;
                }
                return 0;
            }
        }

        public byte SK2
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.SK2;
                }
                return 0;
            }
        }

        public byte SK3
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.SK3;
                }
                return 0;
            }
        }

        public byte SK4
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.SK4;
                }
                return 0;
            }
        }

        public byte SK5
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.SK5;
                }
                return 0;
            }
        }

        public byte SK6
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.SK6;
                }
                return 0;
            }
        }

        public byte SK7
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.SK7;
                }
                return 0;
            }
        }

        public byte RK_NO
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.RK_NO;
                }
                return 0;
            }
        }

        public byte RK0
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.RK0;
                }
                return 0;
            }
        }

        public byte RK1
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.RK1;
                }
                return 0;
            }
        }

        public byte RK2
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.RK2;
                }
                return 0;
            }
        }

        public byte RK3
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.RK3;
                }
                return 0;
            }
        }

        public byte RK4
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.RK4;
                }
                return 0;
            }
        }

        public byte RK5
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.RK5;
                }
                return 0;
            }
        }

        public byte RK6
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.RK6;
                }
                return 0;
            }
        }

        public byte RK7
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.RK7;
                }
                return 0;
            }
        }

        public byte RS_232
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.RS_232;
                }
                return 0;
            }
        }

        public byte BIT9_MARK
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.BIT9_MARK;
                }
                return 0;
            }
        }

        public byte SEL_SL
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.SEL_SL;
                }
                return 0;
            }
        }

        public byte UN_DB20
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.UN_DB20;
                }
                return 0;
            }
        }

        public byte SETBYTETIME
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.SETBYTETIME;
                }
                return 0;
            }
        }

        public byte SETWAITTIME
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.SETWAITTIME;
                }
                return 0;
            }
        }

        public byte SETRECBBOUT
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.SETRECBBOUT;
                }
                return 0;
            }
        }

        public byte SETRECFROUT
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.SETRECFROUT;
                }
                return 0;
            }
        }

        public byte SETLINKTIME
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.LINKBLOCK;
                }
                return 0;
            }
        }

        public byte COMS
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.COMS;
                }
                return 0;
            }
        }

        public byte REC_FR
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.REC_FR;
                }
                return 0;
            }
        }

        public byte REC_LEN_1
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.REC_LEN_1;
                }
                return 0;
            }
        }

        public byte SET55_BAUD
        {
            get
            {
                if (box is W80.Commbox)
                {
                    return W80.Commbox.SET55_BAUD;
                }
                return 0;
            }
        }

        public abstract void FinishExecute(bool isFinish);
    }
}
