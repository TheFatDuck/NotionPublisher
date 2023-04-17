namespace Lib.Common.Data.DAO
{
    public class IssuePageDao
    {
        public IssueDao issueDao { get; set; }
        public PageDao? pageDao { get; set; }
        public IssuePageDao() { }
    }
}
