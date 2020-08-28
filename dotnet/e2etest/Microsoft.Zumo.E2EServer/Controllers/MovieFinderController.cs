// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Zumo.E2EServer.DataObjects;
using Microsoft.Zumo.E2EServer.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Zumo.E2EServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieFinderController : ControllerBase
    {
        private readonly AppDbContext dbContext;

        public MovieFinderController(AppDbContext context)
        {
            dbContext = context;
        }

        [HttpGet("title/{*title}")]
        public Task<MovieResult> GetByTitle(string title)
        {
            return GetMovie("Title", title);
        }

        [HttpGet("date/{year}/{month}/{day}")]
        public Task<MovieResult> GetByDate(int year, int month, int day)
        {
            return GetMovie("ReleaseDate", new DateTime(year, month, day));
        }

        [HttpPost("moviesOnSameYear")]
        public async Task<IActionResult> FetchMoviesSameYear(Movie movie)
        {
            if (movie == null)
            {
                return BadRequest();
            }
            var orderBy = GetQueryValue("orderby", "Title");
            return Ok(await GetMovie("Year", movie.Year, orderBy));
        }

        [HttpPost("moviesWithSameDuration")]
        public async Task<IActionResult> FetchMoviesSameDuration(Movie movie)
        {
            if (movie == null)
            {
                return BadRequest();
            }
            var orderBy = GetQueryValue("orderby", "Title");
            return Ok(await GetMovie("Duration", movie.Duration, orderBy));
        }

        private async Task<MovieResult> GetMovie(string field, object value, string orderBy = null)
        {
            var Movies = dbContext.Movies;
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
            var v = Request.Query.FirstOrDefault(p => p.Key.ToLowerInvariant() == key.ToLowerInvariant());
            return (!string.IsNullOrEmpty(v.Value.ToString()) ? v.Value.ToString() : defaultValue);
        }

        private static IQueryable<Movie> Where(IQueryable<Movie> t2, string columnName, object value)
        {
            t2 = (columnName.ToLowerInvariant()) switch
            {
                "title" => t2.Where(p => p.Title == (string)value),
                "releasedate" => t2.Where(p => p.ReleaseDate == (DateTime)value),
                "year" => t2.Where(p => p.Year == (int)value),
                "duration" => t2.Where(p => p.Duration == (int)value),
                _ => throw new ArgumentOutOfRangeException("field"),
            };
            return t2;
        }

        private static IQueryable<Movie> OrderBy(IQueryable<Movie> t2, string columnName)
        {
            t2 = (columnName.ToLowerInvariant()) switch
            {
                "id" => t2.OrderBy(p => p.Id),
                "title" => t2.OrderBy(p => p.Title),
                "releasedate" => t2.OrderBy(p => p.ReleaseDate),
                "year" => t2.OrderBy(p => p.Year),
                "duration" => t2.OrderBy(p => p.Duration),
                _ => throw new ArgumentOutOfRangeException("field"),
            };
            return t2;
        }

        public class MovieResult
        {
            public Movie[] Movies { get; set; }
        }
    }
}
