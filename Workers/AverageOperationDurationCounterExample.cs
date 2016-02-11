namespace PerfCountersPoc.CounterExamples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an example of a custom <see cref="PerformanceCounterType.AverageTimer32"/> counter.
    /// 
    /// The <see cref="PerformanceCounterType.AverageTimer32"/>  counter is, according to MSDN:  
    /// "An average counter that measures the time it takes, on average, to complete a process or operation."
    /// 
    /// The value that each sample represents is the average time of all operations measured since the last sample.  
    /// A sampled value is calculated by taking the elapsed time of all operations since the last sample, and 
    /// dividing it by the number of operations since the last sample.
    /// 
    /// How to use the counter:
    /// ------------------------
    /// You actually need to use two counters to get this to work. 
    /// A base <see cref="PerformanceCounterType.AverageBase"/> counter and an <see cref="PerformanceCounterType.AverageTimer32"/> counter.
    /// 
    /// You increment the base counter every time an operation finishes, and
    /// You calculate the duration of each operation in ticks (e.g. using a <see cref="Stopwatch"/>) and increment 
    /// the main counter by that duration.
    /// 
    /// Possible usage scenarios:
    /// -------------------------
    /// * Anything where you want to keep track of the average duration of operations.
    /// 
    /// This contrived example
    /// -----------------------
    /// Simulates operations of duration up to 450ms occuring constantly, with a delay of up to 50ms between operations.
    /// 
    /// So at least 2 operations should occur every second, 
    /// and the counter values (i.e. the average duration of the operations that occured in the last sample) should fluctuate 
    /// around the 250ms mark
    /// 
    /// </summary>
    public class AverageOperationDurationCounterExample : CounterExampleBase, ICounterExample
    {
        private const string CounterName = "AverageOperationDuration";
        private const PerformanceCounterType CounterType = PerformanceCounterType.AverageTimer32;
        private PerformanceCounter perfCounter;

        private const string BaseCounterName = "AverageOperationDurationBase";
        private const PerformanceCounterType BaseCounterType = PerformanceCounterType.AverageBase;
        private PerformanceCounter perfCounterBase;

        private const int MaxOperationDuration = 450;
        private const int MaxDelayBetweenOperations = 50;

        public AverageOperationDurationCounterExample(string counterCategory)
            : base(counterCategory)
        {
        }

        public async Task DoStuffThatUpdatesCounters(CancellationToken cancellationToken)
        {
            this.perfCounter = new PerformanceCounter(this.CounterCategory, CounterName)
            {
                ReadOnly = false,
                RawValue = 0
            };

            this.perfCounterBase = new PerformanceCounter(this.CounterCategory, BaseCounterName)
            {
                ReadOnly = false,
                RawValue = 0
            };

            var rnd = new Random(DateTime.Now.Millisecond);

            while (!cancellationToken.IsCancellationRequested)
            {
                await this.LogOperation(rnd, cancellationToken);

                // simulate a delay between operations
                await Task.Delay(rnd.Next(MaxDelayBetweenOperations), cancellationToken);
            }
        }

        private async Task LogOperation(Random rnd, CancellationToken cancellationToken)
        {
            var watch = new Stopwatch();
            watch.Start();

            // simulate a duration of an operation
            await Task.Delay(rnd.Next(MaxOperationDuration), cancellationToken);

            watch.Stop();

            this.perfCounter.IncrementBy(watch.ElapsedTicks);
            this.perfCounterBase.Increment();
        }

        public IEnumerable<CounterCreationData> GetCounterCreationData()
        {
            return new[]
            {
                new CounterCreationData { CounterType = CounterType, CounterName = CounterName, CounterHelp = "some help"},
                new CounterCreationData { CounterType = BaseCounterType, CounterName = BaseCounterName, CounterHelp = "some other help"}
            };
        }
    }
}