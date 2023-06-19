using Lib.Common.Data.DAO;
using Microsoft.AspNetCore.Mvc;
using RedmineApi.Attributes;
using RedmineApi.Data.Entities;
using RedmineApi.Repositories;
using RedmineApi.Services;
using Serilog;

namespace RedmineApi.Controllers
{
    [ApiKeyAttribute]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly Serilog.ILogger _logger;
        private readonly DBManager _dbManager;
        public UserController(DBManager dbManager)
        {
            _logger = Log.Logger.ForContext<UserController>();
            _dbManager = dbManager;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserDto userDao)
        {
            if(string.IsNullOrEmpty(userDao.np_api_key))
                return BadRequest("No api key.");

            User user = _dbManager.SelectUser(userDao.np_api_key);
            if (user != null)
            {
                List<Project> projects = _dbManager.SelectUserProjects(user.user_id);
                if (projects == null || projects.Count == 0)
                    return BadRequest("No projects found.");
                UserDto foundUserDao = new UserDto() {
                    user_id = user.user_id,
                    np_api_key = user.np_api_key,
                    rm_api_key = user.rm_api_key,
                    nt_api_key = user.nt_api_key,
                    nt_db_id = user.nt_db_id,
                    project_keys = projects.Select(p => p.project_id).ToList()
                };
                return Ok(foundUserDao);
            }
            else
                return BadRequest("No user found.");
        }

        [HttpPost]
        public IActionResult RegisterUser([FromBody] UserDto userDao)
        {
            if (_dbManager.SelectUser(userDao.np_api_key) != null)
                return BadRequest("Already exists user.");

            User user = _dbManager.RegisterUser(userDao);
            if (user != null)
            {
                userDao.user_id = user.user_id;
                userDao.np_api_key = user.np_api_key;
                return Ok(userDao);
            }
            else
                return BadRequest();
        }
    }
}
