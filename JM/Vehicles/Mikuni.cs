using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using JM.Core;
using JM.Diag;

namespace JM.Vehicles
{
    public class Mikuni : AbstractECU
    {
        private Dictionary<int, byte[]> failureCmds;
        private Dictionary<int, DataCalcDelegate> failureCalc;
        private MikuniOptions options;

        public Mikuni(ICommbox commbox, MikuniOptions options)
            : base(commbox)
        {
            this.options = options;

            FailureCmdsInit();

            ProtocolInit();

            DataStreamInit();
        }

        private void ProtocolInit()
        {
            Protocol = Commbox.CreateProtocol(ProtocolType.MIKUNI);

            if (Protocol == null)
            {
                throw new Exception(Database.GetText("Not Protocol", "System"));
            }

            Pack = new MikuniPack();

            if (!Protocol.Config(options))
            {
                throw new Exception(Database.GetText("Communication Fail", "System"));
            }
        }

        private void DataStreamInit()
        {
            DataStreamCalc = new Dictionary<string, DataCalcDelegate>();
            DataStreamCalc["ER"] = (recv) =>
            {
				return string.Format("{0:F0}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16)) * 500) / 256);
                //return string.Format("{0}", (Convert.ToUInt16(Encoding.ASCII.GetString(recv), 16) / 256) * 500);
            };
            DataStreamCalc["BV"] = (recv) =>
            {
				return string.Format("{0:F1}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16))) / 512);
            };
            DataStreamCalc["TPS"] = (recv) =>
            {
				return string.Format("{0:F1}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16))) / 512);
            };
            DataStreamCalc["MAT"] = (recv) =>
            {
				return string.Format("{0:F1}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16))) / 256 - 50);
            };
            DataStreamCalc["ET"] = (recv) =>
            {
				return string.Format("{0:F1}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16))) / 256 - 50);
            };
            DataStreamCalc["BP"] = (recv) =>
            {
				return string.Format("{0:F1}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16))) / 512);
            };
            DataStreamCalc["MP"] = (recv) =>
            {
				return string.Format("{0:F1}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16))) / 512);
            };
            DataStreamCalc["IT"] = (recv) =>
            {
				return string.Format("{0:F1}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16)) * 15) / 256 - 22.5);
                //return string.Format("{0}", (Convert.ToUInt16(Encoding.ASCII.GetString(recv), 16) / 256) * 15 - 22.5);
            };
            DataStreamCalc["IPW"] = (recv) =>
            {
				return string.Format("{0:F0}", (Convert.ToDouble(Convert.ToInt32(Encoding.ASCII.GetString(recv), 16))) / 2);
            };
            DataStreamCalc["TS"] = (recv) =>
            {
                if ((recv[0] & 0x40) != 0)
                {
                    return Database.GetText("Tilt", "System");
                }
                else
                {
                    return Database.GetText("No Tilt", "System");
                }
            };
            DataStreamCalc["ERF"] = (recv) =>
            {
				if ((Convert.ToUInt16(Encoding.ASCII.GetString(recv)) & 0x0001) == 1)
                {
                    return Database.GetText("Running", "System");
                }
                else
                {
                    return Database.GetText("Stopped", "System");
                }
            };
            DataStreamCalc["IS"] = (recv) =>
            {
                if ((recv[0] & 0x40) != 0)
                {
                    return Database.GetText("Idle", "System");
                }
                else
                {
                    return Database.GetText("Not Idle", "System");
                }
            };
        }

        private void FailureCmdsInit()
        {
            failureCmds = new Dictionary<int, byte[]>(15);
            failureCalc = new Dictionary<int, DataCalcDelegate>(15);

            failureCmds.Add(1, Database.GetCommand("Manifold Pressure Failure", "Mikuni"));
            failureCalc.Add(1, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Low
                {
                    return "0040";
                }
                else if ((data & 0xE000) != 0) // High
                {
                    return "0080";
                }
                return "0000";
            });

            failureCmds.Add(2, Database.GetCommand("O2 Sensor Failure", "Mikuni"));
            failureCalc.Add(2, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Low
                {
                    return "0140";
                }
                else if ((data & 0xE000) != 0) // High
                {
                    return "0180";
                }
                return "0000";
            });

            failureCmds.Add(3, Database.GetCommand("TPS Sensor Failure", "Mikuni"));
            failureCalc.Add(3, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Low
                {
                    return "0240";
                }
                else if ((data & 0xE000) != 0) // High
                {
                    return "0280";
                }
                return "0000";
            });

            failureCmds.Add(4, Database.GetCommand("Sensor Source Failure", "Mikuni"));
            failureCalc.Add(4, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Low
                {
                    return "0340";
                }
                else if ((data & 0xE000) != 0) // High
                {
                    return "0380";
                }
                return "0000";
            });

            failureCmds.Add(5, Database.GetCommand("Battery Voltage Failure", "Mikuni"));
            failureCalc.Add(5, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Low
                {
                    return "0540";
                }
                else if ((data & 0xE000) != 0) // High
                {
                    return "0580";
                }
                return "0000";
            });

            failureCmds.Add(
                6,
                Database.GetCommand("Engine Temperature Sensor Failure", "Mikuni")
            );
            failureCalc.Add(6, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Low
                {
                    return "0640";
                }
                else if ((data & 0xE000) != 0) // High
                {
                    return "0680";
                }
                return "0000";
            });

            failureCmds.Add(7, Database.GetCommand("Manifold Temperature Failure", "Mikuni"));
            failureCalc.Add(7, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Low
                {
                    return "0740";
                }
                else if ((data & 0xE000) != 0) // High
                {
                    return "0780";
                }
                return "0000";
            });

            failureCmds.Add(8, Database.GetCommand("Tilt Sensor Failure", "Mikuni"));
            failureCalc.Add(8, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Low
                {
                    return "0840";
                }
                else if ((data & 0xE000) != 0) // High
                {
                    return "0880";
                }
                return "0000";
            });

            failureCmds.Add(9, Database.GetCommand("DCP Failure", "Mikuni"));
            failureCalc.Add(9, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Short
                {
                    return "2040";
                }
                else if ((data & 0xE000) != 0) // Open
                {
                    return "2080";
                }
                return "0000";
            });

