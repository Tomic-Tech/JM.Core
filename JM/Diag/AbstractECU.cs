using System;
using System.Collections.Generic;

namespace JM.Diag
{
    public abstract class AbstractECU
    {
        public enum ActiveState
        {
            Positive,
            Negative,
            Idle,
            Stop
        }

        public delegate string DataCalcDelegate(byte[] recv);
        public delegate string ActiveTest(bool on);

        private Dictionary<string, DataCalcDelegate> dataStreamCalc;
        private Dictionary<string, DataCalcDelegate> troubleCodeCalc;
        private Dictionary<string, ActiveTest> activeTests;
        private ICommbox commbox;
        private IProtocol protocol;
        private IPack pack;
        protected bool stopReadDataStream;
        private ActiveState activeOn;

        public ActiveState ActiveOn
        {
            get { return activeOn; }
            set { activeOn = value; }
        }

        public AbstractECU(ICommbox commbox)
        {
            this.commbox = commbox;
            this.protocol = null;
            this.pack = new NoPack();
            stopReadDataStream = false;
        }

        public void StopReadDataStream()
        {
            stopReadDataStream = true;
        }

        protected ICommbox Commbox
        {
            get
            {
                return commbox;
            }
        }

        protected IProtocol Protocol
        {
            get
            {
                return protocol;
            }
            set
            {
                protocol = value;
            }
        }

        protected IPack Pack
        {
            get
            {
                return pack;
            }
            set
            {
                pack = value;
            }
        }

        protected Dictionary<string, DataCalcDelegate> DataStreamCalc
        {
            get
            {
                return dataStreamCalc;
            }
            set
            {
                dataStreamCalc = value;
            }
        }

        public Dictionary<string, DataCalcDelegate> TroubleCodeCalc
        {
            get
            {
                return troubleCodeCalc;
            }
            set
            {
                troubleCodeCalc = value;
            }
        }


        protected Dictionary<string, ActiveTest> ActiveTests
        {
            get
            {
                return activeTests;
            }
            set
            {
                activeTests = value;
            }
        }
    }
}

