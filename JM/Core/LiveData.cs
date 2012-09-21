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
        private string description;

        public LiveData(string shortName, 
            string content, 
            string unit, 
            string defaultValue, 
            int cmdID, 
            string description, 
            bool enabled)
        {
            this.shortName = shortName;
            this.content = content;
            this.unit = unit;
            this.defaultValue = defaultValue;
            this.cmdID = cmdID;
            this.description = description;
            this.enabled = enabled;
            this.showed = true;
        }

        public string ShortName
        {
            get { return shortName; }
        }

        public string Content
        {
            get { return content; }
        }

        public string Unit
        {
            get { return unit; }
        }

        public string DefaultValue
        {
            get { return defaultValue; }
        }

        public string Value
        {
            get { return this.value; }
            set
            {
                this.value = value;
                OnPropertyChanged("Value");
            }
        }

        public string Description
        {
            get { return description; }
        }

        public int CmdID
        {
            get { return cmdID; }
        }

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        public bool Showed
        {
            get { return showed; }
            set
            {
                showed = value;
                OnPropertyChanged("Showed");
            }
        }

        public int Index
        {
            get { return index; }
            set { index = value; }
        }

    }
}

