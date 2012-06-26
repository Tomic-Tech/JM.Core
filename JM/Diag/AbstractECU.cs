using System;
using System.Collections.Generic;

namespace JM.Diag
{
    public abstract class AbstractECU
    {
        public delegate string CalcDataStream(byte[] recv);

        public delegate string ActiveTest(bool on);

        private Dictionary<string, CalcDataStream> calcDatas;
        private Dictionary<string, ActiveTest> activeTests;
        private Core.VehicleDB db;
        private ICommbox commbox;
        private IProtocol protocol;
        private IPack pack;
        protected bool stopReadDataStream;

        public AbstractECU(Core.VehicleDB db, ICommbox commbox)
        {
            this.db = db;
            this.commbox = commbox;
            this.protocol = null;
            this.pack = new NoPack();
            stopReadDataStream = false;
        }

        public void StopReadDataStream()
        {
            stopReadDataStream = true;
        }

        protected Core.VehicleDB Db
        {
            get
            {
                return db;
            }
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

        protected Dictionary<string, CalcDataStream> CalcDatas
        {
            get
            {
                return calcDatas;
            }
            set
            {
                calcDatas = value;
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

