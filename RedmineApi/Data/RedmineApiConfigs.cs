using Lib.Common.Data;

namespace RedmineApi.Data
{
    public class RedmineApiConfigs : NPConfigs
    {
        public string PgHost { get; set; }
        public string PgPort { get; set; }
        public string PgUser { get; set; }
        public string PgPass { get; set; }
        public bool PgClean { get; set; }
        public string AllowedHosts { get; set; }

        public RedmineApiConfigs(string esUrl, string esUser, string esPass,
            string pgHost, string pgPort, string pgUser, string pgPass, string pgClean,
            string allowedHost) : base(esUrl, esUser, esPass)
        {
            this.PgHost = pgHost;
            this.PgPort = pgPort;
            this.PgUser = pgUser;
            this.PgPass = pgPass;
            if(pgClean.Equals("true", StringComparison.OrdinalIgnoreCase)
                || pgClean.Equals("t", StringComparison.OrdinalIgnoreCase))
                this.PgClean = true;
            else 
                this.PgClean = false;
            this.AllowedHosts = allowedHost;
        }
    }
}
