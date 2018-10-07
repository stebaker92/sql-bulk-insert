using System.Data;

namespace FtpBulkDownloader
{
    internal class AppSettings
    {
        public string ConnectionString { get; set; }
        public string DbColumnName { get; set; }

        public string LoggingApi { get; set; }
        public string SlackUrl { get; set; }
        public string SlackChannel { get; set; }

        public string FtpServer { get; set; }
        public string FtpUserName { get; set; }
        public string FtpPassword { get; set; }
        public string SourceFilePath { get; set; }
        public string DestinationFile { get; set; }
    }
}