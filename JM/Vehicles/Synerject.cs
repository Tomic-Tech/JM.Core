using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using JM.Core;
using JM.Diag;

namespace JM.Vehicles
{
    public class Synerject : AbstractECU
    {
        public const int TesterID = 0xF1;
        public const int Physical = 0x11;
        public const int Functional = 0x10;
        public const int OBDServices = 0x33;
        private const byte ReturnControlToECU = 0x00;
        private const byte ReportCurrentState = 0x01;
        private const byte ResetToDefault = 0x04;
        private const byte ShortTermAdjustments = 0x07;
        private const byte LongTermAdjustments = 0x08;
        private readonly byte[] keepLink;
        private readonly byte[] startCommunication;
        private readonly byte[] startDiagnosticSession;
        private readonly byte[] stopDiagnosticSession;
        private readonly byte[] stopCommunication;
        private KWPOptions options;
        private LiveDataVector ldVec;

        public Synerject(ICommbox commbox)
            : base(commbox)
        {
            keepLink = Database.GetCommand("KeepLink", "Synerject");
            startCommunication = Database.GetCommand("Start Communication", "Synerject");
            startDiagnosticSession = Database.GetCommand("Start DiagnosticSession", "Synerject");
            stopDiagnosticSession = Database.GetCommand("Stop DiagnosticSession", "Synerject");
            stopCommunication = Database.GetCommand("Stop Communication", "Synerject");

            DataStreamInit();
            ActiveTestInit();

            ProtocolInit();

            ActiveOn = ActiveState.Stop;
        }

        private void ProtocolInit()
        {
            Protocol = Commbox.CreateProtocol(ProtocolType.ISO14230);
            if (Protocol == null)
                throw new Exception(Database.GetText("Not Protocol", "System"));

            options = new KWPOptions();
            options.Baudrate = 10416;
            options.SourceAddress = TesterID;
            options.TargetAddress = Physical;
            options.MsgMode = KWPMode.Mode8X;
            options.LinkMode = KWPMode.Mode8X;
            options.StartType = KWPStartType.Fast;
            options.ComLine = 7;

            Pack = new KWPPack();
            Pack.Config(options);

            options.FastCmd = Pack.Pack(startCommunication, 0, startCommunication.Length);
            if (!Protocol.Config(options))
                throw new Exception(Database.GetText("Communication Fail", "System"));
            //Protocol.SetKeepLink(keepLink, 0, keepLink.Length, Pack);
            //Protocol.KeepLink(true);

        }

