using Lib.Common.Data.DAO;

namespace RedminePublisher.Data
{
    public class NotionIssuePage
    {
        public string DatabaseId { get; set; }
        public IssueDao issueDao { get; set; }
        public string ConvertToNotionJson()
        {
            string jsonProperties = @$"
            {{
	            ""parent"": {{ ""database_id"": ""{this.DatabaseId}"" }},
                ""properties"": {{
                    ""Number"": {{ ""number"": {this.issueDao.issue_id} }},
                    ""Status"": {{
                        ""select"": {{ ""name"": ""{this.issueDao.status}"" }}
                    }},
                    ""Type"": {{
                        ""select"": {{ ""name"": ""{this.issueDao.issue_type}"" }}
                    }},
                    ""Link"": {{ ""url"": ""http://src.infinitt.com/issues/{this.issueDao.issue_id}"" }},
                    ""Author"": {{
                        ""rich_text"": [
                            {{
                                ""type"": ""text"",
                                ""text"": {{ ""content"": ""{this.issueDao.author}""}}
                            }}
                        ]
                    }},
                    ""LastUpdated"":{{
                        ""date"": {{
                            ""start"": ""{this.issueDao.updated_on.AddHours(9).ToString("yyyy-MM-ddTHH:mm:ssZ")/*GMT+0 => GMT+9*/}"",
                            ""time_zone"": ""Asia/Seoul"" 
                        }}
                    }},";
            if (!string.IsNullOrWhiteSpace(this.issueDao.category_name))
            {
                jsonProperties += @$"
                    ""Product"":{{
                        ""select"": {{ ""name"": ""{this.issueDao.category_name}""}}
                    }},";
            }
            if (!string.IsNullOrWhiteSpace(this.issueDao.target_version))
            {
                jsonProperties += @$"
                    ""Version"": {{
                        ""select"": {{""name"": ""{this.issueDao.target_version}""}}
                    }},";
            }
            if (!string.IsNullOrWhiteSpace(this.issueDao.assigned_to))
            {
                jsonProperties += @$"
                    ""AssignedTo"": {{
                        ""rich_text"": [
                            {{
                                ""type"": ""text"",
                                ""text"": {{""content"": ""{this.issueDao.assigned_to}""}}
                            }}
                        ]
                    }},";
            }
            jsonProperties += @$"
                    ""Title"": {{
                        ""title"": [
                            {{
                                ""type"": ""text"",
                                ""text"": {{ ""content"": ""#{this.issueDao.issue_id} {this.issueDao.title.Replace("\"", "\\\"").Replace("\\", "\\\\")}"" }}
                            }}
                        ]
                    }}
                }}
            }}";
            return jsonProperties;
        }
        public NotionIssuePage(string databseId, IssueDao issueDao)
        {
            DatabaseId = databseId;
            this.issueDao = issueDao;
        }
    }
}
