using System;
using System.Collections.Generic;
using System.Text;

namespace JM.Diag
{
    public class NoPack : IPack
    {
        public byte[] Pack(byte[] data, int offset, int count)
        {
            return JustCopy(data, offset, count);
        }

        public byte[] Unpack(byte[] data, int offset, int count)
        {
            return JustCopy(data, offset, count);
        }

        private static byte[] JustCopy(byte[] data, int offset, int count)
        {
            try
            {
                byte[] result = new byte[count];
                Array.Copy(data, offset, result, 0, count);
                return result;
            }
            catch
            {
                return null;
            }
        }

        public bool Config(object obj)
        {
            return true;
        }
    }
}
