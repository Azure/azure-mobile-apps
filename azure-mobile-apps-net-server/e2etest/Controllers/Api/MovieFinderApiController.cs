// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Tracing;
using Microsoft.Azure.Mobile.Server.Config;
using ZumoE2EServerApp.DataObjects;
using ZumoE2EServerApp.Models;

namespace ZumoE2EServerApp.Controllers
{
    [MobileAppController]
    public class MovieFinderApiController : ApiController
    {
        [HttpGet, Route("api/movieFinder/title/{*title}")]
        public Task<MovieResult> getByTitle(string title)
        {
            return getMovie("Title", title);
        }

        [HttpGet, Route("api/movieFinder/date/{year}/{month}/{day}")]
        public Task<MovieResult> getByDate(int year, int month, int day)
        {
            return getMovie("ReleaseDate", new DateTime(year, month, day));
        }

        [HttpPost, Route("api/movieFinder/moviesOnSameYear")]
        public Task<MovieResult> fetchMoviesSameYear(Movie movie)
        {
            if (movie == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            var orderBy = GetQueryValue("orderby", "Title");
            return getMovie("Year", movie.Year, orderBy);
        }

        [HttpPost, Route("api/movieFinder/moviesWithSameDuration")]
        public Task<MovieResult> fetchMoviesSameDuration(Movie movie)
        {
            if (movie == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            var orderBy = GetQueryValue("orderby", "Title");
            return getMovie("Duration", movie.Duration, orderBy);
        }

        private async Task<MovieResult> getMovie(string field, object value, string orderBy = null)
        {
            SDKClientTestContext context = new SDKClientTestContext();
            var Movies = context.Movies;
            ITraceWriter traceWriter = this.Configuration.Services.GetTraceWriter();
            traceWriter.Debug("table: " + "Movies");
            traceWriter.Debug("Field: " + field + ", value: " + value);
            traceWriter.Debug("OrderBy: " + (orderBy == null ? "<null>" : orderBy));
            IQueryable<Movie> t2 = Movies.Where(p => true);
            t2 = Where(t2, field, value);
            if (orderBy != null)
            {
                t2 = OrderBy(t2, orderBy);
            }

            var results = await t2.ToListAsync();
            return new MovieResult() { Movies = results.ToArray() };
        }

        private string GetQueryValue(string key, string defaultValue)
        {
            var v = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => p.Key.ToLowerInvariant() == key.ToLowerInvariant());
            return (!string.IsNullOrEmpty(v.Value) ? v.Value : defaultValue);
        }

        private static IQueryable<Movie> Where(IQueryable<Movie> t2, string columnName, object value)
        {
            switch (columnName.ToLowerInvariant())
            {
                case "title":
                    t2 = t2.Where(p => p.Title == (string)value);
                    break;

                case "releasedate":
                    t2 = t2.Where(p => p.ReleaseDate == (DateTime)value);
                    break;

                case "year":
                    t2 = t2.Where(p => p.Year == (int)value);
                    break;

                case "duration":
                    t2 = t2.Where(p => p.Duration == (int)value);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("field");
            }
            return t2;
        }

        private static IQueryable<Movie> OrderBy(IQueryable<Movie> t2, string columnName)
        {
            switch (columnName.ToLowerInvariant())
            {
                case "id":
                    t2 = t2.OrderBy(p => p.Id);
                    break;

                case "title":
                    t2 = t2.OrderBy(p => p.Title);
                    break;

                case "releasedate":
                    t2 = t2.OrderBy(p => p.ReleaseDate);
                    break;

                case "year":
                    t2 = t2.OrderBy(p => p.Year);
                    break;

                case "duration":
                    t2 = t2.OrderBy(p => p.Duration);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("field");
            }
            return t2;
        }

        public class MovieResult
        {
            public Movie[] Movies { get; set; }
        }
    }
}