using System;
using System.Collections.Generic;
using System.Text;

namespace JM.Core
{
    public class TroubleCode : Notifier
    {
        private string code;
        private string content;

        public TroubleCode()
        {
            code = null;
            content = null;
        }

        public TroubleCode(string code, string content)
        {
            this.code = code;
            this.content = content;
        }

        public string Code
        {
            get { return code; }
            set 
            { 
                code = value;
                OnPropertyChanged("Code");
            }
        }

        public string Content
        {
            get { return content; }
            set
            {
                content = value;
                OnPropertyChanged("Content");
            }
        }
    }
}
