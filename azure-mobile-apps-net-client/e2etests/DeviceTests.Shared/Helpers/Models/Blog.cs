// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace DeviceTests.Shared.Helpers.Models
{
    [DataTable("blog_posts")]
    public class BlogPost
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "commentCount")]
        public int CommentCount { get; set; }
    }

    [DataTable("blog_comments")]
    public class BlogComment
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "postid")]
        public string BlogPostId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "commentText")]
        public string Text { get; set; }
    }

    [DataContract(Name = "blog_posts")]
    public class DataContractBlogPost
    {
        [DataMember]
        public string Id = null;

        [DataMember(Name = "title")]
        public string Title;

        [DataMember(Name = "commentCount")]
        public int CommentCount { get; set; }
    }
}
