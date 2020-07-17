using Azure.Mobile.Server;
using Azure.Mobile.Server.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;

namespace Azure.Mobile.Common.Test
{
    public class MoviesController : TableController<Movie>
    {
        public MoviesController(): base() { }

        public MoviesController(MovieDbContext context): base(new EntityTableRepository<Movie>(context)) { }

        public void SetRequest(HttpMethod method, string uri, Dictionary<string,string> headers = null)
        {
            var context = new DefaultHttpContext();
            var u = new Uri(uri);
            context.Request.Method = method.ToString();
            context.Request.Host = new HostString(u.Host, u.Port);
            context.Request.Path = u.AbsolutePath;
            context.Request.QueryString = new QueryString(u.Query);
            if (u.Query.Length > 0)
            {
                var nvc = HttpUtility.ParseQueryString(u.Query);
                var qc = new Dictionary<string, StringValues>();
                foreach (var x in nvc.AllKeys)
                {
                    qc[x] = new StringValues(nvc[x]);
                }
                context.Request.Query = new QueryCollection(qc);

            }
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    context.Request.Headers[kv.Key] = new StringValues(kv.Value);
                }
            }
            ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext() { HttpContext = context };
        }

        #region IsAuthorized
        public override bool IsAuthorized(TableOperation operation, Movie item)
        {
            IsAuthorizedCallCount++;
            LastAuthorizationOperation = operation;
            LastAuthorizationItem = item;
            return IsAuthorizedResult;
        }

        public bool BaseIsAuthorized(TableOperation operation, Movie item) => base.IsAuthorized(operation, item);

        public int IsAuthorizedCallCount { get; private set; } = 0;
        public TableOperation LastAuthorizationOperation { get; private set; }
        public Movie LastAuthorizationItem { get; private set; }
        public bool IsAuthorizedResult { get; set; } = true;
        #endregion

        #region PrepareItemForStore
        public Movie BasePrepareItemForStore(Movie item) => base.PrepareItemForStore(item);

        public override Movie PrepareItemForStore(Movie item)
        {
            PrepareItemForStoreCallCount++;
            LastPreparedItem = item.Clone();
            return PrepareItemFunc(item);
        }

        public int PrepareItemForStoreCallCount { get; private set; } = 0;
        public Movie LastPreparedItem { get; private set; }
        public Func<Movie, Movie> PrepareItemFunc { get; set; } = movie => movie;
        #endregion
    }
}
