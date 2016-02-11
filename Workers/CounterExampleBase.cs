namespace PerfCountersPoc.CounterExamples
{
    public abstract class CounterExampleBase
    {
        protected string CounterCategory { get; private set; }

        protected CounterExampleBase (string counterCategory)
        {
            this.CounterCategory = counterCategory;
        }
    }
}