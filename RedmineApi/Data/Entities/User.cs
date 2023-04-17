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
    [Table("np_user")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public int user_id { get; set; }

        /// <summary>
        /// NotionPublisher api key.
        /// </summary>
        [Column("np_api_key")]
        public string np_api_key { get; set; }

        /// <summary>
        /// Redmine api key.
        /// </summary>
        [Column("rm_api_key")]
        public string rm_api_key { get; set; }

        /// <summary>
        /// Notion api key.
        /// </summary>
        [Column("nt_api_key")]
        public string nt_api_key { get; set; }

        /// <summary>
        /// Notion database id.
        /// </summary>
        [Column("nt_db_id")]
        public string nt_db_id { get; set; }

        public ICollection<Config> configs { get; set; }
        public ICollection<Page> pages { get; set; }

        public User() { }
        public User(UserDao userDao)
        {
            this.np_api_key = userDao.np_api_key;
            this.rm_api_key = userDao.rm_api_key;
            this.nt_api_key = userDao.nt_api_key;
            this.nt_db_id = userDao.nt_db_id;
        }
    }
}
