using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace BCPBuilderConfig
{
    public class StopwatchPlus : Stopwatch
    {
        // protected 
        List<TimeSpan> LapTimes = new List<TimeSpan>();

        // contsuctor starts the stopwatch
        public StopwatchPlus(bool bStart = true) : base()
        {
            this.Start();
        }

        // return the string representation of the difference between the last lap
        public string LapTimeString()
        {
            TimeSpan currentTS = this.Elapsed;
            TimeSpan lastTS = LapTimes.LastOrDefault();

            string sElapsedTime = ElapsedTimeString(currentTS - lastTS, "{0:00}:{1:00}:{2:00}.{3:00}");

            // add the lap to the list
            LapTimes.Add(currentTS);

            return (sElapsedTime);
        }


        // return the string representation of the elapsed times
        public string ElapsedTimeString(string sFormat = "{0:00}:{1:00}:{2:00}.{3:00}")
        {
            return (ElapsedTimeString(this, sFormat));
        }


        // return the string representation of the elapsed times
        public static string ElapsedTimeString(System.Diagnostics.Stopwatch sw, string sFormat)
        {
            TimeSpan ts = sw.Elapsed;
            return (ElapsedTimeString(ts, sFormat));
        }

        // return the string representation of the elapsed times
        public static string ElapsedTimeString(TimeSpan ts, string sFormat)
        {
            // Format and display the TimeSpan value. 
            string elapsedTime = String.Format(sFormat, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            return (elapsedTime);
        }

    }
}

