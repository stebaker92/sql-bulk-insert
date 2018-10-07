using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;

namespace FtpBulkDownloader
{
    class Program
    {
        public static IConfiguration Configuration { get; set; }

        private static AppSettings appSettings;

        private static string environment;

        /// <summary>
        /// The amount of rows to process at once from the downloaded csv file
        /// </summary>
        private static int pageSize = 200_000;

        static void Main(string[] args)
        {
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", optional: true);

            Configuration = builder.Build();

            appSettings = Configuration.GetSection("AppSettings").Get<AppSettings>();

            var timer = new Stopwatch();

            timer.Start();

            Log("Running...");

            // Allow SFTP with certificate issues
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, certificate, chain, sslPolicyErrors) => true;

            try
            {
                var lines = DownloadFile();

                var hasLines = lines.Any();
                var pageCount = 0;

                var dataClient = new DatabaseClient(appSettings.ConnectionString, new LoggingClient(appSettings.LoggingApi));

                while (hasLines)
                {
                    var currentLines = lines.Skip(pageCount * pageSize).Take(pageSize).ToList();

                    currentLines = currentLines.Select(x => x.TrimEnd()).ToList();

                    dataClient.InsertData(currentLines, pageCount, appSettings.DbColumnName);

                    pageCount++;

                    if (currentLines.Count != pageSize)
                    {
                        hasLines = false;
                    }
                }

                dataClient.Consolidate();

                dataClient.Close();
            }
            catch (WebException ex)
            {
                Log(ex.Message);
                new SlackClient(environment, appSettings.SlackUrl, appSettings.SlackChannel).Alert("FTP import failed: " + ex.Message);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                new SlackClient(environment, appSettings.SlackUrl, appSettings.SlackChannel).Alert("FTP import failed: " + ex.Message + " Inner Exception:" + ex.InnerException?.Message);
            }

            timer.Stop();

            Console.WriteLine(timer.ElapsedMilliseconds + "millisecs");
            Console.WriteLine(TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds).TotalMinutes + "minutes");

            Console.ReadLine();
        }

        static void Log(string message)
        {
            Console.WriteLine(DateTime.Now + " " + message);
            new LoggingClient(appSettings.LoggingApi).Log(message).Wait();
        }

        static void Error(Exception ex)
        {
            Console.WriteLine(DateTime.Now + " " + ex.Message);
        }

        static string[] DownloadFile()
        {
            Log("Attempting to connect to FTP");

            var ftpClient = new FtpClient(appSettings.FtpServer,
                                        appSettings.FtpUserName,
                                        appSettings.FtpPassword,
                                        new LoggingClient(appSettings.LoggingApi));



            ftpClient.DownloadFile(appSettings.SourceFilePath, appSettings.DestinationFile);

            string[] lines = null;

            if (Path.GetExtension(appSettings.SourceFilePath) == "zip")
            {
                Log("Unzipping file");

                using (ZipArchive za = ZipFile.OpenRead(appSettings.DestinationFile))
                {
                    var entity = za.Entries.First();
                    entity.ExtractToFile(entity.Name, overwrite: true);

                    lines = File.ReadAllLines(entity.Name);
                    Log("Extracted file. " + lines.Count() + " lines found");
                }
            }
            else
            {
                lines = File.ReadAllLines(appSettings.DestinationFile);
                Log(lines.Count() + " lines found");
            }

            return lines;
        }

    }
}
