using Lib.Common.Data.DAO;
using RedmineApi.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineApi.Repositories
{
    public interface IRepoPostgres
    {
        User SelectUserByKey(string userApiKey);
        User RegisterUser(UserDto userDao);


        Config SelectConfig(int userId, string entry);
        string SelectConfigValue(int userId, string entry);
        bool InsertConfig(Config config);
        bool InsertConfigs(List<Config> configs);
        bool UpdateConfig(Config config);


        Project SelectProjectById(int projectId);
        List<Project> SelectProjects();
        List<UserProject> SelectUserProjects(int user_id);
        bool InsertProject(Project project);
        bool InsertProjects(List<Project> projects);
        bool UpdateProject(Project project);


        Issue SelectIssue(int issueId);
        List<IssuePage> SelectIssuesForPage();
        bool InsertIssue(Issue issue);
        bool InsertIssues(List<Issue> issues);
        bool UpdateIssue(Issue issue);


        bool InsertPage(Page page);
        bool UpdatePage(Page page);


        bool UpsertIssuePage(Issue issue, Page page);
    }
}
