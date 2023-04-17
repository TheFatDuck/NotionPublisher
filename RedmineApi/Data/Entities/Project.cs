using Lib.Common.Data.DAO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineApi.Data.Entities
{
    [Table("project")]
    public class Project
    {
        [Key]
        [Column("project_id")]
        public int project_id { get; set; }
        [Column("project_name")]
        public string project_name { get; set; }

        public Project() { }
        public Project(ProjectDao projectDao)
        {
            project_id = Convert.ToInt32(projectDao.project_id);
            project_name = projectDao.project_name;
        }
    }
}
