using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JM.Diag
{
    public interface ICommbox
    {
        bool Open();

        bool Close();

        bool SetConnector(ConnectorType cnn);

        IProtocol CreateProtocol(ProtocolType type);
    }
}
