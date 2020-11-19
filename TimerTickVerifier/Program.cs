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
                Console.WriteLine("Give timer resolution");
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
            var stateTimer = new Timer(_ => milliseconds.Add(stopWatch.ElapsedMilliseconds),
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
            // milliseconds.ForEach(x => Console.WriteLine(x.ToString()));

            //var diffs = GetDiffs(milliseconds);
            var diffs = GetDiffs2(milliseconds).ToList();
            //Console.WriteLine("Diffs:");

            //diffs.ForEach(x =>
            //{
            //    Console.WriteLine(x);
            //});

            var (average, stdDev) = GetMeanStandardDeviation(diffs);
            Console.WriteLine($"Avg: {average}, StdDev: {stdDev}");
        }

        private static List<double> GetDiffs(List<long> milliseconds)
        {
            if (milliseconds.Count < 2)
                return new List<double>();

            var diff = (double)milliseconds.First();
            return milliseconds.Skip(1).Select(x =>
            {
                var previous = diff;
                diff = x;
                return (double)x - previous;
            }).ToList();
        }

        private static IEnumerable<double> GetDiffs2(List<long> milliseconds)
        {
            for (var i = 1; i < milliseconds.Count; i++)
            {
                yield return (double)milliseconds[i] - milliseconds[i-1];
            }
        }

        private static Tuple<double, double> GetMeanStandardDeviation(ICollection<double> values)
        {
            return values.Count == 1
                ? new Tuple<double, double>(values.First(), 0.0)
                : values.MeanStandardDeviation();
        }
    }
}
