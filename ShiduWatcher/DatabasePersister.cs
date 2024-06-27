using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using ShiduWatcher.Types;

namespace ShiduWatcher
{
    public class DatabasePersister : IDisposable
    {
        private string connectionString = "Data Source=usage_stats.db;Version=3;";
        private SQLiteConnection connection;
        private SQLiteCommand programUsageInsertCommand;
        private SQLiteCommand webpageUsageInsertCommand;

        public DatabasePersister()
        {
            InitializeDatabase();

            connection = new SQLiteConnection(connectionString);
            connection.Open();

            string insertQuery = @"
            INSERT INTO ProgramUsage (ProcessName, ExecutablePath, Timestamp, Duration)
            VALUES (@ProcessName, @ExecutablePath, @Timestamp, @Duration)";
            programUsageInsertCommand = new SQLiteCommand(insertQuery, connection);
            programUsageInsertCommand.Parameters.Add(new SQLiteParameter("@ProcessName"));
            programUsageInsertCommand.Parameters.Add(new SQLiteParameter("@ExecutablePath"));
            programUsageInsertCommand.Parameters.Add(new SQLiteParameter("@Timestamp"));
            programUsageInsertCommand.Parameters.Add(new SQLiteParameter("@Duration"));

            string webpageUsageInsertQuery = @"
            INSERT INTO WebpageUsage (Domain, Timestamp, Duration)
            VALUES (@Domain, @Timestamp, @Duration)";
            webpageUsageInsertCommand = new SQLiteCommand(webpageUsageInsertQuery, connection);
            webpageUsageInsertCommand.Parameters.Add(new SQLiteParameter("@Domain"));
            webpageUsageInsertCommand.Parameters.Add(new SQLiteParameter("@Timestamp"));
            webpageUsageInsertCommand.Parameters.Add(new SQLiteParameter("@Duration"));
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS ProgramUsage (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp DATETIME,
                    ProcessName TEXT,
                    ExecutablePath TEXT,
                    Duration INTEGER,
                    UNIQUE(ProcessName, Timestamp)
                )";
                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }

                string createWebpageUsageTableQuery = @"
                CREATE TABLE IF NOT EXISTS WebpageUsage (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp DATETIME,
                    Domain TEXT,
                    Duration INTEGER,
                    UNIQUE(Domain, Timestamp)
                )";
                using (var command = new SQLiteCommand(createWebpageUsageTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public async Task SaveProgramUsageAsync(ProgramUsage usage)
        {
            programUsageInsertCommand.Parameters["@ProcessName"].Value = usage.ProcessName;
            programUsageInsertCommand.Parameters["@ExecutablePath"].Value = usage.ExecutablePath;
            programUsageInsertCommand.Parameters["@Timestamp"].Value = usage.StartTime;
            programUsageInsertCommand.Parameters["@Duration"].Value = (int)usage.Duration.TotalSeconds;

            await programUsageInsertCommand.ExecuteNonQueryAsync();
        }

        public async Task SaveWebpageUsageAsync(WebpageUsage usage)
        {
            webpageUsageInsertCommand.Parameters["@Domain"].Value = usage.Domain;
            webpageUsageInsertCommand.Parameters["@Timestamp"].Value = usage.StartTime;
            webpageUsageInsertCommand.Parameters["@Duration"].Value = (int)usage.Duration.TotalSeconds;

            await webpageUsageInsertCommand.ExecuteNonQueryAsync();
        }

        public async Task<UsageReport<ProgramUsageSummary>> GetUsageReportAsync(DateTime startTime, DateTime endTime)
        {
            var report = new UsageReport<ProgramUsageSummary>
            {
                TotalDuration = 0,
                Details = new List<ProgramUsageSummary>()
            };

            string query = @"
            SELECT ProcessName, ExecutablePath, Timestamp, Duration
            FROM ProgramUsage
            WHERE Timestamp BETWEEN @StartTime AND @EndTime
            ORDER BY ProcessName, Timestamp";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@EndTime", endTime);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var programUsageDict = new Dictionary<string, ProgramUsageSummary>();

                    while (await reader.ReadAsync())
                    {
                        string processName = string.Empty;
                        string executablePath = string.Empty;
                        DateTime timestamp = default;
                        int duration = 0;

                        try { processName = reader.GetString(0); } catch { }
                        try { executablePath = reader.GetString(1); } catch { }
                        try { timestamp = reader.GetDateTime(2); } catch { }
                        try { duration = reader.GetInt32(3); } catch { }

                        if (processName.Length == 0)
                        {
                            continue;
                        }

                        if (!programUsageDict.ContainsKey(processName))
                        {
                            programUsageDict[processName] = new ProgramUsageSummary
                            {
                                ProcessName = processName,
                                ExecutablePath = executablePath,
                                TotalDuration = 0,
                                Usage = new List<UsageDetail>()
                            };
                        }

                        var programUsage = programUsageDict[processName];
                        programUsage.TotalDuration += duration;
                        programUsage.Usage.Add(new UsageDetail
                        {
                            Timestamp = timestamp,
                            Duration = duration
                        });

                        report.TotalDuration += duration;
                    }

                    report.Details.AddRange(programUsageDict.Values);
                }
            }

            return report;
        }

        public async Task<UsageReport<WebpageUsageSummary>> GetWebpageUsageReportAsync(DateTime startTime, DateTime endTime)
        {
            var report = new UsageReport<WebpageUsageSummary>
            {
                TotalDuration = 0,
                Details = new List<WebpageUsageSummary>()
            };

            string query = @"
            SELECT Domain, Timestamp, Duration
            FROM WebpageUsage
            WHERE Timestamp BETWEEN @StartTime AND @EndTime
            ORDER BY Domain, Timestamp";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StartTime", startTime);
                command.Parameters.AddWithValue("@EndTime", endTime);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var webpageUsageDict = new Dictionary<string, WebpageUsageSummary>();

                    while (await reader.ReadAsync())
                    {
                        string domain = string.Empty;
                        DateTime timestamp = default;
                        int duration = 0;

                        try { domain = reader.GetString(0); } catch { }
                        try { timestamp = reader.GetDateTime(2); } catch { }
                        try { duration = reader.GetInt32(3); } catch { }

                        if (domain.Length == 0)
                        {
                            continue;
                        }

                        if (!webpageUsageDict.ContainsKey(domain))
                        {
                            webpageUsageDict[domain] = new WebpageUsageSummary
                            {
                                Domain = domain,
                                TotalDuration = 0,
                                Usage = new List<UsageDetail>()
                            };
                        }

                        var webpageUsage = webpageUsageDict[domain];
                        webpageUsage.TotalDuration += duration;
                        webpageUsage.Usage.Add(new UsageDetail
                        {
                            Timestamp = timestamp,
                            Duration = duration
                        });

                        report.TotalDuration += duration;
                    }

                    report.Details.AddRange(webpageUsageDict.Values);
                }
            }

            return report;
        }

        public void Dispose()
        {
            programUsageInsertCommand?.Dispose();
            webpageUsageInsertCommand?.Dispose();
            connection?.Dispose();
        }
    }
}
