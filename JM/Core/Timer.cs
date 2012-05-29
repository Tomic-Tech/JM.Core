using System;

namespace JM.Core
{
    public class Timer
    {
        private TimeSpan time;

        public Timer()
        {
            time = new TimeSpan();
        }

        public Timer(TimeSpan time)
        {
            this.time = time;
        }

        public static Timer FromMicroseconds(double time)
        {
            Timer result = new Timer(TimeSpan.FromTicks(Convert.ToInt64(time * 10)));
            return result;
        }

        public static Timer FromMilliseconds(double time)
        {
            return new Timer(TimeSpan.FromMilliseconds(time));
        }

        public static Timer FromSeconds(double time)
        {
            return new Timer(TimeSpan.FromSeconds(time));
        }

        public double Microseconds
        {
            get
            {
                return time.Ticks / 10;
            }
            set
            {
                time = TimeSpan.FromTicks(Convert.ToInt64(value * 10));
            }
        }

        public double Milliseconds
        {
            get
            {
                return time.TotalMilliseconds;
            }
            set
            {
                time = TimeSpan.FromMilliseconds(value);
            }
        }

        public double Seconds
        {
            get
            {
                return time.TotalSeconds;
            }
            set
            {
                time = TimeSpan.FromSeconds(value);
            }
        }

        public TimeSpan TimeSpan
        {
            get
            {
                return time;
            }
        }

    }
}