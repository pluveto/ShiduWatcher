namespace ShiduWatcher.Types
{
    public class ProgramUsageDetail
    {
        public DateTime Timestamp { get; set; }
        public double Duration { get; set; }
    }

    public class ProgramUsageSummary
    {
        public required string ProcessName { get; set; }
        public required string ExecutablePath { get; set; }
        public double TotalDuration { get; set; }
        public required List<ProgramUsageDetail> Usage { get; set; }
    }

    public class UsageReport
    {
        public double TotalDuration { get; set; }
        public required List<ProgramUsageSummary> Details { get; set; }
    }

    public class ProgramUsage
    {
        public string ProcessName { get; }
        public string ExecutablePath { get; }
        public DateTime StartTime { get; }
        public TimeSpan Duration { get; }

        public ProgramUsage(string processName, string executablePath, DateTime startTime, TimeSpan duration)
        {
            ProcessName = processName;
            ExecutablePath = executablePath;
            StartTime = startTime;
            Duration = duration;
        }

        public ProgramUsage accumulate(ProgramUsage other)
        {
            if (ProcessName != other.ProcessName)
            {
                throw new ArgumentException("ProgramUsage objects must have the same process name");
            }

            return new ProgramUsage(ProcessName, ExecutablePath, StartTime, other.StartTime + other.Duration - StartTime);
        }

        public override string ToString()
        {
            return $"ProgramUsage({ProcessName}, {ExecutablePath}, {StartTime}, {Duration})";
        }
    }

}
