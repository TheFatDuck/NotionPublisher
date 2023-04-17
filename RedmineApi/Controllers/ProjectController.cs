using Lib.Common.Data.DAO;
using Microsoft.AspNetCore.Mvc;
using RedmineApi.Data.Entities;
using RedmineApi.Services;
using Serilog;

namespace RedmineApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10);
        private readonly Serilog.ILogger _logger;
        private readonly DBManager _dbManager;
        public ProjectController(DBManager dbManager)
        {
            _logger = Log.Logger.ForContext<ProjectController>();
            _dbManager = dbManager;
        }
        // GET: api/project
        [HttpGet]
        public IEnumerable<int> Get()
        {
            List<Project> projects = _dbManager.SelectProjects();
            int[] projectKeys = projects.Select(p => p.project_id).ToArray();
            return projectKeys;
        }

        // PUT api/project
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ProjectDao projectDao)
        {
            await _semaphore.WaitAsync();
            try
            {
                Project project = new Project(projectDao);
                if (!_dbManager.UpsertProject(project))
                    return BadRequest("Failed to PUT project");
                return Ok(project.project_id);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
