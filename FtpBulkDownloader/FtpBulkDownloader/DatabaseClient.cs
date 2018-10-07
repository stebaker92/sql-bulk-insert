using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace FtpBulkDownloader
{
    internal class DatabaseClient
    {
        private readonly LoggingClient loggingClient;
        private string connectionString;
        private SqlConnection conn;

        /// <summary>
        /// Stored procedure to merge the existing & current data sets
        /// </summary>
        private string procName = "FTP_Merge";

        /// <summary>
        /// A temporary / staging table for the new data.
        /// </summary>
        private string stagingTable = "FTP_Staging";

        public DatabaseClient(string connectionString, LoggingClient loggingClient)
        {
            this.connectionString = connectionString;
            this.loggingClient = loggingClient;


            conn = new SqlConnection(connectionString);

            using (var command = new SqlCommand(string.Empty, conn))
            {
                conn.Open();

                loggingClient.Log("Truncating staging table").Wait();

                command.CommandText = "TRUNCATE TABLE " + this.stagingTable;
                command.ExecuteNonQuery();
            }
        }

        public void InsertData(List<string> list, int pageNumber, string dbColumnName)
        {
            DataTable dt = new DataTable();

            loggingClient.Log("Creating data table from list").Wait();

            dt = ConvertToDataTable(list, dbColumnName);

            // TODO - do we need this?
            using (var command = new SqlCommand(string.Empty, conn))
            {
                loggingClient.Log($"Inserting page {pageNumber} into staging table").Wait();

                // Bulk insert into staging table
                using (SqlBulkCopy bulkcopy = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.TableLock))
                {
                    bulkcopy.BulkCopyTimeout = (int)TimeSpan.FromMinutes(5).TotalSeconds;
                    bulkcopy.DestinationTableName = this.stagingTable;
                    bulkcopy.BatchSize = 5_000;
                    bulkcopy.WriteToServer(dt);
                    bulkcopy.Close();
                }

                loggingClient.Log($"Inserted page {pageNumber}").Wait();
            }
        }

        public void Consolidate()
        {
            using (var command = new SqlCommand(string.Empty, conn))
            {
                loggingClient.Log("Running table consolidation").Wait();
                command.CommandTimeout = (int)TimeSpan.FromMinutes(10).TotalSeconds;
                command.CommandText = "EXEC " + this.procName;
                var result = command.ExecuteScalar() as int?;

                loggingClient.Log($"Consolidation finished. Rows changed: {result}").Wait();
            }
        }

        private static DataTable ConvertToDataTable<T>(List<T> list, string dbColumnName)
        {
            var dt = new DataTable();
            dt.Columns.Add(dbColumnName);

            foreach (var li in list)
            {
                dt.Rows.Add(li);
            }

            return dt;
        }

        internal void Close()
        {
            conn.Close();
            conn.Dispose();
        }
    }
}