using System;
using System.Runtime.InteropServices;
using System.Text;

namespace JM.Core
{
    public class LiveData : Notifier
    {
        private IntPtr p;

        public LiveData(IntPtr p)
        {
            this.p = p;
        }

        [DllImport("JMCore", EntryPoint = "live_data_short_name", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
        private static extern string GetShortName(IntPtr p);

        [DllImport("JMCore", EntryPoint = "live_data_content", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
        private static extern string GetContent(IntPtr p);

        [DllImport("JMCore", EntryPoint = "live_data_unit", CallingConvention = CallingConvention.Cdecl)]
        [return:MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(UTF8Marshaler))]
        private static extern string GetUnit(IntPtr p);

        [DllImport("JMCore", EntryPoint = "live_data_default_value", CallingConvention = CallingConvention.Cdecl)]
        [return:MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(UTF8Marshaler))]
        private static extern string GetDefaultValue(IntPtr p);

        [DllImport("JMCore", EntryPoint = "live_data_value", CallingConvention = CallingConvention.Cdecl)]
        [return:MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(UTF8Marshaler))]
        private static extern string GetValue(IntPtr p);

        [DllImport("JMCore", EntryPoint = "live_data_get_value", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetValue(IntPtr p, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]string value);

        [DllImport("JMCore", EntryPoint = "live_data_cmd_id", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetCmdID(IntPtr p);

        [DllImport("JMCore", EntryPoint = "live_data_enabled", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetEnabled(IntPtr p);

        [DllImport("JMCore", EntryPoint = "live_data_set_enabled", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetEnabled(IntPtr p, int enabled);

        [DllImport("JMCore", EntryPoint = "live_data_showed", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool GetShowed(IntPtr p);

        [DllImport("JMCore", EntryPoint = "live_data_set_showed", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetShowed(IntPtr p, bool enabled);

        [DllImport("JMCore", EntryPoint = "live_data_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetIndex(IntPtr p);

        [DllImport("JMCore", EntryPoint = "live_data_set_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetIndex(IntPtr p, int enabled);

        public string ShortName
        {
            get { return GetShortName(p); }
        }
        
        public string Content
        {
            get { return GetContent(p); }
        }
        
        public string Unit
        {
            get { return GetUnit(p); }
        }
        
        public string DefaultValue
        {
            get { return GetDefaultValue(p); }
        }
        
        public string Value
        {
            get
            {
                return GetValue(p);
            }
            set
            {
                SetValue(p, value);
                OnPropertyChanged("Value");
            }
        }
        
        public int CmdID
        {
            get { return GetCmdID(p); }
        }

        public bool Enabled
        {
            get
            {
                int ret = GetEnabled(p);
                return ret != 0;
            }
            set
            {
                SetEnabled(p, value ? 1 : 0);
                OnPropertyChanged("Enabled");
            }
        }

        public bool Showed
        {
            get { return GetShowed(p); }
            set { SetShowed(p, value);
                OnPropertyChanged("Showed"); }
        }

        public int Index
        {
            get { return GetIndex(p); }
            set { SetIndex(p, value); }
        }
        
    }
}

