using Lib.Common.Data;
using Lib.Common.Data.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineLoader.Data
{
    public class RedmineLoaderConfigs : NPConfigs
    {
        private string _RmUrl;
        public string RmUrl 
        {
            get
            {
                return this._RmUrl;
            }
            set
            {
                if(!value.EndsWith("/"))
                    value += "/";
                this._RmUrl = value;
            }
        }
        private string _NpApiUrl;
        public string NpApiUrl 
        { 
            get
            {
                return this._NpApiUrl;
            }
            set
            {
                if(!value.EndsWith("/"))
                    value += "/";
                this._NpApiUrl = value;
            }
        }
        public string NpApiKey { get; set; }
        public UserDao UserInfo { get; set; }

        public RedmineLoaderConfigs(string esUrl, string esUser, string esPass,
            string rmUrl, string npApiUrl, string npApiKey) : base(esUrl, esUser, esPass)
        {
            this.RmUrl = rmUrl;
            this.NpApiUrl = npApiUrl;
            this.NpApiKey = npApiKey;
        }
    }
}
