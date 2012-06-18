using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JM.Diag
{
    public interface ICommbox
    {
        void Open();

        void Close();

        void SetConnector(ConnectorType cnn);

        IProtocol CreateProtocol(ProtocolType type);
    }
}
