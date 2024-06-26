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
        private ProgramUsage? _currentUsage;
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

        public async void UpdateUsage(ProgramUsage newUsage)
        {
            if (_isPaused)
            {
                return;
            }

            if (_currentUsage == null || _currentUsage.ProcessName != newUsage.ProcessName)
            {
                _currentUsage = newUsage;
            }
            else
            {
                _currentUsage = _currentUsage.accumulate(newUsage);
            }

            if (_verbose)
            {
                Console.WriteLine(_currentUsage);
            }

            if (_currentUsage.Duration > _minValidDuration)
            {
                await _databasePersister.SaveProgramUsageAsync(_currentUsage);
            }
        }

        public async Task<UsageReport> GetUsageReportAsync(DateTime startTime, DateTime endTime)
        {
            return await _databasePersister.GetUsageReportAsync(startTime, endTime);
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
