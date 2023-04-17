using RedmineApi.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineApi.Models
{
    public class NotionIssuePage
    {
        public string DatabaseId { get; set; }
        public Issue issue { get; set; }
        public string ConvertToNotionJson()
        {
            string jsonProperties = @$"
            {{
	            ""parent"": {{ ""database_id"": ""{this.DatabaseId}"" }},
                ""properties"": {{
                    ""Number"": {{ ""number"": {this.issue.issue_id} }},
                    ""Status"": {{
                        ""select"": {{ ""name"": ""{this.issue.status}"" }}
                    }},
                    ""Type"": {{
                        ""select"": {{ ""name"": ""{this.issue.issue_type}"" }}
                    }},
                    ""Link"": {{ ""url"": ""http://src.infinitt.com/issues/{this.issue.issue_id}"" }},
                    ""Author"": {{
                        ""rich_text"": [
                            {{
                                ""type"": ""text"",
                                ""text"": {{ ""content"": ""{this.issue.author}""}}
                            }}
                        ]
                    }},
                    ""LastUpdated"":{{
                        ""date"": {{
                            ""start"": ""{this.issue.updated_on.AddHours(9).ToString("yyyy-MM-ddTHH:mm:ssZ")/*GMT+0 => GMT+9*/}"",
                            ""time_zone"": ""Asia/Seoul"" 
                        }}
                    }},";
                if (!string.IsNullOrWhiteSpace(this.issue.category_name))
                {
                    jsonProperties += @$"
                    ""Product"":{{
                        ""select"": {{ ""name"": ""{this.issue.category_name}""}}
                    }},";
                }
            if (!string.IsNullOrWhiteSpace(this.issue.target_version))
            {
                jsonProperties += @$"
                    ""Version"": {{
                        ""select"": {{""name"": ""{this.issue.target_version}""}}
                    }},";
            }
            if (!string.IsNullOrWhiteSpace(this.issue.assigned_to))
                {
                    jsonProperties += @$"
                    ""AssignedTo"": {{
                        ""rich_text"": [
                            {{
                                ""type"": ""text"",
                                ""text"": {{""content"": ""{this.issue.assigned_to}""}}
                            }}
                        ]
                    }},";
                }
                jsonProperties += @$"
                    ""Title"": {{
                        ""title"": [
                            {{
                                ""type"": ""text"",
                                ""text"": {{ ""content"": ""#{this.issue.issue_id} {this.issue.title.Replace("\"", "\\\"").Replace("\\", "\\\\")}"" }}
                            }}
                        ]
                    }}
                }}
            }}";
            return jsonProperties;
        }
        public NotionIssuePage(string databseId, Issue issue)
        {
            DatabaseId= databseId;
            this.issue = issue;
        }
    }
}
