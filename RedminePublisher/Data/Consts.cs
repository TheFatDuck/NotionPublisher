using Lib.Common;

namespace RedminePublisher.Data
{
    internal class Consts : CommonConsts
    {
        public static readonly string ENV_NAME_NP_API_URL = "NP_API_URL";
        public static readonly string ENV_NAME_NP_API_KEY = "NP_API_KEY";
        public static readonly string ENV_NAME_NOTION_API_VERSION = "NOTION_API_VERSION";

        /// <summary>
        /// A endpoint of notion api for pages.
        /// Create: endpoint
        /// Update: endpoint+"/page_id".
        /// </summary>
        public static readonly string NOTION_API_ENDPOINT_PAGE = "https://api.notion.com/v1/pages";
        /// <summary>
        /// A endpoint of notion api for databases.
        /// Retrieve : endpoint+"/database_id"
        /// </summary>
        public static readonly string NOTION_API_ENDPOINT_DATABASE = "https://api.notion.com/v1/databases";

        public static readonly string NAME_REQ_HEADER_NOTION_API_KEY = "Authorization";
        public static readonly string NAME_REQ_HEADER_NOTION_API_VERSION = "Notion-Version";
    }
}
