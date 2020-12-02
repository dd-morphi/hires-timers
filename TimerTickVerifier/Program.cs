using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using MathNet.Numerics.Statistics;

namespace TimerTickVerifier
{
    class Program
    {
        const int TEST_TIME = 1000;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Give timer resolution in ms");
                return;
            }

            var interval = Int32.Parse(args[0]);

            
            PrintResults(TestSystemThreadingTimer(interval, TEST_TIME));
            PrintResults(TestSystemTimersTimer(interval, TEST_TIME));
            PrintResults(TestMonoSystemThreadingTimer(interval, TEST_TIME));
        }

        private static List<long> TestSystemTimersTimer(long interval, int testTime)
        {
            var milliseconds = new List<long>();
            var stopWatch = new Stopwatch();

            Console.WriteLine("System.Timers timer");
            stopWatch.Start();
            var aTimer = new System.Timers.Timer(interval);
            aTimer.Elapsed += (s, e) => milliseconds.Add(stopWatch.ElapsedMilliseconds);
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

            Thread.Sleep(testTime);

            aTimer.Stop();
            aTimer.Dispose();

            stopWatch.Stop();
            return milliseconds;
        }

        private static List<long> TestSystemThreadingTimer(long interval, int testTime)
        {
            var milliseconds = new List<long>();
            var stopWatch = new Stopwatch();

            Console.WriteLine("System.Threading timer");
            stopWatch.Start();
            var stateTimer = new System.Threading.Timer(_ => milliseconds.Add(stopWatch.ElapsedMilliseconds),
                null, interval, interval);

            Thread.Sleep(testTime);

            stateTimer.Dispose();
            stopWatch.Stop();
            return milliseconds;
        }

        private static List<long> TestMonoSystemThreadingTimer(long interval, int testTime)
        {
            var milliseconds = new List<long>();
            var stopWatch = new Stopwatch();

            Console.WriteLine("System.Threading timer taken from MONO implementation");
            stopWatch.Start();
            var monoTimer = new Mono.System.Threading.Timer(_ => milliseconds.Add(stopWatch.ElapsedMilliseconds),
                null, interval, interval);

            Thread.Sleep(testTime);

            monoTimer.Dispose();
            stopWatch.Stop();
            return milliseconds;
        }

        private static void PrintResults(List<long> milliseconds)
        {
            var diffs = GetDiffs(milliseconds).ToList();
            var (average, stdDev) = GetMeanStandardDeviation(diffs);

            Console.WriteLine($"Avg: {average}, StdDev: {stdDev}");
        }

        private static IEnumerable<double> GetDiffs(IList<long> milliseconds)
        {
            var list = new List<double>();
            for (var i = 1; i < milliseconds.Count; i++)
            {
                list.Add((double)milliseconds[i] - milliseconds[i - 1]);
            }

            return list;
        }

        private static Tuple<double, double> GetMeanStandardDeviation(ICollection<double> values)
        {
            return values.Count == 1
                ? new Tuple<double, double>(values.First(), 0.0)
                : values.MeanStandardDeviation();
        }
    }
}