        private void DataStreamInit()
        {
            DataStreamCalc = new Dictionary<string, DataCalcDelegate>();

            // recv is the ReadDataByLocalIdentifier positive Response
            // Copy from protocol document
            // Data Byte    Parameter Name                                  Cvt     Hex Value   Mnemonic
            // #1d          readDataByLocalIdentifier Response Service Id   M       #61h        RDBLIPR
            // #2d          recordLocalIdentifier                           M       #XXh        RLI
            // #3d          recordValue#1                                   M       #XXh        RV
            // ...          ...                                             M       ...         ...
            // #nd          recordValue#n                                   M       #XXh        RV
            // Because in array we are starts by 0 index, so recv[0] is #1d, recv[1] is #2d, 
            //   recv[2] is the #3d, recv[3] is the #4d, and so on.

                // Name Size    Conversion (hex/bin)    Conversion (physical)   Resol.
            DataStreamCalc["AMP"] = (recv) =>
            {
                // AMP  2   0...9F6H    0...2550    1
                return string.Format("{0}", Convert.ToUInt32(recv[2] * 256 + recv[3]));
            };

            DataStreamCalc["CRASH"] = (recv) =>
            {
                // CRASH    2   0...3FFH    0...4.9951  5/1024
                return string.Format("{0:F4}", Convert.ToDouble(recv[4] * 256 + recv[5]) * 5 / 1024);
            };

            DataStreamCalc["CTR_ERR_DYN_NR"] = (recv) =>
            {
                // CTR_ERR_DYN_NR   1   0...FFH 0...255 1
                return string.Format("{0}", Convert.ToUInt32(recv[6]));
            };

            DataStreamCalc["CUR_IGC_DIAG_cyl1"] = (recv) =>
            {
                // CUR_IGC_DIAG_cyl1    2   0...3FFH    0...4.9951  5/1024
                return string.Format("{0:F4}", Convert.ToDouble(recv[7] * 256 + recv[8]) * 5 / 1024);
            };

            DataStreamCalc["DIST_ACT_MIL"] = (recv) =>
            {
                // DIST_ACT_MIL 2   0...FFFFH   0...65535   1
                return string.Format("{0}", Convert.ToUInt32(recv[9] * 256 + recv[10]));
            };

            DataStreamCalc["ENG_HOUR"] = (recv) =>
            {
                // ENG_HOUR 2   0...FFFFH   0...5461.25 1/12
                return string.Format("{0:F4}", Convert.ToDouble(recv[11] * 256 + recv[12]) / 12);
            };

            DataStreamCalc["IGA_1"] = (recv) =>
            {
                // IGA_1    1   0...FFH -30...89.53 15/32
                double v = Convert.ToDouble(recv[13]) * 15 / 32 - 30;
                string value = string.Format("{0:F4}", v);
                var minMax = ldVec["IGA_1"].MinMax;
                double min = Convert.ToDouble(minMax["Min"]);
                double max = Convert.ToDouble(minMax["Max"]);
                if (v < min || v > max)
                {
                    ldVec["IGA_1"].OutOfRange = true;
                }
                else
                {
                    ldVec["IGA_1"].OutOfRange = false;
                }
                return value;
            };

            DataStreamCalc["IGA_CTR_IS"] = (recv) =>
            {
                // IGA_CTL_IS   1   0...FFH -30...89.53 15/32
                return string.Format("{0:F4}", Convert.ToDouble(recv[14]) * 15 / 32 - 30);
            };

            DataStreamCalc["INH_IV"] = (recv) =>
            {
                // INH_IV   1   0...3FH 0...63  1
                if ((recv[15] & 0x01) != 0)
                {
                    return Database.GetText("Fuel - Cut", "System");
                }
                else
                {
                    return Database.GetText("Fuel - Not Cut", "System");
                }
            };

            DataStreamCalc["INJ_MODE"] = (recv) =>
            {
                // INJ_MODE 1   0...6H  0...6   1
                switch (recv[16])
                {
                    case 0:
                        return Database.GetText("Ban", "Synerject");
                    case 1:
                        return Database.GetText("Static", "Synerject");
                    case 2:
                        return Database.GetText("Early Fuel Injection", "Synerject");
                    case 3:
                        return Database.GetText("Early Phase Jet", "Synerject");
                    case 4:
                        return Database.GetText("2 Stoke", "Synerject");
                    case 5:
                        return Database.GetText("4 Stoke", "Synerject");
                    case 6:
                        return Database.GetText("4 Stoke Undetermined Phase", "Synerject");
                    default:
                        return "";
                }
            };

            DataStreamCalc["ISA_AD_T_DLY"] = (recv) =>
            {
                // ISA_AD_T_DLY 1   0...FFH -12.8...12.7    0.1
                return string.Format("{0:F4}", Convert.ToDouble(recv[17]) / 10 - 12.8);
            };

            DataStreamCalc["ISA_ANG_DUR_MEC"] = (recv) =>
            {
                //  ISA_ANG_DUR_MEC 2   0...600H    0...720.00  15/32
                return string.Format("{0:F4}", Convert.ToDouble(recv[18] * 256 + recv[19]) * 15 / 32);
            };

            DataStreamCalc["ISA_CTL_IS"] = (recv) =>
            {
                // ISA_CTL_IS   1   0...FFH -120...119.06   15/16
                return string.Format("{0:F4}", Convert.ToDouble(recv[20]) * 15 / 16 - 120);
            };

            DataStreamCalc["ISC_ISA_AD_MV"] = (recv) =>
            {
                // ISC_ISA_AD_MV    1   0...FFH -120...119.06   15/16
                return string.Format("{0:F4}", Convert.ToDouble(recv[21]) * 15 / 16 - 120);
            };

            DataStreamCalc["LAMB_SP"] = (recv) =>
            {
                // LAMB_SP  1   0...FFH 0.5...1.4960    1/256
                return string.Format("{0:F4}", Convert.ToDouble(recv[22]) / 256 + 0.5);
            };

            DataStreamCalc["LV_AFR"] = (recv) =>
            {
                // LV_AFR   1   0...1H  0...1   1
                // bit 7
				if ((recv[23] & 0x80) != 0)
                {
                    return Database.GetText("Thick", "System");
                }
                else
                {
                    return Database.GetText("Thin", "System");
                }
            };

            DataStreamCalc["LV_CELP"] = (recv) =>
            {
                // LV_CELP  1   0...1H  0...1   1
                // bit 6
				if ((recv[23] & 0x40) != 0)
                {
                    return Database.GetText("Yes", "System");
                }
                else
                {
                    return Database.GetText("No", "System");
                }
            };

            DataStreamCalc["LV_CUT_OUT"] = (recv) =>
            {
                // LV_CUT_OUT  1   0...1H  0...1   1
                // bit 5
				if ((recv[23] & 0x20) != 0)
                {
                    return Database.GetText("Oil - Cut", "System");
                }
                else
                {
                    return Database.GetText("Oil - Not Cut", "System");
                }
            };

            DataStreamCalc["LV_EOL_EFP_PRIM"] = (recv) =>
            {
                // LV_EOL_EFP_PRIM  1   0...1H  0...1   1
                // bit 4
				if ((recv[23] & 0x10) != 0)
                {
                    return Database.GetText("Yes", "System");
                }
                else
                {
                    return Database.GetText("No", "System");
                }
            };

            DataStreamCalc["LV_EOL_EFP_PRIM_ACT"] = (recv) =>
            {
                // LV_EOL_EFP_PRIM_ACT 1   0...1H  0...1   1
                // bit 3
				if ((recv[23] & 0x08) != 0)
                {
                    return Database.GetText("Yes", "System");
                }
                else
                {
                    return Database.GetText("No", "System");
                }
            };

            DataStreamCalc["LV_IMMO_PROG"] = (recv) =>
            {
                // LV_IMMO_PROG    1   0...1H  0...1   1
                // bit 2
				if ((recv[23] & 0x04) != 0)
                {
                    return Database.GetText("Yes", "System");
                }
                else
                {
                    return Database.GetText("No", "System");
                }
            };

            DataStreamCalc["LV_IMMO_ECU_PROG"] = (recv) =>
            {
                // LV_IMMO_ECU_PROG 1   0...1H  0...1   1
                // bit 1
				if ((recv[23] & 0x02) != 0)
                {
                    return Database.GetText("Yes", "System");
                }
                else
                {
                    return Database.GetText("No", "System");
                }
            };

            DataStreamCalc["LV_LOCK_IMOB"] = (recv) =>
            {
                // LV_LOCK_IMOB 1   0...1H  0...1   1
                // bit 0
				if ((recv[23] & 0x01) != 0)
                {
                    return Database.GetText("Yes", "System");
                }
                else
                {
                    return Database.GetText("No", "System");
                }
            };

            DataStreamCalc["LV_LSCL_1"] = (recv) =>
            {
                // LV_LSCL_1    1   0...1H  0...1   1
                // bit 4
				if ((recv[24] & 0x10) != 0)
                {
                    return Database.GetText("Yes", "System");
                }
                else
                {
                    return Database.GetText("No", "System");
                }
            };

            DataStreamCalc["LV_LSH_UP_1"] = (recv) =>
            {
                // LV_LSH_UP_1  1   0...1H  0...1   1
                // bit 3
				if ((recv[24] & 0x08) != 0)
                {
                    return Database.GetText("Yes", "System");
                }
                else
                {
                    return Database.GetText("No", "System");
                }
            };

            DataStreamCalc["LV_REQ_ISC"] = (recv) =>
            {
                // LV_REQ_ISC   1   0...1H  0...1   1
                // bit 2
				if ((recv[24] & 0x04) != 0)
                {
                    return Database.GetText("Idle Controlling", "System");
                }
                else
                {
                    return Database.GetText("Idle Not Controlling", "System");
                }
            };

            DataStreamCalc["LV_VIP"] = (recv) =>
            {
                // LV_VIP   1   0...1H  0...1   1
                // bit 1
				if ((recv[24] & 0x02) != 0)
                {
                    return Database.GetText("Yes", "System");
                }
                else
                {
                    return Database.GetText("No", "System");
                }
            };

            DataStreamCalc["LV_EOP"] = (recv) =>
            {
                // LV_EOP
                // bit 0
				if ((recv[24] & 0x01) != 0)
                {
                    return Database.GetText("Yes", "System");
                }
                else
                {
                    return Database.GetText("No", "System");
                }
            };

            DataStreamCalc["MAF"] = (recv) =>
            {
                // MAF  2   0...FFFFH   0...1023.984    1/64
                return string.Format("{0:F4}", Convert.ToDouble(recv[25] * 256 + recv[26]) / 64);
            };

            DataStreamCalc["MAF_THR"] = (recv) =>
            {
                // MAF_THR  2   0...FFFFH   0...1023.984    1/64
                return string.Format("{0:F4}", Convert.ToDouble(recv[27] * 256 + recv[28]) / 64);
            };

            DataStreamCalc["MAP"] = (recv) =>
            {
                // MAP  2   0...9F6H    0...2550    1
                return string.Format("{0}", Convert.ToUInt32(recv[29] * 256 + recv[30]));
            };

            DataStreamCalc["MAP_UP"] = (recv) =>
            {
                // MAP_UP   2   0...9F6H    0...2550    1
                return string.Format("{0}", Convert.ToUInt32(recv[31] * 256 + recv[32]));
            };

            DataStreamCalc["MFF_AD_ADD_MMV_REL"] = (recv) =>
            {
                // MFF_AD_ADD_MMV_REL   2   0...FFFFH   -128...127.9960 1/256
                return string.Format("{0:F4}", Convert.ToDouble(recv[33] * 256 + recv[34]) / 256 - 128);
            };

            DataStreamCalc["MFF_AD_FAC_MMV_REL"] = (recv) =>
            {
                // MFF_AD_FAC_MMV_REL   2   0...FFFFH   -32...31.99902  1/1024
                return string.Format("{0:F4}", Convert.ToDouble(recv[35] * 256 + recv[36]) / 1024 - 32);
            };

            DataStreamCalc["MFF_AD_ADD_MMV"] = (recv) =>
            {
                // MFF_AD_ADD_MMV   2   0...FFFFH   -128...127.9960 1/256
                return string.Format("{0:F4}", Convert.ToDouble(recv[37] * 256 + recv[38]) / 256 - 128);
            };

            DataStreamCalc["MFF_AD_FAC_MMV"] = (recv) =>
            {
                // MFF_AD_FAC_MMV   2.  0...FFFFH   -32...32.99902  1/1024
                return string.Format("{0:F4}", Convert.ToDouble(recv[39] * 256 + recv[40]) / 1024 - 32);
            };

            DataStreamCalc["MFF_INJ_HOM"] = (recv) =>
            {
                // MFF_INJ_HOM  2   0...FFFFH   0...255.9960    1/256
                return string.Format("{0:F4}", Convert.ToDouble(recv[41] * 256 + recv[42]) / 256);
            };

            DataStreamCalc["MFF_WUP_COR"] = (recv) =>
            {
                // MFF_WUP_COR  1   0...FFH 0...0.9960  1/256
                return string.Format("{0:F4}", Convert.ToDouble(recv[43]) / 256);
            };

            DataStreamCalc["MOD_IGA"] = (recv) =>
            {
                // MOD_IGA  1   0H/1H   NOT_PHASE/PHASED    1
                if (recv[44] == 0)
                {
                    return Database.GetText("Undetermined Phase", "System");
                }
                else
                {
                    return Database.GetText("Phase", "System");
                }
            };

            DataStreamCalc["N"] = (recv) => 
            {
                // N    2   0..4650H    0...18000   1
                UInt32 v = Convert.ToUInt32(recv[45] * 256 + recv[46]);
                UInt32 min = Convert.ToUInt32(ldVec["N"].MinMax["Min"]);
                UInt32 max = Convert.ToUInt32(ldVec["N"].MinMax["Max"]);
                if (v < min || v > max)
                {
                    ldVec["N"].OutOfRange = true;
                }
                else
                {
                    ldVec["N"].OutOfRange = false;
                }
                return string.Format("{0}", v);
            };

            DataStreamCalc["N_MAX_THD"] = (recv) =>
            {
                // N_MAX_THD    2   0...4650H   0...18000   1
                return string.Format("{0}", Convert.ToUInt32(recv[47] * 256 + recv[48]));
            };

            DataStreamCalc["N_SP_ISC"] = (recv) =>
            {
                // N_SP_ISC 2   0...FFFFH   -32768...32767  1
                return string.Format("{0}", Convert.ToInt32(recv[49] * 256 + recv[50]) - 32768);
            };

            DataStreamCalc["SOI_1"] = (recv) =>
            {
                // SOI_1    2   0...600H    -180...540.00   15/32
                return string.Format("{0:F4}", Convert.ToDouble(recv[51] * 256 + recv[52]) * 15 / 32 - 180);
            };

            DataStreamCalc["STATE_EFP"] = (recv) =>
            {
                // STATE_EFP    1   0H/1H/2H    EFP_OFF/EFP_ON/EFP_PRIME    1
                if (recv[53] == 0)
                {
                    return Database.GetText("Close", "System");
                }
                else if (recv[53] == 1)
                {
                    return Database.GetText("Open", "System");
                }
                else
                {
                    return Database.GetText("Prime Pump", "System");
                }
            };

            DataStreamCalc["STATE_ENGSTATE"] = (recv) =>
            {
                // STATE_ENGSTATE   1   0H/1H/2H/3H/4H/5H   ES/ST/IS/PL/PU/PUC  1
                switch (recv[54])
                {
                    case 0:
                        return Database.GetText("Stopped", "System");
                    case 1:
                        return Database.GetText("Running", "System");
                    case 2:
                        return Database.GetText("Idle", "System");
                    case 3:
                        return Database.GetText("Part Load", "System");
                    case 4:
                        return Database.GetText("Inverted", "System");
                    case 5:
                        return Database.GetText("Inverted - Cut", "System");
                    default:
                        return "";
                }
            };

            DataStreamCalc["TCO"] = (recv) =>
            {
                // TCO  1   0..FFH  -40...215   1
                Int32 v = Convert.ToInt32(recv[55]) - 40;
                Int32 min = Convert.ToInt32(ldVec["TCO"].MinMax["Min"]);
                Int32 max = Convert.ToInt32(ldVec["TCO"].MinMax["Max"]);
                if (v < min || v > max)
                {
                    ldVec["TCO"].OutOfRange = true;
                }
                else
                {
                    ldVec["TCO"].OutOfRange = false;
                }
                return string.Format("{0}", v);
            };

            DataStreamCalc["TCOPWM"] = (recv) =>
            {
                // TCOPWM   1   0...FFH 0...99.6    25/64
                return string.Format("{0:F4}", Convert.ToDouble(recv[56]) * 25 / 64);
            };

            DataStreamCalc["TD_1"] = (recv) =>
            {
                // TD_1 2   0...FFFFH   0...262.140 0.004
                return string.Format("{0:F4}", Convert.ToDouble(recv[57] * 256 + recv[58]) * 0.004);
            };

            DataStreamCalc["TI_HOM_1"] = (recv) =>
            {
                // TI_HOM_1 2   0...FFFFH   0...262.140 0.004
                return string.Format("{0:F4}", Convert.ToDouble(recv[59] * 256 + recv[60]) * 0.004);
            };

            DataStreamCalc["TI_LAM_COR"] = (recv) => 
            {
                // TI_LAM_COR   2   0...FFFFH   -32...32.99902  1/1024
                double v = Convert.ToDouble(recv[61] * 256 + recv[62]) / 1024 - 32;
                double min = Convert.ToDouble(ldVec["TI_LAM_COR"].MinMax["Min"]);
                double max = Convert.ToDouble(ldVec["TI_LAM_COR"].MinMax["Max"]);
                if (v < min || v > max)
                {
                    ldVec["TI_LAM_COR"].OutOfRange = true;
                }
                else
                {
                    ldVec["TI_LAM_COR"].OutOfRange = false;
                }
                return string.Format("{0:F4}", v);
            };

            DataStreamCalc["TIA"] = (recv) => 
            {
                // TIA  1   0...FFH -40...215   1
                Int32 v = Convert.ToInt32(recv[63]) - 40;
                Int32 min = Convert.ToInt32(ldVec["TIA"].MinMax["Min"]);
                Int32 max = Convert.ToInt32(ldVec["TIA"].MinMax["Max"]);
                if (v < min || v > max)
                {
                    ldVec["TIA"].OutOfRange = true;
                }
                else
                {
                    ldVec["TIA"].OutOfRange = false;
                }
                return string.Format("{0}", v);
            };

            DataStreamCalc["TIA_CYL"] = (recv) => 
            {
                // TIA_CYL  1   0...FFH -40...215   1
                return string.Format("{0}", Convert.ToInt32(recv[64]) - 40);
            };

            DataStreamCalc["TPS_MTC_1"] = (recv) => 
            {
                // TPS_MTC_1    2   0...FFFFH   0...127.9980    1/512
                double v = Convert.ToDouble(recv[65] * 256 + recv[66]) / 512;
                double min = Convert.ToDouble(ldVec["TPS_MTC_1"].MinMax["Min"]);
                double max = Convert.ToDouble(ldVec["TPS_MTC_1"].MinMax["Max"]);
                if (v < min || v > max)
                {
                    ldVec["TPS_MTC_1"].OutOfRange = true;
                }
                else
                {
                    ldVec["TPS_MTC_1"].OutOfRange = false;
                }
                return string.Format("{0:F4}", v);
            };

            DataStreamCalc["V_TPS_AD_BOL_1"] = (recv) => 
            {
                // V_TPS_AD_BOL_1   2   0...3FFH    0...4.9951  5/1024
                return string.Format("{0:F4}", Convert.ToDouble(recv[67] * 256 + recv[68]) * 5 / 1024);
            };

            DataStreamCalc["VBK_MMV"] = (recv) => 
            {
                // VBK_MMV  1   0...FFH 4...19.937  1/16
                double v = Convert.ToDouble(recv[69]) / 16 + 4;
                double min = Convert.ToDouble(ldVec["VBK_MMV"].MinMax["Min"]) + 10;
                double max = Convert.ToDouble(ldVec["VBK_MMV"].MinMax["Max"]);
                if (v < min || v > max)
                {
                    ldVec["VBK_MMV"].OutOfRange = true;
                }
                else
                {
                    ldVec["VBK_MMV"].OutOfRange = false;
                }
                return string.Format("{0:F4}", v);
            };

            DataStreamCalc["VLS_UP_1"] = (recv) => 
            {
                // VLS_UP_1 2   0...3FFH    0...4.9951  5/1024
                double v = Convert.ToDouble(recv[70] * 256 + recv[71]) * 5 / 1024;
                double min = Convert.ToDouble(ldVec["VLS_UP_1"].MinMax["Min"]);
                double max = Convert.ToDouble(ldVec["VLS_UP_1"].MinMax["Max"]);
                if (v < min || v > max)
                {
                    ldVec["VLS_UP_1"].OutOfRange = true;
                }
                else
                {
                    ldVec["VLS_UP_1"].OutOfRange = false;
                }
                return string.Format("{0:F4}", v);
            };

            DataStreamCalc["VS_8"] = (recv) => 
            {
                // VS_8 1   0...FFH 0...255 1
                return string.Format("{0}", Convert.ToUInt32(recv[72]));
            };

            DataStreamCalc["V_TPS_1_BAS"] = (recv) => 
            {
                // V_TPS_1_BAS  2   0...3FFH    0...4.9951  5/1024
                return string.Format("{0:F4}", Convert.ToDouble(recv[73] * 256 + recv[74] * 5 / 1024));
            };

            DataStreamCalc["LV_SAV"] = (recv) => 
            {
                // LV_SAV   1   0...1H  0...1   -
                if (recv[75] == 0)
                {
                    return Database.GetText("Yes", "System");
                }
                else
                {
                    return Database.GetText("No", "System");
                }
            };
        }

