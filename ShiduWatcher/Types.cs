namespace ShiduWatcher.Types
{
    public class UsageDetail
    {
        public DateTime Timestamp { get; set; }
        public double Duration { get; set; }
    }

    public class ProgramUsageSummary
    {
        public required string ProcessName { get; set; }
        public required string ExecutablePath { get; set; }
        public double TotalDuration { get; set; }
        public required List<UsageDetail> Usage { get; set; }
    }

    public class WebpageUsageSummary
    {
        public required string Url { get; set; }
        public required string Domain { get; set; }
        public double TotalDuration { get; set; }
        public required List<UsageDetail> Usage { get; set; }
    }

    public class UsageReport<T>
    {
        public double TotalDuration { get; set; }
        public required List<T> Details { get; set; }
    }

    public class ProgramUsage
    {
        public string ProcessName { get; }
        public string ExecutablePath { get; }
        public DateTime StartTime { get; }
        public TimeSpan Duration { get; set; }

        public ProgramUsage(string processName, string executablePath, DateTime startTime, TimeSpan duration)
        {
            ProcessName = processName;
            ExecutablePath = executablePath;
            StartTime = startTime;
            Duration = duration;
        }

        public override string ToString()
        {
            return $"ProgramUsage({ProcessName}, {ExecutablePath}, {StartTime}, {Duration})";
        }
    }

    public class WebpageUsage
    {
        public string Url { get; }
        public string Domain { get; }

        public DateTime StartTime { get; }
        public TimeSpan Duration { get; }

        public WebpageUsage(string url, string domain, DateTime startTime, TimeSpan duration)
        {
            Url = url;
            Domain = domain;
            StartTime = startTime;
            Duration = duration;
        }
    }

}
