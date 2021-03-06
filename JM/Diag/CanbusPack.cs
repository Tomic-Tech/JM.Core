﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JM.Core;

namespace JM.Diag
{
    public class CanbusPack : IPack
    {
        private byte[] flowControl = new byte[8];
        protected CanbusOptions options;

        public CanbusPack()
        {
            flowControl[0] = 0x30;
            flowControl[1] = 0x00;
            flowControl[2] = 0x00;
            flowControl[3] = 0x00;
            flowControl[4] = 0x00;
            flowControl[5] = 0x00;
            flowControl[6] = 0x00;
            flowControl[7] = 0x00;
        }

        public byte[] Pack(byte[] data, int offset, int count)
        {
            try
            {
                if (count > 8 || count <= 0)
                {
                    return null;
                }

                byte[] result = null;

                if (options.IdMode == CanbusIDMode.Standard)
                {
                    result = new byte[3 + count];
                    result[1] = Utils.HighByte(Utils.LowWord(options.TargetID));
                    result[2] = Utils.LowByte(Utils.LowWord(options.TargetID));
                    if (options.FrameType == CanbusFrameType.Data)
                    {
                        result[0] = Utils.LowByte(count | (int)CanbusIDMode.Standard | (int)CanbusFrameType.Data);
                    }
                    else
                    {
                        result[0] = Utils.LowByte(count | (int)CanbusIDMode.Standard | (int)CanbusFrameType.Remote);
                    }
                    Array.Copy(data, offset, result, 3, count);
                }
                else
                {
                    result = new byte[5 + count];
                    result[1] = Utils.HighByte(Utils.HighWord(options.TargetID));
                    result[2] = Utils.LowByte(Utils.HighWord(options.TargetID));
                    result[3] = Utils.HighByte(Utils.LowWord(options.TargetID));
                    result[4] = Utils.LowByte(Utils.LowWord(options.TargetID));
                    if (options.FrameType == CanbusFrameType.Data)
                    {
                        result[0] = Utils.LowByte(count | (int)CanbusIDMode.Extension | (int)CanbusFrameType.Data);
                    }
                    else
                    {
                        result[0] = Utils.LowByte(count | (int)CanbusIDMode.Extension | (int)CanbusFrameType.Remote);
                    }
                    Array.Copy(data, offset, result, 5, count);
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
                if (count <= 0)
                {
                    return null;
                }

                int length = 0;
                int mode = (data[offset] & ((int)CanbusIDMode.Extension | (int)CanbusFrameType.Remote));
                byte[] result = null;
                if (mode == ((int)CanbusIDMode.Standard | (int)CanbusFrameType.Data))
                {
                    length = data[offset] & 0x0F;
                    if (length != count - 3)
                    {
                        return null;
                    }

                    result = new byte[length];
                    Array.Copy(data, offset, result, 0, length);
                }
                else if (mode == ((int)CanbusIDMode.Extension | (int)CanbusFrameType.Remote))
                {
                    length = data[offset] & 0x0F;
                    if (length != count - 5)
                    {
                        return null;
                    }
                    result = new byte[length];
                    Array.Copy(data, offset, result, 0, length);
                }

                return result;
            }
            catch
            {
                return null;
            }
        }

        public bool Config(object opts)
        {
            if (opts is CanbusOptions)
            {
                options = opts as CanbusOptions;
                return true;
            }
            return false;
        }
    }
}