        private void ActiveTestInit()
        {
            ActiveTests = new Dictionary<string, ActiveTest>();
            ActiveTests[Database.GetText("Injector", "System")] = (on) =>
            {
                byte[] cmd = Database.GetCommand("Activate Injector", "Synerject");

                if (!on)
                {
                    cmd[3] = 0x00;
                }
                else
                {
                    cmd[3] = 0x01;
                }

                byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (recv[0] != 0x7F)
                {
                    if (!on)
                        return Database.GetText("Injector Off Test Finish", "QingQi");
                    return Database.GetText("Injector On Test Finish", "QingQi");
                }
                throw new IOException(Database.GetText("Active Test Fail", "System"));
            };

            ActiveTests[Database.GetText("Ignition Coil", "System")] = (on) =>
            {
                byte[] cmd = Database.GetCommand("Activate Ignition Coil", "Synerject");

                if (!on)
                {
                    cmd[3] = 0x00;
                }

                byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (recv[0] != 0x7F)
                {
                    if (!on)
                        return Database.GetText("Ignition Coil Off Test Finish", "QingQi");
                    return Database.GetText("Ignition Coil On Test Finish", "QingQi");
                }
                throw new IOException(Database.GetText("Active Test Fail", "System"));
            };

            ActiveTests[Database.GetText("Fuel Pump", "System")] = (on) =>
            {
                byte[] cmd = Database.GetCommand("Activate The Fuel Pump", "Synerject");

                if (!on)
                {
                    cmd[3] = 0x00;
                }

                byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                if (recv[0] != 0x7F)
                {
                    if (!on)
                        return Database.GetText("Fuel Pump Off Test Finish", "QingQi");
                    return Database.GetText("Fuel Pump On Test Finish", "QingQi");
                }
                throw new IOException(Database.GetText("Active Test Fail", "System"));
            };
        }

