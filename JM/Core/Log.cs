using System;
using System.Collections.Generic;
using System.Text;

namespace JM.Core
{
    public static class Log
    {
        static Log()
        {

        }

        public static void Write(string tag, byte[] buffer, int offset, int count)
        {
            StringBuilder text = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                text.AppendFormat("{0:X2} ", buffer[offset + i]);
            }
            Write(tag, text.ToString());
        }

        public static void Write(string tag, string text)
        {
#if OS_ANDROID
            Android.Util.Log.Debug("JMScanner", tag + ": " + text);
#endif
        }
    }
}
