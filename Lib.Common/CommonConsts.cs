using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Common
{
    public class CommonConsts
    {
        #region ServiceName
        public const string NAME_SERVICE_REDMINE_API = "REDMINE_API";
        public const string NAME_SERVICE_REDMINE_LOADER = "REDMINE_LOADER";
        public const string NAME_SERVICE_REDMINE_PUBLISHER = "REDMINE_PUBLISHER";
        #endregion ServiceName

        #region EnvironmentVariablesName
        public const string ENV_NAME_ES_URL = "ES_URL";
        public const string ENV_NAME_ES_USER = "ES_USER";
        public const string ENV_NAME_ES_PASS = "ES_PASSWORD";
        #endregion EnvironmentVariablesName

        #region RequestHeaderName
        public const string NAME_REQ_HEADER_NP_API_KEY = "np-api-key";
        public const string NAME_REQ_HEADER_RM_API_KEY = "X-Redmine-API-Key";
        #endregion RequestHeaderName

        /// <summary>
        /// Maximum request time for HTTP requests.
        /// </summary>
        public const int HTTP_REQUEST_TIMEOUT = 30;
    }
}
