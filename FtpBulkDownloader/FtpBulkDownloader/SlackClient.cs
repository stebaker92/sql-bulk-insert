using Slack.Webhooks;

namespace FtpBulkDownloader
{
    class SlackClient
    {
        private readonly string environment;
        private readonly string url;
        private string channel;

        public SlackClient(string environment, string url, string channel)
        {
            this.environment = environment;
            this.url = url;
            this.channel = channel;
        }

        internal void Alert(string message)
        {
            var slackMessage = $"[{environment}] {message}";

            var successful = new  Slack.Webhooks.SlackClient(url).PostAsync(new SlackMessage
            {
                Channel = channel,
                IconEmoji = Emoji.IceCream,
                Text = slackMessage,
                Username = "FtpBulkDownloader"
            }).Result;
        }
    }
}
