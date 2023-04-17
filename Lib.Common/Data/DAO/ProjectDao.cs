using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Common.Data.DAO
{
    public class ProjectDao
    {
        public string project_id { get; set; }
        public string project_name { get; set; }

        public ProjectDao() { }
        public ProjectDao(string projectId, string projectName)
        {
            project_id = projectId;
            project_name = projectName;
        }
    }
}
