using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace JM.Core
{
    public class LiveDataVector : IDisposable
    {
        private IntPtr p;
        private bool disposed = false;
        private List<LiveData> liveDatas;
        private List<int> showIndexes;
        private Dictionary<int, int> showPositions;
        private List<int> enabledIndexes;
        private int currentEnabledIndex;
        private int enabledSize;
        private int showSize;
        private object mutex;

        [DllImport("JMCore", EntryPoint = "live_data_vector_new", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr New();

        [DllImport("JMCore", EntryPoint = "live_data_vector_dispose", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Dispose(IntPtr p);

        [DllImport("JMCore", EntryPoint = "live_data_vector_index", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Index(IntPtr p, int index);

        [DllImport("JMCore", EntryPoint = "live_data_vector_size", CallingConvention = CallingConvention.Cdecl)]
        private static extern uint Size(IntPtr p);

        public LiveDataVector()
        {
            p = New();
            liveDatas = new List<LiveData>();
            showIndexes = new List<int>();
            showPositions = new Dictionary<int, int>();
            enabledIndexes = new List<int>();
            currentEnabledIndex = -1;
            enabledSize = -1;
            showSize = -1;
            mutex = new object();

            for (int i = 0; i < Size(p); i++)
            {
                IntPtr ptr = Index(p, i);
                if (ptr != IntPtr.Zero)
                {
                    LiveData ld = new LiveData(ptr);
                    ld.PropertyChanged += (sender, e) =>
                    {
                        LiveData liveData = (LiveData)sender;
                        if (e.PropertyName == "Value")
                        {

                        }
                        else if (e.PropertyName == "Showed")
                        {
                            SetShowed(liveData.Index, liveData.Showed);
                        }
                        else if (e.PropertyName == "Enabled")
                        {
                            SetEnabled(liveData.Index, liveData.Enabled);
                        }
                    };
                    ld.Enabled = ld.Enabled;
                    ld.Showed = ld.Showed;
                    liveDatas.Add(ld);
                }
            }
        }

        internal IntPtr UnmanagedPointer
        {
            get { return p; }
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

        ~LiveDataVector()
        {
            Dispose(true);
        }

        public LiveData this[int index]
        {
            get { return liveDatas[index]; }
        }

        public int Count
        {
            get { return Convert.ToInt32(Size(p)); }
        }

        public int EnabledCount
        {
            get { return enabledSize; }
        }

        public int ShowedCount
        {
            get { return showSize; }
        }

        public void SetShowed(int index, bool showed)
        {
            lock (mutex)
            {
                if (this[index].Showed)
                {
                    if (this[index].Enabled && showed)
                    {
                        ++showSize;
                    }
                }
                else
                {
                    if (this[index].Enabled && !showed)
                    {
                        --showSize;
                    }
                }
            }
        }

        public void SetEnabled(int index, bool enabled)
        {
            lock (mutex)
            {
                if (this[index].Enabled)
                {
                    if (enabled)
                    {
                        ++enabledSize;
                        if (this[index].Showed)
                        {
                            ++showSize;
                        }
                    }
                }
                else
                {
                    if (!enabled)
                    {
                        --enabledSize;
                        if (this[index].Showed)
                        {
                            --showSize;
                        }
                    }
                }
            }
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
                if (index > showIndexes.Count)
                {
                    return -1;
                }
            }

            return showIndexes[index];
        }

        public void DeployEnabledIndex()
        {
            lock (mutex)
            {
                enabledIndexes.Clear();
                for (int i = 0; i < Count; i++)
                {
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

                currentEnabledIndex = showIndexes[0];
            }
        }
    }
}

