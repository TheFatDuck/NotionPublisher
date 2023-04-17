using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedmineApi.Services;
using Serilog;
using RedmineApi.Attributes;
using RedmineApi.Data.Entities;
using Lib.Common.Data.DAO;

namespace RedmineApi.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : Controller
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10);
        private readonly Serilog.ILogger _logger;
        private readonly DBManager _dbManager;
        public ConfigController(DBManager dbManager)
        {
            _logger = Log.Logger.ForContext<ConfigController>();
            _dbManager = dbManager;
        }

        // GET: api/config?id=#&entry=#
        [HttpGet]
        public async Task<IActionResult> GetConfig(int id, string entry)
        {
            await _semaphore.WaitAsync();

            try
            {
                string configValue = _dbManager.SelectConfig(id, entry);
                return Ok(configValue);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        [HttpPut]
        public async Task<IActionResult> PutConfig([FromBody] ConfigDao configDao)
        {
            Config config = new Config(configDao);
            if (!_dbManager.UpsertConfig(config))
                return BadRequest("Failed to PUT config");
            return Ok(config.config_id);
        }
    }
}
