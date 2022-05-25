// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Content
{
    public class StaticHtmlActionResultTests
    {
        [Fact]
        public async Task ExecuteAsync_DoesNotEscapeCurlyBrackets_WhenNoReplacements()
        {
            var result = new StaticHtmlActionResult("Microsoft.Azure.Mobile.Server.Content.Replacements.html");
            HttpResponseMessage response = await result.ExecuteAsync(new CancellationToken());

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal("This {0} has {1} three {2} replacements and some {{curly}} brackets.", content);
            Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task ExecuteAsync_Replaces_WhenReplacements()
        {
            var result = new StaticHtmlActionResult("Microsoft.Azure.Mobile.Server.Content.Replacements.html", "one", "two", "three");
            HttpResponseMessage response = await result.ExecuteAsync(new CancellationToken());

            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal("This one has two three three replacements and some {curly} brackets.", content);
            Assert.Equal("text/html", response.Content.Headers.ContentType.MediaType);
        }
    }
}
