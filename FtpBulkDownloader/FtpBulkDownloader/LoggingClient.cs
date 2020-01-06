using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FtpBulkDownloader
{
    internal class LoggingClient
    {
        private readonly string url;

        public LoggingClient(string url)
        {
            this.url = url;
        }

        public async Task Log(string log)
        {
            // TODO: implement your logging here
            
            return; 
            
            var logContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("YourLoggingField", log),
            });

            HttpResponseMessage response = await new HttpClient().PostAsync(new Uri(url), logContent);
        }
    }
}