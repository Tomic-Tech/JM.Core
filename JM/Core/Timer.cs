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
            ticks = Convert.ToInt64((Stopwatch.Frequency / 1000) * time.TotalMilliseconds);
        }

        public static Timer FromMicroseconds(double time)
        {
            return new Timer(Convert.ToInt64((Stopwatch.Frequency / 1000000) * time));
        }

        public static Timer FromMilliseconds(double time)
        {
            return new Timer(Convert.ToInt64((Stopwatch.Frequency / 1000) * time));
        }

        public static Timer FromSeconds(double time)
        {
            return new Timer(Convert.ToInt64(Stopwatch.Frequency * time));
        }

        public double Microseconds
        {
            get
            {
                return Convert.ToInt64((ticks / Stopwatch.Frequency) * 1000000);
            }
            set
            {
                ticks = Convert.ToInt64((Stopwatch.Frequency / 1000000) * value);
            }
        }

        public double Milliseconds
        {
            get
            {
                return Convert.ToInt64((ticks / Stopwatch.Frequency) * 1000);
            }
            set
            {
                ticks = Convert.ToInt64((Stopwatch.Frequency / 1000) * value);
            }
        }

        public double Seconds
        {
            get
            {
                return Convert.ToInt64((ticks / Stopwatch.Frequency));
            }
            set
            {
                ticks = Convert.ToInt64(Stopwatch.Frequency * value);
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