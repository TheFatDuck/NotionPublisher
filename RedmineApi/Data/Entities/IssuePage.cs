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

        public IssuePageDto ConvertToDto()
        {
            return new IssuePageDto()
            {
                issueDao = issue.ConvertToDto(),
                pageDao = (page != null) ? page.ConvertToDto() : null
            };
        }
    }
}
