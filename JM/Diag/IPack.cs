using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JM.Diag
{
    public interface IPack
    {
        byte[] Pack(byte[] data, int offset, int count);

        byte[] Unpack(byte[] data, int offset, int count);

        void Config(object obj);
    }
}