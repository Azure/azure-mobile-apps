// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SQLiteStore.Tests.Helpers
{
    /// <summary>
    /// ServiceFilter that allows a test to control the HTTP pipeline and
    /// analyze a request and provide a set response.
    /// </summary>
    public class TestHttpHandler : DelegatingHandler
    {
        HttpResponseMessage nullResponse;
        int responseIndex = 0;

        public TestHttpHandler()
        {
            this.Requests = new List<HttpRequestMessage>();
            this.Responses = new List<HttpResponseMessage>();
            this.RequestContents = new List<string>();

            this.nullResponse = CreateResponse(String.Empty);
        }

        public HttpRequestMessage Request
        {
            get { return this.Requests.Count == 0 ? null : this.Requests[this.Requests.Count - 1]; }
            set
            {
                this.Requests.Clear();
                this.Requests.Add(value);
            }
        }

        public List<HttpRequestMessage> Requests { get; set; }
        public List<string> RequestContents { get; set; }

        public HttpResponseMessage Response
        {
            get { return this.Responses.Count == 0 ? null : this.Responses[this.Responses.Count - 1]; }
            set
            {
                this.responseIndex = 0;
                this.Responses.Clear();
                this.Responses.Add(value);
            }
        }

        public List<HttpResponseMessage> Responses { get; set; }

        public Func<HttpRequestMessage, Task<HttpRequestMessage>> OnSendingRequest { get; set; }

        public void SetResponseContent(string content)
        {
            this.Response = CreateResponse(content);
        }

        public void AddResponseContent(string content)
        {
            this.Responses.Add(CreateResponse(content));
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string content = request.Content == null ? null : await request.Content.ReadAsStringAsync();
            this.RequestContents.Add(content);

            if (this.OnSendingRequest != null)
            {
                this.Requests.Add(await this.OnSendingRequest(request));
            }
            else
            {
                this.Requests.Add(request);
            }

            if (responseIndex < this.Responses.Count)
            {
                return Responses[responseIndex++];
            }

            return nullResponse;
        }

        public static HttpResponseMessage CreateResponse(string content, HttpStatusCode code = HttpStatusCode.OK)
        {
            return new HttpResponseMessage(code)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
        }
    }

    public static class TestHttpDelegatingHandler
    {
        public static DelegatingHandler CreateTestHttpHandler(string expectedUri, HttpMethod expectedMethod, string responseContent, HttpStatusCode? httpStatusCode = null, Uri location = null, string expectedRequestContent = null)
        {
            var handler = new TestHttpHandler
            {
                OnSendingRequest = message =>
                {
                    Assert.Equal(expectedUri, message.RequestUri.OriginalString);
                    Assert.Equal(expectedMethod, message.Method);

                    if (expectedRequestContent != null)
                    {
                        var messageContent = Regex.Replace(message.Content.ReadAsStringAsync().Result, @"\s+", String.Empty);
                        expectedRequestContent = Regex.Replace(expectedRequestContent, @"\s+", String.Empty);
                        Assert.Equal(expectedRequestContent, messageContent);
                    }

                    return Task.FromResult(message);
                }
            };

            if (responseContent != null)
            {
                handler.SetResponseContent(responseContent);
            }
            else
            {
                handler.Response = new HttpResponseMessage(HttpStatusCode.OK);
            }

            if (location != null)
            {
                handler.Response.Headers.Location = location;
            }

            if (httpStatusCode.HasValue)
            {
                handler.Response.StatusCode = httpStatusCode.Value;
            }
            return handler;
        }
    }

    internal class ComplexDelegatingHandler : DelegatingHandler
    {
        private static List<string> allMessages = new List<string>();

        public string MessageBeforeSend { get; private set; }
        public string MessageAfterSend { get; private set; }

        public ComplexDelegatingHandler(string messageBeforeSend, string messageAfterSend)
        {
            this.MessageBeforeSend = messageBeforeSend;
            this.MessageAfterSend = messageAfterSend;
        }

        public static void ClearStoredMessages()
        {
            allMessages.Clear();
        }

        public static IEnumerable<string> AllMessages
        {
            get { return allMessages; }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            allMessages.Add(this.MessageBeforeSend);
            var response = await base.SendAsync(request, cancellationToken);
            allMessages.Add(this.MessageAfterSend);
            return response;
        }
    }
}
