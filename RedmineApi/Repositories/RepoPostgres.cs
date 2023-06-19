using Lib.Common.Data.DAO;
using Lib.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedmineApi.Data;
using RedmineApi.Data.Entities;
using Serilog;
using System.Reflection;

namespace RedmineApi.Repositories
{
    public class RepoPostgres : IRepoPostgres
    {
        private readonly Serilog.ILogger _logger;
        PostgresDbContext _pDbContext;
        /// <summary> 
        /// RepoPostgres constructor.
        /// </summary>
        /// <param name="configuration"></param>
        /// <exception cref="DBInitializeFailException"></exception>
        public RepoPostgres(RedmineApiConfigs redmineApiConfigs)
        {
            _logger = Log.Logger.ForContext<RepoPostgres>();
            try
            {
                _pDbContext = new PostgresDbContext(redmineApiConfigs);
                if (redmineApiConfigs.PgClean)
                {
                    _pDbContext.Database.EnsureDeleted();
                    redmineApiConfigs.PgClean = false;
                }
                _pDbContext.Database.EnsureCreated();

            }
            catch (DBInitializeFailException ex)
            {
                throw ex;
            }
        }


        #region user
        public User SelectUserByKey(string npApiKey)
        {
            User? user = _pDbContext.Users.SingleOrDefault(u => u.np_api_key == npApiKey);
            _logger.Debug("{db_operation} is executed: {result}", "SelectUserByKey", user == null ? 0 : user.user_id);
            return user;
        }
        public User RegisterUser(UserDto userDao)
        {
            if(string.IsNullOrEmpty(userDao.np_api_key))
                userDao.np_api_key = Guid.NewGuid().ToString();
            User newUser = new User(userDao);
            using (var transaction = _pDbContext.Database.BeginTransaction())
            {
                List<Project> projects = userDao.project_keys.Select(key => new Project { project_id = key, project_name = "" }).ToList();
                _pDbContext.Projects.AddRange(projects);
                _pDbContext.Users.Add(newUser);
                _pDbContext.UserProjects.AddRange(projects.Select(p => new UserProject { 
                    user_id = newUser.user_id,
                    user = newUser,
                    project_id = p.project_id,
                    project = p
                }));
                try
                {
                    _pDbContext.SaveChanges();
                    transaction.Commit();
                    _logger.Debug("{db_operation} is executed: {result}", "RegisterUser", newUser.np_api_key);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Log.Error($"Failed to RegisterUser. {(ex.InnerException == null ? ex.Message : ex.InnerException.Message)}");
                    return null;
                }
            }
            return SelectUserByKey(userDao.np_api_key);
        }
        //public User InsertUser(UserDao userDao)
        //{
        //    string methodName = MethodBase.GetCurrentMethod().Name;
        //    User newUser = new User(userDao);
        //    _pDbContext.Users.Add(newUser);
        //    try
        //    {
        //        _pDbContext.SaveChanges();
        //        _logger.Debug("{db_operation} is executed: {result}", "InsertUser", newUser.np_api_key);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error($"Failed to SetUser. {(ex.InnerException == null ? ex.Message : ex.InnerException.Message)}");
        //        return null;
        //    }
        //    return SelectUserByKey(userDao.np_api_key);
        //}
        #endregion user

