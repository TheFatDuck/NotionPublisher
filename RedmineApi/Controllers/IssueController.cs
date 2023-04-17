using Lib.Common;
using Lib.Common.Data.DAO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using RedmineApi.Attributes;
using RedmineApi.Data;
using RedmineApi.Data.Entities;
using RedmineApi.Services;
using Serilog;
using System.Text.Json;

namespace RedmineApi.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class IssueController : Controller
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10);
        private readonly Serilog.ILogger _logger;
        private readonly DBManager _dbManager;
        public IssueController(DBManager dbManager)
        {
            _logger = Log.Logger.ForContext<IssueController>();
            _dbManager = dbManager;
        }
        // GET api/issue/<issue_id>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            await _semaphore.WaitAsync();
            try
            {
                Issue issue = _dbManager.SelectIssue(id);
                if (issue != null)
                {
                    string result = JsonSerializer.Serialize(issue);
                    return Ok(result);
                }
                return NotFound(id);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // PUT api/issue
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] IssueDao issueDao)
        {
            await _semaphore.WaitAsync();
            try
            {
                Issue issue = new Issue(issueDao);
                if (!_dbManager.UpsertIssue(issue))
                    return BadRequest("Failed to PUT issue");
                return Ok(issue.issue_id);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // GET api/issue/updated
        [HttpGet("updated")]
        public async Task<IActionResult> GetUpdatedIssues()
        {
            await _semaphore.WaitAsync();
            try
            {
                List<IssuePage> issues = _dbManager.SelectIssuesForPage();
                List<IssuePageDao> issueDaos = issues.Select(ip => ip.ConvertToDao()).ToList();
                return Ok(issueDaos);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        // PUT api/issue/updated
        [HttpPut("updated")]
        public async Task<IActionResult> PutUpdatedIssuePage([FromBody] IssuePageDao issuePageDao)
        {

            await _semaphore.WaitAsync();
            try
            {
                Issue issue = new Issue(issuePageDao.issueDao);
                issue.last_posted_on = DateTime.Now.AddMinutes(1);
                Page page = new Page(issuePageDao.pageDao);
                if (!_dbManager.UpsertIssuePage(issue, page))
                    return BadRequest("Failed to PutUpdatedIssuePage");
                return Ok(issue.issue_id);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
