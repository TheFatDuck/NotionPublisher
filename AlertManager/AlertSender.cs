using Lib.Common;
using Lib.Common.Exceptions;
using System.Runtime.CompilerServices;
using System.Text;

namespace AlertManager
{
    public class AlertSender : IDisposable
    {
        private bool isDisposed = false;
        private readonly string _slackAppToken;
        private readonly string _slackChannelId;
        public AlertSender(string appToken, string channelid)
        {
            _slackAppToken = appToken;
            _slackChannelId = channelid;
        }
        public bool Send(string globalUrl, Activetarget promUnactiveTarget)
        {
            if(string.IsNullOrEmpty(_slackAppToken) 
                || string.IsNullOrEmpty(_slackChannelId))
            {
                throw new MissingRequiredFieldException();
            }

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(CommonConsts.HTTP_REQUEST_TIMEOUT);
                using (var reqMsg = new HttpRequestMessage(HttpMethod.Post, $"https://slack.com/api/chat.postMessage"))
                {
                    reqMsg.Headers.Add("Authorization", $"Bearer {_slackAppToken}");
                    SlackAlertMessage slackMessage = new SlackAlertMessage(
                        _slackChannelId,
                        $"Prometheus server health anomaly({promUnactiveTarget.health}) detected.",
                        promUnactiveTarget.lastError.Replace(@"""", @"\"""),
                        globalUrl,
                        promUnactiveTarget.scrapePool,
                        promUnactiveTarget.scrapeUrl,
                        promUnactiveTarget.lastScrape.ToString("yyyy.MM.dd HH:mm:ss")
                    );
                    var content = new StringContent(slackMessage.GetMessageJson(), Encoding.UTF8, "application/json");
                    reqMsg.Content = content;
                    var response = client.Send(reqMsg);
                }
            }

            return true;
        }
        ~AlertSender()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing) { }
                isDisposed = true;
            }
        }
    }
}
