using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace JM.Core
{
    public class LiveData : Notifier, IComparable
    {
        private string shortName;
        private string content;
        private string unit;
        private string defaultValue;
        private string commandName;
        private string commandClass;
        private string description;
        private string value;
        private bool enabled;
        private bool showed;
        private int index;
        private int position;
        private bool outOfRange;
        private Dictionary<string, string> minMax;

        public LiveData(string shortName, 
            string content, 
            string unit, 
            string defaultValue, 
            string commandName,
            string commandClass,
            string description, 
            int index,
            bool enabled)
        {
            this.shortName = shortName;
            this.content = content;
            this.unit = unit;
            this.defaultValue = defaultValue;
            this.commandName = commandName;
            this.commandClass = commandClass;
            this.description = description;
            this.index = index;
            this.enabled = enabled;
            this.showed = true;
            this.position = -1;
            this.outOfRange = false;
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

        public string CommandName
        {
            get { return commandName; }
        }

        public string CommandClass
        {
            get { return commandClass; }
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

        public int Position
        {
            get { return position; }
            set { position = value; }
        }

        public int CompareTo(object obj)
        {
            int result;
            try
            {
                LiveData ld = obj as LiveData;
                if (this.Index > ld.Index)
                {
                    result = 1;
                }
                else if (this.Index < ld.Index)
                {
                    result = -1;
                }
                else
                {
                    result = 0;
                }
                return result;
            }
            catch
            {
                throw;
            }
        }

        public bool OutOfRange
        {
            get { return outOfRange; }
            set { outOfRange = value; }
        }

        public Dictionary<string, string> MinMax
        {
            get
            {
                if (minMax == null)
                {
                    string[] values = defaultValue.Split('~');
                    minMax = new Dictionary<string, string>();
                    minMax["Min"] = values[0];
                    minMax["Max"] = values[1];
                }
                return minMax;
            }
        }
    }
}

