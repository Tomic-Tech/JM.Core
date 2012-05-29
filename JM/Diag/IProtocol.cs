using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JM.Diag
{
    public interface IProtocol
    {
        byte[] Pack(byte[] data, int offset, int count);
        byte[] Unpack(byte[] data, int offset, int count);
    }
}
