using System.ComponentModel.DataAnnotations.Schema;

namespace RedmineApi.Data.Entities
{
    [Table("user_project")]
    public class UserProject
    {
        [ForeignKey(nameof(User.user_id))]
        [Column("user_id")]
        public int user_id { get; set; }
        public User user { get; set; }

        [ForeignKey(nameof(Project.project_id))]
        [Column("project_id")]
        public int project_id { get; set; }
        public Project project { get; set; }
    }
}
