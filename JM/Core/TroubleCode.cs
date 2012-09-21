using System;
using System.Collections.Generic;
using System.Text;

namespace JM.Core
{
    public class TroubleCode : Notifier
    {
        private string code;
        private string content;
        private string description;

        public TroubleCode()
        {
            code = null;
            content = null;
            description = null;
        }

        public TroubleCode(string code, string content)
            : this()
        {
            this.code = code;
            this.content = content;
        }

        public TroubleCode(string code, string content, string description)
            : this(code, content)
        {
            this.description = description;
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

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }
    }
}
