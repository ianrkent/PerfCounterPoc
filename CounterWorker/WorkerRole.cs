namespace PerfCountersPOC.Cloud.Worker
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.ServiceRuntime;

    using PerfCountersPoc.CounterExamples;

    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private const string CounterCategory = "PerfCounterPocCustomCounters";

        public override void Run()
        {
            Trace.TraceInformation("CounterWorker is running");

            try
            {
                RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            var result = base.OnStart();

            Trace.TraceInformation("CounterWorker has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("CounterWorker is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("CounterWorker has stopped");
        }

        private static async Task RunAsync(CancellationToken cancellationToken)
        {
            var workers = GetCounterWorkers();
            var tasks = workers.Select(worker => new Task(() => worker.DoStuffThatUpdatesCounters(cancellationToken))).ToList();

            foreach (var task in tasks)
            {
                task.Start();
            }

            await Task.WhenAll(tasks);
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
    }
}
