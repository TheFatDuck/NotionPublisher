using Lib.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertManager
{
    public class AlertManagerConfigs : NPConfigs
    {
        #region Settings for Prometheus
        private string _PromUrl;
        public string PromUrl
        {
            get
            {
                return this._PromUrl;
            }
            set
            {
                if (!value.EndsWith("/"))
                    value += "/";
                this._PromUrl = value;
            }
        }
        public string PromJob;
        #endregion Settings for Prometheus

        #region Settings for Slack
        public string SlackAppToken;
        public string SlackChannelId;
        #endregion Settings for Slack

        public AlertManagerConfigs(string esUrl, string esUser, string esPass
            , string promUrl, string promJob
            , string slackAppToken, string slackChannelId) : base(esUrl, esUser, esPass)
        {
            PromUrl = promUrl; 
            PromJob = promJob; 
            SlackAppToken = slackAppToken; 
            SlackChannelId = slackChannelId;
        }
    }
}
