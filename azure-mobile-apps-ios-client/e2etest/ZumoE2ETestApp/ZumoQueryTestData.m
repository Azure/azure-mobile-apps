// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoQueryTestData.h"
#import "ZumoTestGlobals.h"

@implementation ZumoQueryTestData

+ (NSArray *)getMovies {

    static NSArray *allItems = nil;
    if (!allItems) {
        allItems = @[
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(142),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1994 month:10 day:14],
    @"title": @"The Shawshank Redemption",
    @"year": @(1994)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(175),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1972 month:3 day:24],
    @"title": @"The Godfather",
    @"year": @(1972)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(200),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1974 month:12 day:20],
    @"title": @"The Godfather: Part II",
    @"year": @(1974)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(168),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1994 month:10 day:14],
    @"title": @"Pulp Fiction",
    @"year": @(1994)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(161),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1967 month:12 day:29],
    @"title": @"The Good, the Bad and the Ugly",
    @"year": @(1966)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(96),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1957 month:4 day:10],
    @"title": @"12 Angry Men",
    @"year": @(1957)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(152),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2008 month:7 day:18],
    @"title": @"The Dark Knight",
    @"year": @(2008)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(195),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1993 month:12 day:15],
    @"title": @"Schindler's List",
    @"year": @(1993)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(201),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2003 month:12 day:17],
    @"title": @"The Lord of the Rings: The Return of the King",
    @"year": @(2003)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(139),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1999 month:10 day:15],
    @"title": @"Fight Club",
    @"year": @(1999)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(127),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1980 month:5 day:21],
    @"title": @"Star Wars: Episode V - The Empire Strikes Back",
    @"year": @(1980)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(133),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1975 month:11 day:21],
    @"title": @"One Flew Over the Cuckoo's Nest",
    @"year": @(1975)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(178),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2001 month:12 day:19],
    @"title": @"The Lord of the Rings: The Fellowship of the Ring",
    @"year": @(2001)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(148),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2010 month:7 day:16],
    @"title": @"Inception",
    @"year": @(2010)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(146),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1990 month:9 day:19],
    @"title": @"Goodfellas",
    @"year": @(1990)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(121),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1977 month:5 day:25],
    @"title": @"Star Wars",
    @"year": @(1977)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(141),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1956 month:11 day:19],
    @"title": @"Seven Samurai",
    @"year": @(1954)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(136),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1999 month:3 day:31],
    @"title": @"The Matrix",
    @"year": @(1999)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(142),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1994 month:7 day:6],
    @"title": @"Forrest Gump",
    @"year": @(1994)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(130),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2002 month:1 day:1],
    @"title": @"City of God",
    @"year": @(2002)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(179),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2002 month:12 day:18],
    @"title": @"The Lord of the Rings: The Two Towers",
    @"year": @(2002)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(175),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1968 month:12 day:21],
    @"title": @"Once Upon a Time in the West",
    @"year": @(1968)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(127),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1995 month:9 day:22],
    @"title": @"Se7en",
    @"year": @(1995)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(118),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1991 month:2 day:14],
    @"title": @"The Silence of the Lambs",
    @"year": @(1991)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(102),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1943 month:1 day:23],
    @"title": @"Casablanca",
    @"year": @(1942)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(106),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1995 month:8 day:16],
    @"title": @"The Usual Suspects",
    @"year": @(1995)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(115),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1981 month:6 day:12],
    @"title": @"Raiders of the Lost Ark",
    @"year": @(1981)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(112),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1955 month:1 day:13],
    @"title": @"Rear Window",
    @"year": @(1954)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(109),
    @"mpaaRating": @"TV-14",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1960 month:9 day:8],
    @"title": @"Psycho",
    @"year": @(1960)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(130),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1947 month:1 day:6],
    @"title": @"It's a Wonderful Life",
    @"year": @(1946)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(110),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1994 month:11 day:18],
    @"title": @"Léon: The Professional",
    @"year": @(1994)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(110),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1950 month:8 day:25],
    @"title": @"Sunset Blvd.",
    @"year": @(1950)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(113),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2000 month:10 day:11],
    @"title": @"Memento",
    @"year": @(2000)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(165),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2012 month:7 day:20],
    @"title": @"The Dark Knight Rises",
    @"year": @(2012)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(119),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1999 month:2 day:12],
    @"title": @"American History X",
    @"year": @(1998)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(153),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1979 month:8 day:15],
    @"title": @"Apocalypse Now",
    @"year": @(1979)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(152),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1991 month:7 day:3],
    @"title": @"Terminator 2: Judgment Day",
    @"year": @(1991)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(95),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1964 month:1 day:29],
    @"title": @"Dr. Strangelove or: How I Learned to Stop Worrying and Love the Bomb",
    @"year": @(1964)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(169),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1998 month:7 day:24],
    @"title": @"Saving Private Ryan",
    @"year": @(1998)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(117),
    @"mpaaRating": @"TV-14",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1979 month:5 day:25],
    @"title": @"Alien",
    @"year": @(1979)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(136),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1959 month:9 day:26],
    @"title": @"North by Northwest",
    @"year": @(1959)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(87),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1931 month:3 day:7],
    @"title": @"City Lights",
    @"year": @(1931)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(125),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2001 month:7 day:20],
    @"title": @"Spirited Away",
    @"year": @(2001)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(119),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1941 month:9 day:5],
    @"title": @"Citizen Kane",
    @"year": @(1941)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(87),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1936 month:2 day:25],
    @"title": @"Modern Times",
    @"year": @(1936)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(142),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1980 month:5 day:23],
    @"title": @"The Shining",
    @"year": @(1980)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(129),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1958 month:7 day:21],
    @"title": @"Vertigo",
    @"year": @(1958)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(116),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1985 month:7 day:3],
    @"title": @"Back to the Future",
    @"year": @(1985)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(122),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1999 month:10 day:1],
    @"title": @"American Beauty",
    @"year": @(1999)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(117),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1931 month:8 day:30],
    @"title": @"M",
    @"year": @(1931)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(150),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2003 month:3 day:28],
    @"title": @"The Pianist",
    @"year": @(2002)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(151),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2006 month:10 day:6],
    @"title": @"The Departed",
    @"year": @(2006)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(113),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1976 month:2 day:8],
    @"title": @"Taxi Driver",
    @"year": @(1976)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(103),
    @"mpaaRating": @"G",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2010 month:6 day:18],
    @"title": @"Toy Story 3",
    @"year": @(2010)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(88),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1957 month:10 day:25],
    @"title": @"Paths of Glory",
    @"year": @(1957)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(118),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1999 month:2 day:12],
    @"title": @"Life Is Beautiful",
    @"year": @(1997)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(107),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1944 month:4 day:24],
    @"title": @"Double Indemnity",
    @"year": @(1944)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(154),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1986 month:7 day:18],
    @"title": @"Aliens",
    @"year": @(1986)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(98),
    @"mpaaRating": @"G",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2008 month:6 day:27],
    @"title": @"WALL-E",
    @"year": @(2008)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(137),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2006 month:3 day:23],
    @"title": @"The Lives of Others",
    @"year": @(2006)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(136),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1972 month:2 day:2],
    @"title": @"A Clockwork Orange",
    @"year": @(1971)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(122),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2001 month:4 day:24],
    @"title": @"Amélie",
    @"year": @(2001)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(155),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2000 month:5 day:5],
    @"title": @"Gladiator",
    @"year": @(2000)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(189),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1999 month:12 day:10],
    @"title": @"The Green Mile",
    @"year": @(1999)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(112),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2011 month:11 day:2],
    @"title": @"The Intouchables",
    @"year": @(2011)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(227),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1963 month:1 day:30],
    @"title": @"Lawrence of Arabia",
    @"year": @(1962)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(129),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1963 month:3 day:16],
    @"title": @"To Kill a Mockingbird",
    @"year": @(1962)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(130),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2006 month:10 day:20],
    @"title": @"The Prestige",
    @"year": @(2006)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(125),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1941 month:3 day:7],
    @"title": @"The Great Dictator",
    @"year": @(1940)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(99),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1992 month:10 day:23],
    @"title": @"Reservoir Dogs",
    @"year": @(1992)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(149),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1982 month:2 day:10],
    @"title": @"Das Boot",
    @"year": @(1981)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(102),
    @"mpaaRating": @"NC-17",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2000 month:10 day:27],
    @"title": @"Requiem for a Dream",
    @"year": @(2000)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(93),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1949 month:8 day:31],
    @"title": @"The Third Man",
    @"year": @(1949)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(126),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1948 month:1 day:24],
    @"title": @"The Treasure of the Sierra Madre",
    @"year": @(1948)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(108),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2004 month:3 day:19],
    @"title": @"Eternal Sunshine of the Spotless Mind",
    @"year": @(2004)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(155),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1990 month:2 day:23],
    @"title": @"Cinema Paradiso",
    @"year": @(1988)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(139),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1984 month:5 day:23],
    @"title": @"Once Upon a Time in America",
    @"year": @(1984)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(130),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1974 month:6 day:20],
    @"title": @"Chinatown",
    @"year": @(1974)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(138),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1997 month:9 day:19],
    @"title": @"L.A. Confidential",
    @"year": @(1997)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(89),
    @"mpaaRating": @"G",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1994 month:6 day:24],
    @"title": @"The Lion King",
    @"year": @(1994)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(134),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1983 month:5 day:25],
    @"title": @"Star Wars: Episode VI - Return of the Jedi",
    @"year": @(1983)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(116),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1987 month:6 day:26],
    @"title": @"Full Metal Jacket",
    @"year": @(1987)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(91),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1975 month:5 day:25],
    @"title": @"Monty Python and the Holy Grail",
    @"year": @(1975)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(177),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1995 month:5 day:24],
    @"title": @"Braveheart",
    @"year": @(1995)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(103),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1952 month:4 day:11],
    @"title": @"Singin' in the Rain",
    @"year": @(1952)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(120),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2003 month:11 day:21],
    @"title": @"Oldboy",
    @"year": @(2003)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(120),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1959 month:3 day:29],
    @"title": @"Some Like It Hot",
    @"year": @(1959)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(160),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1984 month:9 day:19],
    @"title": @"Amadeus",
    @"year": @(1984)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(114),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1927 month:3 day:13],
    @"title": @"Metropolis",
    @"year": @(1927)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(88),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1951 month:12 day:26],
    @"title": @"Rashomon",
    @"year": @(1950)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(93),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1949 month:12 day:13],
    @"title": @"Bicycle Thieves",
    @"year": @(1948)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(141),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1968 month:4 day:6],
    @"title": @"2001: A Space Odyssey",
    @"year": @(1968)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(131),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1992 month:8 day:7],
    @"title": @"Unforgiven",
    @"year": @(1992)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(138),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1951 month:1 day:15],
    @"title": @"All About Eve",
    @"year": @(1950)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(125),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1960 month:9 day:16],
    @"title": @"The Apartment",
    @"year": @(1960)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(127),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1989 month:5 day:24],
    @"title": @"Indiana Jones and the Last Crusade",
    @"year": @(1989)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(129),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1974 month:1 day:10],
    @"title": @"The Sting",
    @"year": @(1973)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(129),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1980 month:12 day:19],
    @"title": @"Raging Bull",
    @"year": @(1980)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(161),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1957 month:12 day:14],
    @"title": @"The Bridge on the River Kwai",
    @"year": @(1957)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(131),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1988 month:7 day:15],
    @"title": @"Die Hard",
    @"year": @(1988)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(116),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1958 month:2 day:6],
    @"title": @"Witness for the Prosecution",
    @"year": @(1957)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(140),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2005 month:6 day:15],
    @"title": @"Batman Begins",
    @"year": @(2005)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(123),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2011 month:3 day:16],
    @"title": @"A Separation",
    @"year": @(2011)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(89),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1988 month:4 day:16],
    @"title": @"Grave of the Fireflies",
    @"year": @(1988)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(118),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2007 month:1 day:19],
    @"title": @"Pan's Labyrinth",
    @"year": @(2006)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(156),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2004 month:9 day:16],
    @"title": @"Downfall",
    @"year": @(2004)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(129),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1939 month:10 day:19],
    @"title": @"Mr. Smith Goes to Washington",
    @"year": @(1939)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(75),
    @"mpaaRating": @"TV-MA",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1961 month:9 day:13],
    @"title": @"Yojimbo",
    @"year": @(1961)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(172),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1963 month:7 day:4],
    @"title": @"The Great Escape",
    @"year": @(1963)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(132),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1967 month:5 day:10],
    @"title": @"For a Few Dollars More",
    @"year": @(1965)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(102),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2001 month:1 day:19],
    @"title": @"Snatch.",
    @"year": @(2000)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(153),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2009 month:8 day:21],
    @"title": @"Inglourious Basterds",
    @"year": @(2009)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(108),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1954 month:6 day:24],
    @"title": @"On the Waterfront",
    @"year": @(1954)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(124),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1980 month:10 day:10],
    @"title": @"The Elephant Man",
    @"year": @(1980)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(96),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1958 month:10 day:13],
    @"title": @"The Seventh Seal",
    @"year": @(1957)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(81),
    @"mpaaRating": @"TV-G",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1995 month:11 day:22],
    @"title": @"Toy Story",
    @"year": @(1995)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(100),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1941 month:10 day:18],
    @"title": @"The Maltese Falcon",
    @"year": @(1941)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(170),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1995 month:12 day:15],
    @"title": @"Heat",
    @"year": @(1995)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(75),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1927 month:2 day:24],
    @"title": @"The General",
    @"year": @(1926)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(116),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2009 month:1 day:9],
    @"title": @"Gran Torino",
    @"year": @(2008)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(130),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1940 month:4 day:12],
    @"title": @"Rebecca",
    @"year": @(1940)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(117),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1982 month:6 day:25],
    @"title": @"Blade Runner",
    @"year": @(1982)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(143),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2012 month:5 day:4],
    @"title": @"The Avengers",
    @"year": @(2012)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(91),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1959 month:6 day:22],
    @"title": @"Wild Strawberries",
    @"year": @(1957)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(98),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1996 month:4 day:5],
    @"title": @"Fargo",
    @"year": @(1996)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(68),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1921 month:2 day:6],
    @"title": @"The Kid",
    @"year": @(1921)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(170),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1983 month:12 day:9],
    @"title": @"Scarface",
    @"year": @(1983)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(108),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1958 month:6 day:8],
    @"title": @"Touch of Evil",
    @"year": @(1958)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(117),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1998 month:3 day:6],
    @"title": @"The Big Lebowski",
    @"year": @(1998)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(162),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1985 month:6 day:1],
    @"title": @"Ran",
    @"year": @(1985)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(182),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1979 month:2 day:23],
    @"title": @"The Deer Hunter",
    @"year": @(1978)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(126),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1967 month:11 day:1],
    @"title": @"Cool Hand Luke",
    @"year": @(1967)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(147),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2005 month:4 day:1],
    @"title": @"Sin City",
    @"year": @(2005)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(72),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1925 month:6 day:26],
    @"title": @"The Gold Rush",
    @"year": @(1925)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(101),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1951 month:6 day:30],
    @"title": @"Strangers on a Train",
    @"year": @(1951)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(105),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1934 month:2 day:23],
    @"title": @"It Happened One Night",
    @"year": @(1934)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(122),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2007 month:11 day:21],
    @"title": @"No Country for Old Men",
    @"year": @(2007)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(130),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1975 month:6 day:20],
    @"title": @"Jaws",
    @"year": @(1975)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(107),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1999 month:3 day:5],
    @"title": @"Lock, Stock and Two Smoking Barrels",
    @"year": @(1998)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(107),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1999 month:8 day:6],
    @"title": @"The Sixth Sense",
    @"year": @(1999)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(121),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2005 month:2 day:4],
    @"title": @"Hotel Rwanda",
    @"year": @(2004)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(85),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1952 month:7 day:30],
    @"title": @"High Noon",
    @"year": @(1952)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(120),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1986 month:12 day:24],
    @"title": @"Platoon",
    @"year": @(1986)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(109),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1982 month:6 day:25],
    @"title": @"The Thing",
    @"year": @(1982)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(110),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1969 month:10 day:24],
    @"title": @"Butch Cassidy and the Sundance Kid",
    @"year": @(1969)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(101),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1939 month:8 day:25],
    @"title": @"The Wizard of Oz",
    @"year": @(1939)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(178),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1995 month:11 day:22],
    @"title": @"Casino",
    @"year": @(1995)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(94),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1996 month:7 day:19],
    @"title": @"Trainspotting",
    @"year": @(1996)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(111),
    @"mpaaRating": @"TV-14",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2003 month:10 day:10],
    @"title": @"Kill Bill: Vol. 1",
    @"year": @(2003)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(140),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2011 month:9 day:9],
    @"title": @"Warrior",
    @"year": @(2011)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(93),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1977 month:4 day:20],
    @"title": @"Annie Hall",
    @"year": @(1977)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(101),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1946 month:9 day:6],
    @"title": @"Notorious",
    @"year": @(1946)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(129),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2009 month:8 day:13],
    @"title": @"The Secret in Their Eyes",
    @"year": @(2009)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(238),
    @"mpaaRating": @"G",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1940 month:1 day:17],
    @"title": @"Gone with the Wind",
    @"year": @(1939)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(126),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1998 month:1 day:9],
    @"title": @"Good Will Hunting",
    @"year": @(1997)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(118),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2010 month:12 day:24],
    @"title": @"The King's Speech",
    @"year": @(2010)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(129),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1940 month:3 day:15],
    @"title": @"The Grapes of Wrath",
    @"year": @(1940)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(148),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2007 month:9 day:21],
    @"title": @"Into the Wild",
    @"year": @(2007)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(94),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1979 month:8 day:17],
    @"title": @"Life of Brian",
    @"year": @(1979)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(100),
    @"mpaaRating": @"G",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2003 month:5 day:30],
    @"title": @"Finding Nemo",
    @"year": @(2003)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(132),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2006 month:3 day:17],
    @"title": @"V for Vendetta",
    @"year": @(2005)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(98),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2010 month:3 day:26],
    @"title": @"How to Train Your Dragon",
    @"year": @(2010)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(86),
    @"mpaaRating": @"G",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1988 month:4 day:16],
    @"title": @"My Neighbor Totoro",
    @"year": @(1988)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(114),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1946 month:8 day:31],
    @"title": @"The Big Sleep",
    @"year": @(1946)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(105),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1954 month:5 day:29],
    @"title": @"Dial M for Murder",
    @"year": @(1954)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(212),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1960 month:3 day:30],
    @"title": @"Ben-Hur",
    @"year": @(1959)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(107),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1984 month:10 day:26],
    @"title": @"The Terminator",
    @"year": @(1984)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(121),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1976 month:11 day:27],
    @"title": @"Network",
    @"year": @(1976)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(132),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2005 month:1 day:28],
    @"title": @"Million Dollar Baby",
    @"year": @(2004)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(108),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2010 month:12 day:17],
    @"title": @"Black Swan",
    @"year": @(2010)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(93),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1955 month:11 day:24],
    @"title": @"The Night of the Hunter",
    @"year": @(1955)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(158),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2008 month:1 day:25],
    @"title": @"There Will Be Blood",
    @"year": @(2007)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(89),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1986 month:8 day:8],
    @"title": @"Stand by Me",
    @"year": @(1986)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(113),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2002 month:1 day:30],
    @"title": @"Donnie Darko",
    @"year": @(2001)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(101),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1993 month:2 day:12],
    @"title": @"Groundhog Day",
    @"year": @(1993)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(125),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1975 month:9 day:21],
    @"title": @"Dog Day Afternoon",
    @"year": @(1975)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(129),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1996 month:1 day:5],
    @"title": @"Twelve Monkeys",
    @"year": @(1995)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(154),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2000 month:6 day:16],
    @"title": @"Amores Perros",
    @"year": @(2000)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(115),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2007 month:8 day:3],
    @"title": @"The Bourne Ultimatum",
    @"year": @(2007)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(92),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2009 month:4 day:9],
    @"title": @"Mary and Max",
    @"year": @(2009)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(99),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1959 month:11 day:16],
    @"title": @"The 400 Blows",
    @"year": @(1959)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(83),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1967 month:3 day:16],
    @"title": @"Persona",
    @"year": @(1966)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(106),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1967 month:12 day:22],
    @"title": @"The Graduate",
    @"year": @(1967)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(191),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1983 month:2 day:25],
    @"title": @"Gandhi",
    @"year": @(1982)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(85),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1956 month:6 day:6],
    @"title": @"The Killing",
    @"year": @(1956)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(119),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2005 month:6 day:17],
    @"title": @"Howl's Moving Castle",
    @"year": @(2004)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(100),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2012 month:1 day:20],
    @"title": @"The Artist",
    @"year": @(2011)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(98),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1987 month:9 day:25],
    @"title": @"The Princess Bride",
    @"year": @(1987)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(120),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2012 month:10 day:12],
    @"title": @"Argo",
    @"year": @(2012)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(120),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2009 month:1 day:23],
    @"title": @"Slumdog Millionaire",
    @"year": @(2008)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(131),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1966 month:6 day:22],
    @"title": @"Who's Afraid of Virginia Woolf?",
    @"year": @(1966)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(108),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1956 month:7 day:16],
    @"title": @"La Strada",
    @"year": @(1954)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(126),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1962 month:10 day:24],
    @"title": @"The Manchurian Candidate",
    @"year": @(1962)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(134),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1961 month:9 day:25],
    @"title": @"The Hustler",
    @"year": @(1961)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(135),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2002 month:1 day:4],
    @"title": @"A Beautiful Mind",
    @"year": @(2001)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(145),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1969 month:6 day:18],
    @"title": @"The Wild Bunch",
    @"year": @(1969)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(119),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1976 month:12 day:3],
    @"title": @"Rocky",
    @"year": @(1976)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(160),
    @"mpaaRating": @"TV-PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1959 month:9 day:1],
    @"title": @"Anatomy of a Murder",
    @"year": @(1959)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(120),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1953 month:8 day:10],
    @"title": @"Stalag 17",
    @"year": @(1953)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(122),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1974 month:3 day:16],
    @"title": @"The Exorcist",
    @"year": @(1973)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(138),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1972 month:12 day:10],
    @"title": @"Sleuth",
    @"year": @(1972)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(80),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1948 month:8 day:28],
    @"title": @"Rope",
    @"year": @(1948)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(184),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1975 month:12 day:18],
    @"title": @"Barry Lyndon",
    @"year": @(1975)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(123),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1962 month:4 day:22],
    @"title": @"The Man Who Shot Liberty Valance",
    @"year": @(1962)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(112),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2009 month:8 day:14],
    @"title": @"District 9",
    @"year": @(2009)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(163),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1980 month:4 day:17],
    @"title": @"Stalker",
    @"year": @(1979)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(101),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2002 month:12 day:12],
    @"title": @"Infernal Affairs",
    @"year": @(2002)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(118),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1953 month:9 day:2],
    @"title": @"Roman Holiday",
    @"year": @(1953)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(103),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1998 month:6 day:5],
    @"title": @"The Truman Show",
    @"year": @(1998)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(111),
    @"mpaaRating": @"G",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2007 month:6 day:29],
    @"title": @"Ratatouille",
    @"year": @(2007)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(143),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2003 month:7 day:9],
    @"title": @"Pirates of the Caribbean: The Curse of the Black Pearl",
    @"year": @(2003)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(106),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2008 month:12 day:12],
    @"title": @"Ip Man",
    @"year": @(2008)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(112),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2007 month:5 day:23],
    @"title": @"The Diving Bell and the Butterfly",
    @"year": @(2007)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(130),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2011 month:7 day:15],
    @"title": @"Harry Potter and the Deathly Hallows: Part 2",
    @"year": @(2011)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(99),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1967 month:1 day:18],
    @"title": @"A Fistful of Dollars",
    @"year": @(1964)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(125),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1951 month:12 day:1],
    @"title": @"A Streetcar Named Desire",
    @"year": @(1951)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(92),
    @"mpaaRating": @"G",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2001 month:11 day:2],
    @"title": @"Monsters, Inc.",
    @"year": @(2001)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(133),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1994 month:2 day:25],
    @"title": @"In the Name of the Father",
    @"year": @(1993)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(127),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2009 month:5 day:8],
    @"title": @"Star Trek",
    @"year": @(2009)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(84),
    @"mpaaRating": @"G",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1991 month:11 day:22],
    @"title": @"Beauty and the Beast",
    @"year": @(1991)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(136),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1968 month:6 day:12],
    @"title": @"Rosemary's Baby",
    @"year": @(1968)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(104),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1950 month:10 day:13],
    @"title": @"Harvey",
    @"year": @(1950)
    },
    @{
        @"bestPictureWinner": @NO,
        @"duration": @(117),
        @"mpaaRating": @"PG",
        @"releaseDate": [ZumoTestGlobals createDateWithYear:1984 month:3 day:11],
        @"title": @"Nauticaä of the Valley of the Wind",
        @"year": @(1984)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(109),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2009 month:1 day:30],
    @"title": @"The Wrestler",
    @"year": @(2008)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(133),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1930 month:8 day:24],
    @"title": @"All Quiet on the Western Front",
    @"year": @(1930)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(98),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1996 month:2 day:23],
    @"title": @"La Haine",
    @"year": @(1995)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(133),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1988 month:12 day:16],
    @"title": @"Rain Man",
    @"year": @(1988)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(66),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1925 month:12 day:24],
    @"title": @"Battleship Potemkin",
    @"year": @(1925)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(138),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2010 month:2 day:19],
    @"title": @"Shutter Island",
    @"year": @(2010)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(81),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1929 month:6 day:3],
    @"title": @"Nosferatu",
    @"year": @(1922)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(103),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2003 month:9 day:19],
    @"title": @"Spring, Summer, Fall, Winter... and Spring",
    @"year": @(2003)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(96),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1979 month:4 day:25],
    @"title": @"Manhattan",
    @"year": @(1979)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(138),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2003 month:10 day:15],
    @"title": @"Mystic River",
    @"year": @(2003)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(102),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1938 month:2 day:18],
    @"title": @"Bringing Up Baby",
    @"year": @(1938)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(108),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1943 month:1 day:15],
    @"title": @"Shadow of a Doubt",
    @"year": @(1943)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(125),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2004 month:1 day:9],
    @"title": @"Big Fish",
    @"year": @(2003)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(124),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1986 month:8 day:2],
    @"title": @"Castle in the Sky",
    @"year": @(1986)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(151),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1973 month:12 day:16],
    @"title": @"Papillon",
    @"year": @(1973)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(76),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1993 month:10 day:29],
    @"title": @"The Nightmare Before Christmas",
    @"year": @(1993)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(119),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1987 month:6 day:3],
    @"title": @"The Untouchables",
    @"year": @(1987)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(127),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1993 month:6 day:11],
    @"title": @"Jurassic Park",
    @"year": @(1993)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(115),
    @"mpaaRating": @"R",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2008 month:10 day:24],
    @"title": @"Let the Right One In",
    @"year": @(2008)
    },
    @{
    @"bestPictureWinner": @YES,
    @"duration": @(109),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1967 month:10 day:14],
    @"title": @"In the Heat of the Night",
    @"year": @(1967)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(170),
    @"mpaaRating": @"PG-13",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2009 month:12 day:24],
    @"title": @"3 Idiots",
    @"year": @(2009)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(118),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1944 month:9 day:23],
    @"title": @"Arsenic and Old Lace",
    @"year": @(1944)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(119),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1956 month:3 day:13],
    @"title": @"The Searchers",
    @"year": @(1956)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(98),
    @"mpaaRating": @"PG",
    @"releaseDate": [ZumoTestGlobals createDateWithYear:2000 month:9 day:29],
    @"title": @"In the Mood for Love",
    @"year": @(2000)
    },
    @{
    @"bestPictureWinner": @NO,
    @"duration": @(141),
    @"mpaaRating": [NSNull null],
    @"releaseDate": [ZumoTestGlobals createDateWithYear:1959 month:4 day:4],
    @"title": @"Rio Bravo",
    @"year": @(1959)
    }
    ];
    }

    return allItems;
    
}
@end
