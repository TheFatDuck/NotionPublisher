using Lib.Common.Data.DAO;

namespace RedmineApi.Data.Entities
{
    public class IssuePage
    {
        public Issue issue { get; set; }
        public Page? page { get; set; }

        public IssuePage() { }
        public IssuePage(Issue issue, Page page)
        {
            this.issue = issue;
            this.page = page;
        }

        public IssuePageDao ConvertToDao()
        {
            return new IssuePageDao()
            {
                issueDao = issue.ConvertToDao(),
                pageDao = (page != null) ? page.ConvertToDao() : null
            };
        }
    }
}
