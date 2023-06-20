using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace AlertManager
{
    public class SlackAlertMessage
    {
        private string ChannelId { get; set; }
        private string Title { get; set; }
        private string Message { get; set; }
        private readonly string AlertImageUrl = "https://i.ibb.co/f0LKkTx/Danger-128.png";
        private string PromServer { get; set; }
        private string JobName { get; set; }
        private string ScrapeUrl { get; set; }
        private string LastWorkingDttm { get; set; }

        public SlackAlertMessage(string channelId, string title, string message,
            string promServer, string jobName, string scrapeUrl, string lastWorkingDttm)
        {
            ChannelId = channelId;
            Title = title;
            Message = message;
            PromServer = promServer;
            JobName = jobName;
            ScrapeUrl = scrapeUrl;
            LastWorkingDttm = lastWorkingDttm;
        }

        public string GetMessageJson()
        {
            string message = "";
            message += $"{{ " +
                $"\"channel\":\"{ChannelId}\", " +
                $"\"blocks\" : [" +
                $"    {{ \"type\":\"header\", " +
                $"       \"text\":{{\"type\":\"plain_text\", \"text\":\"{Title}\", \"emoji\":true}}" +
                $"    }}," +
                $"    {{ \"type\": \"section\", " +
                $"       \"text\": {{ \"type\": \"mrkdwn\", \"text\": \"{Message}\"}}, " +
                $"       \"accessory\": {{ \"type\": \"image\", \"image_url\": \"{AlertImageUrl}\", \"alt_text\":\"Danger\"}}" +
                $"    }}, " +
                $"    {{ \"type\": \"section\", " +
                $"       \"fields\": [ " +
                $"          {{ \"type\": \"mrkdwn\", \"text\": \"*Server:*\\n{PromServer}\" }}, " +
                $"          {{ \"type\": \"mrkdwn\", \"text\": \"*Job Name*\\n{JobName}\" }} " +
                $"        ] " +
                $"    }}, " +
                $"    {{ \"type\": \"section\", " +
                $"       \"fields\": [ " +
                $"          {{ \"type\": \"mrkdwn\", \"text\": \"*Scrape Url:*\\n{ScrapeUrl}\" }}, " +
                $"          {{ \"type\": \"mrkdwn\", \"text\": \"*Lase Working*\\n{LastWorkingDttm}\" }} " +
                $"        ] " +
                $"    }}" +
                $" ] " + // End of block
                $"}}";
            return message;
        }
    }
}
