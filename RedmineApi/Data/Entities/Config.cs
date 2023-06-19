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
    [Table("config")]
    public class Config
    {
        [Key]
        [Column("config_id")]
        public int config_id { get; set; }
        [Column("entry")]
        public string entry { get; set; }
        [Column("value")]
        public string value  { get; set; }

        [ForeignKey(nameof(User.user_id))]
        [Column("user_id")]
        public int user_id { get; set; }
        public User user { get; set; }
        public Config(int user_id, string entry, string value)
        {
            this.user_id = user_id;
            this.entry = entry;
            this.value = value;
        }
        public Config(ConfigDto configDao)
        {
            user_id = Convert.ToInt32(configDao.user_id);
            entry = configDao.entry;
            value = configDao.value;
        }
    }
}
