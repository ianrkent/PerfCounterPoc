namespace PerfCountersPoc.CounterExamples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an example of a custom <see cref="PerformanceCounterType.RateOfCountsPerSecond32"/> counter.
    /// 
    /// The <see cref="PerformanceCounterType.RateOfCountsPerSecond32"/>  counter is, according to MSDN:  
    /// "A difference counter that shows the average number of operations completed during each second of the sample interval"
    /// 
    /// A value that each sample represents is the number of operations that have been completed since 
    /// the previous sample was taken.  
    /// 
    /// This is calculated by taking the current value of the counter when it is sampled and 
    /// subtracting the value of the counter at the previous sample, and dividing by the # of seconds elapsed between samples.
    /// 
    /// How to use the counter:
    /// ------------------------
    /// You just have to update the counter to keep track of the number of operations that have been completed. 
    /// So every time an operation completes, call perfCounter.Increment()
    /// 
    /// Possible usage scenarios:
    /// -------------------------
    /// Any time you want to monitor the throughput of a component, by looking at the number of operations per second that it is doing. E.g:
    /// * the number fulfillment decisions that are made per second for orders being processed, 
    /// 
    /// This contrived example
    /// -----------------------
    /// This example is a little bit too involved for a contrived example, but it operates in either a Slow, Medium or Fast mode for a few seconds at a time.
    /// So the number of operations completed per second will change depending on the mode, and this will lead to a stepped graph in the performance monitor.
    /// 
    /// </summary>
    public class OperationsPerSecondCounterExample : CounterExampleBase, ICounterExample
    {
        private enum SpeedMode
        {
            Slow = 5,
            Medium = 15,
            Fast = 40
        }

        private const string CounterName = "OperationsPerSecond";
        private const PerformanceCounterType CounterType = PerformanceCounterType.RateOfCountsPerSecond32;
        private PerformanceCounter perfCounter;

        public OperationsPerSecondCounterExample(string counterCategory)
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

            while (!cancellationToken.IsCancellationRequested)
            {
                var speed = GetSpeed();
                await this.GoAtSpeedForASecond(speed, cancellationToken);
            }

            Console.WriteLine("Exiting the 'OperationsPerSecond' worker");
        }

        private async Task GoAtSpeedForASecond(SpeedMode speed, CancellationToken cancellationToken)
        {
            var nrOperationsToDo = (int)speed;
            var operationDuration = 1000 / nrOperationsToDo;

            for (var i = 0; i < nrOperationsToDo; i++)
            {
                await Task.Delay(operationDuration, cancellationToken);
                this.perfCounter.Increment(); // increment the perfcounter each time an operation completes.
            }
        }

        private static SpeedMode GetSpeed()
        {
            var currentMinute = DateTime.Now.Second / 2;

            return currentMinute <= 10
                ? SpeedMode.Fast 
                : currentMinute <= 20
                    ? SpeedMode.Slow
                    : SpeedMode.Medium;
        }

        public IEnumerable<CounterCreationData> GetCounterCreationData()
        {
            return new[]
                       {
                           new CounterCreationData { CounterType = CounterType, CounterName = CounterName, CounterHelp = "some help"}
                       };
        }
    }
}