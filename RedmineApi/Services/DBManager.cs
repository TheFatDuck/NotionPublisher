using Lib.Common.Data.DAO;
using Lib.Common.Exceptions;
using RedmineApi.Data;
using RedmineApi.Data.Entities;
using RedmineApi.Models;
using RedmineApi.Repositories;
using Serilog;

namespace RedmineApi.Services
{
    public class DBManager
    {
        IRepoPostgres repoPostgres;
        public DBManager(RedmineApiConfigs configs)
        {
            try
            {
                repoPostgres = new RepoPostgres(configs);
            }
            catch(DBInitializeFailException ex)
            {
                Log.Error($"Failed to initialize Postgresql database. {ex.Message}({ex.GetErrorCode})");
                throw ex;
            }
        }

        public bool CheckAlive()
        {
            return false;
        }

        #region user
        public User SelectUser(string npApiKey)
        {
            return repoPostgres.SelectUserByKey(npApiKey);
        }
        public User RegisterUser(UserDao userDao)
        {
            return repoPostgres.RegisterUser(userDao);
        }
        #endregion user

        #region config
        public string SelectConfig(int userKey, string entry)
        {
            return repoPostgres.SelectConfigValue(userKey, entry);
        }
        public bool UpsertConfig(Config config)
        {
            Config oldConfig = repoPostgres.SelectConfig(config.user_id, config.entry);
            if (oldConfig == null)
                return repoPostgres.InsertConfig(config);
            else
                return repoPostgres.UpdateConfig(config);
        }
        #endregion config

        #region project
        public List<Project> SelectProjects()
        {
            return repoPostgres.SelectProjects();
        }
        public List<Project> SelectUserProjects(int user_id)
        {
            return repoPostgres.SelectProjects();
        }
        public bool InsertProjects(List<Project> projects)
        {
            return repoPostgres.InsertProjects(projects);
        }
        public bool UpsertProject(Project project)
        {
            Project oldProject = repoPostgres.SelectProjectById(project.project_id);
            if (oldProject == null)
                return repoPostgres.InsertProject(project);
            else if (oldProject.project_name != project.project_name)
                return repoPostgres.UpdateProject(project);
            // If not updated, Skip update.
            return true;
        }
        #endregion project

        #region issue
        public Issue SelectIssue(int issueId)
        {
            return repoPostgres.SelectIssue(issueId);
        }
        public List<IssuePage> SelectIssuesForPage()
        {
            return repoPostgres.SelectIssuesForPage();
        }
        /// <summary>
        /// Insert or Update Issue. If issue is not found, insert it. Otherwise, update it.
        /// </summary>
        /// <param name="issue"></param>
        /// <returns></returns>
        public bool UpsertIssue(Issue issue)
        {
            Issue oldIssue = repoPostgres.SelectIssue(issue.issue_id);
            if (oldIssue == null)
                return repoPostgres.InsertIssue(issue);
            else if (oldIssue.IsUpdated(issue))
                return repoPostgres.UpdateIssue(issue);
            // If not updated, Skip update.
            return true;
        }
        #endregion issue

        #region page
        public bool UpsertIssuePage(Issue issue, Page page)
        {
            return repoPostgres.UpsertIssuePage(issue, page);
        }
        public bool InsertPage(Page page)
        {
            return repoPostgres.InsertPage(page);
        }
        #endregion page
    }
}
