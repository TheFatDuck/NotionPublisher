namespace Lib.Common.Data.DAO
{
    public class IssuePageDto
    {
        public IssueDto issueDao { get; set; }
        public PageDto? pageDao { get; set; }
        public IssuePageDto() { }
    }
}