        public List<TroubleCode> ReadTroubleCode(bool isHistory)
        {
            byte[] cmd = Database.GetCommand("Read DTC By Status", "Synerject");
            //byte[] cmd = new byte[] { 0x18, 0x00, 0x00, 0x00 };
            //byte[] cmd = new byte[] { 0x18, 0x00, 0x00, 0x01 };
            byte[] result = Protocol.SendAndRecv(startDiagnosticSession, 0, startDiagnosticSession.Length, Pack);
            if (result == null || result[0] != 0x50)
            {
                throw new IOException(Database.GetText("Read Trouble Code Fail", "System"));
            }

            result = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            Protocol.SendAndRecv(stopDiagnosticSession, 0, stopDiagnosticSession.Length, Pack);
            Protocol.SendAndRecv(stopCommunication, 0, stopCommunication.Length, Pack);

            if (result == null || result[0] != 0x58)
            {
                throw new IOException(Database.GetText("Read Trouble Code Fail", "System"));
            }

            int dtcNum = Convert.ToInt32(result[1]);

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
                tcs.Add(Database.GetTroubleCode(code, "Synerject"));
            }

            if (tcs.Count == 0)
            {
                throw new IOException(Database.GetText("None Trouble Code", "System"));
            }
            return tcs;
        }

