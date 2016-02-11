namespace PerfCountersPoc.CounterExamples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an example of a custom <see cref="PerformanceCounterType.NumberOfItems32"/> counter.
    /// 
    /// The <see cref="PerformanceCounterType.NumberOfItems32"/> and <see cref="PerformanceCounterType.NumberOfItems64"/> counters 
    /// are, according to MSDN:  "An instantaneous counter that shows the most recently observed value. Used, for example, 
    /// to maintain a simple count of items or operations"
    /// 
    /// How to use the counter:
    /// ------------------------
    /// To use counters of this type, update the RawValue property of the counter when applicable.  
    /// When the counter is sampled, the value that was last set to the RawValue property will be returned.
    /// 
    /// Possible usage scenarios:
    /// -------------------------
    ///  * Report on the size of a cache - you will have to update the RawValue with the size of the cache when applicable.
    ///  * Counting the number of transactions/orders/hits etc.  The caveat with this is you need to consider when to reset the counter to Zero.
    /// 
    /// This contrived example
    /// -----------------------
    /// This example sets the raw value over time based on the mathematical sine function. 
    /// One full sine curve will be covered in 60 seconds, so you should see a nice sine graph appearing in your perfmon tool :)
    /// 
    /// </summary>
    public class AlwaysHasAValueCounterExample : CounterExampleBase, ICounterExample
    {
        private const string CounterName = "AlwaysHasAValue";
        private const PerformanceCounterType CounterType = PerformanceCounterType.NumberOfItems32;

        public AlwaysHasAValueCounterExample(string counterCategory)
            : base(counterCategory)
        {
        }

        public async Task DoStuffThatUpdatesCounters(CancellationToken cancellationToken)
        {
            var perfCounter = new PerformanceCounter(this.CounterCategory, CounterName)
            {
                ReadOnly = false
            };

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(200, cancellationToken);    
                perfCounter.RawValue = CalculateCurrentCounterValue(stopWatch);
            }

            Console.WriteLine("Exiting the 'AlwaysHasAValue' worker");
        }

        private static long CalculateCurrentCounterValue(Stopwatch stopWatch)
        {
            // Use a value based on the Mathematical Sine function, which always returns a value beteen -1 and 1
            // Translate that to a value between 0 and 100
            return (long)((Math.Sin(stopWatch.Elapsed.TotalMilliseconds * 0.0001) + 1) * 50);
        }

        public IEnumerable<CounterCreationData> GetCounterCreationData()
        {
            return new[] { new CounterCreationData { CounterType = CounterType, CounterName = CounterName } };
        }
    }
}