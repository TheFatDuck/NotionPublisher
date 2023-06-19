using Lib.Common.Data.DAO;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedmineApi.Data.Entities
{
    [Table("issue")]
    public class Issue
    {
        /// <summary>
        /// Id of issue. PK.
        /// </summary>
        [Key]
        [Column("issue_id")]
        public int issue_id { get; set; }

        /// <summary>
        /// ID of the project the issue belongs to.
        /// </summary>
        [Column("project_id")]
        public int project_id { get; set; }

        /// <summary>
        /// The type of issue, such as defect, feature, or improve.
        /// Appears as tracker_name in Redmine API.
        /// </summary>
        [Column("issue_type")]
        public string issue_type { get; set; }

        /// <summary>
        /// The status of the issue.
        /// </summary>
        [Column("status")]
        public string status { get; set; }

        /// <summary>
        /// Issue priority.
        /// </summary>
        [Column("priority")]
        public string priority { get; set; }

        /// <summary>
        /// The person who created the issue.
        /// </summary>
        [Column("author")]
        public string author { get; set; }

        /// <summary>
        /// Who has the issue now.
        /// </summary>
        [Column("assigned_to")]
        public string? assigned_to { get; set; }

        /// <summary>
        /// The version the issue should apply to.
        /// Appears as fixed_version_name in Redmine API.
        /// </summary>
        [Column("target_version")]
        public string? target_version { get; set; }

        /// <summary>
        /// Category of issue.
        /// </summary>
        [Column("category_name")]
        public string? category_name { get; set; }

        /// <summary>
        /// The parent issue that contains the issue.
        /// </summary>
        [Column("parent_issue_id")]
        public int? parent_issue_id { get; set; }

        /// <summary>
        /// Issue title.
        /// </summary>
        [Column("title")]
        public string title { get; set; }

        /// <summary>
        /// The time the issue was created.
        /// </summary>
        [Column("created_on")]
        private DateTime _created_on;
        public DateTime created_on 
        {
            get
            {
                return this._created_on;
            }
            set 
            {
                this._created_on = DateTime.SpecifyKind(value, DateTimeKind.Utc); 
            }
        }

        /// <summary>
        /// Time the issue was last updated.
        /// </summary>
        [Column("updated_on")]
        private DateTime _updated_on;
        public DateTime updated_on
        {
            get
            {
                return this._updated_on;
            }
            set
            {
                this._updated_on = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
        }

        /// <summary>
        /// The time the issue was posted or updated to the NotionPage.
        /// </summary>
        [Column("last_posted_on")]
        private DateTime _last_posted_on;
        public DateTime last_posted_on
        {
            get
            {
                return this._last_posted_on;
            }
            set
            {
                this._last_posted_on = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
        }

        public ICollection<Page> pages { get; set; }

        public Issue() { }
        public Issue(IssueDto issueDao)
        {
            this.issue_id = issueDao.issue_id;
            this.project_id = issueDao.project_id;
            this.issue_type = issueDao.issue_type;
            this.status = issueDao.status;
            this.priority = issueDao.priority;
            this.author = issueDao.author;
            this.assigned_to = issueDao.assigned_to;
            this.target_version = issueDao.target_version;
            this.category_name = issueDao.category_name;
            this.parent_issue_id = issueDao.parent_issue_id;
            this.title = issueDao.title;
            this.created_on = issueDao.created_on;
            this.updated_on = issueDao.updated_on;
            this.last_posted_on = DateTime.Now.AddDays(-1);
        }

        public bool IsUpdated(Issue newIssue)
        {
            if(this.issue_id != newIssue.issue_id)
                return false;
            if( this.last_posted_on < newIssue.updated_on
                && (this.project_id != newIssue.project_id
                || this.issue_type != newIssue.issue_type
                || this.status != newIssue.status
                || this.priority != newIssue.priority
                || this.assigned_to != newIssue.assigned_to
                || this.target_version != newIssue.target_version
                || this.category_name != newIssue.category_name
                || this.parent_issue_id != newIssue.parent_issue_id
                || this.title != newIssue.title))
            {
                return true;
            }
            return false;
        }

        public IssueDto ConvertToDto()
        {
            return new IssueDto
            {
                issue_id = this.issue_id,
                project_id = this.project_id,
                issue_type = this.issue_type,
                status = this.status,
                priority = this.priority,
                author = this.author,
                assigned_to = this.assigned_to,
                target_version = this.target_version,
                category_name = this.category_name,
                parent_issue_id = this.parent_issue_id,
                title = this.title,
                created_on = this.created_on,
                updated_on = this.updated_on
            };
        }
    }
}
