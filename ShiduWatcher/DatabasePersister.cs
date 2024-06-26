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
        private SQLiteCommand checkCommand;
        private SQLiteCommand insertCommand;
        private SQLiteCommand updateCommand;

        public DatabasePersister()
        {
            InitializeDatabase();

            connection = new SQLiteConnection(connectionString);
            connection.Open();

            string checkQuery = @"
            SELECT COUNT(1) FROM ProgramUsage 
            WHERE ProcessName = @ProcessName AND Timestamp = @Timestamp";
            checkCommand = new SQLiteCommand(checkQuery, connection);
            checkCommand.Parameters.Add(new SQLiteParameter("@ProcessName"));
            checkCommand.Parameters.Add(new SQLiteParameter("@Timestamp"));

            string insertQuery = @"
            INSERT INTO ProgramUsage (ProcessName, ExecutablePath, Timestamp, Duration)
            VALUES (@ProcessName, @ExecutablePath, @Timestamp, @Duration)";
            insertCommand = new SQLiteCommand(insertQuery, connection);
            insertCommand.Parameters.Add(new SQLiteParameter("@ProcessName"));
            insertCommand.Parameters.Add(new SQLiteParameter("@ExecutablePath"));
            insertCommand.Parameters.Add(new SQLiteParameter("@Timestamp"));
            insertCommand.Parameters.Add(new SQLiteParameter("@Duration"));

            string updateQuery = @"
            UPDATE ProgramUsage 
            SET Duration = @Duration 
            WHERE ProcessName = @ProcessName AND Timestamp = @Timestamp";
            updateCommand = new SQLiteCommand(updateQuery, connection);
            updateCommand.Parameters.Add(new SQLiteParameter("@ProcessName"));
            updateCommand.Parameters.Add(new SQLiteParameter("@Timestamp"));
            updateCommand.Parameters.Add(new SQLiteParameter("@Duration"));
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
            }
        }

        public async Task SaveProgramUsageAsync(ProgramUsage usage)
        {
            checkCommand.Parameters["@ProcessName"].Value = usage.ProcessName;
            checkCommand.Parameters["@Timestamp"].Value = usage.StartTime;

            long? count = (long?)await checkCommand.ExecuteScalarAsync();

            if (count != null && count > 0)
            {
                updateCommand.Parameters["@ProcessName"].Value = usage.ProcessName;
                updateCommand.Parameters["@Timestamp"].Value = usage.StartTime;
                updateCommand.Parameters["@Duration"].Value = (int)usage.Duration.TotalSeconds;

                await updateCommand.ExecuteNonQueryAsync();
            }
            else
            {
                insertCommand.Parameters["@ProcessName"].Value = usage.ProcessName;
                insertCommand.Parameters["@ExecutablePath"].Value = usage.ExecutablePath;
                insertCommand.Parameters["@Timestamp"].Value = usage.StartTime;
                insertCommand.Parameters["@Duration"].Value = (int)usage.Duration.TotalSeconds;

                await insertCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task<UsageReport> GetUsageReportAsync(DateTime startTime, DateTime endTime)
        {
            var report = new UsageReport
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
                                Usage = new List<ProgramUsageDetail>()
                            };
                        }

                        var programUsage = programUsageDict[processName];
                        programUsage.TotalDuration += duration;
                        programUsage.Usage.Add(new ProgramUsageDetail
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

        public void Dispose()
        {
            checkCommand?.Dispose();
            insertCommand?.Dispose();
            updateCommand?.Dispose();
            connection?.Dispose();
        }
    }
}
