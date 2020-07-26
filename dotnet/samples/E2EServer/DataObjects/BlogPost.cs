using Azure.Mobile.Server.Entity;

namespace E2EServer.DataObjects
{
    public class BlogPost : EntityTableData
    {
        public string Title { get; set; }
        public int CommentCount { get; set; }
        public bool ShowComments { get; set; }
        public string Data { get; set; }
    }
}
