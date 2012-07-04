using System;
using System.Text;
using System.Runtime.InteropServices;

namespace JM.Core
{
    public class VehicleDB : IDisposable
    {
        IntPtr p;
        bool disposed = false;

        [DllImport("JMCore", EntryPoint = "database_new")]
        private static extern IntPtr New([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]string filePath);

        [DllImport("JMCore", EntryPoint = "database_dispose")]
        private static extern void Dispose(IntPtr p);

        [DllImport("JMCore", EntryPoint = "database_set_tc_catalog")]
        private static extern void SetTCCatalog(IntPtr p, [MarshalAs(UnmanagedType.LPStr)]string text);

        [DllImport("JMCore", EntryPoint = "database_set_ld_catalog")]
        private static extern void SetLDCatalog(IntPtr p, [MarshalAs(UnmanagedType.LPStr)]string text);

        [DllImport("JMCore", EntryPoint = "database_set_cmd_catalog")]
        private static extern bool SetCMDCatalog(IntPtr p, [MarshalAs(UnmanagedType.LPStr)]string text);

        [DllImport("JMCore", EntryPoint = "database_get_text")]
        private static extern bool GetText(IntPtr p, [MarshalAs(UnmanagedType.LPStr)]string name, [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder text);

        [DllImport("JMCore", EntryPoint = "database_get_trouble_code")]
        private static extern bool GetTroubleCode(IntPtr p, [MarshalAs(UnmanagedType.LPStr)]string code, [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder text);

        //[DllImport("JMCore", EntryPoint = "database_get_live_data")]
        //private static extern bool GetLiveData(IntPtr p, IntPtr vec);
        [DllImport("JMCore", EntryPoint = "database_live_data_prepare")]
        private static extern bool LiveDataPrepare(IntPtr p);

        [DllImport("JMCore", EntryPoint = "database_live_data_next")]
        private static extern bool LiveDataNext(IntPtr p, [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder shortName, [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder content, [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder unit, [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder defaultValue, [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder value, out Int32 cmdID);

        [DllImport("JMCore", EntryPoint = "database_get_command")]
        private static extern bool GetCommand(IntPtr p, [MarshalAs(UnmanagedType.LPStr)]string name, byte[] buffer, out UInt32 count);

        [DllImport("JMCore", EntryPoint = "database_get_command_by_id")]
        private static extern bool GetCommand(IntPtr p, Int32 id, byte[] buffer, out UInt32 count);

        public VehicleDB(string filePath)
        {
            p = New(filePath);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (!disposed)
            {
                if (isDisposing)
                {
                    Dispose(p);
                    disposed = true;
                }
            }
        }

        public void Close()
        {
            ((IDisposable)this).Dispose();
        }

        ~VehicleDB()
        {
            Dispose(false);
        }

        public string TCCatalog
        {
            set
            {
                SetTCCatalog(p, value);
            }
        }

        public string LDCatalog
        {
            set
            {
                SetLDCatalog(p, value);
            }
        }

        public string CMDCatalog
        {
            set
            {
                SetCMDCatalog(p, value);
            }
        }

        public string GetText(string name)
        {
            StringBuilder builder = new StringBuilder(1024);
            if (GetText(p, name, builder))
            {
                return builder.ToString();
            }

            return null;
        }

        public string GetTroubleCode(string code)
        {
            StringBuilder builder = new StringBuilder(100);
            if (GetTroubleCode(p, code, builder))
            {
                return builder.ToString();
            }

            return null;
        }

        public LiveDataVector GetLiveData()
        {
            LiveDataVector vec = new LiveDataVector();
            if (!LiveDataPrepare(p))
            {
                throw new System.Exception("Can't prepare live data");
            }
            while (true)
            {
                StringBuilder shortName = new StringBuilder(100);
                StringBuilder content = new StringBuilder(100);
                StringBuilder unit = new StringBuilder(100);
                StringBuilder defaultValue = new StringBuilder(100);
                StringBuilder value = new StringBuilder(100);
                int cmdID = 0;

                if (LiveDataNext(p, shortName, content, unit, defaultValue, value, out cmdID))
                {
                    vec.Add(new LiveData(shortName.ToString(), content.ToString(), unit.ToString(), defaultValue.ToString(), value.ToString(), cmdID, true));
                }
                else
                    break;
            }
            return vec;
        }

        //public LiveDataVector GetLiveData()
        //{
        //    LiveDataVector vec = new LiveDataVector();
        //    if (!GetLiveData(p, vec.UnmanagedPointer))
        //    {
        //        vec.Close();
        //        return null;
        //    }

        //    vec.Initialize();
        //    return vec;
        //}

        public byte[] GetCommand(string name)
        {
            byte[] buffer = new byte[1024];
            UInt32 count;
            if (GetCommand(p, name, buffer, out count))
            {
                byte[] result = new byte[count];
                Array.Copy(buffer, result, count);
                return result;
            }

            return null;
        }

        public byte[] GetCommand(int id)
        {
            byte[] buffer = new byte[1024];
            UInt32 count;
            if (GetCommand(p, id, buffer, out count))
            {
                byte[] result = new byte[count];
                Array.Copy(buffer, result, count);
                return result;
            }

            return null;
        }

    }
}

