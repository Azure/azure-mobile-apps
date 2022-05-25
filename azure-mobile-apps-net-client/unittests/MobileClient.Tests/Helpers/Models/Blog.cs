// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Runtime.Serialization;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace MobileClient.Tests.Helpers
{
    [DataTable("blog_posts")]
    public class BlogPost
    {
        public int Id { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "commentCount")]
        public int CommentCount { get; set; }
    }

    [DataTable("blog_comments")]
    public class BlogComment
    {
        public int Id { get; set; }

        [JsonProperty(PropertyName = "postid")]
        public int BlogPostId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "commentText")]
        public string Text { get; set; }
    }

    [DataContract(Name = "blog_posts")]
    public class DataContractBlogPost
    {
        [DataMember]
        public int Id = 0;

        [DataMember(Name = "title")]
        public string Title;

        [DataMember(Name = "commentCount")]
        public int CommentCount { get; set; }
    }
}
