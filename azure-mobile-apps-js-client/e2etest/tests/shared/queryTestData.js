// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="../testFramework.js" />

function createQueryTestData() {
    var movies = [];

    movies.push({
        title: "The Shawshank Redemption",
        year: 1994,
        duration: 142,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1994, 9, 14)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Godfather",
        year: 1972,
        duration: 175,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1972, 2, 24)),
        bestPictureWinner: true
    });
    movies.push({
        title: "The Godfather: Part II",
        year: 1974,
        duration: 200,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1974, 11, 20)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Pulp Fiction",
        year: 1994,
        duration: 168,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1994, 9, 14)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Good, the Bad and the Ugly",
        year: 1966,
        duration: 161,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1967, 11, 29)),
        bestPictureWinner: false
    });
    movies.push({
        title: "12 Angry Men",
        year: 1957,
        duration: 96,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1957, 3, 10)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Dark Knight",
        year: 2008,
        duration: 152,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2008, 6, 18)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Schindler's List",
        year: 1993,
        duration: 195,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1993, 11, 15)),
        bestPictureWinner: true
    });
    movies.push({
        title: "The Lord of the Rings: The Return of the King",
        year: 2003,
        duration: 201,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2003, 11, 17)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Fight Club",
        year: 1999,
        duration: 139,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1999, 9, 15)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Star Wars: Episode V - The Empire Strikes Back",
        year: 1980,
        duration: 127,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1980, 4, 21)),
        bestPictureWinner: false
    });
    movies.push({
        title: "One Flew Over the Cuckoo's Nest",
        year: 1975,
        duration: 133,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1975, 10, 21)),
        bestPictureWinner: true
    });
    movies.push({
        title: "The Lord of the Rings: The Fellowship of the Ring",
        year: 2001,
        duration: 178,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2001, 11, 19)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Inception",
        year: 2010,
        duration: 148,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2010, 6, 16)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Goodfellas",
        year: 1990,
        duration: 146,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1990, 8, 19)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Star Wars",
        year: 1977,
        duration: 121,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1977, 4, 25)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Seven Samurai",
        year: 1954,
        duration: 141,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1956, 10, 19)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Matrix",
        year: 1999,
        duration: 136,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1999, 2, 31)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Forrest Gump",
        year: 1994,
        duration: 142,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(1994, 6, 06)),
        bestPictureWinner: true
    });
    movies.push({
        title: "City of God",
        year: 2002,
        duration: 130,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2002, 0, 01)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Lord of the Rings: The Two Towers",
        year: 2002,
        duration: 179,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2002, 11, 18)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Once Upon a Time in the West",
        year: 1968,
        duration: 175,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(1968, 11, 21)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Se7en",
        year: 1995,
        duration: 127,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1995, 8, 22)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Silence of the Lambs",
        year: 1991,
        duration: 118,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1991, 1, 14)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Casablanca",
        year: 1942,
        duration: 102,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1943, 0, 23)),
        bestPictureWinner: true
    });
    movies.push({
        title: "The Usual Suspects",
        year: 1995,
        duration: 106,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1995, 7, 16)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Raiders of the Lost Ark",
        year: 1981,
        duration: 115,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1981, 5, 12)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Rear Window",
        year: 1954,
        duration: 112,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1955, 0, 13)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Psycho",
        year: 1960,
        duration: 109,
        mpaaRating: "TV-14",
        releaseDate: new Date(Date.UTC(1960, 8, 8)),
        bestPictureWinner: false
    });
    movies.push({
        title: "It's a Wonderful Life",
        year: 1946,
        duration: 130,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1947, 0, 06)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Léon: The Professional",
        year: 1994,
        duration: 110,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1994, 10, 18)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Sunset Blvd.",
        year: 1950,
        duration: 110,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1950, 7, 25)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Memento",
        year: 2000,
        duration: 113,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2000, 9, 11)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Dark Knight Rises",
        year: 2012,
        duration: 165,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2012, 6, 20)),
        bestPictureWinner: false
    });
    movies.push({
        title: "American History X",
        year: 1998,
        duration: 119,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1999, 1, 12)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Apocalypse Now",
        year: 1979,
        duration: 153,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1979, 7, 15)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Terminator 2: Judgment Day",
        year: 1991,
        duration: 152,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1991, 6, 03)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Dr. Strangelove or: How I Learned to Stop Worrying and Love the Bomb",
        year: 1964,
        duration: 95,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1964, 0, 29)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Saving Private Ryan",
        year: 1998,
        duration: 169,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1998, 6, 24)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Alien",
        year: 1979,
        duration: 117,
        mpaaRating: "TV-14",
        releaseDate: new Date(Date.UTC(1979, 4, 25)),
        bestPictureWinner: false
    });
    movies.push({
        title: "North by Northwest",
        year: 1959,
        duration: 136,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1959, 8, 26)),
        bestPictureWinner: false
    });
    movies.push({
        title: "City Lights",
        year: 1931,
        duration: 87,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1931, 2, 7)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Spirited Away",
        year: 2001,
        duration: 125,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(2001, 6, 20)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Citizen Kane",
        year: 1941,
        duration: 119,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1941, 8, 5)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Modern Times",
        year: 1936,
        duration: 87,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1936, 1, 25)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Shining",
        year: 1980,
        duration: 142,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1980, 4, 23)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Vertigo",
        year: 1958,
        duration: 129,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1958, 6, 21)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Back to the Future",
        year: 1985,
        duration: 116,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1985, 6, 03)),
        bestPictureWinner: false
    });
    movies.push({
        title: "American Beauty",
        year: 1999,
        duration: 122,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1999, 9, 01)),
        bestPictureWinner: true
    });
    movies.push({
        title: "M",
        year: 1931,
        duration: 117,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1931, 7, 30)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Pianist",
        year: 2002,
        duration: 150,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2003, 2, 28)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Departed",
        year: 2006,
        duration: 151,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2006, 9, 06)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Taxi Driver",
        year: 1976,
        duration: 113,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1976, 1, 08)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Toy Story 3",
        year: 2010,
        duration: 103,
        mpaaRating: "G",
        releaseDate: new Date(Date.UTC(2010, 5, 18)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Paths of Glory",
        year: 1957,
        duration: 88,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1957, 9, 25)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Life Is Beautiful",
        year: 1997,
        duration: 118,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(1999, 1, 12)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Double Indemnity",
        year: 1944,
        duration: 107,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1944, 3, 24)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Aliens",
        year: 1986,
        duration: 154,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1986, 6, 18)),
        bestPictureWinner: false
    });
    movies.push({
        title: "WALL-E",
        year: 2008,
        duration: 98,
        mpaaRating: "G",
        releaseDate: new Date(Date.UTC(2008, 5, 27)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Lives of Others",
        year: 2006,
        duration: 137,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2006, 2, 23)),
        bestPictureWinner: false
    });
    movies.push({
        title: "A Clockwork Orange",
        year: 1971,
        duration: 136,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1972, 1, 02)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Amélie",
        year: 2001,
        duration: 122,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2001, 3, 24)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Gladiator",
        year: 2000,
        duration: 155,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2000, 4, 05)),
        bestPictureWinner: true
    });
    movies.push({
        title: "The Green Mile",
        year: 1999,
        duration: 189,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1999, 11, 10)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Intouchables",
        year: 2011,
        duration: 112,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2011, 10, 02)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Lawrence of Arabia",
        year: 1962,
        duration: 227,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1963, 0, 30)),
        bestPictureWinner: true
    });
    movies.push({
        title: "To Kill a Mockingbird",
        year: 1962,
        duration: 129,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1963, 2, 16)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Prestige",
        year: 2006,
        duration: 130,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2006, 9, 20)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Great Dictator",
        year: 1940,
        duration: 125,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1941, 2, 7)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Reservoir Dogs",
        year: 1992,
        duration: 99,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1992, 9, 23)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Das Boot",
        year: 1981,
        duration: 149,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1982, 1, 10)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Requiem for a Dream",
        year: 2000,
        duration: 102,
        mpaaRating: "NC-17",
        releaseDate: new Date(Date.UTC(2000, 9, 27)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Third Man",
        year: 1949,
        duration: 93,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1949, 7, 31)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Treasure of the Sierra Madre",
        year: 1948,
        duration: 126,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1948, 0, 24)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Eternal Sunshine of the Spotless Mind",
        year: 2004,
        duration: 108,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2004, 2, 19)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Cinema Paradiso",
        year: 1988,
        duration: 155,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1990, 1, 23)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Once Upon a Time in America",
        year: 1984,
        duration: 139,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1984, 4, 23)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Chinatown",
        year: 1974,
        duration: 130,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1974, 5, 20)),
        bestPictureWinner: false
    });
    movies.push({
        title: "L.A. Confidential",
        year: 1997,
        duration: 138,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1997, 8, 19)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Lion King",
        year: 1994,
        duration: 89,
        mpaaRating: "G",
        releaseDate: new Date(Date.UTC(1994, 5, 24)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Star Wars: Episode VI - Return of the Jedi",
        year: 1983,
        duration: 134,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1983, 4, 25)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Full Metal Jacket",
        year: 1987,
        duration: 116,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1987, 5, 26)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Monty Python and the Holy Grail",
        year: 1975,
        duration: 91,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1975, 4, 25)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Braveheart",
        year: 1995,
        duration: 177,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1995, 4, 24)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Singin' in the Rain",
        year: 1952,
        duration: 103,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1952, 3, 11)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Oldboy",
        year: 2003,
        duration: 120,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2003, 10, 21)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Some Like It Hot",
        year: 1959,
        duration: 120,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1959, 2, 29)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Amadeus",
        year: 1984,
        duration: 160,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1984, 8, 19)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Metropolis",
        year: 1927,
        duration: 114,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1927, 2, 13)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Rashomon",
        year: 1950,
        duration: 88,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1951, 11, 26)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Bicycle Thieves",
        year: 1948,
        duration: 93,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1949, 11, 13)),
        bestPictureWinner: false
    });
    movies.push({
        title: "2001: A Space Odyssey",
        year: 1968,
        duration: 141,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1968, 3, 6)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Unforgiven",
        year: 1992,
        duration: 131,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1992, 7, 07)),
        bestPictureWinner: true
    });
    movies.push({
        title: "All About Eve",
        year: 1950,
        duration: 138,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1951, 0, 15)),
        bestPictureWinner: true
    });
    movies.push({
        title: "The Apartment",
        year: 1960,
        duration: 125,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1960, 8, 16)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Indiana Jones and the Last Crusade",
        year: 1989,
        duration: 127,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1989, 4, 24)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Sting",
        year: 1973,
        duration: 129,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1974, 0, 10)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Raging Bull",
        year: 1980,
        duration: 129,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1980, 11, 19)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Bridge on the River Kwai",
        year: 1957,
        duration: 161,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1957, 11, 14)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Die Hard",
        year: 1988,
        duration: 131,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1988, 6, 15)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Witness for the Prosecution",
        year: 1957,
        duration: 116,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1958, 1, 6)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Batman Begins",
        year: 2005,
        duration: 140,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2005, 5, 15)),
        bestPictureWinner: false
    });
    movies.push({
        title: "A Separation",
        year: 2011,
        duration: 123,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2011, 2, 16)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Grave of the Fireflies",
        year: 1988,
        duration: 89,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1988, 3, 16)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Pan's Labyrinth",
        year: 2006,
        duration: 118,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2007, 0, 19)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Downfall",
        year: 2004,
        duration: 156,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2004, 8, 16)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Mr. Smith Goes to Washington",
        year: 1939,
        duration: 129,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1939, 9, 19)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Yojimbo",
        year: 1961,
        duration: 75,
        mpaaRating: "TV-MA",
        releaseDate: new Date(Date.UTC(1961, 8, 13)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Great Escape",
        year: 1963,
        duration: 172,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1963, 6, 4)),
        bestPictureWinner: false
    });
    movies.push({
        title: "For a Few Dollars More",
        year: 1965,
        duration: 132,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1967, 4, 10)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Snatch.",
        year: 2000,
        duration: 102,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2001, 0, 19)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Inglourious Basterds",
        year: 2009,
        duration: 153,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2009, 7, 21)),
        bestPictureWinner: false
    });
    movies.push({
        title: "On the Waterfront",
        year: 1954,
        duration: 108,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1954, 5, 24)),
        bestPictureWinner: true
    });
    movies.push({
        title: "The Elephant Man",
        year: 1980,
        duration: 124,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1980, 9, 10)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Seventh Seal",
        year: 1957,
        duration: 96,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1958, 9, 13)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Toy Story",
        year: 1995,
        duration: 81,
        mpaaRating: "TV-G",
        releaseDate: new Date(Date.UTC(1995, 10, 22)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Maltese Falcon",
        year: 1941,
        duration: 100,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1941, 9, 18)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Heat",
        year: 1995,
        duration: 170,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1995, 11, 15)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The General",
        year: 1926,
        duration: 75,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1927, 1, 24)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Gran Torino",
        year: 2008,
        duration: 116,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2009, 0, 09)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Rebecca",
        year: 1940,
        duration: 130,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1940, 3, 12)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Blade Runner",
        year: 1982,
        duration: 117,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1982, 5, 25)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Avengers",
        year: 2012,
        duration: 143,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2012, 4, 04)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Wild Strawberries",
        year: 1957,
        duration: 91,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1959, 5, 22)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Fargo",
        year: 1996,
        duration: 98,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1996, 3, 05)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Kid",
        year: 1921,
        duration: 68,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1921, 1, 6)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Scarface",
        year: 1983,
        duration: 170,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1983, 11, 09)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Touch of Evil",
        year: 1958,
        duration: 108,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(1958, 5, 8)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Big Lebowski",
        year: 1998,
        duration: 117,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1998, 2, 06)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Ran",
        year: 1985,
        duration: 162,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1985, 5, 01)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Deer Hunter",
        year: 1978,
        duration: 182,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1979, 1, 23)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Cool Hand Luke",
        year: 1967,
        duration: 126,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1967, 10, 1)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Sin City",
        year: 2005,
        duration: 147,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2005, 3, 01)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Gold Rush",
        year: 1925,
        duration: 72,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1925, 5, 26)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Strangers on a Train",
        year: 1951,
        duration: 101,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1951, 5, 30)),
        bestPictureWinner: false
    });
    movies.push({
        title: "It Happened One Night",
        year: 1934,
        duration: 105,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1934, 1, 23)),
        bestPictureWinner: true
    });
    movies.push({
        title: "No Country for Old Men",
        year: 2007,
        duration: 122,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2007, 10, 21)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Jaws",
        year: 1975,
        duration: 130,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1975, 5, 20)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Lock, Stock and Two Smoking Barrels",
        year: 1998,
        duration: 107,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1999, 2, 05)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Sixth Sense",
        year: 1999,
        duration: 107,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(1999, 7, 06)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Hotel Rwanda",
        year: 2004,
        duration: 121,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2005, 1, 04)),
        bestPictureWinner: false
    });
    movies.push({
        title: "High Noon",
        year: 1952,
        duration: 85,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1952, 6, 30)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Platoon",
        year: 1986,
        duration: 120,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1986, 11, 24)),
        bestPictureWinner: true
    });
    movies.push({
        title: "The Thing",
        year: 1982,
        duration: 109,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1982, 5, 25)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Butch Cassidy and the Sundance Kid",
        year: 1969,
        duration: 110,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1969, 9, 24)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Wizard of Oz",
        year: 1939,
        duration: 101,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1939, 7, 25)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Casino",
        year: 1995,
        duration: 178,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1995, 10, 22)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Trainspotting",
        year: 1996,
        duration: 94,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1996, 6, 19)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Kill Bill: Vol. 1",
        year: 2003,
        duration: 111,
        mpaaRating: "TV-14",
        releaseDate: new Date(Date.UTC(2003, 9, 10)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Warrior",
        year: 2011,
        duration: 140,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2011, 8, 09)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Annie Hall",
        year: 1977,
        duration: 93,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1977, 3, 20)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Notorious",
        year: 1946,
        duration: 101,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1946, 8, 6)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Secret in Their Eyes",
        year: 2009,
        duration: 129,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2009, 7, 13)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Gone with the Wind",
        year: 1939,
        duration: 238,
        mpaaRating: "G",
        releaseDate: new Date(Date.UTC(1940, 0, 17)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Good Will Hunting",
        year: 1997,
        duration: 126,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1998, 0, 09)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The King's Speech",
        year: 2010,
        duration: 118,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2010, 11, 24)),
        bestPictureWinner: true
    });
    movies.push({
        title: "The Grapes of Wrath",
        year: 1940,
        duration: 129,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1940, 2, 15)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Into the Wild",
        year: 2007,
        duration: 148,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2007, 8, 21)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Life of Brian",
        year: 1979,
        duration: 94,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1979, 7, 17)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Finding Nemo",
        year: 2003,
        duration: 100,
        mpaaRating: "G",
        releaseDate: new Date(Date.UTC(2003, 4, 30)),
        bestPictureWinner: false
    });
    movies.push({
        title: "V for Vendetta",
        year: 2005,
        duration: 132,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2006, 2, 17)),
        bestPictureWinner: false
    });
    movies.push({
        title: "How to Train Your Dragon",
        year: 2010,
        duration: 98,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(2010, 2, 26)),
        bestPictureWinner: false
    });
    movies.push({
        title: "My Neighbor Totoro",
        year: 1988,
        duration: 86,
        mpaaRating: "G",
        releaseDate: new Date(Date.UTC(1988, 3, 16)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Big Sleep",
        year: 1946,
        duration: 114,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1946, 7, 31)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Dial M for Murder",
        year: 1954,
        duration: 105,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1954, 4, 29)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Ben-Hur",
        year: 1959,
        duration: 212,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1960, 2, 30)),
        bestPictureWinner: true
    });
    movies.push({
        title: "The Terminator",
        year: 1984,
        duration: 107,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1984, 9, 26)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Network",
        year: 1976,
        duration: 121,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1976, 10, 27)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Million Dollar Baby",
        year: 2004,
        duration: 132,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2005, 0, 28)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Black Swan",
        year: 2010,
        duration: 108,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2010, 11, 17)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Night of the Hunter",
        year: 1955,
        duration: 93,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1955, 10, 24)),
        bestPictureWinner: false
    });
    movies.push({
        title: "There Will Be Blood",
        year: 2007,
        duration: 158,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2008, 0, 25)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Stand by Me",
        year: 1986,
        duration: 89,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1986, 7, 08)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Donnie Darko",
        year: 2001,
        duration: 113,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2002, 0, 30)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Groundhog Day",
        year: 1993,
        duration: 101,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1993, 1, 12)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Dog Day Afternoon",
        year: 1975,
        duration: 125,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1975, 8, 21)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Twelve Monkeys",
        year: 1995,
        duration: 129,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1996, 0, 05)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Amores Perros",
        year: 2000,
        duration: 154,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2000, 5, 16)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Bourne Ultimatum",
        year: 2007,
        duration: 115,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2007, 7, 03)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Mary and Max",
        year: 2009,
        duration: 92,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(2009, 3, 09)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The 400 Blows",
        year: 1959,
        duration: 99,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1959, 10, 16)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Persona",
        year: 1966,
        duration: 83,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1967, 2, 16)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Graduate",
        year: 1967,
        duration: 106,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1967, 11, 22)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Gandhi",
        year: 1982,
        duration: 191,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1983, 1, 25)),
        bestPictureWinner: true
    });
    movies.push({
        title: "The Killing",
        year: 1956,
        duration: 85,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1956, 5, 6)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Howl's Moving Castle",
        year: 2004,
        duration: 119,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(2005, 5, 17)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Artist",
        year: 2011,
        duration: 100,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2012, 0, 20)),
        bestPictureWinner: true
    });
    movies.push({
        title: "The Princess Bride",
        year: 1987,
        duration: 98,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1987, 8, 25)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Argo",
        year: 2012,
        duration: 120,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2012, 9, 12)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Slumdog Millionaire",
        year: 2008,
        duration: 120,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2009, 0, 23)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Who's Afraid of Virginia Woolf?",
        year: 1966,
        duration: 131,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1966, 5, 22)),
        bestPictureWinner: false
    });
    movies.push({
        title: "La Strada",
        year: 1954,
        duration: 108,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1956, 6, 16)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Manchurian Candidate",
        year: 1962,
        duration: 126,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1962, 9, 24)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Hustler",
        year: 1961,
        duration: 134,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1961, 8, 25)),
        bestPictureWinner: false
    });
    movies.push({
        title: "A Beautiful Mind",
        year: 2001,
        duration: 135,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2002, 0, 04)),
        bestPictureWinner: true
    });
    movies.push({
        title: "The Wild Bunch",
        year: 1969,
        duration: 145,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1969, 5, 18)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Rocky",
        year: 1976,
        duration: 119,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1976, 11, 03)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Anatomy of a Murder",
        year: 1959,
        duration: 160,
        mpaaRating: "TV-PG",
        releaseDate: new Date(Date.UTC(1959, 8, 1)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Stalag 17",
        year: 1953,
        duration: 120,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1953, 7, 10)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Exorcist",
        year: 1973,
        duration: 122,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1974, 2, 16)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Sleuth",
        year: 1972,
        duration: 138,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1972, 11, 10)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Rope",
        year: 1948,
        duration: 80,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1948, 7, 28)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Barry Lyndon",
        year: 1975,
        duration: 184,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1975, 11, 18)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Man Who Shot Liberty Valance",
        year: 1962,
        duration: 123,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1962, 3, 22)),
        bestPictureWinner: false
    });
    movies.push({
        title: "District 9",
        year: 2009,
        duration: 112,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2009, 7, 14)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Stalker",
        year: 1979,
        duration: 163,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1980, 3, 17)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Infernal Affairs",
        year: 2002,
        duration: 101,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2002, 11, 12)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Roman Holiday",
        year: 1953,
        duration: 118,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1953, 8, 2)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Truman Show",
        year: 1998,
        duration: 103,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1998, 5, 05)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Ratatouille",
        year: 2007,
        duration: 111,
        mpaaRating: "G",
        releaseDate: new Date(Date.UTC(2007, 5, 29)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Pirates of the Caribbean: The Curse of the Black Pearl",
        year: 2003,
        duration: 143,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2003, 6, 09)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Ip Man",
        year: 2008,
        duration: 106,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2008, 11, 12)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Diving Bell and the Butterfly",
        year: 2007,
        duration: 112,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2007, 4, 23)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Harry Potter and the Deathly Hallows: Part 2",
        year: 2011,
        duration: 130,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2011, 6, 15)),
        bestPictureWinner: false
    });
    movies.push({
        title: "A Fistful of Dollars",
        year: 1964,
        duration: 99,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1967, 0, 18)),
        bestPictureWinner: false
    });
    movies.push({
        title: "A Streetcar Named Desire",
        year: 1951,
        duration: 125,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1951, 11, 1)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Monsters, Inc.",
        year: 2001,
        duration: 92,
        mpaaRating: "G",
        releaseDate: new Date(Date.UTC(2001, 10, 02)),
        bestPictureWinner: false
    });
    movies.push({
        title: "In the Name of the Father",
        year: 1993,
        duration: 133,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1994, 1, 25)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Star Trek",
        year: 2009,
        duration: 127,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2009, 4, 08)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Beauty and the Beast",
        year: 1991,
        duration: 84,
        mpaaRating: "G",
        releaseDate: new Date(Date.UTC(1991, 10, 22)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Rosemary's Baby",
        year: 1968,
        duration: 136,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1968, 5, 12)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Harvey",
        year: 1950,
        duration: 104,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1950, 9, 13)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Nauticaä of the Valley of the Wind",
        year: 1984,
        duration: 117,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1984, 2, 11)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Wrestler",
        year: 2008,
        duration: 109,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2009, 0, 30)),
        bestPictureWinner: false
    });
    movies.push({
        title: "All Quiet on the Western Front",
        year: 1930,
        duration: 133,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1930, 7, 24)),
        bestPictureWinner: true
    });
    movies.push({
        title: "La Haine",
        year: 1995,
        duration: 98,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1996, 1, 23)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Rain Man",
        year: 1988,
        duration: 133,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1988, 11, 16)),
        bestPictureWinner: true
    });
    movies.push({
        title: "Battleship Potemkin",
        year: 1925,
        duration: 66,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1925, 11, 24)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Shutter Island",
        year: 2010,
        duration: 138,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2010, 1, 19)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Nosferatu",
        year: 1922,
        duration: 81,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1929, 5, 3)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Spring, Summer, Fall, Winter... and Spring",
        year: 2003,
        duration: 103,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2003, 8, 19)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Manhattan",
        year: 1979,
        duration: 96,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1979, 3, 25)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Mystic River",
        year: 2003,
        duration: 138,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2003, 9, 15)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Bringing Up Baby",
        year: 1938,
        duration: 102,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1938, 1, 18)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Shadow of a Doubt",
        year: 1943,
        duration: 108,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1943, 0, 15)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Big Fish",
        year: 2003,
        duration: 125,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2004, 0, 09)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Castle in the Sky",
        year: 1986,
        duration: 124,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1986, 7, 02)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Papillon",
        year: 1973,
        duration: 151,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1973, 11, 16)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Nightmare Before Christmas",
        year: 1993,
        duration: 76,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(1993, 9, 29)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Untouchables",
        year: 1987,
        duration: 119,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(1987, 5, 03)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Jurassic Park",
        year: 1993,
        duration: 127,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(1993, 5, 11)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Let the Right One In",
        year: 2008,
        duration: 115,
        mpaaRating: "R",
        releaseDate: new Date(Date.UTC(2008, 9, 24)),
        bestPictureWinner: false
    });
    movies.push({
        title: "In the Heat of the Night",
        year: 1967,
        duration: 109,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1967, 9, 14)),
        bestPictureWinner: true
    });
    movies.push({
        title: "3 Idiots",
        year: 2009,
        duration: 170,
        mpaaRating: "PG-13",
        releaseDate: new Date(Date.UTC(2009, 11, 24)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Arsenic and Old Lace",
        year: 1944,
        duration: 118,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1944, 8, 23)),
        bestPictureWinner: false
    });
    movies.push({
        title: "The Searchers",
        year: 1956,
        duration: 119,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1956, 2, 13)),
        bestPictureWinner: false
    });
    movies.push({
        title: "In the Mood for Love",
        year: 2000,
        duration: 98,
        mpaaRating: "PG",
        releaseDate: new Date(Date.UTC(2000, 8, 29)),
        bestPictureWinner: false
    });
    movies.push({
        title: "Rio Bravo",
        year: 1959,
        duration: 141,
        mpaaRating: null,
        releaseDate: new Date(Date.UTC(1959, 3, 4)),
        bestPictureWinner: false
    });

    return movies;
}

zumo.tests.getQueryTestData = createQueryTestData;
