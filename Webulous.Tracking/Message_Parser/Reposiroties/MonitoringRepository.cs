using Dapper;
using Message_Parser.Entities;
using MySqlConnector;
using System.Data;

namespace Message_Parser.Reposiroties
{
    public class MonitoringRepository
    {
        public void Upsert(FileMonitoring monitoring, MySqlConnection connection)
        {
            connection.Execute("""
                INSERT INTO file_monitoring (file_name, treated_date, speed, treated_logs, skipped_logs, status)
                VALUES (@FileName, @TreatementDate, @Speed, @TreatedLogs, @SkippedLogs, @Status)
                ON DUPLICATE KEY UPDATE
                    treated_date = @TreatementDate,
                    status = @Status
                """, monitoring);
        }

        public void Update(FileMonitoring monitoring, MySqlConnection connection)
        {
            connection.Execute("""
                UPDATE file_monitoring SET speed = @Speed, treated_logs = @TreatedLogs, skipped_logs = @SkippedLogs, status = @Status
                WHERE file_name = @FileName
                """,
                monitoring);
                
        }
    }
}
