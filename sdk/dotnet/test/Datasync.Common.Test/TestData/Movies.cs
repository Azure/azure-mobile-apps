// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Datasync.Common.Test.TestData
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public static class Movies
    {
        /// <summary>
        /// Creates a list of movies based on an <see cref="IMovie"/> based type.
        /// </summary>
        /// <typeparam name="T">The new type of the movie.</typeparam>
        /// <returns>A list of movies</returns>
        public static List<T> OfType<T>() where T : ITableData, IMovie, new()
        {
            List<T> result = new();

            for (var idx = 0; idx < MovieList.Length; idx++)
            {
                var addition = new T() { Id = Utils.GetMovieId(idx) };
                MovieList[idx].CopyTo(addition);
                result.Add(addition);
            }

            return result;
        }

        /// <summary>
        /// The number of movies in the list.  This must be a const so that it can be
        /// used in attributes.
        /// </summary>
        public const int Count = 248;

        /// <summary>
        /// The list of movies.
        /// </summary>
        public static MovieBase[] MovieList { get; } = new MovieBase[]
        {
            new MovieBase /* 000 */
            {
                BestPictureWinner = false,
                Duration = 142,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1994, 10, 14, 0, 0, 0, TimeSpan.Zero),
                Title = "The Shawshank Redemption",
                Year = 1994
            },
            new MovieBase /* 001 */
            {
                BestPictureWinner = true,
                Duration = 175,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1972, 03, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "The Godfather",
                Year = 1972
            },
            new MovieBase /* 002 */
            {
                BestPictureWinner = true,
                Duration = 200,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1974, 12, 20, 0, 0, 0, TimeSpan.Zero),
                Title = "The Godfather: Part II",
                Year = 1974
            },
            new MovieBase /* 003 */
            {
                BestPictureWinner = false,
                Duration = 168,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1994, 10, 14, 0, 0, 0, TimeSpan.Zero),
                Title = "Pulp Fiction",
                Year = 1994
            },
            new MovieBase /* 004 */
            {
                BestPictureWinner = false,
                Duration = 161,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1967, 12, 29, 0, 0, 0, TimeSpan.Zero),
                Title = "The Good, the Bad and the Ugly",
                Year = 1966
            },
            new MovieBase /* 005 */
            {
                BestPictureWinner = false,
                Duration = 96,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1957, 4, 10, 0, 0, 0, TimeSpan.Zero),
                Title = "12 Angry Men",
                Year = 1957
            },
            new MovieBase /* 006 */
            {
                BestPictureWinner = false,
                Duration = 152,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2008, 07, 18, 0, 0, 0, TimeSpan.Zero),
                Title = "The Dark Knight",
                Year = 2008
            },
            new MovieBase /* 007 */
            {
                BestPictureWinner = true,
                Duration = 195,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1993, 12, 15, 0, 0, 0, TimeSpan.Zero),
                Title = "Schindler's List",
                Year = 1993
            },
            new MovieBase /* 008 */
            {
                BestPictureWinner = true,
                Duration = 201,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2003, 12, 17, 0, 0, 0, TimeSpan.Zero),
                Title = "The Lord of the Rings: The Return of the King",
                Year = 2003
            },
            new MovieBase /* 009 */
            {
                BestPictureWinner = false,
                Duration = 139,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1999, 10, 15, 0, 0, 0, TimeSpan.Zero),
                Title = "Fight Club",
                Year = 1999
            },
            new MovieBase /* 010 */
            {
                BestPictureWinner = false,
                Duration = 127,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1980, 05, 21, 0, 0, 0, TimeSpan.Zero),
                Title = "Star Wars: Episode V - The Empire Strikes Back",
                Year = 1980
            },
            new MovieBase /* 011 */
            {
                BestPictureWinner = true,
                Duration = 133,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1975, 11, 21, 0, 0, 0, TimeSpan.Zero),
                Title = "One Flew Over the Cuckoo's Nest",
                Year = 1975
            },
            new MovieBase /* 012 */
            {
                BestPictureWinner = false,
                Duration = 178,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2001, 12, 19, 0, 0, 0, TimeSpan.Zero),
                Title = "The Lord of the Rings: The Fellowship of the Ring",
                Year = 2001
            },
            new MovieBase /* 013 */
            {
                BestPictureWinner = false,
                Duration = 148,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2010, 07, 16, 0, 0, 0, TimeSpan.Zero),
                Title = "Inception",
                Year = 2010
            },
            new MovieBase /* 014 */
            {
                BestPictureWinner = false,
                Duration = 146,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1990, 09, 19, 0, 0, 0, TimeSpan.Zero),
                Title = "Goodfellas",
                Year = 1990
            },
            new MovieBase /* 015 */
            {
                BestPictureWinner = false,
                Duration = 121,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1977, 05, 25, 0, 0, 0, TimeSpan.Zero),
                Title = "Star Wars",
                Year = 1977
            },
            new MovieBase /* 016 */
            {
                BestPictureWinner = false,
                Duration = 141,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1956, 11, 19, 0, 0, 0, TimeSpan.Zero),
                Title = "Seven Samurai",
                Year = 1954
            },
            new MovieBase /* 017 */
            {
                BestPictureWinner = false,
                Duration = 136,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1999, 03, 31, 0, 0, 0, TimeSpan.Zero),
                Title = "The Matrix",
                Year = 1999
            },
            new MovieBase /* 018 */
            {
                BestPictureWinner = true,
                Duration = 142,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(1994, 07, 06, 0, 0, 0, TimeSpan.Zero),
                Title = "Forrest Gump",
                Year = 1994
            },
            new MovieBase /* 019 */
            {
                BestPictureWinner = false,
                Duration = 130,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2002, 01, 01, 0, 0, 0, TimeSpan.Zero),
                Title = "City of God",
                Year = 2002
            },
            new MovieBase /* 020 */
            {
                BestPictureWinner = false,
                Duration = 179,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2002, 12, 18, 0, 0, 0, TimeSpan.Zero),
                Title = "The Lord of the Rings: The Two Towers",
                Year = 2002
            },
            new MovieBase /* 021 */
            {
                BestPictureWinner = false,
                Duration = 175,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(1968, 12, 21, 0, 0, 0, TimeSpan.Zero),
                Title = "Once Upon a Time in the West",
                Year = 1968
            },
            new MovieBase /* 022 */
            {
                BestPictureWinner = false,
                Duration = 127,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1995, 09, 22, 0, 0, 0, TimeSpan.Zero),
                Title = "Se7en",
                Year = 1995
            },
            new MovieBase /* 023 */
            {
                BestPictureWinner = true,
                Duration = 118,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1991, 02, 14, 0, 0, 0, TimeSpan.Zero),
                Title = "The Silence of the Lambs",
                Year = 1991
            },
            new MovieBase /* 024 */
            {
                BestPictureWinner = true,
                Duration = 102,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1943, 01, 23, 0, 0, 0, TimeSpan.Zero),
                Title = "Casablanca",
                Year = 1942
            },
            new MovieBase /* 025 */
            {
                BestPictureWinner = false,
                Duration = 106,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1995, 08, 16, 0, 0, 0, TimeSpan.Zero),
                Title = "The Usual Suspects",
                Year = 1995
            },
            new MovieBase /* 026 */
            {
                BestPictureWinner = false,
                Duration = 115,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1981, 06, 12, 0, 0, 0, TimeSpan.Zero),
                Title = "Raiders of the Lost Ark",
                Year = 1981
            },
            new MovieBase /* 027 */
            {
                BestPictureWinner = false,
                Duration = 112,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1955, 01, 13, 0, 0, 0, TimeSpan.Zero),
                Title = "Rear Window",
                Year = 1954
            },
            new MovieBase /* 028 */
            {
                BestPictureWinner = false,
                Duration = 109,
                Rating = "TV-14",
                ReleaseDate = new DateTimeOffset(1960, 9, 8, 0, 0, 0, TimeSpan.Zero),
                Title = "Psycho",
                Year = 1960
            },
            new MovieBase /* 029 */
            {
                BestPictureWinner = false,
                Duration = 130,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1947, 01, 06, 0, 0, 0, TimeSpan.Zero),
                Title = "It's a Wonderful Life",
                Year = 1946
            },
            new MovieBase /* 030 */
            {
                BestPictureWinner = false,
                Duration = 110,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1994, 11, 18, 0, 0, 0, TimeSpan.Zero),
                Title = "Léon: The Professional",
                Year = 1994
            },
            new MovieBase /* 031 */
            {
                BestPictureWinner = false,
                Duration = 110,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1950, 08, 25, 0, 0, 0, TimeSpan.Zero),
                Title = "Sunset Blvd.",
                Year = 1950
            },
            new MovieBase /* 032 */
            {
                BestPictureWinner = false,
                Duration = 113,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2000, 10, 11, 0, 0, 0, TimeSpan.Zero),
                Title = "Memento",
                Year = 2000
            },
            new MovieBase /* 033 */
            {
                BestPictureWinner = false,
                Duration = 165,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2012, 07, 20, 0, 0, 0, TimeSpan.Zero),
                Title = "The Dark Knight Rises",
                Year = 2012
            },
            new MovieBase /* 034 */
            {
                BestPictureWinner = false,
                Duration = 119,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1999, 02, 12, 0, 0, 0, TimeSpan.Zero),
                Title = "American History X",
                Year = 1998
            },
            new MovieBase /* 035 */
            {
                BestPictureWinner = false,
                Duration = 153,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1979, 08, 15, 0, 0, 0, TimeSpan.Zero),
                Title = "Apocalypse Now",
                Year = 1979
            },
            new MovieBase /* 036 */
            {
                BestPictureWinner = false,
                Duration = 152,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1991, 07, 03, 0, 0, 0, TimeSpan.Zero),
                Title = "Terminator 2: Judgment Day",
                Year = 1991
            },
            new MovieBase /* 037 */
            {
                BestPictureWinner = false,
                Duration = 95,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1964, 01, 29, 0, 0, 0, TimeSpan.Zero),
                Title = "Dr. Strangelove or: How I Learned to Stop Worrying and Love the Bomb",
                Year = 1964
            },
            new MovieBase /* 038 */
            {
                BestPictureWinner = false,
                Duration = 169,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1998, 07, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "Saving Private Ryan",
                Year = 1998
            },
            new MovieBase /* 039 */
            {
                BestPictureWinner = false,
                Duration = 117,
                Rating = "TV-14",
                ReleaseDate = new DateTimeOffset(1979, 05, 25, 0, 0, 0, TimeSpan.Zero),
                Title = "Alien",
                Year = 1979
            },
            new MovieBase /* 040 */
            {
                BestPictureWinner = false,
                Duration = 136,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1959, 09, 26, 0, 0, 0, TimeSpan.Zero),
                Title = "North by Northwest",
                Year = 1959
            },
            new MovieBase /* 041 */
            {
                BestPictureWinner = false,
                Duration = 87,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1931, 03, 07, 0, 0, 0, TimeSpan.Zero),
                Title = "City Lights",
                Year = 1931
            },
            new MovieBase /* 042 */
            {
                BestPictureWinner = false,
                Duration = 125,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(2001, 07, 20, 0, 0, 0, TimeSpan.Zero),
                Title = "Spirited Away",
                Year = 2001
            },
            new MovieBase /* 043 */
            {
                BestPictureWinner = false,
                Duration = 119,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1941, 9, 5, 0, 0, 0, TimeSpan.Zero),
                Title = "Citizen Kane",
                Year = 1941
            },
            new MovieBase /* 044 */
            {
                BestPictureWinner = false,
                Duration = 87,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1936, 02, 25, 0, 0, 0, TimeSpan.Zero),
                Title = "Modern Times",
                Year = 1936
            },
            new MovieBase /* 045 */
            {
                BestPictureWinner = false,
                Duration = 142,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1980, 05, 23, 0, 0, 0, TimeSpan.Zero),
                Title = "The Shining",
                Year = 1980
            },
            new MovieBase /* 046 */
            {
                BestPictureWinner = false,
                Duration = 129,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1958, 07, 21, 0, 0, 0, TimeSpan.Zero),
                Title = "Vertigo",
                Year = 1958
            },
            new MovieBase /* 047 */
            {
                BestPictureWinner = false,
                Duration = 116,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1985, 07, 03, 0, 0, 0, TimeSpan.Zero),
                Title = "Back to the Future",
                Year = 1985
            },
            new MovieBase /* 048 */
            {
                BestPictureWinner = true,
                Duration = 122,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1999, 10, 01, 0, 0, 0, TimeSpan.Zero),
                Title = "American Beauty",
                Year = 1999
            },
            new MovieBase /* 049 */
            {
                BestPictureWinner = false,
                Duration = 117,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1931, 08, 30, 0, 0, 0, TimeSpan.Zero),
                Title = "M",
                Year = 1931
            },
            new MovieBase /* 050 */
            {
                BestPictureWinner = false,
                Duration = 150,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2003, 03, 28, 0, 0, 0, TimeSpan.Zero),
                Title = "The Pianist",
                Year = 2002
            },
            new MovieBase /* 051 */
            {
                BestPictureWinner = true,
                Duration = 151,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2006, 10, 06, 0, 0, 0, TimeSpan.Zero),
                Title = "The Departed",
                Year = 2006
            },
            new MovieBase /* 052 */
            {
                BestPictureWinner = false,
                Duration = 113,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1976, 02, 08, 0, 0, 0, TimeSpan.Zero),
                Title = "Taxi Driver",
                Year = 1976
            },
            new MovieBase /* 053 */
            {
                BestPictureWinner = false,
                Duration = 103,
                Rating = "G",
                ReleaseDate = new DateTimeOffset(2010, 06, 18, 0, 0, 0, TimeSpan.Zero),
                Title = "Toy Story 3",
                Year = 2010
            },
            new MovieBase /* 054 */
            {
                BestPictureWinner = false,
                Duration = 88,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1957, 10, 25, 0, 0, 0, TimeSpan.Zero),
                Title = "Paths of Glory",
                Year = 1957
            },
            new MovieBase /* 055 */
            {
                BestPictureWinner = false,
                Duration = 118,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(1999, 02, 12, 0, 0, 0, TimeSpan.Zero),
                Title = "Life Is Beautiful",
                Year = 1997
            },
            new MovieBase /* 056 */
            {
                BestPictureWinner = false,
                Duration = 107,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1944, 04, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "Double Indemnity",
                Year = 1944
            },
            new MovieBase /* 057 */
            {
                BestPictureWinner = false,
                Duration = 154,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1986, 07, 18, 0, 0, 0, TimeSpan.Zero),
                Title = "Aliens",
                Year = 1986
            },
            new MovieBase /* 058 */
            {
                BestPictureWinner = false,
                Duration = 98,
                Rating = "G",
                ReleaseDate = new DateTimeOffset(2008, 06, 27, 0, 0, 0, TimeSpan.Zero),
                Title = "WALL-E",
                Year = 2008
            },
            new MovieBase /* 059 */
            {
                BestPictureWinner = false,
                Duration = 137,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2006, 03, 23, 0, 0, 0, TimeSpan.Zero),
                Title = "The Lives of Others",
                Year = 2006
            },
            new MovieBase /* 060 */
            {
                BestPictureWinner = false,
                Duration = 136,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1972, 02, 02, 0, 0, 0, TimeSpan.Zero),
                Title = "A Clockwork Orange",
                Year = 1971
            },
            new MovieBase /* 061 */
            {
                BestPictureWinner = false,
                Duration = 122,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2001, 04, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "Amélie",
                Year = 2001
            },
            new MovieBase /* 062 */
            {
                BestPictureWinner = true,
                Duration = 155,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2000, 05, 05, 0, 0, 0, TimeSpan.Zero),
                Title = "Gladiator",
                Year = 2000
            },
            new MovieBase /* 063 */
            {
                BestPictureWinner = false,
                Duration = 189,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1999, 12, 10, 0, 0, 0, TimeSpan.Zero),
                Title = "The Green Mile",
                Year = 1999
            },
            new MovieBase /* 064 */
            {
                BestPictureWinner = false,
                Duration = 112,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2011, 11, 02, 0, 0, 0, TimeSpan.Zero),
                Title = "The Intouchables",
                Year = 2011
            },
            new MovieBase /* 065 */
            {
                BestPictureWinner = true,
                Duration = 227,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1963, 01, 30, 0, 0, 0, TimeSpan.Zero),
                Title = "Lawrence of Arabia",
                Year = 1962
            },
            new MovieBase /* 066 */
            {
                BestPictureWinner = false,
                Duration = 129,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1963, 03, 16, 0, 0, 0, TimeSpan.Zero),
                Title = "To Kill a Mockingbird",
                Year = 1962
            },
            new MovieBase /* 067 */
            {
                BestPictureWinner = false,
                Duration = 130,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2006, 10, 20, 0, 0, 0, TimeSpan.Zero),
                Title = "The Prestige",
                Year = 2006
            },
            new MovieBase /* 068 */
            {
                BestPictureWinner = false,
                Duration = 125,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1941, 3, 7, 0, 0, 0, TimeSpan.Zero),
                Title = "The Great Dictator",
                Year = 1940
            },
            new MovieBase /* 069 */
            {
                BestPictureWinner = false,
                Duration = 99,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1992, 10, 23, 0, 0, 0, TimeSpan.Zero),
                Title = "Reservoir Dogs",
                Year = 1992
            },
            new MovieBase /* 070 */
            {
                BestPictureWinner = false,
                Duration = 149,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1982, 02, 10, 0, 0, 0, TimeSpan.Zero),
                Title = "Das Boot",
                Year = 1981
            },
            new MovieBase /* 071 */
            {
                BestPictureWinner = false,
                Duration = 102,
                Rating = "NC-17",
                ReleaseDate = new DateTimeOffset(2000, 10, 27, 0, 0, 0, TimeSpan.Zero),
                Title = "Requiem for a Dream",
                Year = 2000
            },
            new MovieBase /* 072 */
            {
                BestPictureWinner = false,
                Duration = 93,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1949, 08, 31, 0, 0, 0, TimeSpan.Zero),
                Title = "The Third Man",
                Year = 1949
            },
            new MovieBase /* 073 */
            {
                BestPictureWinner = false,
                Duration = 126,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1948, 01, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "The Treasure of the Sierra Madre",
                Year = 1948
            },
            new MovieBase /* 074 */
            {
                BestPictureWinner = false,
                Duration = 108,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2004, 03, 19, 0, 0, 0, TimeSpan.Zero),
                Title = "Eternal Sunshine of the Spotless Mind",
                Year = 2004
            },
            new MovieBase /* 075 */
            {
                BestPictureWinner = false,
                Duration = 155,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1990, 02, 23, 0, 0, 0, TimeSpan.Zero),
                Title = "Cinema Paradiso",
                Year = 1988
            },
            new MovieBase /* 076 */
            {
                BestPictureWinner = false,
                Duration = 139,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1984, 05, 23, 0, 0, 0, TimeSpan.Zero),
                Title = "Once Upon a Time in America",
                Year = 1984
            },
            new MovieBase /* 077 */
            {
                BestPictureWinner = false,
                Duration = 130,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1974, 06, 20, 0, 0, 0, TimeSpan.Zero),
                Title = "Chinatown",
                Year = 1974
            },
            new MovieBase /* 078 */
            {
                BestPictureWinner = false,
                Duration = 138,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1997, 09, 19, 0, 0, 0, TimeSpan.Zero),
                Title = "L.A. Confidential",
                Year = 1997
            },
            new MovieBase /* 079 */
            {
                BestPictureWinner = false,
                Duration = 89,
                Rating = "G",
                ReleaseDate = new DateTimeOffset(1994, 06, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "The Lion King",
                Year = 1994
            },
            new MovieBase /* 080 */
            {
                BestPictureWinner = false,
                Duration = 134,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1983, 05, 25, 0, 0, 0, TimeSpan.Zero),
                Title = "Star Wars: Episode VI - Return of the Jedi",
                Year = 1983
            },
            new MovieBase /* 081 */
            {
                BestPictureWinner = false,
                Duration = 116,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1987, 06, 26, 0, 0, 0, TimeSpan.Zero),
                Title = "Full Metal Jacket",
                Year = 1987
            },
            new MovieBase /* 082 */
            {
                BestPictureWinner = false,
                Duration = 91,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1975, 05, 25, 0, 0, 0, TimeSpan.Zero),
                Title = "Monty Python and the Holy Grail",
                Year = 1975
            },
            new MovieBase /* 083 */
            {
                BestPictureWinner = true,
                Duration = 177,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1995, 05, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "Braveheart",
                Year = 1995
            },
            new MovieBase /* 084 */
            {
                BestPictureWinner = false,
                Duration = 103,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1952, 04, 11, 0, 0, 0, TimeSpan.Zero),
                Title = "Singin' in the Rain",
                Year = 1952
            },
            new MovieBase /* 085 */
            {
                BestPictureWinner = false,
                Duration = 120,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2003, 11, 21, 0, 0, 0, TimeSpan.Zero),
                Title = "Oldboy",
                Year = 2003
            },
            new MovieBase /* 086 */
            {
                BestPictureWinner = false,
                Duration = 120,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1959, 03, 29, 0, 0, 0, TimeSpan.Zero),
                Title = "Some Like It Hot",
                Year = 1959
            },
            new MovieBase /* 087 */
            {
                BestPictureWinner = true,
                Duration = 160,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1984, 09, 19, 0, 0, 0, TimeSpan.Zero),
                Title = "Amadeus",
                Year = 1984
            },
            new MovieBase /* 088 */
            {
                BestPictureWinner = false,
                Duration = 114,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1927, 03, 13, 0, 0, 0, TimeSpan.Zero),
                Title = "Metropolis",
                Year = 1927
            },
            new MovieBase /* 089 */
            {
                BestPictureWinner = false,
                Duration = 88,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1951, 12, 26, 0, 0, 0, TimeSpan.Zero),
                Title = "Rashomon",
                Year = 1950
            },
            new MovieBase /* 090 */
            {
                BestPictureWinner = false,
                Duration = 93,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1949, 12, 13, 0, 0, 0, TimeSpan.Zero),
                Title = "Bicycle Thieves",
                Year = 1948
            },
            new MovieBase /* 091 */
            {
                BestPictureWinner = false,
                Duration = 141,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1968, 4, 6, 0, 0, 0, TimeSpan.Zero),
                Title = "2001: A Space Odyssey",
                Year = 1968
            },
            new MovieBase /* 092 */
            {
                BestPictureWinner = true,
                Duration = 131,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1992, 08, 07, 0, 0, 0, TimeSpan.Zero),
                Title = "Unforgiven",
                Year = 1992
            },
            new MovieBase /* 093 */
            {
                BestPictureWinner = true,
                Duration = 138,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1951, 1, 15, 0, 0, 0, TimeSpan.Zero),
                Title = "All About Eve",
                Year = 1950
            },
            new MovieBase /* 094 */
            {
                BestPictureWinner = true,
                Duration = 125,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1960, 9, 16, 0, 0, 0, TimeSpan.Zero),
                Title = "The Apartment",
                Year = 1960
            },
            new MovieBase /* 095 */
            {
                BestPictureWinner = false,
                Duration = 127,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1989, 05, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "Indiana Jones and the Last Crusade",
                Year = 1989
            },
            new MovieBase /* 096 */
            {
                BestPictureWinner = true,
                Duration = 129,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1974, 01, 10, 0, 0, 0, TimeSpan.Zero),
                Title = "The Sting",
                Year = 1973
            },
            new MovieBase /* 097 */
            {
                BestPictureWinner = false,
                Duration = 129,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1980, 12, 19, 0, 0, 0, TimeSpan.Zero),
                Title = "Raging Bull",
                Year = 1980
            },
            new MovieBase /* 098 */
            {
                BestPictureWinner = true,
                Duration = 161,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1957, 12, 14, 0, 0, 0, TimeSpan.Zero),
                Title = "The Bridge on the River Kwai",
                Year = 1957
            },
            new MovieBase /* 099 */
            {
                BestPictureWinner = false,
                Duration = 131,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1988, 07, 15, 0, 0, 0, TimeSpan.Zero),
                Title = "Die Hard",
                Year = 1988
            },
            new MovieBase /* 100 */
            {
                BestPictureWinner = false,
                Duration = 116,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1958, 2, 6, 0, 0, 0, TimeSpan.Zero),
                Title = "Witness for the Prosecution",
                Year = 1957
            },
            new MovieBase /* 101 */
            {
                BestPictureWinner = false,
                Duration = 140,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2005, 06, 15, 0, 0, 0, TimeSpan.Zero),
                Title = "Batman Begins",
                Year = 2005
            },
            new MovieBase /* 102 */
            {
                BestPictureWinner = false,
                Duration = 123,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2011, 03, 16, 0, 0, 0, TimeSpan.Zero),
                Title = "A Separation",
                Year = 2011
            },
            new MovieBase /* 103 */
            {
                BestPictureWinner = false,
                Duration = 89,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1988, 04, 16, 0, 0, 0, TimeSpan.Zero),
                Title = "Grave of the Fireflies",
                Year = 1988
            },
            new MovieBase /* 104 */
            {
                BestPictureWinner = false,
                Duration = 118,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2007, 01, 19, 0, 0, 0, TimeSpan.Zero),
                Title = "Pan's Labyrinth",
                Year = 2006
            },
            new MovieBase /* 105 */
            {
                BestPictureWinner = false,
                Duration = 156,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2004, 09, 16, 0, 0, 0, TimeSpan.Zero),
                Title = "Downfall",
                Year = 2004
            },
            new MovieBase /* 106 */
            {
                BestPictureWinner = false,
                Duration = 129,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1939, 10, 19, 0, 0, 0, TimeSpan.Zero),
                Title = "Mr. Smith Goes to Washington",
                Year = 1939
            },
            new MovieBase /* 107 */
            {
                BestPictureWinner = false,
                Duration = 75,
                Rating = "TV-MA",
                ReleaseDate = new DateTimeOffset(1961, 09, 13, 0, 0, 0, TimeSpan.Zero),
                Title = "Yojimbo",
                Year = 1961
            },
            new MovieBase /* 108 */
            {
                BestPictureWinner = false,
                Duration = 172,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1963, 7, 4, 0, 0, 0, TimeSpan.Zero),
                Title = "The Great Escape",
                Year = 1963
            },
            new MovieBase /* 109 */
            {
                BestPictureWinner = false,
                Duration = 132,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1967, 5, 10, 0, 0, 0, TimeSpan.Zero),
                Title = "For a Few Dollars More",
                Year = 1965
            },
            new MovieBase /* 110 */
            {
                BestPictureWinner = false,
                Duration = 102,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2001, 01, 19, 0, 0, 0, TimeSpan.Zero),
                Title = "Snatch.",
                Year = 2000
            },
            new MovieBase /* 111 */
            {
                BestPictureWinner = false,
                Duration = 153,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2009, 08, 21, 0, 0, 0, TimeSpan.Zero),
                Title = "Inglourious Basterds",
                Year = 2009
            },
            new MovieBase /* 112 */
            {
                BestPictureWinner = true,
                Duration = 108,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1954, 06, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "On the Waterfront",
                Year = 1954
            },
            new MovieBase /* 113 */
            {
                BestPictureWinner = false,
                Duration = 124,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1980, 10, 10, 0, 0, 0, TimeSpan.Zero),
                Title = "The Elephant Man",
                Year = 1980
            },
            new MovieBase /* 114 */
            {
                BestPictureWinner = false,
                Duration = 96,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1958, 10, 13, 0, 0, 0, TimeSpan.Zero),
                Title = "The Seventh Seal",
                Year = 1957
            },
            new MovieBase /* 115 */
            {
                BestPictureWinner = false,
                Duration = 81,
                Rating = "TV-G",
                ReleaseDate = new DateTimeOffset(1995, 11, 22, 0, 0, 0, TimeSpan.Zero),
                Title = "Toy Story",
                Year = 1995
            },
            new MovieBase /* 116 */
            {
                BestPictureWinner = false,
                Duration = 100,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1941, 10, 18, 0, 0, 0, TimeSpan.Zero),
                Title = "The Maltese Falcon",
                Year = 1941
            },
            new MovieBase /* 117 */
            {
                BestPictureWinner = false,
                Duration = 170,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1995, 12, 15, 0, 0, 0, TimeSpan.Zero),
                Title = "Heat",
                Year = 1995
            },
            new MovieBase /* 118 */
            {
                BestPictureWinner = false,
                Duration = 75,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1927, 02, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "The General",
                Year = 1926
            },
            new MovieBase /* 119 */
            {
                BestPictureWinner = false,
                Duration = 116,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2009, 01, 09, 0, 0, 0, TimeSpan.Zero),
                Title = "Gran Torino",
                Year = 2008
            },
            new MovieBase /* 120 */
            {
                BestPictureWinner = true,
                Duration = 130,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1940, 04, 12, 0, 0, 0, TimeSpan.Zero),
                Title = "Rebecca",
                Year = 1940
            },
            new MovieBase /* 121 */
            {
                BestPictureWinner = false,
                Duration = 117,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1982, 06, 25, 0, 0, 0, TimeSpan.Zero),
                Title = "Blade Runner",
                Year = 1982
            },
            new MovieBase /* 122 */
            {
                BestPictureWinner = false,
                Duration = 143,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2012, 05, 04, 0, 0, 0, TimeSpan.Zero),
                Title = "The Avengers",
                Year = 2012
            },
            new MovieBase /* 123 */
            {
                BestPictureWinner = false,
                Duration = 91,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1959, 06, 22, 0, 0, 0, TimeSpan.Zero),
                Title = "Wild Strawberries",
                Year = 1957
            },
            new MovieBase /* 124 */
            {
                BestPictureWinner = false,
                Duration = 98,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1996, 04, 05, 0, 0, 0, TimeSpan.Zero),
                Title = "Fargo",
                Year = 1996
            },
            new MovieBase /* 125 */
            {
                BestPictureWinner = false,
                Duration = 68,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1921, 2, 6, 0, 0, 0, TimeSpan.Zero),
                Title = "The Kid",
                Year = 1921
            },
            new MovieBase /* 126 */
            {
                BestPictureWinner = false,
                Duration = 170,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1983, 12, 09, 0, 0, 0, TimeSpan.Zero),
                Title = "Scarface",
                Year = 1983
            },
            new MovieBase /* 127 */
            {
                BestPictureWinner = false,
                Duration = 108,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(1958, 6, 8, 0, 0, 0, TimeSpan.Zero),
                Title = "Touch of Evil",
                Year = 1958
            },
            new MovieBase /* 128 */
            {
                BestPictureWinner = false,
                Duration = 117,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1998, 03, 06, 0, 0, 0, TimeSpan.Zero),
                Title = "The Big Lebowski",
                Year = 1998
            },
            new MovieBase /* 129 */
            {
                BestPictureWinner = false,
                Duration = 162,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1985, 06, 01, 0, 0, 0, TimeSpan.Zero),
                Title = "Ran",
                Year = 1985
            },
            new MovieBase /* 130 */
            {
                BestPictureWinner = true,
                Duration = 182,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1979, 02, 23, 0, 0, 0, TimeSpan.Zero),
                Title = "The Deer Hunter",
                Year = 1978
            },
            new MovieBase /* 131 */
            {
                BestPictureWinner = false,
                Duration = 126,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1967, 11, 1, 0, 0, 0, TimeSpan.Zero),
                Title = "Cool Hand Luke",
                Year = 1967
            },
            new MovieBase /* 132 */
            {
                BestPictureWinner = false,
                Duration = 147,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2005, 04, 01, 0, 0, 0, TimeSpan.Zero),
                Title = "Sin City",
                Year = 2005
            },
            new MovieBase /* 133 */
            {
                BestPictureWinner = false,
                Duration = 72,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1925, 6, 26, 0, 0, 0, TimeSpan.Zero),
                Title = "The Gold Rush",
                Year = 1925
            },
            new MovieBase /* 134 */
            {
                BestPictureWinner = false,
                Duration = 101,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1951, 06, 30, 0, 0, 0, TimeSpan.Zero),
                Title = "Strangers on a Train",
                Year = 1951
            },
            new MovieBase /* 135 */
            {
                BestPictureWinner = true,
                Duration = 105,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1934, 02, 23, 0, 0, 0, TimeSpan.Zero),
                Title = "It Happened One Night",
                Year = 1934
            },
            new MovieBase /* 136 */
            {
                BestPictureWinner = true,
                Duration = 122,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2007, 11, 21, 0, 0, 0, TimeSpan.Zero),
                Title = "No Country for Old Men",
                Year = 2007
            },
            new MovieBase /* 137 */
            {
                BestPictureWinner = false,
                Duration = 130,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1975, 06, 20, 0, 0, 0, TimeSpan.Zero),
                Title = "Jaws",
                Year = 1975
            },
            new MovieBase /* 138 */
            {
                BestPictureWinner = false,
                Duration = 107,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1999, 03, 05, 0, 0, 0, TimeSpan.Zero),
                Title = "Lock, Stock and Two Smoking Barrels",
                Year = 1998
            },
            new MovieBase /* 139 */
            {
                BestPictureWinner = false,
                Duration = 107,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(1999, 08, 06, 0, 0, 0, TimeSpan.Zero),
                Title = "The Sixth Sense",
                Year = 1999
            },
            new MovieBase /* 140 */
            {
                BestPictureWinner = false,
                Duration = 121,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2005, 02, 04, 0, 0, 0, TimeSpan.Zero),
                Title = "Hotel Rwanda",
                Year = 2004
            },
            new MovieBase /* 141 */
            {
                BestPictureWinner = false,
                Duration = 85,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1952, 07, 30, 0, 0, 0, TimeSpan.Zero),
                Title = "High Noon",
                Year = 1952
            },
            new MovieBase /* 142 */
            {
                BestPictureWinner = true,
                Duration = 120,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1986, 12, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "Platoon",
                Year = 1986
            },
            new MovieBase /* 143 */
            {
                BestPictureWinner = false,
                Duration = 109,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1982, 06, 25, 0, 0, 0, TimeSpan.Zero),
                Title = "The Thing",
                Year = 1982
            },
            new MovieBase /* 144 */
            {
                BestPictureWinner = false,
                Duration = 110,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1969, 10, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "Butch Cassidy and the Sundance Kid",
                Year = 1969
            },
            new MovieBase /* 145 */
            {
                BestPictureWinner = false,
                Duration = 101,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1939, 08, 25, 0, 0, 0, TimeSpan.Zero),
                Title = "The Wizard of Oz",
                Year = 1939
            },
            new MovieBase /* 146 */
            {
                BestPictureWinner = false,
                Duration = 178,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1995, 11, 22, 0, 0, 0, TimeSpan.Zero),
                Title = "Casino",
                Year = 1995
            },
            new MovieBase /* 147 */
            {
                BestPictureWinner = false,
                Duration = 94,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1996, 07, 19, 0, 0, 0, TimeSpan.Zero),
                Title = "Trainspotting",
                Year = 1996
            },
            new MovieBase /* 148 */
            {
                BestPictureWinner = false,
                Duration = 111,
                Rating = "TV-14",
                ReleaseDate = new DateTimeOffset(2003, 10, 10, 0, 0, 0, TimeSpan.Zero),
                Title = "Kill Bill: Vol. 1",
                Year = 2003
            },
            new MovieBase /* 149 */
            {
                BestPictureWinner = false,
                Duration = 140,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2011, 09, 09, 0, 0, 0, TimeSpan.Zero),
                Title = "Warrior",
                Year = 2011
            },
            new MovieBase /* 150 */
            {
                BestPictureWinner = true,
                Duration = 93,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1977, 04, 20, 0, 0, 0, TimeSpan.Zero),
                Title = "Annie Hall",
                Year = 1977
            },
            new MovieBase /* 151 */
            {
                BestPictureWinner = false,
                Duration = 101,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1946, 9, 6, 0, 0, 0, TimeSpan.Zero),
                Title = "Notorious",
                Year = 1946
            },
            new MovieBase /* 152 */
            {
                BestPictureWinner = false,
                Duration = 129,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2009, 08, 13, 0, 0, 0, TimeSpan.Zero),
                Title = "The Secret in Their Eyes",
                Year = 2009
            },
            new MovieBase /* 153 */
            {
                BestPictureWinner = true,
                Duration = 238,
                Rating = "G",
                ReleaseDate = new DateTimeOffset(1940, 01, 17, 0, 0, 0, TimeSpan.Zero),
                Title = "Gone with the Wind",
                Year = 1939
            },
            new MovieBase /* 154 */
            {
                BestPictureWinner = false,
                Duration = 126,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1998, 01, 09, 0, 0, 0, TimeSpan.Zero),
                Title = "Good Will Hunting",
                Year = 1997
            },
            new MovieBase /* 155 */
            {
                BestPictureWinner = true,
                Duration = 118,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2010, 12, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "The King's Speech",
                Year = 2010
            },
            new MovieBase /* 156 */
            {
                BestPictureWinner = false,
                Duration = 129,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1940, 03, 15, 0, 0, 0, TimeSpan.Zero),
                Title = "The Grapes of Wrath",
                Year = 1940
            },
            new MovieBase /* 157 */
            {
                BestPictureWinner = false,
                Duration = 148,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2007, 09, 21, 0, 0, 0, TimeSpan.Zero),
                Title = "Into the Wild",
                Year = 2007
            },
            new MovieBase /* 158 */
            {
                BestPictureWinner = false,
                Duration = 94,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1979, 08, 17, 0, 0, 0, TimeSpan.Zero),
                Title = "Life of Brian",
                Year = 1979
            },
            new MovieBase /* 159 */
            {
                BestPictureWinner = false,
                Duration = 100,
                Rating = "G",
                ReleaseDate = new DateTimeOffset(2003, 05, 30, 0, 0, 0, TimeSpan.Zero),
                Title = "Finding Nemo",
                Year = 2003
            },
            new MovieBase /* 160 */
            {
                BestPictureWinner = false,
                Duration = 132,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2006, 03, 17, 0, 0, 0, TimeSpan.Zero),
                Title = "V for Vendetta",
                Year = 2005
            },
            new MovieBase /* 161 */
            {
                BestPictureWinner = false,
                Duration = 98,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(2010, 03, 26, 0, 0, 0, TimeSpan.Zero),
                Title = "How to Train Your Dragon",
                Year = 2010
            },
            new MovieBase /* 162 */
            {
                BestPictureWinner = false,
                Duration = 86,
                Rating = "G",
                ReleaseDate = new DateTimeOffset(1988, 04, 16, 0, 0, 0, TimeSpan.Zero),
                Title = "My Neighbor Totoro",
                Year = 1988
            },
            new MovieBase /* 163 */
            {
                BestPictureWinner = false,
                Duration = 114,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1946, 08, 31, 0, 0, 0, TimeSpan.Zero),
                Title = "The Big Sleep",
                Year = 1946
            },
            new MovieBase /* 164 */
            {
                BestPictureWinner = false,
                Duration = 105,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1954, 05, 29, 0, 0, 0, TimeSpan.Zero),
                Title = "Dial M for Murder",
                Year = 1954
            },
            new MovieBase /* 165 */
            {
                BestPictureWinner = true,
                Duration = 212,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1960, 03, 30, 0, 0, 0, TimeSpan.Zero),
                Title = "Ben-Hur",
                Year = 1959
            },
            new MovieBase /* 166 */
            {
                BestPictureWinner = false,
                Duration = 107,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1984, 10, 26, 0, 0, 0, TimeSpan.Zero),
                Title = "The Terminator",
                Year = 1984
            },
            new MovieBase /* 167 */
            {
                BestPictureWinner = false,
                Duration = 121,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1976, 11, 27, 0, 0, 0, TimeSpan.Zero),
                Title = "Network",
                Year = 1976
            },
            new MovieBase /* 168 */
            {
                BestPictureWinner = true,
                Duration = 132,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2005, 01, 28, 0, 0, 0, TimeSpan.Zero),
                Title = "Million Dollar Baby",
                Year = 2004
            },
            new MovieBase /* 169 */
            {
                BestPictureWinner = false,
                Duration = 108,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2010, 12, 17, 0, 0, 0, TimeSpan.Zero),
                Title = "Black Swan",
                Year = 2010
            },
            new MovieBase /* 170 */
            {
                BestPictureWinner = false,
                Duration = 93,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1955, 11, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "The Night of the Hunter",
                Year = 1955
            },
            new MovieBase /* 171 */
            {
                BestPictureWinner = false,
                Duration = 158,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2008, 01, 25, 0, 0, 0, TimeSpan.Zero),
                Title = "There Will Be Blood",
                Year = 2007
            },
            new MovieBase /* 172 */
            {
                BestPictureWinner = false,
                Duration = 89,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1986, 08, 08, 0, 0, 0, TimeSpan.Zero),
                Title = "Stand by Me",
                Year = 1986
            },
            new MovieBase /* 173 */
            {
                BestPictureWinner = false,
                Duration = 113,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2002, 01, 30, 0, 0, 0, TimeSpan.Zero),
                Title = "Donnie Darko",
                Year = 2001
            },
            new MovieBase /* 174 */
            {
                BestPictureWinner = false,
                Duration = 101,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1993, 02, 12, 0, 0, 0, TimeSpan.Zero),
                Title = "Groundhog Day",
                Year = 1993
            },
            new MovieBase /* 175 */
            {
                BestPictureWinner = false,
                Duration = 125,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1975, 09, 21, 0, 0, 0, TimeSpan.Zero),
                Title = "Dog Day Afternoon",
                Year = 1975
            },
            new MovieBase /* 176 */
            {
                BestPictureWinner = false,
                Duration = 129,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1996, 01, 05, 0, 0, 0, TimeSpan.Zero),
                Title = "Twelve Monkeys",
                Year = 1995
            },
            new MovieBase /* 177 */
            {
                BestPictureWinner = false,
                Duration = 154,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2000, 06, 16, 0, 0, 0, TimeSpan.Zero),
                Title = "Amores Perros",
                Year = 2000
            },
            new MovieBase /* 178 */
            {
                BestPictureWinner = false,
                Duration = 115,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2007, 08, 03, 0, 0, 0, TimeSpan.Zero),
                Title = "The Bourne Ultimatum",
                Year = 2007
            },
            new MovieBase /* 179 */
            {
                BestPictureWinner = false,
                Duration = 92,
                Rating = null,
                ReleaseDate = new DateTimeOffset(2009, 04, 09, 0, 0, 0, TimeSpan.Zero),
                Title = "Mary and Max",
                Year = 2009
            },
            new MovieBase /* 180 */
            {
                BestPictureWinner = false,
                Duration = 99,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1959, 11, 16, 0, 0, 0, TimeSpan.Zero),
                Title = "The 400 Blows",
                Year = 1959
            },
            new MovieBase /* 181 */
            {
                BestPictureWinner = false,
                Duration = 83,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1967, 03, 16, 0, 0, 0, TimeSpan.Zero),
                Title = "Persona",
                Year = 1966
            },
            new MovieBase /* 182 */
            {
                BestPictureWinner = false,
                Duration = 106,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1967, 12, 22, 0, 0, 0, TimeSpan.Zero),
                Title = "The Graduate",
                Year = 1967
            },
            new MovieBase /* 183 */
            {
                BestPictureWinner = true,
                Duration = 191,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1983, 02, 25, 0, 0, 0, TimeSpan.Zero),
                Title = "Gandhi",
                Year = 1982
            },
            new MovieBase /* 184 */
            {
                BestPictureWinner = false,
                Duration = 85,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1956, 6, 6, 0, 0, 0, TimeSpan.Zero),
                Title = "The Killing",
                Year = 1956
            },
            new MovieBase /* 185 */
            {
                BestPictureWinner = false,
                Duration = 119,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(2005, 06, 17, 0, 0, 0, TimeSpan.Zero),
                Title = "Howl's Moving Castle",
                Year = 2004
            },
            new MovieBase /* 186 */
            {
                BestPictureWinner = true,
                Duration = 100,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2012, 01, 20, 0, 0, 0, TimeSpan.Zero),
                Title = "The Artist",
                Year = 2011
            },
            new MovieBase /* 187 */
            {
                BestPictureWinner = false,
                Duration = 98,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1987, 09, 25, 0, 0, 0, TimeSpan.Zero),
                Title = "The Princess Bride",
                Year = 1987
            },
            new MovieBase /* 188 */
            {
                BestPictureWinner = false,
                Duration = 120,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2012, 10, 12, 0, 0, 0, TimeSpan.Zero),
                Title = "Argo",
                Year = 2012
            },
            new MovieBase /* 189 */
            {
                BestPictureWinner = true,
                Duration = 120,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2009, 01, 23, 0, 0, 0, TimeSpan.Zero),
                Title = "Slumdog Millionaire",
                Year = 2008
            },
            new MovieBase /* 190 */
            {
                BestPictureWinner = false,
                Duration = 131,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1966, 06, 22, 0, 0, 0, TimeSpan.Zero),
                Title = "Who's Afraid of Virginia Woolf?",
                Year = 1966
            },
            new MovieBase /* 191 */
            {
                BestPictureWinner = false,
                Duration = 108,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1956, 07, 16, 0, 0, 0, TimeSpan.Zero),
                Title = "La Strada",
                Year = 1954
            },
            new MovieBase /* 192 */
            {
                BestPictureWinner = false,
                Duration = 126,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1962, 10, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "The Manchurian Candidate",
                Year = 1962
            },
            new MovieBase /* 193 */
            {
                BestPictureWinner = false,
                Duration = 134,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1961, 09, 25, 0, 0, 0, TimeSpan.Zero),
                Title = "The Hustler",
                Year = 1961
            },
            new MovieBase /* 194 */
            {
                BestPictureWinner = true,
                Duration = 135,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2002, 01, 04, 0, 0, 0, TimeSpan.Zero),
                Title = "A Beautiful Mind",
                Year = 2001
            },
            new MovieBase /* 195 */
            {
                BestPictureWinner = false,
                Duration = 145,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1969, 06, 18, 0, 0, 0, TimeSpan.Zero),
                Title = "The Wild Bunch",
                Year = 1969
            },
            new MovieBase /* 196 */
            {
                BestPictureWinner = true,
                Duration = 119,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1976, 12, 03, 0, 0, 0, TimeSpan.Zero),
                Title = "Rocky",
                Year = 1976
            },
            new MovieBase /* 197 */
            {
                BestPictureWinner = false,
                Duration = 160,
                Rating = "TV-PG",
                ReleaseDate = new DateTimeOffset(1959, 9, 1, 0, 0, 0, TimeSpan.Zero),
                Title = "Anatomy of a Murder",
                Year = 1959
            },
            new MovieBase /* 198 */
            {
                BestPictureWinner = false,
                Duration = 120,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1953, 8, 10, 0, 0, 0, TimeSpan.Zero),
                Title = "Stalag 17",
                Year = 1953
            },
            new MovieBase /* 199 */
            {
                BestPictureWinner = false,
                Duration = 122,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1974, 03, 16, 0, 0, 0, TimeSpan.Zero),
                Title = "The Exorcist",
                Year = 1973
            },
            new MovieBase /* 200 */
            {
                BestPictureWinner = false,
                Duration = 138,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1972, 12, 10, 0, 0, 0, TimeSpan.Zero),
                Title = "Sleuth",
                Year = 1972
            },
            new MovieBase /* 201 */
            {
                BestPictureWinner = false,
                Duration = 80,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1948, 8, 28, 0, 0, 0, TimeSpan.Zero),
                Title = "Rope",
                Year = 1948
            },
            new MovieBase /* 202 */
            {
                BestPictureWinner = false,
                Duration = 184,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1975, 12, 18, 0, 0, 0, TimeSpan.Zero),
                Title = "Barry Lyndon",
                Year = 1975
            },
            new MovieBase /* 203 */
            {
                BestPictureWinner = false,
                Duration = 123,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1962, 4, 22, 0, 0, 0, TimeSpan.Zero),
                Title = "The Man Who Shot Liberty Valance",
                Year = 1962
            },
            new MovieBase /* 204 */
            {
                BestPictureWinner = false,
                Duration = 112,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2009, 08, 14, 0, 0, 0, TimeSpan.Zero),
                Title = "District 9",
                Year = 2009
            },
            new MovieBase /* 205 */
            {
                BestPictureWinner = false,
                Duration = 163,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1980, 04, 17, 0, 0, 0, TimeSpan.Zero),
                Title = "Stalker",
                Year = 1979
            },
            new MovieBase /* 206 */
            {
                BestPictureWinner = false,
                Duration = 101,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2002, 12, 12, 0, 0, 0, TimeSpan.Zero),
                Title = "Infernal Affairs",
                Year = 2002
            },
            new MovieBase /* 207 */
            {
                BestPictureWinner = false,
                Duration = 118,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1953, 9, 2, 0, 0, 0, TimeSpan.Zero),
                Title = "Roman Holiday",
                Year = 1953
            },
            new MovieBase /* 208 */
            {
                BestPictureWinner = false,
                Duration = 103,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1998, 06, 05, 0, 0, 0, TimeSpan.Zero),
                Title = "The Truman Show",
                Year = 1998
            },
            new MovieBase /* 209 */
            {
                BestPictureWinner = false,
                Duration = 111,
                Rating = "G",
                ReleaseDate = new DateTimeOffset(2007, 06, 29, 0, 0, 0, TimeSpan.Zero),
                Title = "Ratatouille",
                Year = 2007
            },
            new MovieBase /* 210 */
            {
                BestPictureWinner = false,
                Duration = 143,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2003, 07, 09, 0, 0, 0, TimeSpan.Zero),
                Title = "Pirates of the Caribbean: The Curse of the Black Pearl",
                Year = 2003
            },
            new MovieBase /* 211 */
            {
                BestPictureWinner = false,
                Duration = 106,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2008, 12, 12, 0, 0, 0, TimeSpan.Zero),
                Title = "Ip Man",
                Year = 2008
            },
            new MovieBase /* 212 */
            {
                BestPictureWinner = false,
                Duration = 112,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2007, 05, 23, 0, 0, 0, TimeSpan.Zero),
                Title = "The Diving Bell and the Butterfly",
                Year = 2007
            },
            new MovieBase /* 213 */
            {
                BestPictureWinner = false,
                Duration = 130,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2011, 07, 15, 0, 0, 0, TimeSpan.Zero),
                Title = "Harry Potter and the Deathly Hallows: Part 2",
                Year = 2011
            },
            new MovieBase /* 214 */
            {
                BestPictureWinner = false,
                Duration = 99,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1967, 01, 18, 0, 0, 0, TimeSpan.Zero),
                Title = "A Fistful of Dollars",
                Year = 1964
            },
            new MovieBase /* 215 */
            {
                BestPictureWinner = false,
                Duration = 125,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1951, 12, 1, 0, 0, 0, TimeSpan.Zero),
                Title = "A Streetcar Named Desire",
                Year = 1951
            },
            new MovieBase /* 216 */
            {
                BestPictureWinner = false,
                Duration = 92,
                Rating = "G",
                ReleaseDate = new DateTimeOffset(2001, 11, 02, 0, 0, 0, TimeSpan.Zero),
                Title = "Monsters, Inc.",
                Year = 2001
            },
            new MovieBase /* 217 */
            {
                BestPictureWinner = false,
                Duration = 133,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1994, 02, 25, 0, 0, 0, TimeSpan.Zero),
                Title = "In the Name of the Father",
                Year = 1993
            },
            new MovieBase /* 218 */
            {
                BestPictureWinner = false,
                Duration = 127,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2009, 05, 08, 0, 0, 0, TimeSpan.Zero),
                Title = "Star Trek",
                Year = 2009
            },
            new MovieBase /* 219 */
            {
                BestPictureWinner = false,
                Duration = 84,
                Rating = "G",
                ReleaseDate = new DateTimeOffset(1991, 11, 22, 0, 0, 0, TimeSpan.Zero),
                Title = "Beauty and the Beast",
                Year = 1991
            },
            new MovieBase /* 220 */
            {
                BestPictureWinner = false,
                Duration = 136,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1968, 06, 12, 0, 0, 0, TimeSpan.Zero),
                Title = "Rosemary's Baby",
                Year = 1968
            },
            new MovieBase /* 221 */
            {
                BestPictureWinner = false,
                Duration = 104,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1950, 10, 13, 0, 0, 0, TimeSpan.Zero),
                Title = "Harvey",
                Year = 1950
            },
            new MovieBase /* 222 */ {
                BestPictureWinner = false,
                Duration = 117,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1984, 3, 11, 0, 0, 0, TimeSpan.Zero),
                Title = "Nauticaä of the Valley of the Wind",
                Year = 1984
            },
            new MovieBase /* 223 */
            {
                BestPictureWinner = false,
                Duration = 109,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2009, 01, 30, 0, 0, 0, TimeSpan.Zero),
                Title = "The Wrestler",
                Year = 2008
            },
            new MovieBase /* 224 */
            {
                BestPictureWinner = true,
                Duration = 133,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1930, 08, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "All Quiet on the Western Front",
                Year = 1930
            },
            new MovieBase /* 225 */
            {
                BestPictureWinner = false,
                Duration = 98,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1996, 02, 23, 0, 0, 0, TimeSpan.Zero),
                Title = "La Haine",
                Year = 1995
            },
            new MovieBase /* 226 */
            {
                BestPictureWinner = true,
                Duration = 133,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1988, 12, 16, 0, 0, 0, TimeSpan.Zero),
                Title = "Rain Man",
                Year = 1988
            },
            new MovieBase /* 227 */
            {
                BestPictureWinner = false,
                Duration = 66,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1925, 12, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "Battleship Potemkin",
                Year = 1925
            },
            new MovieBase /* 228 */
            {
                BestPictureWinner = false,
                Duration = 138,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2010, 02, 19, 0, 0, 0, TimeSpan.Zero),
                Title = "Shutter Island",
                Year = 2010
            },
            new MovieBase /* 229 */
            {
                BestPictureWinner = false,
                Duration = 81,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1929, 6, 3, 0, 0, 0, TimeSpan.Zero),
                Title = "Nosferatu",
                Year = 1922
            },
            new MovieBase /* 230 */
            {
                BestPictureWinner = false,
                Duration = 103,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2003, 09, 19, 0, 0, 0, TimeSpan.Zero),
                Title = "Spring, Summer, Fall, Winter... and Spring",
                Year = 2003
            },
            new MovieBase /* 231 */
            {
                BestPictureWinner = false,
                Duration = 96,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1979, 04, 25, 0, 0, 0, TimeSpan.Zero),
                Title = "Manhattan",
                Year = 1979
            },
            new MovieBase /* 232 */
            {
                BestPictureWinner = false,
                Duration = 138,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2003, 10, 15, 0, 0, 0, TimeSpan.Zero),
                Title = "Mystic River",
                Year = 2003
            },
            new MovieBase /* 233 */
            {
                BestPictureWinner = false,
                Duration = 102,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1938, 2, 18, 0, 0, 0, TimeSpan.Zero),
                Title = "Bringing Up Baby",
                Year = 1938
            },
            new MovieBase /* 234 */
            {
                BestPictureWinner = false,
                Duration = 108,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1943, 1, 15, 0, 0, 0, TimeSpan.Zero),
                Title = "Shadow of a Doubt",
                Year = 1943
            },
            new MovieBase /* 235 */
            {
                BestPictureWinner = false,
                Duration = 125,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2004, 01, 09, 0, 0, 0, TimeSpan.Zero),
                Title = "Big Fish",
                Year = 2003
            },
            new MovieBase /* 236 */
            {
                BestPictureWinner = false,
                Duration = 124,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1986, 08, 02, 0, 0, 0, TimeSpan.Zero),
                Title = "Castle in the Sky",
                Year = 1986
            },
            new MovieBase /* 237 */
            {
                BestPictureWinner = false,
                Duration = 151,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1973, 12, 16, 0, 0, 0, TimeSpan.Zero),
                Title = "Papillon",
                Year = 1973
            },
            new MovieBase /* 238 */
            {
                BestPictureWinner = false,
                Duration = 76,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(1993, 10, 29, 0, 0, 0, TimeSpan.Zero),
                Title = "The Nightmare Before Christmas",
                Year = 1993
            },
            new MovieBase /* 239 */
            {
                BestPictureWinner = false,
                Duration = 119,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(1987, 06, 03, 0, 0, 0, TimeSpan.Zero),
                Title = "The Untouchables",
                Year = 1987
            },
            new MovieBase /* 240 */
            {
                BestPictureWinner = false,
                Duration = 127,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(1993, 06, 11, 0, 0, 0, TimeSpan.Zero),
                Title = "Jurassic Park",
                Year = 1993
            },
            new MovieBase /* 241 */
            {
                BestPictureWinner = false,
                Duration = 115,
                Rating = "R",
                ReleaseDate = new DateTimeOffset(2008, 10, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "Let the Right One In",
                Year = 2008
            },
            new MovieBase /* 242 */
            {
                BestPictureWinner = true,
                Duration = 109,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1967, 10, 14, 0, 0, 0, TimeSpan.Zero),
                Title = "In the Heat of the Night",
                Year = 1967
            },
            new MovieBase /* 243 */
            {
                BestPictureWinner = false,
                Duration = 170,
                Rating = "PG-13",
                ReleaseDate = new DateTimeOffset(2009, 12, 24, 0, 0, 0, TimeSpan.Zero),
                Title = "3 Idiots",
                Year = 2009
            },
            new MovieBase /* 244 */
            {
                BestPictureWinner = false,
                Duration = 118,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1944, 9, 23, 0, 0, 0, TimeSpan.Zero),
                Title = "Arsenic and Old Lace",
                Year = 1944
            },
            new MovieBase /* 245 */
            {
                BestPictureWinner = false,
                Duration = 119,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1956, 3, 13, 0, 0, 0, TimeSpan.Zero),
                Title = "The Searchers",
                Year = 1956
            },
            new MovieBase /* 246 */
            {
                BestPictureWinner = false,
                Duration = 98,
                Rating = "PG",
                ReleaseDate = new DateTimeOffset(2000, 09, 29, 0, 0, 0, TimeSpan.Zero),
                Title = "In the Mood for Love",
                Year = 2000
            },
            new MovieBase /* 247 */
            {
                BestPictureWinner = false,
                Duration = 141,
                Rating = null,
                ReleaseDate = new DateTimeOffset(1959, 4, 4, 0, 0, 0, TimeSpan.Zero),
                Title = "Rio Bravo",
                Year = 1959
            }
        };
    }
}
