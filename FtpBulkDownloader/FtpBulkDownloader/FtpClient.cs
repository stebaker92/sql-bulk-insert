using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FtpBulkDownloader
{
    class FtpClient
    {
        private string ftpServer;
        private string ftpUserName;
        private string ftpPassword;
        private LoggingClient loggingClient;

        public FtpClient(string ftpServer, string ftpUserName, string ftpPassword, LoggingClient loggingClient)
        {
            this.ftpServer = ftpServer;
            this.ftpUserName = ftpUserName;
            this.ftpPassword = ftpPassword;
            
            // TODO - set up dependency injection
            this.loggingClient = loggingClient;
        }

        internal void DownloadFile(string sourceFilePath, string destinationFile)
        {
            var connectionInfo = new ConnectionInfo(ftpServer,
                                        ftpUserName,
                                        new PasswordAuthenticationMethod(ftpUserName, ftpPassword));

            using (var client = new SftpClient(connectionInfo))
            {
                loggingClient.Log("Connecting to FTP server.").Wait();

                client.Connect();

                loggingClient.Log("Connected to FTP server.").Wait();

                loggingClient.Log("Downloading File").Wait();

                using (Stream fileStream = File.Create(destinationFile))
                {
                    client.DownloadFile(sourceFilePath, fileStream);
                }
            }

        }
    }
}