        #region config
        public Config SelectConfig(int userId, string entry)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            Config? config = _pDbContext.Configs.SingleOrDefault(c => c.user_id == userId && c.entry == entry);
            _logger.Debug("{db_operation} is executed: {result}", methodName, config == null ? "" : config.value);
            return config;
        }
        public string SelectConfigValue(int userId, string entry)
        {
            Config? config = _pDbContext.Configs.SingleOrDefault(c => c.user_id == userId && c.entry == entry);
            _logger.Debug("{db_operation} is executed: {result}", "SelectConfigValue", config == null ? "" : config.value);
            return config == null ? "" : config.value;
        }
        public bool InsertConfig(Config config)
        {
            _pDbContext.Configs.Add(config);
            try
            {
                _pDbContext.SaveChanges();
                _logger.Debug("{db_operation} is executed: {result}", "InsertConfig", config.value);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to SetConfig. {(ex.InnerException == null ? ex.Message : ex.InnerException.Message)}");
                return false;
            }
            return true;
        }
        public bool InsertConfigs(List<Config> configs)
        {
            throw new NotImplementedException();
        }
        public bool UpdateConfig(Config config)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            Config oldConfig = _pDbContext.Configs.FirstOrDefault(c => c.user_id == config.user_id && c.entry == config.entry);
            if (oldConfig != null)
            {
                oldConfig = config;
                try
                {
                    _pDbContext.SaveChanges();
                    _logger.Debug("{db_operation} is executed: {result}", methodName, oldConfig.config_id);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to UpdatePage. {(ex.InnerException == null ? ex.Message : ex.InnerException.Message)}");
                    return false;
                }
            }
            else
            {
                Log.Error($"Failed to UpdatePage. No page found.");
                return false;
            }
            return true;
        }
        #endregion config

        #region project
        public Project SelectProjectById(int projectId)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            Project? project = _pDbContext.Projects.SingleOrDefault(p => p.project_id == projectId);
            _logger.Debug("{db_operation} is executed: {result}", methodName, project == null ? 0 : project.project_id);
            return project;
        }
        public List<Project> SelectProjects()
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            var result = _pDbContext.Projects.ToList();
            _logger.Debug("{db_operation} is executed: {result}", methodName, result.Count);
            return result;
        }
        public List<UserProject> SelectUserProjects(int user_id)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            List<UserProject> result = _pDbContext.UserProjects.Where(up => up.user_id == user_id).ToList();
            _logger.Debug("{db_operation} is executed: {result}", methodName, result.Count);
            return result;
        }
        public bool InsertProject(Project project)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            _pDbContext.Projects.Add(project);
            try
            {
                _pDbContext.SaveChanges();
                _logger.Debug("{db_operation} is executed: {result}", methodName, project.project_id);
            }
            catch (Exception ex)
            {
                Log.Error(GetErrorLog(methodName, ex));
                return false;
            }
            return true;
        }
        public bool InsertProjects(List<Project> projects)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            _pDbContext.Projects.AddRange(projects);
            try
            {
                _pDbContext.SaveChanges();
                _logger.Debug("{db_operation} is executed: {result}", methodName, projects.Count);
            }
            catch (Exception ex)
            {
                Log.Error(GetErrorLog(methodName, ex));
                return false;
            }
            return true;
        }
        public bool UpdateProject(Project project)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            Project oldProject = _pDbContext.Projects.FirstOrDefault(p => p.project_id == project.project_id);
            if (oldProject != null && oldProject.project_name != project.project_name)
            {
                oldProject.project_name = project.project_name;
                try
                {
                    _pDbContext.SaveChanges();
                    _logger.Debug("{db_operation} is executed: {result}", methodName, oldProject.project_id);
                }
                catch (Exception ex)
                {
                    Log.Error(GetErrorLog(methodName, ex));
                    return false;
                }
            }
            else
            {
                Log.Error($"Failed to UpdateProject. No project found.");
                return false;
            }
            return true;
        }
        #endregion project

        #region issue
        /// <summary>
        /// Return Issue. if not found, return null.
        /// </summary>
        /// <param name="issueId"></param>
        /// <returns></returns>
        public Issue SelectIssue(int issueId)
        {
            Issue foundIssue = _pDbContext.Issues.SingleOrDefault(i => i.issue_id == issueId);
            _logger.Debug("{db_operation} is executed: {result}", "SelectIssue", foundIssue == null ? -1 : foundIssue.issue_id);
            return foundIssue;
        }
        public List<IssuePage> SelectIssuesForPage()
        {
            List<IssuePage> result = null;
            var query = from issue in _pDbContext.Issues
                        join page in _pDbContext.Pages on issue.issue_id equals page.issue_id into pages
                        from page in pages.DefaultIfEmpty()
                        where issue.updated_on > issue.last_posted_on
                        select new IssuePage() { issue = issue, page = page };
            result = query.ToList();
            _logger.Debug("{db_operation} is executed: {result}", "SelectIssuesForPage", result.Count);
            return result;
        }
        public bool InsertIssue(Issue issue)
        {
            _pDbContext.Issues.Add(issue);
            try
            {
                _pDbContext.SaveChanges();
                _logger.Debug("{db_operation} is executed: {result}", "InsertIssue", issue.issue_id);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to SetIssue. {(ex.InnerException == null ? ex.Message : ex.InnerException.Message)}");
                return false;
            }
            return true;
        }
        public bool InsertIssues(List<Issue> issues)
        {
            // Update old issues.
            List<int> issueIds = issues.Select(i => i.issue_id).ToList();
            List<Issue> oldIssues = _pDbContext.Issues.Where(i => issueIds.Contains(i.issue_id)).ToList();
            List<int> oldIssueIds = oldIssues.Select(i => i.issue_id).ToList();
            foreach (Issue oi in oldIssues)
            {
                Issue newIssue = issues.FirstOrDefault(i => i.issue_id == oi.issue_id);
                if (newIssue != null)
                {
                    oi.last_posted_on = newIssue.last_posted_on;
                    issues.Remove(newIssue);
                }
            }

            // Add new issues
            _pDbContext.Issues.AddRange(issues);
            try
            {
                _pDbContext.SaveChanges();
                _logger.Debug("{db_operation} is executed: {result}", "InsertIssues", issues.Count);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to SetIssues. {(ex.InnerException == null ? ex.Message : ex.InnerException.Message)}");
                return false;
            }
            return true;
        }
        public bool UpdateIssue(Issue issue)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            Issue oldIssue = _pDbContext.Issues.FirstOrDefault(r => r.issue_id == issue.issue_id);
            if (oldIssue != null)
            {
                oldIssue = issue;
                try
                {
                    _pDbContext.SaveChanges();
                    _logger.Debug("{db_operation} is executed: {result}", "InsertIssues", oldIssue.issue_id);
                }
                catch (Exception ex)
                {
                    Log.Error(GetErrorLog(methodName, ex));
                    return false;
                }
            }
            else
            {
                Log.Error($"Failed to UpdateIssue. No issue found.");
                return false;
            }
            return true;
        }
        #endregion issue

        #region page
        public bool UpsertIssuePage(Issue issue, Page page)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;

            using (var transaction = _pDbContext.Database.BeginTransaction())
            {
                try
                {
                    // Update issue - Do not need to insert issue.
                    Issue existsIssue = _pDbContext.Issues.FirstOrDefault(i => i.issue_id == issue.issue_id);
                    if (existsIssue == null)
                    {
                        // Unknown exception
                        throw new DataNotFoundException("Issue not found.");
                    }
                    else
                    {
                        existsIssue.last_posted_on = issue.last_posted_on;
                    }
                    // Insert page - Do not need to update page.
                    Page existsPage = _pDbContext.Pages.FirstOrDefault(p => p.page_id == page.page_id);
                    if (existsPage == null)
                    {
                        _pDbContext.Pages.Add(page);
                    }
                    _pDbContext.SaveChanges();
                    transaction.Commit();
                    _logger.Debug("{db_operation} is executed: {result}", methodName, issue.issue_id);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _logger.Error("{db_operation} is failed: {message}, {issue_id}", GetErrorLog(methodName, ex), issue.issue_id);
                    return false;
                }
            }
            return true;
        }

        public bool InsertPage(Page page)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            
            // Check already exists.
            Page existsPage = _pDbContext.Pages.FirstOrDefault(p => p.page_id == page.page_id);
            if (existsPage != null) return true;
            
            _pDbContext.Pages.Add(page);
            try
            {
                _pDbContext.SaveChanges();
                _logger.Debug("{db_operation} is executed: {result}", "InsertPage", page.page_id);
            }
            catch (Exception ex)
            {
                Log.Error(GetErrorLog(methodName, ex));
                return false;
            }
            return true;
        }
        public bool UpdatePage(Page page)
        {
            string methodName = MethodBase.GetCurrentMethod().Name;
            Page oldPage = _pDbContext.Pages.FirstOrDefault(r => r.page_id == page.page_id && r.issue_id == page.issue_id);
            if (oldPage != null)
            {
                oldPage = page;
                try
                {
                    _pDbContext.SaveChanges();
                    _logger.Debug("{db_operation} is executed: {result}", "UpdatePage", oldPage.page_id);
                }
                catch (Exception ex)
                {
                    Log.Error(GetErrorLog(methodName, ex));
                    return false;
                }
            }
            else
            {
                Log.Error(GetErrorLog(methodName, "Page not found."));
                return false;
            }
            return true;
        }
        #endregion page


        private string GetErrorLog(string methodName, Exception ex)
        {
            return $"Failed to {methodName}. {(ex.InnerException == null ? ex.Message : ex.InnerException.Message)}";
        }
        private string GetErrorLog(string methodName, string errorMessage)
        {
            return $"Failed to {methodName}. {errorMessage}";
        }
    }
}
