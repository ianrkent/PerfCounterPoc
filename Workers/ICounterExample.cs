namespace PerfCountersPoc.CounterExamples
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ICounterExample
    {
        Task DoStuffThatUpdatesCounters(CancellationToken cancellationToken);

        IEnumerable<CounterCreationData> GetCounterCreationData();
    }
}