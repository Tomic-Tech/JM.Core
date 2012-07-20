using System;
using System.Diagnostics;

namespace JM.Core
{
    public class Timer
    {
        private long ticks;

        public Timer()
        {
            ticks = 0;
        }

        public Timer(long ticks)
        {
            this.ticks = ticks;
        }

        public Timer(TimeSpan time)
        {
            ticks = Convert.ToInt64((Convert.ToDouble(Stopwatch.Frequency) / 1000) * time.TotalMilliseconds);
        }

        public static Timer FromMicroseconds(long time)
        {
            return new Timer(Stopwatch.Frequency * time / 1000000);
        }

        public static Timer FromMilliseconds(long time)
        {
            return new Timer(Stopwatch.Frequency * time / 1000);
        }

        public static Timer FromSeconds(long time)
        {
            return new Timer(Stopwatch.Frequency * time);
        }

        public long Microseconds
        {
            get
            {
                return ticks * 1000000 / Stopwatch.Frequency;
            }
            set
            {
                ticks = Stopwatch.Frequency * value / 1000000;
            }
        }

        public long Milliseconds
        {
            get
            {
                return ticks * 1000 / Stopwatch.Frequency;
            }
            set
            {
                ticks = Stopwatch.Frequency * value / 1000;
            }
        }

        public long Seconds
        {
            get
            {
                return ticks / Stopwatch.Frequency;
            }
            set
            {
                ticks = Stopwatch.Frequency * value;
            }
        }

        //public TimeSpan TimeSpan
        //{
        //    get
        //    {
        //        return TimeSpan.FromMilliseconds(Milliseconds);
        //    }
        //}

        public long Ticks
        {
            get
            {
                return ticks;
            }
        }

    }
}