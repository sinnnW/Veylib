using System;
using System.Collections.Generic;
using System.Linq;

namespace Veylib
{
    public static class WorkLoadController
    {
        public static List<List<T>> Seperate<T>(List<T> items, int loadCount)
        {
            var loads = new List<List<T>>();

            // Create lists
            for (int x = 0; x < loadCount; x++)
                loads.Add(new List<T>());

            // Actually seperate the loads
            for (int x = 0; x < items.Count; x++)
                loads[x % loadCount].Add(items[x]);

            // Remove blank loads
            loads = loads.Where(load => load.Count > 0).ToList();

            return loads;
        }
    }

    public class General
    {
        /// <summary>
        /// Get the epoch time
        /// </summary>
        /// https://stackoverflow.com/questions/9453101/how-do-i-get-epoch-time-in-c
        public static long EpochTime
        {
            get
            {
                TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                return (long)t.TotalSeconds;
            }
        }

        /// <summary>
        /// Get the epoch time from DateTime
        /// </summary>
        /// <param name="from"></param>
        /// <returns>Epoch time</returns>
        public static long FromDateTime(DateTime from)
        {
            return ((DateTimeOffset)DateTime.SpecifyKind(from, DateTimeKind.Utc)).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Get a DateTime from epoch time
        /// </summary>
        /// <returns>DateTime</returns>
        /// https://stackoverflow.com/questions/2883576/how-do-you-convert-epoch-time-in-c
        public static DateTime FromEpoch(long epoch)
        {
            return DateTimeOffset.FromUnixTimeSeconds(epoch).DateTime;
        }
    }
}