        public void ClearTroubleCode()
        {
            byte[] cmd = Database.GetCommand("Clear Trouble Code1", "Synerject");
            byte[] recv = Protocol.SendAndRecv(startDiagnosticSession, 0, startDiagnosticSession.Length, Pack);

            if (recv == null || recv[0] != 0x50)
            {
                throw new IOException(Database.GetText("Clear Trouble Code Fail", "System"));
            }

            recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            Protocol.SendAndRecv(stopDiagnosticSession, 0, stopDiagnosticSession.Length, Pack);
            Protocol.SendAndRecv(stopCommunication, 0, stopCommunication.Length, Pack);


            if (recv == null || recv[0] != 0x54)
            {
                throw new IOException(Database.GetText("Clear Trouble Code Fail", "System"));
            }

        }

        public void ReadDataStream(Core.LiveDataVector vec)
        {
            byte[] calcBuff = new byte[128];
            ldVec = vec;
            var items = vec.Items;
            byte[] cmd = Database.GetCommand("Read Data By Local Identifier1", "Synerject");
            stopReadDataStream = false;

            byte[] recv = Protocol.SendAndRecv(startDiagnosticSession, 0, startDiagnosticSession.Length, Pack);
            if (recv == null || recv[0] != 0x50)
                throw new IOException(Database.GetText("Communication Fail", "System"));

            recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (recv == null)
            {
                Protocol.SendAndRecv(stopDiagnosticSession, 0, stopDiagnosticSession.Length, Pack);
                Protocol.SendAndRecv(stopCommunication, 0, stopCommunication.Length, Pack);
                throw new IOException(Database.GetText("Communication Fail", "System"));
            }
            Array.Copy(recv, calcBuff, recv.Length);
            Task task = Task.Factory.StartNew(() =>
            {
                while (!stopReadDataStream)
                {
                    Thread.Sleep(50);
                    Thread.Yield();
                    byte[] temp = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                    if (temp != null)
                    {
                        Array.Copy(temp, calcBuff, temp.Length);
                    }
                }
            });
            while (!stopReadDataStream)
            {
                foreach (var item in items)
                {
                    item.Value = DataStreamCalc[item.ShortName](calcBuff);
                    Thread.Sleep(10);
                    Thread.Yield();
                    if (stopReadDataStream)
                        break;
                }
                //int i = vec.NextShowedIndex();
                //vec[i].Value = DataStreamCalc[vec[i].ShortName](recv);
                //Thread.Sleep(10);
                //Thread.Yield();
                //byte[] recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                //if (recv == null)
                //    throw new IOException(Database.GetText("Communication Fail"));
                //for (int i = 0; i < vec.ShowedCount; i++)
                //{
                //    int j = vec.NextShowedIndex();
                //    vec[j].Value = DataStreamCalc[vec[j].ShortName](recv);
                //}
                //int i = vec.NextShowedIndex();
                //if (recv == null)
                //{
                //    continue;
                //}
                //// calc
                //vec[i].Value = DataStreamCalc[vec[i].ShortName](recv);
            }
            task.Wait();
            Protocol.SendAndRecv(stopDiagnosticSession, 0, stopDiagnosticSession.Length, Pack);
            Protocol.SendAndRecv(stopCommunication, 0, stopCommunication.Length, Pack);
        }

