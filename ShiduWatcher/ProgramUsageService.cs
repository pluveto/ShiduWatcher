using System;
using System.Collections.Generic;
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

        private TimeSpan _minValidDuration = new TimeSpan(0, 0, 10);

        public ProgramUsageService(DatabasePersister databasePersister, int initialInterval = 1000, bool verbose = false)
        {
            _databasePersister = databasePersister;
            _verbose = verbose;
            _isPaused = false;
            _interval = initialInterval;

        }

        public async Task AddUsage(ProgramUsage usage)
        {
            if (usage.Duration < _minValidDuration)
            {
                return;
            }
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
            await _databasePersister.SaveWebpageUsageAsync(usage);
        }

        public void Pause()
        {
            _isPaused = true;
        }

        public void Continue()
        {
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
