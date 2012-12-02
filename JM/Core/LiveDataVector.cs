using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace JM.Core
{
    public class LiveDataVector : List<LiveData>//, INotifyPropertyChanged
    {
        //private bool disposed = false;
        private List<int> showIndexes;
        private Dictionary<int, int> showPositions;
        private List<int> enabledIndexes;
        private int currentEnabledIndex;
        private object mutex;
        private Dictionary<string, LiveData> queryByShortName;

        //public delegate void ValueChange(int index, string value);

        //public ValueChange OnValueChange;

        //[DllImport("JMCore", EntryPoint = "live_data_vector_new", CallingConvention = CallingConvention.Cdecl)]
        //private static extern IntPtr New();

        //[DllImport("JMCore", EntryPoint = "live_data_vector_dispose", CallingConvention = CallingConvention.Cdecl)]
        //private static extern void Dispose(IntPtr p);

        //[DllImport("JMCore", EntryPoint = "live_data_vector_index", CallingConvention = CallingConvention.Cdecl)]
        //private static extern IntPtr Index(IntPtr p, uint index);

        //[DllImport("JMCore", EntryPoint = "live_data_vector_size", CallingConvention = CallingConvention.Cdecl)]
        //private static extern uint Size(IntPtr p);

        public LiveDataVector()
            : base()
        {
            showIndexes = new List<int>();
            showPositions = new Dictionary<int, int>();
            enabledIndexes = new List<int>();
            currentEnabledIndex = -1;
            mutex = new object();
            queryByShortName = new Dictionary<string, LiveData>();
        }

        public new void Add(LiveData ld)
        {
            base.Add(ld);
            queryByShortName[ld.ShortName] = ld;
            //ld.PropertyChanged += (sender, e) =>
            //{
            //    if (OnValueChange != null)
            //    {
            //        OnValueChange(ShowedPosition(ld.Index), ld.Value);
            //    }
            //};
        }

        //internal void Initialize()
        //{
        //    for (int i = 0; i < Size(p); i++)
        //    {
        //        IntPtr ptr = Index(p, Convert.ToUInt32(i));
        //        if (ptr != IntPtr.Zero)
        //        {
        //            LiveData ld = new LiveData(ptr);
        //            ld.Index = i;
        //            liveDatas.Add(ld);
        //            ld.PropertyChanged += (sender, e) =>
        //            {
        //                if (OnValueChange != null)
        //                {
        //                    OnValueChange(ShowedPosition(ld.Index), ld.Value);
        //                }
        //            };
        //        }
        //    }
        //}

        //internal IntPtr UnmanagedPointer
        //{
        //    get { return p; }
        //}

        //void IDisposable.Dispose()
        //{
        //    Dispose(true);
        //    GC.SuppressFinalize(this);
        //}

        //protected virtual void Dispose(bool isDisposing)
        //{
        //    if (!disposed)
        //    {
        //        if (isDisposing)
        //        {
        //            Dispose(p);
        //            disposed = true;
        //        }
        //    }
        //}

        //public void Close()
        //{
        //    ((IDisposable)this).Dispose();
        //}

        //~LiveDataVector()
        //{
        //    Dispose(true);
        //}

        //public LiveData this[int index]
        //{
        //    get { return liveDatas[index]; }
        //}

        //public int Count
        //{
        //    get
        //    {
        //        return liveDatas.Count;
        //    }
        //}

        public int EnabledCount
        {
            get { return enabledIndexes.Count; }
        }

        public int ShowedCount
        {
            get { return showIndexes.Count; }
        }

        public int NextShowedIndex()
        {
            lock (mutex)
            {
                if (showIndexes.Count == 0)
                {
                    return -1;
                }

                int ret = showIndexes[currentEnabledIndex];
                ++currentEnabledIndex;
                if (currentEnabledIndex > showIndexes.Count - 1)
                    currentEnabledIndex = 0;
                return ret;
            }
        }

        public int EnabledIndex(int index)
        {
            lock (mutex)
            {
                if (index > enabledIndexes.Count)
                {
                    return -1;
                }

                return enabledIndexes[index];
            }
        }

        public int ShowedPosition(int index)
        {
            lock (mutex)
            {
                return showPositions[index];
            }
        }

        public int ShowedIndex(int index)
        {
            lock (mutex)
            {
                if (index >= showIndexes.Count)
                {
                    return -1;
                }

                return showIndexes[index];
            }
        }

        public void DeployEnabledIndex()
        {
            lock (mutex)
            {
                Sort();
                enabledIndexes.Clear();
                for (int i = 0; i < Count; i++)
                {
                    this[i].Position = i;
                    if (this[i].Enabled)
                    {
                        enabledIndexes.Add(i);
                    }
                }
            }
        }

        public void DeployShowedIndex()
        {
            lock (mutex)
            {
                showIndexes.Clear();

                int j = 0;
                for (int i = 0; i < Count; i++)
                {
                    if (this[i].Enabled && this[i].Showed)
                    {
                        showIndexes.Add(i);
                        showPositions[i] = j++;
                    }
                }

                //currentEnabledIndex = showIndexes[0];
                currentEnabledIndex = 0;
            }
        }

        public ObservableCollection<LiveData> Items
        {
            get
            {
                DeployShowedIndex();
                ObservableCollection<LiveData> ret = new ObservableCollection<LiveData>();
                foreach(int i in showIndexes)
                {
                    ret.Add(this[i]);
                }
                return ret;
            }
        }

        public LiveData this[string shortName]
        {
            get
            {
                return queryByShortName[shortName];
            }
        }
    }
}

