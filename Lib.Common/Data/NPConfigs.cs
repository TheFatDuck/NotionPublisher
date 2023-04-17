using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Common.Data
{
    public class NPConfigs
    {
        public string EsUrl { get; set; }
        public string EsUser { get; set; }
        public string EsPass { get; set; }

        public NPConfigs(string esUrl, string esUser, string esPass) 
        {
            this.EsUrl = esUrl;
            this.EsUser = esUser;
            this.EsPass = esPass;
        }
    }
}
