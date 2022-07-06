using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace DOL.PerformanceStatistics
{
    public class DiskTransfersPerSecondStatistic : IPerformanceStatistic
    {
        IPerformanceStatistic performanceStatistic;

        public DiskTransfersPerSecondStatistic()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) performanceStatistic = new WindowsDiskTransfersPerSecondStatistic();
            else performanceStatistic = new LinuxDiskTransfersPerSecondStatistic();
        }

        public float GetNextValue() => performanceStatistic.GetNextValue();
    }

#if NET
    [SupportedOSPlatform("Windows")]
#endif
    internal class WindowsDiskTransfersPerSecondStatistic : IPerformanceStatistic
    {
        PerformanceCounter performanceCounter;
        public WindowsDiskTransfersPerSecondStatistic()
        {
            performanceCounter = new PerformanceCounter("PhysicalDisk", "Disk Transfers/sec", "_Total");
        }

        public float GetNextValue() => performanceCounter.NextValue();
    }

#if NET
    [UnsupportedOSPlatform("Windows")]
#endif
    internal class LinuxDiskTransfersPerSecondStatistic : IPerformanceStatistic
    {
        private IPerformanceStatistic diskTransfersPerSecondStatistic;

        public LinuxDiskTransfersPerSecondStatistic()
        {
            diskTransfersPerSecondStatistic = new PerSecondStatistic(new LinuxTotalDiskTransfers());
        }

        public float GetNextValue() => diskTransfersPerSecondStatistic.GetNextValue();
    }
    
    internal class LinuxTotalDiskTransfers : IPerformanceStatistic
    {
        public float GetNextValue()
        {
            var diskstats = File.ReadAllText("/proc/diskstats");
            var transferCount = 0L;
            foreach (var line in diskstats.Split('\n'))
            {
                var columns = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (columns.Length < 14) continue;
                var deviceName = columns[2];
                if (char.IsDigit(deviceName[deviceName.Length - 1])) continue;

                var readIO = Convert.ToInt64(columns[3]);
                var writeIO = Convert.ToInt64(columns[7]);
                var discardIO = 0L;
                if (columns.Length >= 18) discardIO = Convert.ToInt64(columns[14]);
                transferCount += (readIO + writeIO + discardIO);
            }
            return transferCount;
        }
    }
}