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
        void StartKeepLink(bool run);
        void SetKeepLink(byte[] data, int offset, int count, IPack pack);
        void SetTimeout(int txB2B, int rxB2B, int txF2F, int rxF2F, int total);
        void Config(object options);
    }
}
