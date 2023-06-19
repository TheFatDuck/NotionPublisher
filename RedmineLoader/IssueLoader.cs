using Lib.Common;
using Lib.Common.Data.DAO;
using Lib.Common.Exceptions;
using RedmineLoader.Data;
using RedmineWorker.Models;
using Serilog;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace RedmineLoader
{
    public class IssueLoader : BackgroundService
    {
        private const int LIMIT = 100;
        private readonly Serilog.ILogger _logger;
        private readonly string _redmineUrl;
        private readonly string _redmineApiKey;
        private readonly string _npApiUrl;
        private readonly string _npApiKey;
        private readonly List<int> _projectKeys;
        private DateTime _lastWorkingDateTime = DateTime.Now.AddDays(-1);
        private readonly int loadingInterval = 60;

        public IssueLoader(RedmineLoaderConfigs configs)
        {
            _logger = Log.Logger.ForContext<IssueLoader>();

            _redmineUrl = configs.RmUrl;
            _npApiUrl = configs.NpApiUrl;

            _redmineApiKey = configs.UserInfo.rm_api_key;
            _npApiKey = configs.UserInfo.np_api_key;
            _projectKeys = configs.UserInfo.project_keys;
            _logger.Information($"Start loading issues ...");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //PutProjects(LoadProjects());
            List<RedmineProject> projects = new List<RedmineProject>();
            foreach (int projectKey in _projectKeys)
            {
                RedmineProject project = LoadProject(projectKey);
                if (project != null)
                    projects.Add(project);
                
            }
            PutProjects(projects);
            while (!stoppingToken.IsCancellationRequested)
            {
                PutIssues(LoadIssues());
                await Task.Delay(1000 * loadingInterval, stoppingToken);
            }
        }

        #region Project
        private RedmineProject LoadProject(int projectId)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            string reqUri;
            reqUri = $"{_redmineUrl}projects/{projectId}.json";
            _logger.Debug("{operation}: {request_uri}", methodName, reqUri);
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, reqUri))
            {
                requestMessage.Headers.Add("X-Redmine-API-Key", _redmineApiKey);
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(CommonConsts.HTTP_REQUEST_TIMEOUT);
                    var response = client.Send(requestMessage, CancellationToken.None);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = response.Content.ReadAsStringAsync().Result;
                        RedmineProjectResponse resProjectst = JsonSerializer.Deserialize<RedmineProjectResponse>(content);
                        return resProjectst.project;
                    }
                    else
                    {
                        // TODO: Handle error
                        return null;
                    }
                }
            }
        }
        //private List<RedmineProject> LoadProjects()
        //{
        //    string methodName = MethodBase.GetCurrentMethod().Name;
        //    int offset = 0;
        //    List<RedmineProject> projects = new List<RedmineProject>();
        //    while (true)
        //    {
        //        string reqUri;
        //        reqUri = $"{_redmineUrl}projects.json?limit={LIMIT}&offset={offset}";
        //        _logger.Debug("{operation}: {request_uri}", methodName, reqUri);
        //        using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, reqUri))
        //        {
        //            requestMessage.Headers.Add("X-Redmine-API-Key", _redmineApiKey);
        //            using (var client = new HttpClient())
        //            {
        //                client.Timeout = TimeSpan.FromSeconds(CommonConsts.HTTP_REQUEST_TIMEOUT);
        //                var response = client.Send(requestMessage, CancellationToken.None);
        //                if (response.IsSuccessStatusCode)
        //                {
        //                    var content = response.Content.ReadAsStringAsync().Result;
        //                    RedmineProjectsResponse resProjects = JsonSerializer.Deserialize<RedmineProjectsResponse>(content);
        //                    projects.AddRange(resProjects.projects);
        //                    offset += LIMIT;
        //                    if (resProjects.total_count <= offset) break;
        //                }
        //                else
        //                {
        //                    // TODO: Handle error
        //                    projects.Clear();
        //                    return projects;
        //                }
        //            }
        //        }
        //    }
        //    return projects;
        //}
        private void PutProjects(List<RedmineProject> projects)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;

            string reqUri = $"{_npApiUrl}api/project";
            foreach (var project in projects)
            {
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Put, reqUri))
                {
                    requestMessage.Headers.Add(CommonConsts.NAME_REQ_HEADER_NP_API_KEY, _npApiKey);
                    string serializedProjectDao = JsonSerializer.Serialize(new ProjectDao(project.id.ToString(), project.name));
                    requestMessage.Content = new StringContent(serializedProjectDao, Encoding.UTF8, "application/json");
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(CommonConsts.HTTP_REQUEST_TIMEOUT);
                        var response = client.Send(requestMessage, CancellationToken.None);
                        if (response.IsSuccessStatusCode)
                        {
                            _logger.Debug("{operation})({project_id}) success.", methodName, project.id);
                        }
                        else
                        {
                            _logger.Error("{operation})({project_id}) failed: {error_code}", methodName, project.id, response.StatusCode);
                        }
                    }
                }
            }
        }
        #endregion Project

        #region Issue
        private List<RedmineIssue> LoadIssues(int offset = 0)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;

            string reqUri;
            List<RedmineIssue> issues = new List<RedmineIssue>();
            foreach (int projectKey in _projectKeys)
            {
                while (true)
                {
                    reqUri = $"{_redmineUrl}issues.json?limit={LIMIT}&offset={offset}&project_id={projectKey}&updated_on=%3E%3D{_lastWorkingDateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss")}Z";
                    _logger.Debug("{operation}: {request_uri}", methodName, reqUri);
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, reqUri))
                    {
                        requestMessage.Headers.Add(CommonConsts.NAME_REQ_HEADER_RM_API_KEY, _redmineApiKey);
                        try
                        {
                            using (var client = new HttpClient())
                            {
                                client.Timeout = TimeSpan.FromSeconds(CommonConsts.HTTP_REQUEST_TIMEOUT);
                                var response = client.SendAsync(requestMessage, CancellationToken.None).Result;
                                if (response.IsSuccessStatusCode)
                                {
                                    var content = response.Content.ReadAsStringAsync().Result;
                                    RedmineIssuesResponse resIssues = JsonSerializer.Deserialize<RedmineIssuesResponse>(content);
                                    issues.AddRange(resIssues.issues);
                                    offset += LIMIT;
                                    if (resIssues.total_count <= offset) break;
                                }
                                else
                                {
                                    _logger.Error("{error}: {req_url} {message}", response.StatusCode.ToString(), reqUri, $"SendAsync failed.");
                                    issues.Clear();
                                    return issues;
                                }
                            }
                        }
                        catch (TaskCanceledException e)
                        {
                            _logger.Error("{error}: {message}", e.GetType().Name, e.Message);
                            issues.Clear();
                            return issues;
                        }
                        catch(Exception e)
                        {
                            _logger.Error("{error}: {message}", e.GetType().Name, e.Message);
                            issues.Clear();
                            return issues;
                        }
                    }
                }
            }
            _lastWorkingDateTime = DateTime.Now;
            return issues;
        }
        private void PutIssues(List<RedmineIssue> issues)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;

            string reqUrl = $"{_npApiUrl}api/issue";
            foreach (var issue in issues)
            {
                using (var reqMsg = new HttpRequestMessage(HttpMethod.Put, reqUrl))
                {
                    reqMsg.Headers.Add(CommonConsts.NAME_REQ_HEADER_NP_API_KEY, _npApiKey);
                    string serializedIssueDao = JsonSerializer.Serialize(issue.ConvertToDto());
                    reqMsg.Content = new StringContent(serializedIssueDao, Encoding.UTF8, "application/json");
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(CommonConsts.HTTP_REQUEST_TIMEOUT);
                        var response = client.Send(reqMsg, CancellationToken.None);
                        if (response.IsSuccessStatusCode)
                        {
                            _logger.Debug("{operation})({issue_id}) success.", methodName, issue.id);
                        }
                        else
                        {
                            _logger.Error("{operation})({issue_id}) failed: {error_code}", methodName, issue.id, response.StatusCode);
                        }
                    }
                }
            }
        }
        #endregion Issue 
    }
}

