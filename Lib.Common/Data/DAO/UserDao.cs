using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Common.Data.DAO
{
    public class UserDao
    {
        public int user_id { get; set; }
        public string np_api_key { get; set; }
        public string rm_api_key { get; set; }
        public string nt_api_key { get; set; }
        public string nt_db_id { get; set; }
        public List<int> project_keys { get; set; }
        public UserDao() { }
        public UserDao(string npApiKey)
        {
            user_id = 0;
            np_api_key = npApiKey;
            rm_api_key = "";
            nt_api_key = "";
            nt_db_id = "";
            project_keys = new List<int>();
        }
    }
}
