using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using JM.Core;
using JM.Diag;

namespace JM.Vehicles
{
    public class GW250 : AbstractECU
    {
        public const int TesterID = 0xF1;
        public const int ECUID = 0x12;
        private KWPOptions options = new KWPOptions();
        private readonly byte[] startCommunication;
        public GW250(ICommbox commbox)
            : base(commbox)
        {
            startCommunication = Database.GetCommand("Enter1", "GW250");

            DataStreamInit();
            
            ProtocolInit();
        }

        private void DataStreamInit()
        {
            DataStreamCalc = new Dictionary<string, DataCalcDelegate>();
            DataStreamCalc["DTCs"] = (recv) => 
            {
                return string.Format("{0}", Convert.ToUInt32(recv[2]));
            };
            DataStreamCalc["RPM"] = (recv) => 
            {
                return string.Format("{0}", (Convert.ToUInt32(recv[13]) * 256 + Convert.ToUInt32(recv[14])) * 100 / 255);
            };
            DataStreamCalc["TOD"] = (recv) => 
            {
                return string.Format("{0}", (Convert.ToUInt32(recv[15]) * 125) / 255);
            };
            DataStreamCalc["MAP#1"] = (recv) => 
            {
                return string.Format("{0:F1}", (Convert.ToDouble(recv[16]) * 165.7) / 255 - 20);
            };
            DataStreamCalc["TOC"] = (recv) => 
            {
                return string.Format("{0:F1}", (Convert.ToDouble(recv[17]) * 160) / 255 - 30);
            };
            DataStreamCalc["MT"] = (recv) => 
            {
                return string.Format("{0:F1}", (Convert.ToDouble(recv[18]) * 160) / 255 - 30);
            };
            DataStreamCalc["BV"] = (recv) => 
            {
                return string.Format("{0:F1}", (Convert.ToDouble(recv[20]) / 255));
            };
            DataStreamCalc["OV"] = (recv) => 
            {
                return string.Format("{0:F1}", (Convert.ToDouble(recv[21]) * 5) / 255);
            };
            DataStreamCalc["MAP#2"] = (recv) => 
            {
                return string.Format("{0:F1}", (Convert.ToDouble(recv[23]) * 166.7) / 255 - 20);
            };
        }

        private void ProtocolInit()
        {
            Protocol = Commbox.CreateProtocol(ProtocolType.ISO14230);

            if (Protocol == null)
            {
                throw new Exception(Database.GetText("Not Protocol", "System"));
            }

            options.Baudrate = 10416;
            options.ComLine = 7;
            options.SourceAddress = TesterID;
            options.TargetAddress = ECUID;
            options.MsgMode = KWPMode.Mode8X;
            options.LinkMode = KWPMode.Mode8X;
            options.StartType = KWPStartType.Fast;

            Pack = new KWPPack();
            Pack.Config(options);

            options.FastCmd = Pack.Pack(startCommunication, 0, startCommunication.Length);

            if (!Protocol.Config(options))
            {
                throw new Exception(Database.GetText("Communication Fail", "System"));
            }

            //byte[] keepLink = Database.GetCommand("Keep Link", "GW250");

            //Protocol.SetKeepLink(keepLink, 0, keepLink.Length, Pack);
            //Protocol.KeepLink(true);
        }

        public List<TroubleCode> ReadTroubleCode(bool isHistory)
        {
            byte[] cmd = Database.GetCommand("Read Trouble Code", "GW250");
            byte[] result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);

            if (result == null || result[0] != 0x58)
            {
                throw new IOException(Database.GetText("Read Trouble Code Fail", "System"));
            }

            uint dtcNum = Convert.ToUInt32(result[1]);

            List<TroubleCode> tcs = new List<TroubleCode>();

            if (dtcNum == 0)
            {
                throw new IOException(Database.GetText("None Trouble Code", "System"));
            }

            for (int i = 0; i < dtcNum; i++)
            {
                if (!isHistory)
                {
                    if ((result[i * 3 + 4] & 0x40) == 0)
                    {
                        continue;
                    }
                }
                string code = Utils.CalcStdObdTroubleCode(result, i, 3, 2);
                tcs.Add(Database.GetTroubleCode(code, "GW250"));
            }

            if (tcs.Count == 0)
            {
                throw new IOException(Database.GetText("None Trouble Code", "System"));
            }
            return tcs;
        }

        public void ClearTroubleCode()
        {
            byte[] cmd = Database.GetCommand("Clear Trouble Code", "GW250");
            byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);

            if (recv == null || recv[0] != 0x54)
            {
                throw new IOException(Database.GetText("Clear Trouble Code Fail", "System"));
            }
        }

        public void ReadDataStream(LiveDataVector vec)
        {
            var items = vec.Items;
            byte[] cmd = Database.GetCommand("Read Data Stream", "GW250");
            stopReadDataStream = false;

            byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);

            if (recv == null)
            {
                throw new IOException(Database.GetText("Communication Fail", "System"));
            }

            Task task = Task.Factory.StartNew(() => 
            {
                while (!stopReadDataStream)
                {
                    Thread.Sleep(50);
                    Thread.Yield();
                    byte[] temp = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                    if (temp != null)
                    {
                        Array.Copy(temp, recv, recv.Length >= temp.Length ? recv.Length : temp.Length);
                    }
                }
            });

            while (!stopReadDataStream)
            {
                foreach (var item in items)
                {
                    item.Value = DataStreamCalc[item.ShortName](recv);
                    Thread.Sleep(10);
                    Thread.Yield();
                    if (stopReadDataStream)
                        break;
                }
            }

            task.Wait();
        }
    }
}
