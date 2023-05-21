using Lib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineApi.Data
{
    public class Consts : CommonConsts
    {
        public static readonly string ENV_NAME_PG_HOST = "POSTGRES_HOST";
        public static readonly string ENV_NAME_PG_PORT = "POSTGRES_PORT";
        public static readonly string ENV_NAME_PG_USER = "POSTGRES_USER";
        public static readonly string ENV_NAME_PG_PASS = "POSTGRES_PASS";
        public static readonly string ENV_NAME_PG_CLEAN = "POSTGRES_CLEAN";
        public static readonly string ENV_NAME_ALLOWED_HOSTS = "ALLOWED_HOSTS";
        public static readonly string ENV_NAME_ENABLE_SWAGGER = "ENABLE_SWAGGER";

        public static bool IsDbInitialized = false;
        public static readonly int DB_DEFAULT_USER_KEY = 1;

        /// <summary>
        /// A format of value: yyyyMMddHHmmss
        /// </summary>
        public static readonly string CONFIG_LAST_WORKING_DTTM = "LAST_WORKING_DTTM";
        /// <summary>
        /// If this value is "T", reload the projects. (T|F)
        /// </summary>
        public static readonly string CONFIG_LOAD_PROJECTS = "LOAD_PROJECTS";

        /// <summary>
        /// A endpoint of notion api for pages.
        /// Create: endpoint
        /// Update: endpoint+"/page_id".
        /// </summary>
        public static readonly string NOTION_API_ENDPOINT_PAGE = "https://api.notion.com/v1/pages";
    }
}
