using System;
using System.Runtime.InteropServices;
using System.Text;

namespace JM.Core
{
    public class LiveData : Notifier
    {
        private string shortName;
        private string content;
        private string unit;
        private string defaultValue;
        private string value;
        private bool enabled;
        private bool showed;
        private int cmdID;
        private int index;

        //[DllImport("JMCore", EntryPoint = "live_data_get_short_name", CallingConvention = CallingConvention.Cdecl)]
        //private static extern bool GetShortName(IntPtr p, [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder text);

        //[DllImport("JMCore", EntryPoint = "live_data_short_name", CallingConvention = CallingConvention.Cdecl)]
        //[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
        //private static extern string GetShortName(IntPtr p);

        //[DllImport("JMCore", EntryPoint = "live_data_get_content", CallingConvention = CallingConvention.Cdecl)]
        //private static extern bool GetContent(IntPtr p, [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder text);

        //[DllImport("JMCore", EntryPoint = "live_data_content", CallingConvention = CallingConvention.Cdecl)]
        //[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
        //private static extern string GetContent(IntPtr p);

        //[DllImport("JMCore", EntryPoint = "live_data_get_unit", CallingConvention = CallingConvention.Cdecl)]
        //private static extern bool GetUnit(IntPtr p, [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder text);

        //[DllImport("JMCore", EntryPoint = "live_data_unit", CallingConvention = CallingConvention.Cdecl)]
        //[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
        //private static extern string GetUnit(IntPtr p);

        //[DllImport("JMCore", EntryPoint = "live_data_get_default_value", CallingConvention = CallingConvention.Cdecl)]
        //private static extern bool GetDefaultValue(IntPtr p, [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder text);

        //[DllImport("JMCore", EntryPoint = "live_data_default_value", CallingConvention = CallingConvention.Cdecl)]
        //[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
        //private static extern string GetDefaultValue(IntPtr p);

        //[DllImport("JMCore", EntryPoint = "live_data_get_value", CallingConvention = CallingConvention.Cdecl)]
        //private static extern bool GetValue(IntPtr p, [Out, MarshalAs(UnmanagedType.LPWStr)]StringBuilder text);

        //[DllImport("JMCore", EntryPoint = "live_data_value", CallingConvention = CallingConvention.Cdecl)]
        //[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
        //private static extern string GetValue(IntPtr p);

        //[DllImport("JMCore", EntryPoint = "live_data_set_value", CallingConvention = CallingConvention.Cdecl)]
        //private static extern void SetValue(IntPtr p, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]string value);

        //[DllImport("JMCore", EntryPoint = "live_data_cmd_id", CallingConvention = CallingConvention.Cdecl)]
        //private static extern int GetCmdID(IntPtr p);

        //[DllImport("JMCore", EntryPoint = "live_data_enabled", CallingConvention = CallingConvention.Cdecl)]
        //private static extern int GetEnabled(IntPtr p);

        //[DllImport("JMCore", EntryPoint = "live_data_set_enabled", CallingConvention = CallingConvention.Cdecl)]
        //private static extern void SetEnabled(IntPtr p, int enabled);

        //[DllImport("JMCore", EntryPoint = "live_data_showed", CallingConvention = CallingConvention.Cdecl)]
        //private static extern bool GetShowed(IntPtr p);

        //[DllImport("JMCore", EntryPoint = "live_data_set_showed", CallingConvention = CallingConvention.Cdecl)]
        //private static extern void SetShowed(IntPtr p, bool enabled);

        //[DllImport("JMCore", EntryPoint = "live_data_index", CallingConvention = CallingConvention.Cdecl)]
        //private static extern int GetIndex(IntPtr p);

        //[DllImport("JMCore", EntryPoint = "live_data_set_index", CallingConvention = CallingConvention.Cdecl)]
        //private static extern void SetIndex(IntPtr p, int enabled);

        public LiveData(string shortName, string content, string unit, string defaultValue, string value, int cmdID, bool enabled)
        {
            this.shortName = shortName;
            this.content = content;
            this.unit = unit;
            this.defaultValue = defaultValue;
            this.cmdID = cmdID;
            this.enabled = enabled;
            this.showed = true;
        }

        public string ShortName
        {
            get
            {
                return shortName;
            }
        }

        public string Content
        {
            get
            {
                return content;
            }
        }

        public string Unit
        {
            get
            {
                return unit;
            }
        }

        public string DefaultValue
        {
            get
            {
                return defaultValue;
            }
        }

        public string Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
                OnPropertyChanged("Value");
            }
        }

        public int CmdID
        {
            get
            {
                return cmdID;
            }
        }

        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        public bool Showed
        {
            get
            {
                return showed;
            }
            set
            {
                showed = value;
                OnPropertyChanged("Showed");
            }
        }

        public int Index
        {
            get
            {
                return index;
            }
            set
            {
                index = value;
            }
        }

    }
}

