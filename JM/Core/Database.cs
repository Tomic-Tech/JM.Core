using System;
using System.Text;
using System.Runtime.InteropServices;

namespace JM.Core
{
    public static class Database
    {
        [DllImport("JMCore", EntryPoint="database_init", CallingConvention=CallingConvention.Cdecl)]
        public static extern void Init([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(UTF8Marshaler))]string filePath);

        [DllImport("JMCore", EntryPoint="database_dispose", CallingConvention=CallingConvention.Cdecl)]
        public static extern void Dispose();

        [DllImport("JMCore", EntryPoint="database_get_text", CallingConvention=CallingConvention.Cdecl)]
        private static extern bool GetText([MarshalAs(UnmanagedType.LPStr)]string name, 
            [MarshalAs(UnmanagedType.LPStr)]string cls, 
            [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder text);

        [DllImport("JMCore", EntryPoint="database_get_trouble_code", CallingConvention=CallingConvention.Cdecl)]
        private static extern bool GetTroubleCode([MarshalAs(UnmanagedType.LPStr)]string code, 
            [MarshalAs(UnmanagedType.LPStr)]string cls, 
            [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder content, 
            [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder description);

        [DllImport("JMCore", EntryPoint="database_get_command", CallingConvention=CallingConvention.Cdecl)]
        private static extern bool GetCommand([MarshalAs(UnmanagedType.LPStr)]string name,
            [MarshalAs(UnmanagedType.LPStr)]string cls,
            byte[] buffer,
            out UInt32 count);

        [DllImport("JMCore", EntryPoint = "database_get_command_by_id", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool GetCommand(int id, byte[] buffer, out UInt32 count);

        [DllImport("JMCore", EntryPoint="database_live_data_prepare", CallingConvention=CallingConvention.Cdecl)]
        private static extern bool LiveDataPrepare([MarshalAs(UnmanagedType.LPStr)]string cls);

        [DllImport("JMCore", EntryPoint="database_live_data_next", CallingConvention=CallingConvention.Cdecl)]
        private static extern bool LiveDataNext([Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder shortName,
            [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder content,
            [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder unit,
            [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder defaultValue,
            out Int32 cmdID,
            [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder description);


        public static string GetText(string name, string cls)
        {
            StringBuilder builder = new StringBuilder(1024);
            if (GetText(name, cls, builder))
            {
                return builder.ToString();
            }

            return null;
        }

        public static TroubleCode GetTroubleCode(string code, string cls)
        {
            StringBuilder content = new StringBuilder(100);
            StringBuilder description = new StringBuilder(100);
            if (GetTroubleCode(code, cls, content, description))
            {
                return new TroubleCode(code, content.ToString(), description.ToString());
            }

            return null;
        }

        public static LiveDataVector GetLiveData(string cls)
        {
            LiveDataVector vec = new LiveDataVector();
            if (!LiveDataPrepare(cls))
            {
                throw new System.Exception("Can't prepare live data");
            }
            while (true)
            {
                StringBuilder shortName = new StringBuilder(100);
                StringBuilder content = new StringBuilder(100);
                StringBuilder unit = new StringBuilder(100);
                StringBuilder defaultValue = new StringBuilder(100);
                StringBuilder description = new StringBuilder(100);
                int cmdID = 0;

                if (LiveDataNext(shortName, content, unit, defaultValue, out cmdID, description))
                {
                    vec.Add(new LiveData(shortName.ToString(), content.ToString(), unit.ToString(), defaultValue.ToString(), cmdID, description.ToString(), true));
                }
                else
                    break;
            }
            return vec;
        }

        public static byte[] GetCommand(string name, string cls)
        {
            byte[] buffer = new byte[1024];
            UInt32 count;
            if (GetCommand(name, cls, buffer, out count))
            {
                byte[] result = new byte[count];
                Array.Copy(buffer, result, count);
                return result;
            }

            return null;
        }

        public static byte[] GetCommand(int id)
        {
            byte[] buffer = new byte[1024];
            UInt32 count;
            if (GetCommand(id, buffer, out count))
            {
                byte[] result = new byte[count];
                Array.Copy(buffer, result, count);
                return result;
            }

            return null;
        }

    }
}