            failureCmds.Add(10, Database.GetCommand("Ignition Coil Failure", "Mikuni"));
            failureCalc.Add(10, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Short
                {
                    return "2140";
                }
                else if ((data & 0xE000) != 0) // Open
                {
                    return "2180";
                }
                return "0000";
            });

            failureCmds.Add(11, Database.GetCommand("O2 Heater Failure", "Mikuni"));
            failureCalc.Add(11, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Short
                {
                    return "2240";
                }
                else if ((data & 0xE000) != 0) // Open
                {
                    return "2280";
                }
                return "0000";
            });

            failureCmds.Add(12, Database.GetCommand("EEPROM Failure", "Mikuni"));
            failureCalc.Add(12, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Write
                {
                    return "4040";
                }
                else if ((data & 0xE000) != 0) // Read
                {
                    return "4080";
                }
                return "0000";
            });

            failureCmds.Add(13, Database.GetCommand("Air Valve Failure", "Mikuni"));
            failureCalc.Add(13, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Short
                {
                    return "2340";
                }
                else if ((data & 0xE000) != 0) // Open
                {
                    return "2380";
                }
                return "0000";
            });

            failureCmds.Add(14, Database.GetCommand("SAV Failure", "Mikuni"));
            failureCalc.Add(14, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0x1C00) != 0) // Short
                {
                    return "2440";
                }
                else if ((data & 0xE000) != 0) // Open
                {
                    return "2480";
                }
                return "0000";
            });

            failureCmds.Add(15, Database.GetCommand("CPS Failure", "Mikuni"));
            failureCalc.Add(15, recv =>
            {
                uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv), 16);
                if ((data & 0xE000) != 0)
                    return "0940";
                return "0000";
            });
        }

        public List<TroubleCode> ReadCurrentTroubleCode()
        {
            byte[] cmd = Database.GetCommand("Synthetic Failure", "Mikuni");
            byte[] result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);

            if (result == null)
            {
                throw new IOException(Database.GetText("Read Trouble Code Fail", "System"));
            }

            if (result[0] != 0x30 || result[1] != 0x30 || result[2] != 0x30 || result[3] != 0x30)
            {
                List<TroubleCode> ret = new List<TroubleCode>();
                for (int i = 1; i <= 15; i++)
                {
                    result = Protocol.SendAndRecv(failureCmds[i], 0, failureCmds[i].Length, Pack);
                    if (result == null)
                    {
                        throw new IOException(Database.GetText("Read Trouble Code Fail", "System"));
                    }

                    if (result[0] != 0x30 || result[1] != 0x30 || result[2] != 0x30 || result[3] != 0x30)
                    {
                        string code = failureCalc[i](result);
                        ret.Add(Database.GetTroubleCode(code, "Mikuni"));
                    }
                }
                return ret;
            }
            throw new IOException(Database.GetText("None Trouble Code", "System"));
        }

        public List<TroubleCode> ReadHistoryTroubleCode()
        {
            byte[] cmd = Database.GetCommand("Failure History Pointer", "Mikuni");
            byte[] result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (result == null)
            {
                throw new IOException(Database.GetText("Read Trouble Code Fail", "System"));
            }

            List<TroubleCode> ret = new List<TroubleCode>();
            int pointer = Convert.ToInt32(Encoding.ASCII.GetString(result));

            for (int i = 0; i < 16; i++)
            {
                string name;
                int temp = pointer - i - 1;
                if (temp >= 0)
                {
                    name = string.Format("Failure History Buffer{0}", temp);
                }
                else
                {
                    name = string.Format("Failure History Buffer{0}", pointer + 15 - i);
                }
                cmd = Database.GetCommand(name, "Mikuni");
                result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (result == null)
                {
                    throw new IOException(Database.GetText("Read Trouble Code Fail", "System"));
                }
                if (result[0] != 0x30 || result[1] != 0x30 || result[2] != 0x30 || result[3] != 0x30)
                {
                    string code = Encoding.ASCII.GetString(result);
                    ret.Add(Database.GetTroubleCode(code, "Mikuni"));
                }
            }
            if (ret.Count == 0)
            {
                throw new IOException(Database.GetText("None Trouble Code", "System"));
            }
            return ret;
        }

        public void ClearTroubleCode()
        {
            byte[] cmd = Database.GetCommand("Failure History Clear", "Mikuni");
            byte[] result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (result == null || result[0] != 'A')
            {
                throw new IOException(Database.GetText("Clear Trouble Code Fail", "System"));
            }

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));
            cmd = Database.GetCommand("Failure History Pointer", "Mikuni");
            result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (result == null || result[0] != '0' || result[1] != '0' || result[2] != '0' || result[3] != '0')
            {
                throw new IOException(Database.GetText("Clear Trouble Code Fail", "System"));
            }

            for (int i = 0; i < 16; i++)
            {
                string name = string.Format("Failure History Buffer{0}", i);
                cmd = Database.GetCommand(name, "Mikuni");
                result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (result == null || result[0] != '0' || result[1] != '0' || result[2] != '0' || result[3] != '0')
                {
                    throw new IOException(Database.GetText("Clear Trouble Code Fail", "System"));
                }
            }
        }

        public string GetECUVersion ()
		{
			byte[] cmd = Database.GetCommand ("Read ECU Version 1", "Mikuni");
			byte[] result = Protocol.SendAndRecv (cmd, 0, cmd.Length, Pack);
			if (result == null) {
				throw new IOException (Database.GetText ("Read ECU Version Fail", "System"));
			}

			cmd = Database.GetCommand ("Read ECU Version 2", "Mikuni");
			Array.Copy (result, 0, cmd, 2, 4);
			result = Protocol.SendAndRecv (cmd, 0, cmd.Length, Pack);
			if (result == null) {
				throw new IOException (Database.GetText ("Read ECU Version Fail", "System"));
			}

			return Encoding.ASCII.GetString (result);
        }

        public void TPSIdleSetting()
        {
            byte[] cmd = Database.GetCommand("Engine Revolutions", "Mikuni");
            byte[] result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (result == null)
            {
                throw new IOException(Database.GetText("Read Engine RPM Fail", "System"));
            }

            if (result[0] != '0' || result[1] != '0' || result[2] != '0' || result[3] != '0')
            {
                throw new IOException(Database.GetText("Engine RPM Not Zero", "System"));
            }

            cmd = Database.GetCommand("TPS Idle Adjustment", "Mikuni");
            result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (result == null || result[0] != 'A')
            {
                throw new IOException(Database.GetText("TPS Idle Setting Fail", "Mikuni"));
            }
        }

        public void LongTermLearnValueZoneInitialization()
        {
            byte[] cmd = Database.GetCommand("Long Term Learn Value Zone Initialization", "Mikuni");
            byte[] result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (result == null || result[0] != 'A')
            {
                throw new IOException(Database.GetText("Long Term Learn Value Zone Initialization Fail", "Mikuni"));
            }

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));

            for (int i = 1; i < 11; i++)
            {
                string name = string.Format("Long Term Learn Value Zone_{0}", i);
                cmd = Database.GetCommand(name, "Mikuni");
                result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (result == null || result[0] != '0' || result[1] != '0' || result[2] != '8' || result[3] != '0')
                    throw new IOException(Database.GetText("Long Term Learn Value Zone Initialization Fail", "Mikuni"));
            }

        }

        public void ISCLearnValueInitialize()
        {
            byte[] cmd = Database.GetCommand("ISC Learn Value Initialization", "Mikuni");
            byte[] result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (result == null || result[0] != 'A')
            {
                throw new IOException(Database.GetText("ISC Learn Value Initialization Fail", "Mikuni"));
            }
        }

        public void ReadDataStream(Core.LiveDataVector vec)
        {
            stopReadDataStream = false;

            var items = vec.Items;
            //int i = 0;

            while (!stopReadDataStream)
            {
                foreach (var item in items)
                {
                    byte[] cmd = Database.GetCommand(item.CommandName, item.CommandClass);
                    byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                    if (recv == null)
                    {
                        throw new IOException(Database.GetText("Communication Fail", "System"));
                    }
                    // calc
                    item.Value = DataStreamCalc[item.ShortName](recv);
                    if (stopReadDataStream)
                        break;
                }
                //byte[] cmd = Database.GetCommand(items[i].CmdID);
                //byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                //if (recv == null)
                //{
                //    throw new IOException(JM.Core.SysDatabase.GetText("Communication Fail"));
                //}
                //// calc
                //items[i].Value = DataStreamCalc[vec[i].ShortName](recv);
                //i++;
                //if (i == items.Count)
                //{
                //    i = 0;
                //}
            }
        }

        public void StaticDataStream(Core.LiveDataVector vec)
        {
            var items = vec.Items;
            foreach (var item in items)
            {
                byte[] cmd = Database.GetCommand(item.CommandName, item.CommandClass);
                byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (recv == null)
                {
                    throw new IOException(Database.GetText("Communication Fail", "System"));
                }
                item.Value = DataStreamCalc[item.ShortName](recv);
            }
            //vec.DeployShowedIndex();

            //for (int i = 0; i < vec.ShowedCount; i++)
            //{
            //    int index = vec.ShowedIndex(i);
            //    byte[] cmd = Database.GetCommand(vec[index].CommandName, vec[index].CommandClass);
            //    byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            //    if (recv == null)
            //    {
            //        throw new IOException(Database.GetText("Communication Fail", "System"));
            //    }
            //    // calc
            //    vec[index].Value = DataStreamCalc[vec[index].ShortName](recv);
            //}
        }
    }
}
