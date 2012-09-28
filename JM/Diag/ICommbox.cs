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

        ConnectorType Connector { get; set; }
        
        IProtocol CreateProtocol(ProtocolType type);
    }
}
