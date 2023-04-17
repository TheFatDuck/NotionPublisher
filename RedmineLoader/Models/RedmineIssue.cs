using Lib.Common.Data.DAO;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RedmineWorker.Models
{
    public class RedmineIssue
    {
        public int id { get; set; }
        public RedmineProject project { get; set; }
        public Tracker tracker { get; set; }
        public Status status { get; set; }
        public Priority priority { get; set; }
        public Author author { get; set; }
        public Assigned_To assigned_to { get; set; }
        public Fixed_Version fixed_version { get; set; }
        public Category category { get; set; }
        public RedmineIssue parent { get; set; }
        public string subject { get; set; }
        public string description { get; set; }
        public string start_date { get; set; }
        public object due_date { get; set; }
        public int done_ratio { get; set; }
        public bool is_private { get; set; }
        public object estimated_hours { get; set; }
        public CustomField[] custom_fields { get; set; }
        public DateTime created_on { get; set; }
        public DateTime updated_on { get; set; }
        public object closed_on { get; set; }
        public IssueDao ConvertToDao()
        {
            IssueDao issueDao =  new IssueDao()
            {
                issue_id = id,
                project_id = project.id,
                issue_type = tracker.name,
                status = status.name,
                priority = priority.name,
                author = author.name,
                assigned_to = assigned_to.name,
                target_version = (fixed_version == null) ? null : fixed_version.name,
                category_name = (category == null) ? null : category.name,
                parent_issue_id = (parent == null) ? null : parent.id,
                title = subject,
                created_on = created_on,
                updated_on = updated_on,
            };
            return issueDao;
        }
    }

    /// <summary>
    /// Issue types: Defect, Improve ...
    /// </summary>
    public class Tracker
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Status
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Priority
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Author
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Assigned_To
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Category
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Fixed_Version
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}
