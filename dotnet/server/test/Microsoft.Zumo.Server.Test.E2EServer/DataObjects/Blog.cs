// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Zumo.Server.Entity;

namespace Microsoft.Zumo.Server.Test.E2EServer.DataObjects
{
    public class BlogComment : EntityTableData
    {
        public string PostId { get; set; }
        public string CommentText { get; set; }
        public string Name { get; set; }
        public int Test { get; set; }
    }

    public class BlogPost : EntityTableData
    {
        public string Title { get; set; }
        public int CommentCount { get; set; }
        public bool ShowComments { get; set; }
        public string Data { get; set; }
    }
}
