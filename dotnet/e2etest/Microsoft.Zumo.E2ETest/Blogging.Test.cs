// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Zumo.E2ETest.Helpers;
using Microsoft.Zumo.MobileData;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Zumo.E2ETest
{
    public class BlogPost : TableData
    {
        public string Title { get; set; }
        public int CommentCount { get; set; }
    }

    public class BlogComment : TableData
    {
        [JsonPropertyName("postId")]
        public string BlogPostId { get; set; }
        [JsonPropertyName("name")]
        public string UserName { get; set; }
        [JsonPropertyName("commentText")]
        public string Text { get; set; }
    }

    [TestClass]
    public class BloggingTest : BaseTest
    {
        [TestMethod]
        public async Task PostComments()
        {
            var client = GetClient();
            var postTable = client.GetTable<BlogPost>("tables/blog_posts");
            var commentTable = client.GetTable<BlogComment>("tables/blog_comments");

            BlogPost post = await postTable.CreateItemAsync(new BlogPost { Title = "Windows 10" });
            BlogPost highlight = await postTable.CreateItemAsync(new BlogPost { Title = "ZUMO" });
            await commentTable.CreateItemAsync(new BlogComment
            {
                BlogPostId = post.Id,
                UserName = "anonymous",
                Text = "Beta runs great"
            });
            await commentTable.CreateItemAsync(new BlogComment
            {
                BlogPostId = highlight.Id,
                UserName = "anonymous",
                Text = "Whooooo"
            });

            var posts = postTable.GetItems().ToList();
            Assert.AreEqual(2, posts.Count);

            BlogPost first = await postTable.GetItemAsync(post.Id);
            Assert.AreEqual("Windows 10", first.Title);

            // Make sure the comment gets an appropriate ID
            BlogComment opinion = await commentTable.CreateItemAsync(new BlogComment { BlogPostId = first.Id, Text = "Can't wait" });
            Assert.IsFalse(string.IsNullOrWhiteSpace(opinion.Id));

            // Delete all the blog posts
            var postsToDelete = postTable.GetItems().ToList();
            Assert.AreEqual(2, postsToDelete.Count());
            foreach(var item in postsToDelete)
            {
                await postTable.DeleteItemAsync(item);
            }
            var postsFinal = postTable.GetItems().Count();
            Assert.AreEqual(0, postsFinal);

            var commentsToDelete = commentTable.GetItems().ToList();
            Assert.AreEqual(3, commentsToDelete.Count());
            foreach (var item in commentsToDelete)
            {
                await commentTable.DeleteItemAsync(item);
            }
            var commentsFinal = commentTable.GetItems().Count();
            Assert.AreEqual(0, commentsFinal);
        }

        [TestMethod]
        public async Task PostExceptionMessageFromZumoRuntime()
        {
            var client = GetClient();
            var postTable = client.GetTable<BlogPost>("tables/blog_posts");

            try
            {
                await postTable.DeleteItemAsync(new BlogPost { CommentCount = 5, Id = "this_does_not_exist" });
            }
            catch (RequestFailedException e)
            {
                Assert.AreEqual(404, e.Status); // Not found
            }
        }

        [TestMethod]
        public async Task PostExceptionMessageFromUserScript()
        {
            var client = GetClient();
            var postTable = client.GetTable<BlogPost>("tables/blog_posts");

            try
            {
                await postTable.CreateItemAsync(new BlogPost { });
            }
            catch (RequestFailedException e)
            {
                Assert.AreEqual("All blog posts must have a title.", e.Message);
            }
        }

        // FYI: Data Contracts no longer supported
    }
}
