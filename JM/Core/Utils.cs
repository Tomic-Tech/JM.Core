using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JM.Core
{
    public static class Utils
    {
        public static byte HighByte<T>(T x) where T : IConvertible
        {
            ushort integer = System.Convert.ToUInt16(x);
            return (byte)((integer >> 8) & 0xFF);
        }

        public static byte LowByte<T>(T x) where T : IConvertible
        {
            ushort integer = System.Convert.ToUInt16(x);
            return (byte)(integer & 0xFF);
        }

        public static ushort HighWord<T>(T x) where T : IConvertible
        {
            uint integer = System.Convert.ToUInt32(x);
            return (ushort)((integer >> 16) & 0xFFFF);
        }

        public static ushort LowWord<T>(T x) where T : IConvertible
        {
            uint integer = System.Convert.ToUInt32(x);
            return (ushort)(integer & 0xFFFF);
        }

        public static string CalcStdObdTroubleCode(byte[] buffer, int pos, int factor, int offset)
        {
            StringBuilder code = new StringBuilder();
            switch (buffer [pos * factor + offset] & 0xC0)
            {
                case 0x00:
                    code.Append("P");
                    break;
                case 0x40:
                    code.Append("C");
                    break;
                case 0x80:
                    code.Append("B");
                    break;
                default:
                    code.Append("U");
                    break;
            }
            code.AppendFormat("{0:X2}", buffer [pos * factor + offset]);
            code.AppendFormat("{0:X2}", buffer [pos * factor + offset + 1]);
            return code.ToString();
        }
    }
}
