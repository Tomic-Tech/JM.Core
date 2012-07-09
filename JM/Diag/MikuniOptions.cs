using System;
using System.Collections.Generic;
using System.Text;

namespace JM.Diag
{
    public class MikuniOptions
    {
        private MikuniParity parity;

        public MikuniParity Parity
        {
            get { return parity; }
            set { parity = value; }
        }

        public MikuniOptions()
        {
            Parity = MikuniParity.None;
        }
    }
}
