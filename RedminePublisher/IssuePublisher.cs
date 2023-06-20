using Lib.Common.Data.DAO;
using RedminePublisher.Data;
using Serilog;
using System.Text;
using System.Text.Json;
using System.Transactions;

namespace RedminePublisher
{
    public class IssuePublisher : BackgroundService
    {
        private readonly Serilog.ILogger _logger;
        private readonly int _userId;
        private readonly string _npApiUrl;
        private readonly string _npApiKey;
        private readonly string _notionApiKey;
        private readonly string _notionApiVersion;
        private readonly string _notionDbId;

        public IssuePublisher(RedminePublisherConfigs config)
        {
            _logger = Log.Logger.ForContext<IssuePublisher>();

            _npApiUrl = config.NpApiUrl;
            _npApiKey = config.NpApiKey;
            _userId = config.UserInfo.user_id;
            _notionApiKey = config.UserInfo.nt_api_key;
            _notionDbId = config.UserInfo.nt_db_id;
            _notionApiVersion = config.NotionApiVersion;

            _logger.Information($"Start publishing issues ...");
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                PublishIssues();
                await Task.Delay(1000 * 10, stoppingToken);
            }
        }
        private void PublishIssues()
        {
            List<IssuePageDto> issues = GetIssuesForPublish();
            // Notion api does not support endpoints for creating multiple pages at once.
            foreach (IssuePageDto issuePageDao in issues)
            {
                IssuePageDto updatedDao = (issuePageDao.pageDao == null) ? CreatePage(issuePageDao) : UpdatePage(issuePageDao);
                if(updatedDao != null)
                {
                    updatedDao.pageDao.user_id = _userId;
                    PutIssuePage(updatedDao);
                }
            }
        }
        private List<IssuePageDto> GetIssuesForPublish()
        {
            List<IssuePageDto> issuePageDaos = new List<IssuePageDto>();

            using (var reqMsg = new HttpRequestMessage(HttpMethod.Get, $"{_npApiUrl}api/issue/updated"))
            {
                reqMsg.Headers.Add(Consts.NAME_REQ_HEADER_NP_API_KEY, _npApiKey);
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(Consts.HTTP_REQUEST_TIMEOUT);
                    var response = client.SendAsync(reqMsg).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var content = response.Content.ReadAsStringAsync().Result;
                        issuePageDaos = JsonSerializer.Deserialize<List<IssuePageDto>>(content);
                    }
                }
            }

            return issuePageDaos;
        }
        private IssuePageDto CreatePage(IssuePageDto issuePageDao)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(Consts.HTTP_REQUEST_TIMEOUT);
                client.DefaultRequestHeaders.Add("Notion-Version", _notionApiVersion);
                client.DefaultRequestHeaders.Add("Authorization", _notionApiKey);
                var requestBodyJson = new StringContent(
                    new NotionIssuePage(_notionDbId, issuePageDao.issueDao).ConvertToNotionJson(),
                    Encoding.UTF8,
                    "application/json");
                try
                {
                    var response = client.PostAsync(Consts.NOTION_API_ENDPOINT_PAGE, requestBodyJson).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var content = response.Content.ReadAsStringAsync().Result;
                        NotionCreatePageResponse noionCreatePageResponse = JsonSerializer.Deserialize<NotionCreatePageResponse>(content);
                        PageDto pageDao = new PageDto() 
                        {
                            page_id = noionCreatePageResponse.id,
                            issue_id = issuePageDao.issueDao.issue_id
                        };
                        issuePageDao.pageDao = pageDao;
                    }
                    else
                    {
                        throw new Exception($"Failed to CreatePage. {response.ReasonPhrase}({response.StatusCode})");
                    }
                }
                catch (Exception ex)
                {
                    //TODO: Handle error.
                    _logger.Error($"Failed to CreatePage. {ex.Message}");
                }

            }
            return issuePageDao;
        }
        private IssuePageDto UpdatePage(IssuePageDto issuePageDao)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(Consts.HTTP_REQUEST_TIMEOUT);
                client.DefaultRequestHeaders.Add("Notion-Version", _notionApiVersion);
                client.DefaultRequestHeaders.Add("Authorization", _notionApiKey);
                var requestBodyJson = new StringContent(
                    new NotionIssuePage(_notionDbId, issuePageDao.issueDao).ConvertToNotionJson(),
                    Encoding.UTF8,
                    "application/json");
                try
                {
                    var response = client.PatchAsync($"{Consts.NOTION_API_ENDPOINT_PAGE}/{issuePageDao.pageDao.page_id}", requestBodyJson).Result;
                    if (response.IsSuccessStatusCode)
                    {

                    }
                    else
                    {
                        //TODO: Handle error.
                        issuePageDao = null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to UpdatePage. {((ex.InnerException == null) ? ex.Message : ex.InnerException.Message)}");
                    issuePageDao = null;
                }

            }
            return issuePageDao;
        }
        private bool PutIssuePage(IssuePageDto issuePageDao)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(Consts.HTTP_REQUEST_TIMEOUT);
                client.DefaultRequestHeaders.Add(Consts.NAME_REQ_HEADER_NP_API_KEY, _npApiKey);
                var requestBodyJson = new StringContent(
                    JsonSerializer.Serialize(issuePageDao),
                    Encoding.UTF8,
                    "application/json");
                var res = client.PutAsync($"{_npApiUrl}api/issue/updated", requestBodyJson).Result;
                if(res.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}