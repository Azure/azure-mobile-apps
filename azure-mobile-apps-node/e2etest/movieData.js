module.exports = {
    stringId: function () {
        return movies.map(function (movie, index) {
            return {
                id: 'Movie ' + ('00' + (index)).slice(-3),
                bestPictureWinner: movie.bestPictureWinner,
                duration: movie.duration,
                mpaaRating: movie.mpaaRating,
                releaseDate: movie.releaseDate,
                title: movie.title,
                year: movie.year
            };
        });
    },
    intId: function () {
        return movies;
    }
};

var movies = [
    {
        bestPictureWinner: false,
        duration: 142,
        mpaaRating: "R",
        releaseDate: releaseDate(1994, 10, 14),
        title: "The Shawshank Redemption",
        year: 1994
    }, {
        bestPictureWinner: true,
        duration: 175,
        mpaaRating: "R",
        releaseDate: releaseDate(1972, 03, 24),
        title: "The Godfather",
        year: 1972
    }, {
        bestPictureWinner: true,
        duration: 200,
        mpaaRating: "R",
        releaseDate: releaseDate(1974, 12, 20),
        title: "The Godfather: Part II",
        year: 1974
    }, {
        bestPictureWinner: false,
        duration: 168,
        mpaaRating: "R",
        releaseDate: releaseDate(1994, 10, 14),
        title: "Pulp Fiction",
        year: 1994
    }, {
        bestPictureWinner: false,
        duration: 161,
        mpaaRating: null,
        releaseDate: releaseDate(1967, 12, 29),
        title: "The Good, the Bad and the Ugly",
        year: 1966
    }, {
        bestPictureWinner: false,
        duration: 96,
        mpaaRating: null,
        releaseDate: releaseDate(1957, 4, 10),
        title: "12 Angry Men",
        year: 1957
    }, {
        bestPictureWinner: false,
        duration: 152,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2008, 07, 18),
        title: "The Dark Knight",
        year: 2008
    }, {
        bestPictureWinner: true,
        duration: 195,
        mpaaRating: "R",
        releaseDate: releaseDate(1993, 12, 15),
        title: "Schindler's List",
        year: 1993
    }, {
        bestPictureWinner: true,
        duration: 201,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2003, 12, 17),
        title: "The Lord of the Rings: The Return of the King",
        year: 2003
    }, {
        bestPictureWinner: false,
        duration: 139,
        mpaaRating: "R",
        releaseDate: releaseDate(1999, 10, 15),
        title: "Fight Club",
        year: 1999
    }, {
        bestPictureWinner: false,
        duration: 127,
        mpaaRating: "PG",
        releaseDate: releaseDate(1980, 05, 21),
        title: "Star Wars: Episode V - The Empire Strikes Back",
        year: 1980
    }, {
        bestPictureWinner: true,
        duration: 133,
        mpaaRating: null,
        releaseDate: releaseDate(1975, 11, 21),
        title: "One Flew Over the Cuckoo's Nest",
        year: 1975
    }, {
        bestPictureWinner: false,
        duration: 178,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2001, 12, 19),
        title: "The Lord of the Rings: The Fellowship of the Ring",
        year: 2001
    }, {
        bestPictureWinner: false,
        duration: 148,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2010, 07, 16),
        title: "Inception",
        year: 2010
    }, {
        bestPictureWinner: false,
        duration: 146,
        mpaaRating: "R",
        releaseDate: releaseDate(1990, 09, 19),
        title: "Goodfellas",
        year: 1990
    }, {
        bestPictureWinner: false,
        duration: 121,
        mpaaRating: "PG",
        releaseDate: releaseDate(1977, 05, 25),
        title: "Star Wars",
        year: 1977
    }, {
        bestPictureWinner: false,
        duration: 141,
        mpaaRating: null,
        releaseDate: releaseDate(1956, 11, 19),
        title: "Seven Samurai",
        year: 1954
    }, {
        bestPictureWinner: false,
        duration: 136,
        mpaaRating: "R",
        releaseDate: releaseDate(1999, 03, 31),
        title: "The Matrix",
        year: 1999
    }, {
        bestPictureWinner: true,
        duration: 142,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(1994, 07, 06),
        title: "Forrest Gump",
        year: 1994
    }, {
        bestPictureWinner: false,
        duration: 130,
        mpaaRating: "R",
        releaseDate: releaseDate(2002, 01, 01),
        title: "City of God",
        year: 2002
    }, {
        bestPictureWinner: false,
        duration: 179,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2002, 12, 18),
        title: "The Lord of the Rings: The Two Towers",
        year: 2002
    }, {
        bestPictureWinner: false,
        duration: 175,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(1968, 12, 21),
        title: "Once Upon a Time in the West",
        year: 1968
    }, {
        bestPictureWinner: false,
        duration: 127,
        mpaaRating: "R",
        releaseDate: releaseDate(1995, 09, 22),
        title: "Se7en",
        year: 1995
    }, {
        bestPictureWinner: true,
        duration: 118,
        mpaaRating: "R",
        releaseDate: releaseDate(1991, 02, 14),
        title: "The Silence of the Lambs",
        year: 1991
    }, {
        bestPictureWinner: true,
        duration: 102,
        mpaaRating: "PG",
        releaseDate: releaseDate(1943, 01, 23),
        title: "Casablanca",
        year: 1942
    }, {
        bestPictureWinner: false,
        duration: 106,
        mpaaRating: "R",
        releaseDate: releaseDate(1995, 08, 16),
        title: "The Usual Suspects",
        year: 1995
    }, {
        bestPictureWinner: false,
        duration: 115,
        mpaaRating: "PG",
        releaseDate: releaseDate(1981, 06, 12),
        title: "Raiders of the Lost Ark",
        year: 1981
    }, {
        bestPictureWinner: false,
        duration: 112,
        mpaaRating: "PG",
        releaseDate: releaseDate(1955, 01, 13),
        title: "Rear Window",
        year: 1954
    }, {
        bestPictureWinner: false,
        duration: 109,
        mpaaRating: "TV-14",
        releaseDate: releaseDate(1960, 9, 8),
        title: "Psycho",
        year: 1960
    }, {
        bestPictureWinner: false,
        duration: 130,
        mpaaRating: "PG",
        releaseDate: releaseDate(1947, 01, 06),
        title: "It's a Wonderful Life",
        year: 1946
    }, {
        bestPictureWinner: false,
        duration: 110,
        mpaaRating: "R",
        releaseDate: releaseDate(1994, 11, 18),
        title: "Léon: The Professional",
        year: 1994
    }, {
        bestPictureWinner: false,
        duration: 110,
        mpaaRating: null,
        releaseDate: releaseDate(1950, 08, 25),
        title: "Sunset Blvd.",
        year: 1950
    }, {
        bestPictureWinner: false,
        duration: 113,
        mpaaRating: "R",
        releaseDate: releaseDate(2000, 10, 11),
        title: "Memento",
        year: 2000
    }, {
        bestPictureWinner: false,
        duration: 165,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2012, 07, 20),
        title: "The Dark Knight Rises",
        year: 2012
    }, {
        bestPictureWinner: false,
        duration: 119,
        mpaaRating: "R",
        releaseDate: releaseDate(1999, 02, 12),
        title: "American History X",
        year: 1998
    }, {
        bestPictureWinner: false,
        duration: 153,
        mpaaRating: "R",
        releaseDate: releaseDate(1979, 08, 15),
        title: "Apocalypse Now",
        year: 1979
    }, {
        bestPictureWinner: false,
        duration: 152,
        mpaaRating: "R",
        releaseDate: releaseDate(1991, 07, 03),
        title: "Terminator 2: Judgment Day",
        year: 1991
    }, {
        bestPictureWinner: false,
        duration: 95,
        mpaaRating: "PG",
        releaseDate: releaseDate(1964, 01, 29),
        title: "Dr. Strangelove or: How I Learned to Stop Worrying and Love the Bomb",
        year: 1964
    }, {
        bestPictureWinner: false,
        duration: 169,
        mpaaRating: "R",
        releaseDate: releaseDate(1998, 07, 24),
        title: "Saving Private Ryan",
        year: 1998
    }, {
        bestPictureWinner: false,
        duration: 117,
        mpaaRating: "TV-14",
        releaseDate: releaseDate(1979, 05, 25),
        title: "Alien",
        year: 1979
    }, {
        bestPictureWinner: false,
        duration: 136,
        mpaaRating: null,
        releaseDate: releaseDate(1959, 09, 26),
        title: "North by Northwest",
        year: 1959
    }, {
        bestPictureWinner: false,
        duration: 87,
        mpaaRating: null,
        releaseDate: releaseDate(1931, 03, 07),
        title: "City Lights",
        year: 1931
    }, {
        bestPictureWinner: false,
        duration: 125,
        mpaaRating: "PG",
        releaseDate: releaseDate(2001, 07, 20),
        title: "Spirited Away",
        year: 2001
    }, {
        bestPictureWinner: false,
        duration: 119,
        mpaaRating: "PG",
        releaseDate: releaseDate(1941, 9, 5),
        title: "Citizen Kane",
        year: 1941
    }, {
        bestPictureWinner: false,
        duration: 87,
        mpaaRating: null,
        releaseDate: releaseDate(1936, 02, 25),
        title: "Modern Times",
        year: 1936
    }, {
        bestPictureWinner: false,
        duration: 142,
        mpaaRating: "R",
        releaseDate: releaseDate(1980, 05, 23),
        title: "The Shining",
        year: 1980
    }, {
        bestPictureWinner: false,
        duration: 129,
        mpaaRating: null,
        releaseDate: releaseDate(1958, 07, 21),
        title: "Vertigo",
        year: 1958
    }, {
        bestPictureWinner: false,
        duration: 116,
        mpaaRating: "PG",
        releaseDate: releaseDate(1985, 07, 03),
        title: "Back to the Future",
        year: 1985
    }, {
        bestPictureWinner: true,
        duration: 122,
        mpaaRating: "R",
        releaseDate: releaseDate(1999, 10, 01),
        title: "American Beauty",
        year: 1999
    }, {
        bestPictureWinner: false,
        duration: 117,
        mpaaRating: null,
        releaseDate: releaseDate(1931, 08, 30),
        title: "M",
        year: 1931
    }, {
        bestPictureWinner: false,
        duration: 150,
        mpaaRating: "R",
        releaseDate: releaseDate(2003, 03, 28),
        title: "The Pianist",
        year: 2002
    }, {
        bestPictureWinner: true,
        duration: 151,
        mpaaRating: "R",
        releaseDate: releaseDate(2006, 10, 06),
        title: "The Departed",
        year: 2006
    }, {
        bestPictureWinner: false,
        duration: 113,
        mpaaRating: "R",
        releaseDate: releaseDate(1976, 02, 08),
        title: "Taxi Driver",
        year: 1976
    }, {
        bestPictureWinner: false,
        duration: 103,
        mpaaRating: "G",
        releaseDate: releaseDate(2010, 06, 18),
        title: "Toy Story 3",
        year: 2010
    }, {
        bestPictureWinner: false,
        duration: 88,
        mpaaRating: null,
        releaseDate: releaseDate(1957, 10, 25),
        title: "Paths of Glory",
        year: 1957
    }, {
        bestPictureWinner: false,
        duration: 118,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(1999, 02, 12),
        title: "Life Is Beautiful",
        year: 1997
    }, {
        bestPictureWinner: false,
        duration: 107,
        mpaaRating: null,
        releaseDate: releaseDate(1944, 04, 24),
        title: "Double Indemnity",
        year: 1944
    }, {
        bestPictureWinner: false,
        duration: 154,
        mpaaRating: "R",
        releaseDate: releaseDate(1986, 07, 18),
        title: "Aliens",
        year: 1986
    }, {
        bestPictureWinner: false,
        duration: 98,
        mpaaRating: "G",
        releaseDate: releaseDate(2008, 06, 27),
        title: "WALL-E",
        year: 2008
    }, {
        bestPictureWinner: false,
        duration: 137,
        mpaaRating: "R",
        releaseDate: releaseDate(2006, 03, 23),
        title: "The Lives of Others",
        year: 2006
    }, {
        bestPictureWinner: false,
        duration: 136,
        mpaaRating: "R",
        releaseDate: releaseDate(1972, 02, 02),
        title: "A Clockwork Orange",
        year: 1971
    }, {
        bestPictureWinner: false,
        duration: 122,
        mpaaRating: "R",
        releaseDate: releaseDate(2001, 04, 24),
        title: "Amélie",
        year: 2001
    }, {
        bestPictureWinner: true,
        duration: 155,
        mpaaRating: "R",
        releaseDate: releaseDate(2000, 05, 05),
        title: "Gladiator",
        year: 2000
    }, {
        bestPictureWinner: false,
        duration: 189,
        mpaaRating: "R",
        releaseDate: releaseDate(1999, 12, 10),
        title: "The Green Mile",
        year: 1999
    }, {
        bestPictureWinner: false,
        duration: 112,
        mpaaRating: "R",
        releaseDate: releaseDate(2011, 11, 02),
        title: "The Intouchables",
        year: 2011
    }, {
        bestPictureWinner: true,
        duration: 227,
        mpaaRating: null,
        releaseDate: releaseDate(1963, 01, 30),
        title: "Lawrence of Arabia",
        year: 1962
    }, {
        bestPictureWinner: false,
        duration: 129,
        mpaaRating: null,
        releaseDate: releaseDate(1963, 03, 16),
        title: "To Kill a Mockingbird",
        year: 1962
    }, {
        bestPictureWinner: false,
        duration: 130,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2006, 10, 20),
        title: "The Prestige",
        year: 2006
    }, {
        bestPictureWinner: false,
        duration: 125,
        mpaaRating: null,
        releaseDate: releaseDate(1941, 3, 7),
        title: "The Great Dictator",
        year: 1940
    }, {
        bestPictureWinner: false,
        duration: 99,
        mpaaRating: "R",
        releaseDate: releaseDate(1992, 10, 23),
        title: "Reservoir Dogs",
        year: 1992
    }, {
        bestPictureWinner: false,
        duration: 149,
        mpaaRating: "R",
        releaseDate: releaseDate(1982, 02, 10),
        title: "Das Boot",
        year: 1981
    }, {
        bestPictureWinner: false,
        duration: 102,
        mpaaRating: "NC-17",
        releaseDate: releaseDate(2000, 10, 27),
        title: "Requiem for a Dream",
        year: 2000
    }, {
        bestPictureWinner: false,
        duration: 93,
        mpaaRating: null,
        releaseDate: releaseDate(1949, 08, 31),
        title: "The Third Man",
        year: 1949
    }, {
        bestPictureWinner: false,
        duration: 126,
        mpaaRating: null,
        releaseDate: releaseDate(1948, 01, 24),
        title: "The Treasure of the Sierra Madre",
        year: 1948
    }, {
        bestPictureWinner: false,
        duration: 108,
        mpaaRating: "R",
        releaseDate: releaseDate(2004, 03, 19),
        title: "Eternal Sunshine of the Spotless Mind",
        year: 2004
    }, {
        bestPictureWinner: false,
        duration: 155,
        mpaaRating: "PG",
        releaseDate: releaseDate(1990, 02, 23),
        title: "Cinema Paradiso",
        year: 1988
    }, {
        bestPictureWinner: false,
        duration: 139,
        mpaaRating: "R",
        releaseDate: releaseDate(1984, 05, 23),
        title: "Once Upon a Time in America",
        year: 1984
    }, {
        bestPictureWinner: false,
        duration: 130,
        mpaaRating: null,
        releaseDate: releaseDate(1974, 06, 20),
        title: "Chinatown",
        year: 1974
    }, {
        bestPictureWinner: false,
        duration: 138,
        mpaaRating: "R",
        releaseDate: releaseDate(1997, 09, 19),
        title: "L.A. Confidential",
        year: 1997
    }, {
        bestPictureWinner: false,
        duration: 89,
        mpaaRating: "G",
        releaseDate: releaseDate(1994, 06, 24),
        title: "The Lion King",
        year: 1994
    }, {
        bestPictureWinner: false,
        duration: 134,
        mpaaRating: "PG",
        releaseDate: releaseDate(1983, 05, 25),
        title: "Star Wars: Episode VI - Return of the Jedi",
        year: 1983
    }, {
        bestPictureWinner: false,
        duration: 116,
        mpaaRating: "R",
        releaseDate: releaseDate(1987, 06, 26),
        title: "Full Metal Jacket",
        year: 1987
    }, {
        bestPictureWinner: false,
        duration: 91,
        mpaaRating: "PG",
        releaseDate: releaseDate(1975, 05, 25),
        title: "Monty Python and the Holy Grail",
        year: 1975
    }, {
        bestPictureWinner: true,
        duration: 177,
        mpaaRating: "R",
        releaseDate: releaseDate(1995, 05, 24),
        title: "Braveheart",
        year: 1995
    }, {
        bestPictureWinner: false,
        duration: 103,
        mpaaRating: null,
        releaseDate: releaseDate(1952, 04, 11),
        title: "Singin' in the Rain",
        year: 1952
    }, {
        bestPictureWinner: false,
        duration: 120,
        mpaaRating: "R",
        releaseDate: releaseDate(2003, 11, 21),
        title: "Oldboy",
        year: 2003
    }, {
        bestPictureWinner: false,
        duration: 120,
        mpaaRating: null,
        releaseDate: releaseDate(1959, 03, 29),
        title: "Some Like It Hot",
        year: 1959
    }, {
        bestPictureWinner: true,
        duration: 160,
        mpaaRating: "PG",
        releaseDate: releaseDate(1984, 09, 19),
        title: "Amadeus",
        year: 1984
    }, {
        bestPictureWinner: false,
        duration: 114,
        mpaaRating: null,
        releaseDate: releaseDate(1927, 03, 13),
        title: "Metropolis",
        year: 1927
    }, {
        bestPictureWinner: false,
        duration: 88,
        mpaaRating: null,
        releaseDate: releaseDate(1951, 12, 26),
        title: "Rashomon",
        year: 1950
    }, {
        bestPictureWinner: false,
        duration: 93,
        mpaaRating: null,
        releaseDate: releaseDate(1949, 12, 13),
        title: "Bicycle Thieves",
        year: 1948
    }, {
        bestPictureWinner: false,
        duration: 141,
        mpaaRating: null,
        releaseDate: releaseDate(1968, 4, 6),
        title: "2001: A Space Odyssey",
        year: 1968
    }, {
        bestPictureWinner: true,
        duration: 131,
        mpaaRating: "R",
        releaseDate: releaseDate(1992, 08, 07),
        title: "Unforgiven",
        year: 1992
    }, {
        bestPictureWinner: true,
        duration: 138,
        mpaaRating: null,
        releaseDate: releaseDate(1951, 1, 15),
        title: "All About Eve",
        year: 1950
    }, {
        bestPictureWinner: true,
        duration: 125,
        mpaaRating: null,
        releaseDate: releaseDate(1960, 9, 16),
        title: "The Apartment",
        year: 1960
    }, {
        bestPictureWinner: false,
        duration: 127,
        mpaaRating: "PG",
        releaseDate: releaseDate(1989, 05, 24),
        title: "Indiana Jones and the Last Crusade",
        year: 1989
    }, {
        bestPictureWinner: true,
        duration: 129,
        mpaaRating: null,
        releaseDate: releaseDate(1974, 01, 10),
        title: "The Sting",
        year: 1973
    }, {
        bestPictureWinner: false,
        duration: 129,
        mpaaRating: "R",
        releaseDate: releaseDate(1980, 12, 19),
        title: "Raging Bull",
        year: 1980
    }, {
        bestPictureWinner: true,
        duration: 161,
        mpaaRating: null,
        releaseDate: releaseDate(1957, 12, 14),
        title: "The Bridge on the River Kwai",
        year: 1957
    }, {
        bestPictureWinner: false,
        duration: 131,
        mpaaRating: "R",
        releaseDate: releaseDate(1988, 07, 15),
        title: "Die Hard",
        year: 1988
    }, {
        bestPictureWinner: false,
        duration: 116,
        mpaaRating: null,
        releaseDate: releaseDate(1958, 2, 6),
        title: "Witness for the Prosecution",
        year: 1957
    }, {
        bestPictureWinner: false,
        duration: 140,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2005, 06, 15),
        title: "Batman Begins",
        year: 2005
    }, {
        bestPictureWinner: false,
        duration: 123,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2011, 03, 16),
        title: "A Separation",
        year: 2011
    }, {
        bestPictureWinner: false,
        duration: 89,
        mpaaRating: null,
        releaseDate: releaseDate(1988, 04, 16),
        title: "Grave of the Fireflies",
        year: 1988
    }, {
        bestPictureWinner: false,
        duration: 118,
        mpaaRating: "R",
        releaseDate: releaseDate(2007, 01, 19),
        title: "Pan's Labyrinth",
        year: 2006
    }, {
        bestPictureWinner: false,
        duration: 156,
        mpaaRating: "R",
        releaseDate: releaseDate(2004, 09, 16),
        title: "Downfall",
        year: 2004
    }, {
        bestPictureWinner: false,
        duration: 129,
        mpaaRating: null,
        releaseDate: releaseDate(1939, 10, 19),
        title: "Mr. Smith Goes to Washington",
        year: 1939
    }, {
        bestPictureWinner: false,
        duration: 75,
        mpaaRating: "TV-MA",
        releaseDate: releaseDate(1961, 09, 13),
        title: "Yojimbo",
        year: 1961
    }, {
        bestPictureWinner: false,
        duration: 172,
        mpaaRating: null,
        releaseDate: releaseDate(1963, 7, 4),
        title: "The Great Escape",
        year: 1963
    }, {
        bestPictureWinner: false,
        duration: 132,
        mpaaRating: "R",
        releaseDate: releaseDate(1967, 5, 10),
        title: "For a Few Dollars More",
        year: 1965
    }, {
        bestPictureWinner: false,
        duration: 102,
        mpaaRating: "R",
        releaseDate: releaseDate(2001, 01, 19),
        title: "Snatch.",
        year: 2000
    }, {
        bestPictureWinner: false,
        duration: 153,
        mpaaRating: "R",
        releaseDate: releaseDate(2009, 08, 21),
        title: "Inglourious Basterds",
        year: 2009
    }, {
        bestPictureWinner: true,
        duration: 108,
        mpaaRating: null,
        releaseDate: releaseDate(1954, 06, 24),
        title: "On the Waterfront",
        year: 1954
    }, {
        bestPictureWinner: false,
        duration: 124,
        mpaaRating: "PG",
        releaseDate: releaseDate(1980, 10, 10),
        title: "The Elephant Man",
        year: 1980
    }, {
        bestPictureWinner: false,
        duration: 96,
        mpaaRating: null,
        releaseDate: releaseDate(1958, 10, 13),
        title: "The Seventh Seal",
        year: 1957
    }, {
        bestPictureWinner: false,
        duration: 81,
        mpaaRating: "TV-G",
        releaseDate: releaseDate(1995, 11, 22),
        title: "Toy Story",
        year: 1995
    }, {
        bestPictureWinner: false,
        duration: 100,
        mpaaRating: null,
        releaseDate: releaseDate(1941, 10, 18),
        title: "The Maltese Falcon",
        year: 1941
    }, {
        bestPictureWinner: false,
        duration: 170,
        mpaaRating: "R",
        releaseDate: releaseDate(1995, 12, 15),
        title: "Heat",
        year: 1995
    }, {
        bestPictureWinner: false,
        duration: 75,
        mpaaRating: null,
        releaseDate: releaseDate(1927, 02, 24),
        title: "The General",
        year: 1926
    }, {
        bestPictureWinner: false,
        duration: 116,
        mpaaRating: "R",
        releaseDate: releaseDate(2009, 01, 09),
        title: "Gran Torino",
        year: 2008
    }, {
        bestPictureWinner: true,
        duration: 130,
        mpaaRating: null,
        releaseDate: releaseDate(1940, 04, 12),
        title: "Rebecca",
        year: 1940
    }, {
        bestPictureWinner: false,
        duration: 117,
        mpaaRating: "R",
        releaseDate: releaseDate(1982, 06, 25),
        title: "Blade Runner",
        year: 1982
    }, {
        bestPictureWinner: false,
        duration: 143,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2012, 05, 04),
        title: "The Avengers",
        year: 2012
    }, {
        bestPictureWinner: false,
        duration: 91,
        mpaaRating: null,
        releaseDate: releaseDate(1959, 06, 22),
        title: "Wild Strawberries",
        year: 1957
    }, {
        bestPictureWinner: false,
        duration: 98,
        mpaaRating: "R",
        releaseDate: releaseDate(1996, 04, 05),
        title: "Fargo",
        year: 1996
    }, {
        bestPictureWinner: false,
        duration: 68,
        mpaaRating: null,
        releaseDate: releaseDate(1921, 2, 6),
        title: "The Kid",
        year: 1921
    }, {
        bestPictureWinner: false,
        duration: 170,
        mpaaRating: "R",
        releaseDate: releaseDate(1983, 12, 09),
        title: "Scarface",
        year: 1983
    }, {
        bestPictureWinner: false,
        duration: 108,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(1958, 6, 8),
        title: "Touch of Evil",
        year: 1958
    }, {
        bestPictureWinner: false,
        duration: 117,
        mpaaRating: "R",
        releaseDate: releaseDate(1998, 03, 06),
        title: "The Big Lebowski",
        year: 1998
    }, {
        bestPictureWinner: false,
        duration: 162,
        mpaaRating: "R",
        releaseDate: releaseDate(1985, 06, 01),
        title: "Ran",
        year: 1985
    }, {
        bestPictureWinner: true,
        duration: 182,
        mpaaRating: "R",
        releaseDate: releaseDate(1979, 02, 23),
        title: "The Deer Hunter",
        year: 1978
    }, {
        bestPictureWinner: false,
        duration: 126,
        mpaaRating: null,
        releaseDate: releaseDate(1967, 11, 1),
        title: "Cool Hand Luke",
        year: 1967
    }, {
        bestPictureWinner: false,
        duration: 147,
        mpaaRating: "R",
        releaseDate: releaseDate(2005, 04, 01),
        title: "Sin City",
        year: 2005
    }, {
        bestPictureWinner: false,
        duration: 72,
        mpaaRating: null,
        releaseDate: releaseDate(1925, 6, 26),
        title: "The Gold Rush",
        year: 1925
    }, {
        bestPictureWinner: false,
        duration: 101,
        mpaaRating: null,
        releaseDate: releaseDate(1951, 06, 30),
        title: "Strangers on a Train",
        year: 1951
    }, {
        bestPictureWinner: true,
        duration: 105,
        mpaaRating: null,
        releaseDate: releaseDate(1934, 02, 23),
        title: "It Happened One Night",
        year: 1934
    }, {
        bestPictureWinner: true,
        duration: 122,
        mpaaRating: "R",
        releaseDate: releaseDate(2007, 11, 21),
        title: "No Country for Old Men",
        year: 2007
    }, {
        bestPictureWinner: false,
        duration: 130,
        mpaaRating: "PG",
        releaseDate: releaseDate(1975, 06, 20),
        title: "Jaws",
        year: 1975
    }, {
        bestPictureWinner: false,
        duration: 107,
        mpaaRating: "R",
        releaseDate: releaseDate(1999, 03, 05),
        title: "Lock, Stock and Two Smoking Barrels",
        year: 1998
    }, {
        bestPictureWinner: false,
        duration: 107,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(1999, 08, 06),
        title: "The Sixth Sense",
        year: 1999
    }, {
        bestPictureWinner: false,
        duration: 121,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2005, 02, 04),
        title: "Hotel Rwanda",
        year: 2004
    }, {
        bestPictureWinner: false,
        duration: 85,
        mpaaRating: null,
        releaseDate: releaseDate(1952, 07, 30),
        title: "High Noon",
        year: 1952
    }, {
        bestPictureWinner: true,
        duration: 120,
        mpaaRating: "R",
        releaseDate: releaseDate(1986, 12, 24),
        title: "Platoon",
        year: 1986
    }, {
        bestPictureWinner: false,
        duration: 109,
        mpaaRating: "R",
        releaseDate: releaseDate(1982, 06, 25),
        title: "The Thing",
        year: 1982
    }, {
        bestPictureWinner: false,
        duration: 110,
        mpaaRating: "PG",
        releaseDate: releaseDate(1969, 10, 24),
        title: "Butch Cassidy and the Sundance Kid",
        year: 1969
    }, {
        bestPictureWinner: false,
        duration: 101,
        mpaaRating: null,
        releaseDate: releaseDate(1939, 08, 25),
        title: "The Wizard of Oz",
        year: 1939
    }, {
        bestPictureWinner: false,
        duration: 178,
        mpaaRating: "R",
        releaseDate: releaseDate(1995, 11, 22),
        title: "Casino",
        year: 1995
    }, {
        bestPictureWinner: false,
        duration: 94,
        mpaaRating: "R",
        releaseDate: releaseDate(1996, 07, 19),
        title: "Trainspotting",
        year: 1996
    }, {
        bestPictureWinner: false,
        duration: 111,
        mpaaRating: "TV-14",
        releaseDate: releaseDate(2003, 10, 10),
        title: "Kill Bill: Vol. 1",
        year: 2003
    }, {
        bestPictureWinner: false,
        duration: 140,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2011, 09, 09),
        title: "Warrior",
        year: 2011
    }, {
        bestPictureWinner: true,
        duration: 93,
        mpaaRating: "PG",
        releaseDate: releaseDate(1977, 04, 20),
        title: "Annie Hall",
        year: 1977
    }, {
        bestPictureWinner: false,
        duration: 101,
        mpaaRating: null,
        releaseDate: releaseDate(1946, 9, 6),
        title: "Notorious",
        year: 1946
    }, {
        bestPictureWinner: false,
        duration: 129,
        mpaaRating: "R",
        releaseDate: releaseDate(2009, 08, 13),
        title: "The Secret in Their Eyes",
        year: 2009
    }, {
        bestPictureWinner: true,
        duration: 238,
        mpaaRating: "G",
        releaseDate: releaseDate(1940, 01, 17),
        title: "Gone with the Wind",
        year: 1939
    }, {
        bestPictureWinner: false,
        duration: 126,
        mpaaRating: "R",
        releaseDate: releaseDate(1998, 01, 09),
        title: "Good Will Hunting",
        year: 1997
    }, {
        bestPictureWinner: true,
        duration: 118,
        mpaaRating: "R",
        releaseDate: releaseDate(2010, 12, 24),
        title: "The King's Speech",
        year: 2010
    }, {
        bestPictureWinner: false,
        duration: 129,
        mpaaRating: null,
        releaseDate: releaseDate(1940, 03, 15),
        title: "The Grapes of Wrath",
        year: 1940
    }, {
        bestPictureWinner: false,
        duration: 148,
        mpaaRating: "R",
        releaseDate: releaseDate(2007, 09, 21),
        title: "Into the Wild",
        year: 2007
    }, {
        bestPictureWinner: false,
        duration: 94,
        mpaaRating: "R",
        releaseDate: releaseDate(1979, 08, 17),
        title: "Life of Brian",
        year: 1979
    }, {
        bestPictureWinner: false,
        duration: 100,
        mpaaRating: "G",
        releaseDate: releaseDate(2003, 05, 30),
        title: "Finding Nemo",
        year: 2003
    }, {
        bestPictureWinner: false,
        duration: 132,
        mpaaRating: "R",
        releaseDate: releaseDate(2006, 03, 17),
        title: "V for Vendetta",
        year: 2005
    }, {
        bestPictureWinner: false,
        duration: 98,
        mpaaRating: "PG",
        releaseDate: releaseDate(2010, 03, 26),
        title: "How to Train Your Dragon",
        year: 2010
    }, {
        bestPictureWinner: false,
        duration: 86,
        mpaaRating: "G",
        releaseDate: releaseDate(1988, 04, 16),
        title: "My Neighbor Totoro",
        year: 1988
    }, {
        bestPictureWinner: false,
        duration: 114,
        mpaaRating: null,
        releaseDate: releaseDate(1946, 08, 31),
        title: "The Big Sleep",
        year: 1946
    }, {
        bestPictureWinner: false,
        duration: 105,
        mpaaRating: "PG",
        releaseDate: releaseDate(1954, 05, 29),
        title: "Dial M for Murder",
        year: 1954
    }, {
        bestPictureWinner: true,
        duration: 212,
        mpaaRating: null,
        releaseDate: releaseDate(1960, 03, 30),
        title: "Ben-Hur",
        year: 1959
    }, {
        bestPictureWinner: false,
        duration: 107,
        mpaaRating: "R",
        releaseDate: releaseDate(1984, 10, 26),
        title: "The Terminator",
        year: 1984
    }, {
        bestPictureWinner: false,
        duration: 121,
        mpaaRating: "R",
        releaseDate: releaseDate(1976, 11, 27),
        title: "Network",
        year: 1976
    }, {
        bestPictureWinner: true,
        duration: 132,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2005, 01, 28),
        title: "Million Dollar Baby",
        year: 2004
    }, {
        bestPictureWinner: false,
        duration: 108,
        mpaaRating: "R",
        releaseDate: releaseDate(2010, 12, 17),
        title: "Black Swan",
        year: 2010
    }, {
        bestPictureWinner: false,
        duration: 93,
        mpaaRating: null,
        releaseDate: releaseDate(1955, 11, 24),
        title: "The Night of the Hunter",
        year: 1955
    }, {
        bestPictureWinner: false,
        duration: 158,
        mpaaRating: "R",
        releaseDate: releaseDate(2008, 01, 25),
        title: "There Will Be Blood",
        year: 2007
    }, {
        bestPictureWinner: false,
        duration: 89,
        mpaaRating: "R",
        releaseDate: releaseDate(1986, 08, 08),
        title: "Stand by Me",
        year: 1986
    }, {
        bestPictureWinner: false,
        duration: 113,
        mpaaRating: "R",
        releaseDate: releaseDate(2002, 01, 30),
        title: "Donnie Darko",
        year: 2001
    }, {
        bestPictureWinner: false,
        duration: 101,
        mpaaRating: "PG",
        releaseDate: releaseDate(1993, 02, 12),
        title: "Groundhog Day",
        year: 1993
    }, {
        bestPictureWinner: false,
        duration: 125,
        mpaaRating: "R",
        releaseDate: releaseDate(1975, 09, 21),
        title: "Dog Day Afternoon",
        year: 1975
    }, {
        bestPictureWinner: false,
        duration: 129,
        mpaaRating: "R",
        releaseDate: releaseDate(1996, 01, 05),
        title: "Twelve Monkeys",
        year: 1995
    }, {
        bestPictureWinner: false,
        duration: 154,
        mpaaRating: "R",
        releaseDate: releaseDate(2000, 06, 16),
        title: "Amores Perros",
        year: 2000
    }, {
        bestPictureWinner: false,
        duration: 115,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2007, 08, 03),
        title: "The Bourne Ultimatum",
        year: 2007
    }, {
        bestPictureWinner: false,
        duration: 92,
        mpaaRating: null,
        releaseDate: releaseDate(2009, 04, 09),
        title: "Mary and Max",
        year: 2009
    }, {
        bestPictureWinner: false,
        duration: 99,
        mpaaRating: null,
        releaseDate: releaseDate(1959, 11, 16),
        title: "The 400 Blows",
        year: 1959
    }, {
        bestPictureWinner: false,
        duration: 83,
        mpaaRating: null,
        releaseDate: releaseDate(1967, 03, 16),
        title: "Persona",
        year: 1966
    }, {
        bestPictureWinner: false,
        duration: 106,
        mpaaRating: null,
        releaseDate: releaseDate(1967, 12, 22),
        title: "The Graduate",
        year: 1967
    }, {
        bestPictureWinner: true,
        duration: 191,
        mpaaRating: "PG",
        releaseDate: releaseDate(1983, 02, 25),
        title: "Gandhi",
        year: 1982
    }, {
        bestPictureWinner: false,
        duration: 85,
        mpaaRating: null,
        releaseDate: releaseDate(1956, 6, 6),
        title: "The Killing",
        year: 1956
    }, {
        bestPictureWinner: false,
        duration: 119,
        mpaaRating: "PG",
        releaseDate: releaseDate(2005, 06, 17),
        title: "Howl's Moving Castle",
        year: 2004
    }, {
        bestPictureWinner: true,
        duration: 100,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2012, 01, 20),
        title: "The Artist",
        year: 2011
    }, {
        bestPictureWinner: false,
        duration: 98,
        mpaaRating: "PG",
        releaseDate: releaseDate(1987, 09, 25),
        title: "The Princess Bride",
        year: 1987
    }, {
        bestPictureWinner: false,
        duration: 120,
        mpaaRating: "R",
        releaseDate: releaseDate(2012, 10, 12),
        title: "Argo",
        year: 2012
    }, {
        bestPictureWinner: true,
        duration: 120,
        mpaaRating: "R",
        releaseDate: releaseDate(2009, 01, 23),
        title: "Slumdog Millionaire",
        year: 2008
    }, {
        bestPictureWinner: false,
        duration: 131,
        mpaaRating: null,
        releaseDate: releaseDate(1966, 06, 22),
        title: "Who's Afraid of Virginia Woolf?",
        year: 1966
    }, {
        bestPictureWinner: false,
        duration: 108,
        mpaaRating: "PG",
        releaseDate: releaseDate(1956, 07, 16),
        title: "La Strada",
        year: 1954
    }, {
        bestPictureWinner: false,
        duration: 126,
        mpaaRating: null,
        releaseDate: releaseDate(1962, 10, 24),
        title: "The Manchurian Candidate",
        year: 1962
    }, {
        bestPictureWinner: false,
        duration: 134,
        mpaaRating: null,
        releaseDate: releaseDate(1961, 09, 25),
        title: "The Hustler",
        year: 1961
    }, {
        bestPictureWinner: true,
        duration: 135,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2002, 01, 04),
        title: "A Beautiful Mind",
        year: 2001
    }, {
        bestPictureWinner: false,
        duration: 145,
        mpaaRating: "R",
        releaseDate: releaseDate(1969, 06, 18),
        title: "The Wild Bunch",
        year: 1969
    }, {
        bestPictureWinner: true,
        duration: 119,
        mpaaRating: "PG",
        releaseDate: releaseDate(1976, 12, 03),
        title: "Rocky",
        year: 1976
    }, {
        bestPictureWinner: false,
        duration: 160,
        mpaaRating: "TV-PG",
        releaseDate: releaseDate(1959, 9, 1),
        title: "Anatomy of a Murder",
        year: 1959
    }, {
        bestPictureWinner: false,
        duration: 120,
        mpaaRating: null,
        releaseDate: releaseDate(1953, 8, 10),
        title: "Stalag 17",
        year: 1953
    }, {
        bestPictureWinner: false,
        duration: 122,
        mpaaRating: "R",
        releaseDate: releaseDate(1974, 03, 16),
        title: "The Exorcist",
        year: 1973
    }, {
        bestPictureWinner: false,
        duration: 138,
        mpaaRating: "PG",
        releaseDate: releaseDate(1972, 12, 10),
        title: "Sleuth",
        year: 1972
    }, {
        bestPictureWinner: false,
        duration: 80,
        mpaaRating: null,
        releaseDate: releaseDate(1948, 8, 28),
        title: "Rope",
        year: 1948
    }, {
        bestPictureWinner: false,
        duration: 184,
        mpaaRating: "PG",
        releaseDate: releaseDate(1975, 12, 18),
        title: "Barry Lyndon",
        year: 1975
    }, {
        bestPictureWinner: false,
        duration: 123,
        mpaaRating: null,
        releaseDate: releaseDate(1962, 4, 22),
        title: "The Man Who Shot Liberty Valance",
        year: 1962
    }, {
        bestPictureWinner: false,
        duration: 112,
        mpaaRating: "R",
        releaseDate: releaseDate(2009, 08, 14),
        title: "District 9",
        year: 2009
    }, {
        bestPictureWinner: false,
        duration: 163,
        mpaaRating: null,
        releaseDate: releaseDate(1980, 04, 17),
        title: "Stalker",
        year: 1979
    }, {
        bestPictureWinner: false,
        duration: 101,
        mpaaRating: "R",
        releaseDate: releaseDate(2002, 12, 12),
        title: "Infernal Affairs",
        year: 2002
    }, {
        bestPictureWinner: false,
        duration: 118,
        mpaaRating: null,
        releaseDate: releaseDate(1953, 9, 2),
        title: "Roman Holiday",
        year: 1953
    }, {
        bestPictureWinner: false,
        duration: 103,
        mpaaRating: "PG",
        releaseDate: releaseDate(1998, 06, 05),
        title: "The Truman Show",
        year: 1998
    }, {
        bestPictureWinner: false,
        duration: 111,
        mpaaRating: "G",
        releaseDate: releaseDate(2007, 06, 29),
        title: "Ratatouille",
        year: 2007
    }, {
        bestPictureWinner: false,
        duration: 143,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2003, 07, 09),
        title: "Pirates of the Caribbean: The Curse of the Black Pearl",
        year: 2003
    }, {
        bestPictureWinner: false,
        duration: 106,
        mpaaRating: "R",
        releaseDate: releaseDate(2008, 12, 12),
        title: "Ip Man",
        year: 2008
    }, {
        bestPictureWinner: false,
        duration: 112,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2007, 05, 23),
        title: "The Diving Bell and the Butterfly",
        year: 2007
    }, {
        bestPictureWinner: false,
        duration: 130,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2011, 07, 15),
        title: "Harry Potter and the Deathly Hallows: Part 2",
        year: 2011
    }, {
        bestPictureWinner: false,
        duration: 99,
        mpaaRating: "R",
        releaseDate: releaseDate(1967, 01, 18),
        title: "A Fistful of Dollars",
        year: 1964
    }, {
        bestPictureWinner: false,
        duration: 125,
        mpaaRating: "PG",
        releaseDate: releaseDate(1951, 12, 1),
        title: "A Streetcar Named Desire",
        year: 1951
    }, {
        bestPictureWinner: false,
        duration: 92,
        mpaaRating: "G",
        releaseDate: releaseDate(2001, 11, 02),
        title: "Monsters, Inc.",
        year: 2001
    }, {
        bestPictureWinner: false,
        duration: 133,
        mpaaRating: "R",
        releaseDate: releaseDate(1994, 02, 25),
        title: "In the Name of the Father",
        year: 1993
    }, {
        bestPictureWinner: false,
        duration: 127,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2009, 05, 08),
        title: "Star Trek",
        year: 2009
    }, {
        bestPictureWinner: false,
        duration: 84,
        mpaaRating: "G",
        releaseDate: releaseDate(1991, 11, 22),
        title: "Beauty and the Beast",
        year: 1991
    }, {
        bestPictureWinner: false,
        duration: 136,
        mpaaRating: "R",
        releaseDate: releaseDate(1968, 06, 12),
        title: "Rosemary's Baby",
        year: 1968
    }, {
        bestPictureWinner: false,
        duration: 104,
        mpaaRating: null,
        releaseDate: releaseDate(1950, 10, 13),
        title: "Harvey",
        year: 1950
    }, {
        bestPictureWinner: false,
        duration: 117,
        mpaaRating: "PG",
        releaseDate: releaseDate(1984, 3, 11),
        title: "Nauticaä of the Valley of the Wind",
        year: 1984
    }, {
        bestPictureWinner: false,
        duration: 109,
        mpaaRating: "R",
        releaseDate: releaseDate(2009, 01, 30),
        title: "The Wrestler",
        year: 2008
    }, {
        bestPictureWinner: true,
        duration: 133,
        mpaaRating: null,
        releaseDate: releaseDate(1930, 08, 24),
        title: "All Quiet on the Western Front",
        year: 1930
    }, {
        bestPictureWinner: false,
        duration: 98,
        mpaaRating: null,
        releaseDate: releaseDate(1996, 02, 23),
        title: "La Haine",
        year: 1995
    }, {
        bestPictureWinner: true,
        duration: 133,
        mpaaRating: "R",
        releaseDate: releaseDate(1988, 12, 16),
        title: "Rain Man",
        year: 1988
    }, {
        bestPictureWinner: false,
        duration: 66,
        mpaaRating: null,
        releaseDate: releaseDate(1925, 12, 24),
        title: "Battleship Potemkin",
        year: 1925
    }, {
        bestPictureWinner: false,
        duration: 138,
        mpaaRating: "R",
        releaseDate: releaseDate(2010, 02, 19),
        title: "Shutter Island",
        year: 2010
    }, {
        bestPictureWinner: false,
        duration: 81,
        mpaaRating: null,
        releaseDate: releaseDate(1929, 6, 3),
        title: "Nosferatu",
        year: 1922
    }, {
        bestPictureWinner: false,
        duration: 103,
        mpaaRating: "R",
        releaseDate: releaseDate(2003, 09, 19),
        title: "Spring, Summer, Fall, Winter... and Spring",
        year: 2003
    }, {
        bestPictureWinner: false,
        duration: 96,
        mpaaRating: "R",
        releaseDate: releaseDate(1979, 04, 25),
        title: "Manhattan",
        year: 1979
    }, {
        bestPictureWinner: false,
        duration: 138,
        mpaaRating: "R",
        releaseDate: releaseDate(2003, 10, 15),
        title: "Mystic River",
        year: 2003
    }, {
        bestPictureWinner: false,
        duration: 102,
        mpaaRating: null,
        releaseDate: releaseDate(1938, 2, 18),
        title: "Bringing Up Baby",
        year: 1938
    }, {
        bestPictureWinner: false,
        duration: 108,
        mpaaRating: null,
        releaseDate: releaseDate(1943, 1, 15),
        title: "Shadow of a Doubt",
        year: 1943
    }, {
        bestPictureWinner: false,
        duration: 125,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2004, 01, 09),
        title: "Big Fish",
        year: 2003
    }, {
        bestPictureWinner: false,
        duration: 124,
        mpaaRating: "PG",
        releaseDate: releaseDate(1986, 08, 02),
        title: "Castle in the Sky",
        year: 1986
    }, {
        bestPictureWinner: false,
        duration: 151,
        mpaaRating: "PG",
        releaseDate: releaseDate(1973, 12, 16),
        title: "Papillon",
        year: 1973
    }, {
        bestPictureWinner: false,
        duration: 76,
        mpaaRating: "PG",
        releaseDate: releaseDate(1993, 10, 29),
        title: "The Nightmare Before Christmas",
        year: 1993
    }, {
        bestPictureWinner: false,
        duration: 119,
        mpaaRating: "R",
        releaseDate: releaseDate(1987, 06, 03),
        title: "The Untouchables",
        year: 1987
    }, {
        bestPictureWinner: false,
        duration: 127,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(1993, 06, 11),
        title: "Jurassic Park",
        year: 1993
    }, {
        bestPictureWinner: false,
        duration: 115,
        mpaaRating: "R",
        releaseDate: releaseDate(2008, 10, 24),
        title: "Let the Right One In",
        year: 2008
    }, {
        bestPictureWinner: true,
        duration: 109,
        mpaaRating: null,
        releaseDate: releaseDate(1967, 10, 14),
        title: "In the Heat of the Night",
        year: 1967
    }, {
        bestPictureWinner: false,
        duration: 170,
        mpaaRating: "PG-13",
        releaseDate: releaseDate(2009, 12, 24),
        title: "3 Idiots",
        year: 2009
    }, {
        bestPictureWinner: false,
        duration: 118,
        mpaaRating: null,
        releaseDate: releaseDate(1944, 9, 23),
        title: "Arsenic and Old Lace",
        year: 1944
    }, {
        bestPictureWinner: false,
        duration: 119,
        mpaaRating: null,
        releaseDate: releaseDate(1956, 3, 13),
        title: "The Searchers",
        year: 1956
    }, {
        bestPictureWinner: false,
        duration: 98,
        mpaaRating: "PG",
        releaseDate: releaseDate(2000, 09, 29),
        title: "In the Mood for Love",
        year: 2000
    }, {
        bestPictureWinner: false,
        duration: 141,
        mpaaRating: null,
        releaseDate: releaseDate(1959, 4, 4),
        title: "Rio Bravo",
        year: 1959
    }
];

// these movie definitions were copied from the .NET tests
// Javascript Date constructor month is zero based
function releaseDate(year, month, day) {
    return new Date(year + '-' + month + '-' + day + ' 00:00:00.000 -00:00');
    //return new Date(year, month - 1, day);
    // return new Date(Date.UTC(year, month - 1, day));
}
