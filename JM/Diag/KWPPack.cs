using System;
using JM.Core;

namespace JM.Diag
{
    public class KWPPack : IPack
    {
        protected KWPOptions options;
        private KWPMode mode;

        public KWPMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        public const int KWP8X_HEADER_LENGTH = 3;
        public const int KWPCX_HEADER_LENGTH = 3;
        public const int KWP80_HEADER_LENGTH = 4;
        public const int KWPXX_HEADER_LENGTH = 1;
        public const int KWP00_HEADER_LENGTH = 2;
        public const int KWP_CHECKSUM_LENGTH = 1;
        public const int KWP_MAX_DATA_LENGTH = 128;

        public KWPPack()
        {
            mode = KWPMode.Mode8X;
        }

        public byte[] Pack(byte[] data, int offset, int count)
        {
            try
            {
                int pos = 0;
                byte checksum = 0;
                int i = 0;
                byte[] result = null;

                if (mode == KWPMode.Mode8X)
                {
                    result = new byte[KWP8X_HEADER_LENGTH + count + KWP_CHECKSUM_LENGTH];
                    result[pos++] = Utils.LowByte(0x80 | count);
                    result[pos++] = Utils.LowByte(options.TargetAddress);
                    result[pos++] = Utils.LowByte(options.SourceAddress);
                }
                else if (mode == KWPMode.ModeCX)
                {
                    result = new byte[KWPCX_HEADER_LENGTH + count + KWP_CHECKSUM_LENGTH];
                    result[pos++] = Utils.LowByte(0xC0 | count);
                    result[pos++] = Utils.LowByte(options.TargetAddress);
                    result[pos++] = Utils.LowByte(options.SourceAddress);
                }
                else if (mode == KWPMode.Mode80)
                {
                    result = new byte[KWP80_HEADER_LENGTH + count + KWP_CHECKSUM_LENGTH];
                    result[pos++] = Utils.LowByte(0x80);
                    result[pos++] = Utils.LowByte(options.TargetAddress);
                    result[pos++] = Utils.LowByte(options.SourceAddress);
                    result[pos++] = Utils.LowByte(count);
                }
                else if (mode == KWPMode.Mode00)
                {
                    result = new byte[KWP00_HEADER_LENGTH + count + KWP_CHECKSUM_LENGTH];
                    result[pos++] = Utils.LowByte(0x00);
                    result[pos++] = Utils.LowByte(count);
                }
                else if (mode == KWPMode.ModeXX)
                {
                    result = new byte[KWPXX_HEADER_LENGTH + count + KWP_CHECKSUM_LENGTH];
                    result[pos++] = Utils.LowByte(count);
                }
                else
                {
                    throw new ArgumentException();
                }

                Array.Copy(data, offset, result, pos, count);
                pos += count;

                for (i = 0; i < pos; i++)
                {
                    checksum += result[i];
                }
                result[i] = checksum;
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
                int length = 0;
                byte[] result = null;

                if ((data[offset] & 0xFF) > 0x80)
                {
                    length = (data[offset] & 0xFF) - 0x80;
                    if (data[offset + 1] != options.SourceAddress)
                    {
                        return null;
                    }
                    if (length != (count - KWP8X_HEADER_LENGTH - KWP_CHECKSUM_LENGTH))
                    {
                        length = data[offset] - 0xC0; // For KWPCX 
                        if (length != (count - KWPCX_HEADER_LENGTH - KWP_CHECKSUM_LENGTH))
                        {
                            return null;
                        }
                        else
                        {
                            offset = offset + KWPCX_HEADER_LENGTH;
                        }
                    }
                    else
                    {
                        offset = offset + KWP8X_HEADER_LENGTH;
                    }
                }
                else if ((data[offset] & 0xFF) == 0x80)
                {
                    length = data[offset + 3] & 0xFF;
                    if (data[offset + 1] != options.SourceAddress)
                    {
                        return null;
                    }

                    if (length != (count - KWP80_HEADER_LENGTH - KWP_CHECKSUM_LENGTH))
                    {
                        return null;
                    }
                    offset = offset + KWP80_HEADER_LENGTH;
                }
                else if (data[offset] == 0x00)
                {
                    length = data[offset + 1] & 0xFF;
                    if (length != (count - KWP00_HEADER_LENGTH - KWP_CHECKSUM_LENGTH))
                    {
                        return null;
                    }
                    offset = offset + KWP00_HEADER_LENGTH;
                }
                else
                {
                    length = data[offset] & 0xFF;
                    if (length != (count - KWPXX_HEADER_LENGTH - KWP_CHECKSUM_LENGTH))
                    {
                        return null;
                    }
                    offset = offset + KWPXX_HEADER_LENGTH;
                }

                result = new byte[length];
                Array.Copy(data, offset, result, 0, length);
                return result;
            }
            catch
            {
                return null;
            }
        }

        public bool Config(object opts)
        {
            if (opts is KWPOptions)
            {
                options = opts as KWPOptions;
                return true;
            }
            return false;
        }
    }
}