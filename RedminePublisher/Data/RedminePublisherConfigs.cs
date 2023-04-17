using Lib.Common.Data;
using Lib.Common.Data.DAO;

namespace RedminePublisher.Data
{
    public class RedminePublisherConfigs: NPConfigs
    {
        private string _npApiUrl;
        public string NpApiUrl 
        {
            get { return this._npApiUrl; }
            set
            {
                if (!value.EndsWith("/"))
                    value += "/";
                this._npApiUrl = value;
            }
        }
        public string NpApiKey { get; set; }
        public string NotionApiVersion { get; set; }
        public UserDao UserInfo { get; set; }

        public RedminePublisherConfigs(string esUrl, string esUser, string esPass,
            string npApiUrl, string npApiKey,string notionApiVersion) : base(esUrl, esUser, esPass)
        {
            this.NpApiUrl = npApiUrl;
            this.NpApiKey = npApiKey;
            this.NotionApiVersion = notionApiVersion;
        }
    }
}
