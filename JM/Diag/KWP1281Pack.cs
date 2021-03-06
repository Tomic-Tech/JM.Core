﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JM.Core;

namespace JM.Diag
{
    class KWP1281Pack : IPack
    {
        private byte frameCounter;
        protected KWP1281Options options;
        public static byte FRAME_END = 0x03;

        KWP1281Pack()
        {
            frameCounter = 0;
        }

        private byte frameCounterIncrement()
        {
            return ++frameCounter;
        }

        public byte[] Pack(byte[] data, int offset, int count)
        {
            try
            {
                byte[] result = new byte[count + 3];
                result[0] = Utils.LowByte(count + 2);
                result[1] = frameCounterIncrement();

                Array.Copy(data, offset, result, 2, count);
                result[result.Length - 1] = FRAME_END;
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
                byte[] result = new byte[count - 2];
                Array.Copy(data, offset + 1, result, 0, count - 2);
                return result;
            }
            catch
            {
                return null;
            }
        }

        public bool Config(object opts)
        {
            if (opts is KWP1281Options)
            {
                options = opts as KWP1281Options;
                return true;
            }
            return false;
        }
    }
}
