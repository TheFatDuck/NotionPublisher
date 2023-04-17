using Lib.Common.Data.DAO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedmineApi.Data.Entities
{
    [Table("page")]
    public class Page
    {
        /// <summary>
        /// Notion page id.
        /// </summary>
        [Key]
        [Column("page_id")]
        public string page_id { get; set; }

        /// <summary>
        /// The id of the user who owns the page.
        /// </summary>
        [ForeignKey(nameof(User.user_id))]
        [Column("user_id")]
        public int user_id { get; set; }

        /// <summary>
        /// The id of the issue the page describes.
        /// </summary>
        [ForeignKey(nameof(Issue.issue_id))]
        [Column("issue_id")]
        public int issue_id { get; set; } 

        public User user { get; set; }
        public Issue issue { get; set; }
        public Page()
        {

        }
        public Page(string page_id, User user, Issue issue)
        {
            this.page_id = page_id;
            this.user = user;
            this.user_id = user.user_id;
            this.issue = issue;
            this.issue_id = issue_id;
        }
        public Page(PageDao pageDao)
        {
            this.page_id = pageDao.page_id;
            this.user_id = pageDao.user_id;
            this.issue_id = pageDao.issue_id;
        }

        public PageDao ConvertToDao()
        {
            return new PageDao()
            {
                page_id = this.page_id,
                user_id = this.user_id,
                issue_id = this.issue_id
            };
        }
    }
}
