using Azure.Mobile.Server.Entity;

namespace Azure.Mobile.Test.E2EServer.DataObjects
{
    public class BlogComment : EntityTableData
    {
        public string PostId { get; set; }
        public string CommentText { get; set; }
        public string Name { get; set; }
        public int Test { get; set; }
    }
}
