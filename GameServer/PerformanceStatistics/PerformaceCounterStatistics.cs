using System.Diagnostics;
using System.Runtime.Versioning;

namespace DOL.PerformanceStatistics
{
#if NET
    [SupportedOSPlatform("Windows")]
#endif
    public class PerformanceCounterStatistic : IPerformanceStatistic
    {
        private PerformanceCounter performanceCounter;

        public PerformanceCounterStatistic(string categoryName, string counterName, string instanceName)
        {
            performanceCounter = new PerformanceCounter(categoryName, counterName, instanceName);
            performanceCounter.NextValue();
        }

        public float GetNextValue() => performanceCounter.NextValue();
    }
}