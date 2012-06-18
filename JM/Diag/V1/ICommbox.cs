using System;

namespace JM.Diag.V1
{
    internal interface ICommbox
    {
        bool NewBatch(byte buffID);

        bool SendOutData(byte[] buffer, int offset, int length);

        bool RunReceive(byte type);

        bool EndBatch();

        bool RunBatch(byte[] buffID, int length, bool isRunMore);

        bool KeepLink(bool isRun);

        byte BuffID { get; set; }

        ConnectorType Connector { get; set; }

        bool SetCommCtrl(byte valueOpen, byte valueClose);

        bool SetCommLine(byte sendLine, byte recvLine);

        bool SetCommLink(byte ctrlWord1, byte ctrlWord2, byte ctrlWord3);

        bool SetCommBaud(double baud);

        bool SetCommTime(byte type, Core.Timer time);

        bool SetLineLevel(byte low, byte high);

        bool CommboxDelay(Core.Timer time);

        int ReadBytes(byte[] buffer, int offset, int length);

        bool StopNow(bool isStop);

        bool DelBatch(byte buffID);

        bool CheckResult(Core.Timer time);

        bool TurnOverOneByOne();

        int ReadData(byte[] buffer, int offset, int length, Core.Timer time);
    }
}