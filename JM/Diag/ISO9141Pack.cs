using System;

namespace JM.Diag
{
    public class ISO9141Pack : IPack
    {
        private ISO9141Options options;

        #region IPack implementation
        public byte[] Pack(byte[] data, int offset, int count)
        {
            try
            {
                byte[] result = new byte[count + 4];
                result[0] = options.Header;
                result[1] = options.TargetAddress;
                result[2] = options.SourceAddress;
                Array.Copy(data, offset, result, 3, count);
                result[result.Length - 1] = 0;

                for (int i = 0; i < count + 3; i++)
                {
                    result[result.Length - 1] += result[i];
                }

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
                byte cs = 0;
                for (int i = 0; i < count - 1; i++)
                {
                    cs += data[offset + i];
                }
                if (cs != data[offset + count - 1])
                {
                    return null;
                }
                byte[] result = new byte[count - 4];
                Array.Copy(data, offset + 3, result, 0, count - 4);
                return result;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        public bool Config(object options)
        {
            if (options is ISO9141Options)
            {
                this.options = options as ISO9141Options;
                return true;
            }
            return false;
        }
    }
}

