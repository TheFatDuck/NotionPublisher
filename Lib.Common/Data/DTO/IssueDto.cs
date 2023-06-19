using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Common.Data.DAO
{
    public class IssueDto
    {
        public int issue_id { get; set; }
        public int project_id { get; set; }
        public string issue_type { get; set; }
        public string status { get; set; }
        public string priority { get; set; }
        public string author { get; set; }
        public string assigned_to { get; set; }
        public string? target_version { get; set; }
        public string? category_name { get; set; }
        public int? parent_issue_id { get; set; }
        public string title { get; set; }
        public DateTime created_on { get; set; }
        public DateTime updated_on { get; set; }
        public IssueDto() { }
    }
}
