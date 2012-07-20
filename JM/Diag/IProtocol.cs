using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JM.Diag
{
    public interface IProtocol
    {
        int SendOneFrame(byte[] data, int offset, int count, IPack pack);

        int SendFrames(byte[] data, int offset, int count, IPack pack);

        byte[] ReadOneFrame(IPack pack);

        byte[] ReadFrames(IPack pack);

        byte[] SendAndRecv(byte[] data, int offset, int count, IPack pack);

        bool KeepLink(bool run);

        bool SetKeepLink(byte[] data, int offset, int count, IPack pack);

        bool SetTimeout(int txB2B, int rxB2B, int txF2F, int rxF2F, int total);

        bool Config(object options);
    }
}
