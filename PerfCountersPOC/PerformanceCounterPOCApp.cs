namespace PerfCountersPOC.Console
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using PerfCountersPoc.CounterExamples;

    public class PerformanceCounterPocApp
    {
        public static string CounterCategory = "PerformanceCounterPoc";

        public static void Main()
        {
            var counterWorkers = GetCounterWorkers().ToList();

            if (!CountersAlreadySetup(counterWorkers))
            {
                SetupCounters(counterWorkers);
                Console.WriteLine("Just setup the Performance Category Counter.  Press enter to exit, and then re-run the application..");
                Console.ReadLine();
                return;
            }

            var cancellationTokenSoure = new CancellationTokenSource();

            foreach (var counterWorker in counterWorkers)
            {
                Task.Factory.StartNew(
                    () =>
                        {
                            Console.WriteLine("starting task");
                            counterWorker.DoStuffThatUpdatesCounters(cancellationTokenSoure.Token);
                        },
                    cancellationTokenSoure.Token);
            }

            Console.WriteLine("Press enter to cancel..");
            Console.ReadLine();
            cancellationTokenSoure.Cancel();
            Console.WriteLine("Workers stopped.  Enter to quit.");
            Console.ReadLine();
        }

        private static bool CountersAlreadySetup(IEnumerable<ICounterExample> counterWorkers)
        {
            return PerformanceCounterCategory.Exists(CounterCategory) && counterWorkers.All(AllCountersFoundForWorker);
        }

        private static bool AllCountersFoundForWorker(ICounterExample counterCounterExample)
        {
            return true;
            var countersForWorker = counterCounterExample.GetCounterCreationData();
            return countersForWorker.All(
                counter =>
                    {
                        // the next line is always throwing a 'category doesnt exist' error :(
                        if (!PerformanceCounterCategory.CounterExists(CounterCategory, counter.CounterName))
                        {
                            return false;
                        }

                        return new PerformanceCounter(CounterCategory, counter.CounterName).CounterType == counter.CounterType;
                    });
        }

        private static IEnumerable<ICounterExample> GetCounterWorkers()
        {
            return new ICounterExample[]
                       {
                           new AlwaysHasAValueCounterExample(CounterCategory),
                           new AverageOperationDurationCounterExample(CounterCategory),
                           new OperationsPerSecondCounterExample(CounterCategory)
                       };
        }

        private static void SetupCounters(IEnumerable<ICounterExample> counterWorkers)
        {
            if (PerformanceCounterCategory.Exists(CounterCategory))
            {
                Console.WriteLine("Deleting Performance Category {0}", CounterCategory);
                PerformanceCounterCategory.Delete(CounterCategory);
            }

            var counterCreationDatas = counterWorkers.SelectMany(counterWorker => counterWorker.GetCounterCreationData()).ToArray();

            var counterDataCollection = new CounterCreationDataCollection(); 
            counterDataCollection.AddRange(counterCreationDatas);

            Console.WriteLine("Creating Performance Category {0}", CounterCategory);
            PerformanceCounterCategory.Create(
                CounterCategory,
                "Demonstrates usage of various performance counter types.",
                PerformanceCounterCategoryType.SingleInstance, 
                counterDataCollection);
        }
    }
}
