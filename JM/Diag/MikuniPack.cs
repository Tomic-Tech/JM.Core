using System;

namespace JM.Diag
{
    public class MikuniPack : IPack
    {
        public const byte HEAD_FORMAT = 0x48;

        public MikuniPack()
        {
        }

        public byte[] Pack(byte[] data, int offset, int count)
        {
            try
            {
                byte[] result = new byte[count + 3];
                result[0] = HEAD_FORMAT;
                Array.Copy(data, offset, result, 1, count);
                result[count + 1] = 0x0D;
                result[count + 2] = 0x0A;

                return result;
            }
            catch
            {
                return null;
            }
        }

        public byte[] Unpack(byte[] data, int offset, int count)
        {
            try
            {
                byte[] result = new byte[count - 3];
                Array.Copy(data, offset + 1, result, 0, count - 3);

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