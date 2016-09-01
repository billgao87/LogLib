using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.Common.Tracers;

namespace MyTraceLogTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Tracer.SetMinTraceLevel(4);
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var task1 = Task.Factory.StartNew(() => Test("Task 1", 100000));
            var task2 = Task.Factory.StartNew(() => Test("Task 2", 100000));
            //var task3 = Task.Factory.StartNew(() => Test("Task 3", 100000));
            //var task4 = Task.Factory.StartNew(() => Test("Task 4", 100000));
            Task.WaitAll(task1, task2);
            watch.Stop();
            Console.WriteLine("Spend " + watch.ElapsedMilliseconds + "ms");

            Console.ReadKey();
        }

        private static void Test(string taskName, int times)
        {
            for (int i = 0; i < times; i++)
            {
                Tracer.TraceWarning(taskName + " Warning");
                Tracer.TraceError(taskName + " Error");
                Tracer.TraceInfo(taskName + " Infor");
            }

            try
            {
                int[] ints = new int[10];
                ints[11] = 10;
            }
            catch (Exception e)
            {
                Tracer.TraceException(e);
            }
        }
    }
}
