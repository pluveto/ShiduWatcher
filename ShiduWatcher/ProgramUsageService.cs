using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShiduWatcher.Types;

namespace ShiduWatcher
{
    public class ProgramUsageService
    {
        private readonly DatabasePersister _databasePersister;
        private bool _verbose;
        private bool _isPaused;
        private int _interval;

        private static readonly TimeSpan defaultMinValidDuration = new TimeSpan(0, 0, 5);
        private TimeSpan _minValidDuration;

        public ProgramUsageService(DatabasePersister databasePersister, int initialInterval = 1000, bool verbose = false, TimeSpan? minValidDuration = null)
        {
            _databasePersister = databasePersister;
            _verbose = verbose;
            _isPaused = false;
            _interval = initialInterval;
            _minValidDuration = minValidDuration ?? defaultMinValidDuration;
        }

        public async Task AddUsage(ProgramUsage usage)
        {
            if (usage.Duration < _minValidDuration)
            {
                return;
            }
            Debug.WriteLine("usage: ", usage.ToString());
            await _databasePersister.SaveProgramUsageAsync(usage);
        }

        public async Task<UsageReport<ProgramUsageSummary>> GetUsageReportAsync(DateTime startTime, DateTime endTime)
        {
            return await _databasePersister.GetUsageReportAsync(startTime, endTime);
        }

        public async Task<UsageReport<WebpageUsageSummary>> GetWebpageUsagesAsync(DateTime startTime, DateTime endTime)
        {
            return await _databasePersister.GetWebpageUsageReportAsync(startTime, endTime);
        }

        public async Task AddWebpageUsage(WebpageUsage usage)
        {
            if (usage.Duration < _minValidDuration)
            {
                return;
            }
            Debug.WriteLine("webpage usage: ", usage.ToString());
            await _databasePersister.SaveWebpageUsageAsync(usage);
        }

        public void Pause()
        {
            Debug.WriteLine("ProgramUsageService is paused");
            _isPaused = true;
        }

        public void Resume()
        {
            Debug.WriteLine("ProgramUsageService is resumed");
            _isPaused = false;
        }

        public bool IsPaused()
        {
            return _isPaused;
        }
        public int GetInterval()
        {
            return _interval;
        }

        public void SetInterval(int interval)
        {
            _interval = interval;
        }
    }
}
