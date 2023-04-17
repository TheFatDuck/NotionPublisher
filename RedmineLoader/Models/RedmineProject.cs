using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineWorker.Models
{

    public class RedmineProject
    {
        public int id { get; set; }
        public string name { get; set; }
        public string identifier { get; set; }
        public string description { get; set; }
        public int status { get; set; }
        public bool is_public { get; set; }
        public bool inherit_members { get; set; }
        public CustomField[] custom_fields { get; set; }
        public DateTime created_on { get; set; }
        public DateTime updated_on { get; set; }
        public RedmineProject? parent { get; set; }
        //public Project ConvertToProject()
        //{
        //    if (id == 0 || string.IsNullOrWhiteSpace(name)) 
        //        return null;

        //    return new Project()
        //    {
        //        project_id= id,
        //        project_name= name,
        //        //parent_project = (parent == null) ? null : parent.ConvertToProject(),
        //        //parent_project_id = (parent == null) ? null : parent.id,
        //    };
        //}
    }
}