        public void StaticDataStream(Core.LiveDataVector vec)
        {
            ldVec = vec;
            byte[] cmd = Database.GetCommand("Read Data By Local Identifier1", "Synerject");
            byte[] recv = Protocol.SendAndRecv(startDiagnosticSession, 0, startDiagnosticSession.Length, Pack);

            if (recv == null || recv[0] != 0x50)
                throw new IOException(Database.GetText("Communication Fail", "System"));

            recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (recv == null || recv[0] != 0x61)
                throw new IOException(Database.GetText("Communication Fail", "System"));

            Protocol.SendAndRecv(stopDiagnosticSession, 0, stopDiagnosticSession.Length, Pack);
            Protocol.SendAndRecv(stopCommunication, 0, stopCommunication.Length, Pack);

            var items = vec.Items;
            foreach (var item in items)
            {
                item.Value = DataStreamCalc[item.ShortName](recv);
            }
            //for (int i = 0; i < vec.ShowedCount; i++)
            //{
            //    int index = vec.ShowedIndex(i);
            //    vec[index].Value = DataStreamCalc[vec[index].ShortName](recv);
            //}
        }

        public string Active(string mode, bool on)
        {
            byte[] buff = Protocol.SendAndRecv(startDiagnosticSession, 0, startDiagnosticSession.Length, Pack);

            if (buff == null || buff[0] != 0x50)
                throw new IOException(Database.GetText("Active Test Fail", "System"));

            string ret = ActiveTests[mode](on);
            Protocol.SendAndRecv(stopDiagnosticSession, 0, stopDiagnosticSession.Length, Pack);
            Protocol.SendAndRecv(stopCommunication, 0, stopCommunication.Length, Pack);
            return ret;
        }

        public void Active(Core.LiveDataVector vec, string mode)
        {
            ldVec = vec;
            byte[] cmd = Database.GetCommand("Read Data By Local Identifier1", "Synerject");
            byte[] buff = Protocol.SendAndRecv(startDiagnosticSession, 0, startDiagnosticSession.Length, Pack);

            if (buff == null || buff[0] != 0x50)
                throw new IOException(Database.GetText("Active Test Fail", "System"));

            ActiveOn = ActiveState.Idle;
            buff = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            if (buff == null)
            {
                Protocol.SendAndRecv(stopDiagnosticSession, 0, stopDiagnosticSession.Length, Pack);
                Protocol.SendAndRecv(stopCommunication, 0, stopCommunication.Length, Pack);
                throw new IOException(Database.GetText("Communication Fail", "System"));
            }

            var items = vec.Items;
            while (ActiveOn != ActiveState.Stop)
            {
                if (ActiveOn == ActiveState.Idle)
                {
                    buff = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
                    foreach (var item in items)
                    {
                        item.Value = DataStreamCalc[item.ShortName](buff);
                    }
                    Thread.Sleep(10);
                }
                else if (ActiveOn == ActiveState.Positive)
                {
                    ActiveTests[mode](true);
                    ActiveOn = ActiveState.Idle;
                    Thread.Sleep(10);
                }
                else if (ActiveOn == ActiveState.Negative)
                {
                    ActiveTests[mode](false);
                    ActiveOn = ActiveState.Idle;
                    Thread.Sleep(10);
                }
            }

            Protocol.SendAndRecv(stopDiagnosticSession, 0, stopDiagnosticSession.Length, Pack);
            Protocol.SendAndRecv(stopCommunication, 0, stopCommunication.Length, Pack);
        }

        public string ReadECUVersion()
        {
            byte[] cmd = Database.GetCommand("Version", "Synerject");

            byte[] recv = Protocol.SendAndRecv(startDiagnosticSession, 0, startDiagnosticSession.Length, Pack);

            if (recv == null || recv[0] != 0x50)
                throw new IOException(Database.GetText("Read ECU Version Fail", "System"));

            recv = Protocol.SendAndRecv(cmd, 0, cmd.Length, Pack);
            Protocol.SendAndRecv(stopDiagnosticSession, 0, stopDiagnosticSession.Length, Pack);
            Protocol.SendAndRecv(stopCommunication, 0, stopCommunication.Length, Pack);

            if (recv == null || recv[0] != 0x61)
                throw new IOException(Database.GetText("Read ECU Version Fail", "System"));

            recv[0] = 0x20;
            recv[1] = 0x20;
            return Encoding.ASCII.GetString(recv);
        }
    }
}
