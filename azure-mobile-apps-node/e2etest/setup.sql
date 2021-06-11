CREATE TABLE [IntIdMovies](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[title] [nvarchar](max) NULL,
	[duration] [int] NOT NULL,
	[mpaaRating] [nvarchar](max) NULL,
	[releaseDate] [datetime] NOT NULL,
	[bestPictureWinner] [bit] NOT NULL,
	[year] [int] NOT NULL,
 CONSTRAINT [PK_ZumoE2EServerApp.IntIdMovies] PRIMARY KEY CLUSTERED
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [Movies](
	[id] [nvarchar](128) NOT NULL DEFAULT (newid()) PRIMARY KEY,
	[title] [nvarchar](max) NULL,
	[duration] [int] NOT NULL,
	[mpaaRating] [nvarchar](max) NULL,
	[releaseDate] [datetime] NOT NULL,
	[bestPictureWinner] [bit] NOT NULL,
	[year] [int] NOT NULL,
	[version] [timestamp] NOT NULL,
	[createdAt] [datetimeoffset](7) NOT NULL DEFAULT (sysutcdatetime()),
	[updatedAt] [datetimeoffset](7) NULL,
	[deleted] [bit] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
SET IDENTITY_INSERT [IntIdMovies] ON

GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (1, N'The Shawshank Redemption', 142, N'R', CAST(N'1994-10-14 00:00:00.000' AS DateTime), 0, 1994)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (2, N'The Godfather', 175, N'R', CAST(N'1972-03-24 00:00:00.000' AS DateTime), 1, 1972)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (3, N'The Godfather: Part II', 200, N'R', CAST(N'1974-12-20 00:00:00.000' AS DateTime), 1, 1974)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (4, N'Pulp Fiction', 168, N'R', CAST(N'1994-10-14 00:00:00.000' AS DateTime), 0, 1994)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (5, N'The Good, the Bad and the Ugly', 161, NULL, CAST(N'1967-12-29 00:00:00.000' AS DateTime), 0, 1966)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (6, N'12 Angry Men', 96, NULL, CAST(N'1957-04-10 00:00:00.000' AS DateTime), 0, 1957)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (7, N'The Dark Knight', 152, N'PG-13', CAST(N'2008-07-18 00:00:00.000' AS DateTime), 0, 2008)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (8, N'Schindler''s List', 195, N'R', CAST(N'1993-12-15 00:00:00.000' AS DateTime), 1, 1993)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (9, N'The Lord of the Rings: The Return of the King', 201, N'PG-13', CAST(N'2003-12-17 00:00:00.000' AS DateTime), 1, 2003)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (10, N'Fight Club', 139, N'R', CAST(N'1999-10-15 00:00:00.000' AS DateTime), 0, 1999)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (11, N'Star Wars: Episode V - The Empire Strikes Back', 127, N'PG', CAST(N'1980-05-21 00:00:00.000' AS DateTime), 0, 1980)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (12, N'One Flew Over the Cuckoo''s Nest', 133, NULL, CAST(N'1975-11-21 00:00:00.000' AS DateTime), 1, 1975)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (13, N'The Lord of the Rings: The Fellowship of the Ring', 178, N'PG-13', CAST(N'2001-12-19 00:00:00.000' AS DateTime), 0, 2001)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (14, N'Inception', 148, N'PG-13', CAST(N'2010-07-16 00:00:00.000' AS DateTime), 0, 2010)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (15, N'Goodfellas', 146, N'R', CAST(N'1990-09-19 00:00:00.000' AS DateTime), 0, 1990)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (16, N'Star Wars', 121, N'PG', CAST(N'1977-05-25 00:00:00.000' AS DateTime), 0, 1977)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (17, N'Seven Samurai', 141, NULL, CAST(N'1956-11-19 00:00:00.000' AS DateTime), 0, 1954)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (18, N'The Matrix', 136, N'R', CAST(N'1999-03-31 00:00:00.000' AS DateTime), 0, 1999)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (19, N'Forrest Gump', 142, N'PG-13', CAST(N'1994-07-06 00:00:00.000' AS DateTime), 1, 1994)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (20, N'City of God', 130, N'R', CAST(N'2002-01-01 00:00:00.000' AS DateTime), 0, 2002)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (21, N'The Lord of the Rings: The Two Towers', 179, N'PG-13', CAST(N'2002-12-18 00:00:00.000' AS DateTime), 0, 2002)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (22, N'Once Upon a Time in the West', 175, N'PG-13', CAST(N'1968-12-21 00:00:00.000' AS DateTime), 0, 1968)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (23, N'Se7en', 127, N'R', CAST(N'1995-09-22 00:00:00.000' AS DateTime), 0, 1995)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (24, N'The Silence of the Lambs', 118, N'R', CAST(N'1991-02-14 00:00:00.000' AS DateTime), 1, 1991)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (25, N'Casablanca', 102, N'PG', CAST(N'1943-01-23 00:00:00.000' AS DateTime), 1, 1942)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (26, N'The Usual Suspects', 106, N'R', CAST(N'1995-08-16 00:00:00.000' AS DateTime), 0, 1995)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (27, N'Raiders of the Lost Ark', 115, N'PG', CAST(N'1981-06-12 00:00:00.000' AS DateTime), 0, 1981)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (28, N'Rear Window', 112, N'PG', CAST(N'1955-01-13 00:00:00.000' AS DateTime), 0, 1954)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (29, N'Psycho', 109, N'TV-14', CAST(N'1960-09-08 00:00:00.000' AS DateTime), 0, 1960)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (30, N'It''s a Wonderful Life', 130, N'PG', CAST(N'1947-01-06 00:00:00.000' AS DateTime), 0, 1946)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (31, N'Léon: The Professional', 110, N'R', CAST(N'1994-11-18 00:00:00.000' AS DateTime), 0, 1994)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (32, N'Sunset Blvd.', 110, NULL, CAST(N'1950-08-25 00:00:00.000' AS DateTime), 0, 1950)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (33, N'Memento', 113, N'R', CAST(N'2000-10-11 00:00:00.000' AS DateTime), 0, 2000)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (34, N'The Dark Knight Rises', 165, N'PG-13', CAST(N'2012-07-20 00:00:00.000' AS DateTime), 0, 2012)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (35, N'American History X', 119, N'R', CAST(N'1999-02-12 00:00:00.000' AS DateTime), 0, 1998)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (36, N'Apocalypse Now', 153, N'R', CAST(N'1979-08-15 00:00:00.000' AS DateTime), 0, 1979)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (37, N'Terminator 2: Judgment Day', 152, N'R', CAST(N'1991-07-03 00:00:00.000' AS DateTime), 0, 1991)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (38, N'Dr. Strangelove or: How I Learned to Stop Worrying and Love the Bomb', 95, N'PG', CAST(N'1964-01-29 00:00:00.000' AS DateTime), 0, 1964)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (39, N'Saving Private Ryan', 169, N'R', CAST(N'1998-07-24 00:00:00.000' AS DateTime), 0, 1998)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (40, N'Alien', 117, N'TV-14', CAST(N'1979-05-25 00:00:00.000' AS DateTime), 0, 1979)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (41, N'North by Northwest', 136, NULL, CAST(N'1959-09-26 00:00:00.000' AS DateTime), 0, 1959)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (42, N'City Lights', 87, NULL, CAST(N'1931-03-07 00:00:00.000' AS DateTime), 0, 1931)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (43, N'Spirited Away', 125, N'PG', CAST(N'2001-07-20 00:00:00.000' AS DateTime), 0, 2001)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (44, N'Citizen Kane', 119, N'PG', CAST(N'1941-09-05 00:00:00.000' AS DateTime), 0, 1941)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (45, N'Modern Times', 87, NULL, CAST(N'1936-02-25 00:00:00.000' AS DateTime), 0, 1936)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (46, N'The Shining', 142, N'R', CAST(N'1980-05-23 00:00:00.000' AS DateTime), 0, 1980)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (47, N'Vertigo', 129, NULL, CAST(N'1958-07-21 00:00:00.000' AS DateTime), 0, 1958)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (48, N'Back to the Future', 116, N'PG', CAST(N'1985-07-03 00:00:00.000' AS DateTime), 0, 1985)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (49, N'American Beauty', 122, N'R', CAST(N'1999-10-01 00:00:00.000' AS DateTime), 1, 1999)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (50, N'M', 117, NULL, CAST(N'1931-08-30 00:00:00.000' AS DateTime), 0, 1931)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (51, N'The Pianist', 150, N'R', CAST(N'2003-03-28 00:00:00.000' AS DateTime), 0, 2002)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (52, N'The Departed', 151, N'R', CAST(N'2006-10-06 00:00:00.000' AS DateTime), 1, 2006)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (53, N'Taxi Driver', 113, N'R', CAST(N'1976-02-08 00:00:00.000' AS DateTime), 0, 1976)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (54, N'Toy Story 3', 103, N'G', CAST(N'2010-06-18 00:00:00.000' AS DateTime), 0, 2010)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (55, N'Paths of Glory', 88, NULL, CAST(N'1957-10-25 00:00:00.000' AS DateTime), 0, 1957)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (56, N'Life Is Beautiful', 118, N'PG-13', CAST(N'1999-02-12 00:00:00.000' AS DateTime), 0, 1997)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (57, N'Double Indemnity', 107, NULL, CAST(N'1944-04-24 00:00:00.000' AS DateTime), 0, 1944)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (58, N'Aliens', 154, N'R', CAST(N'1986-07-18 00:00:00.000' AS DateTime), 0, 1986)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (59, N'WALL-E', 98, N'G', CAST(N'2008-06-27 00:00:00.000' AS DateTime), 0, 2008)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (60, N'The Lives of Others', 137, N'R', CAST(N'2006-03-23 00:00:00.000' AS DateTime), 0, 2006)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (61, N'A Clockwork Orange', 136, N'R', CAST(N'1972-02-02 00:00:00.000' AS DateTime), 0, 1971)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (62, N'Amélie', 122, N'R', CAST(N'2001-04-24 00:00:00.000' AS DateTime), 0, 2001)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (63, N'Gladiator', 155, N'R', CAST(N'2000-05-05 00:00:00.000' AS DateTime), 1, 2000)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (64, N'The Green Mile', 189, N'R', CAST(N'1999-12-10 00:00:00.000' AS DateTime), 0, 1999)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (65, N'The Intouchables', 112, N'R', CAST(N'2011-11-02 00:00:00.000' AS DateTime), 0, 2011)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (66, N'Lawrence of Arabia', 227, NULL, CAST(N'1963-01-30 00:00:00.000' AS DateTime), 1, 1962)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (67, N'To Kill a Mockingbird', 129, NULL, CAST(N'1963-03-16 00:00:00.000' AS DateTime), 0, 1962)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (68, N'The Prestige', 130, N'PG-13', CAST(N'2006-10-20 00:00:00.000' AS DateTime), 0, 2006)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (69, N'The Great Dictator', 125, NULL, CAST(N'1941-03-07 00:00:00.000' AS DateTime), 0, 1940)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (70, N'Reservoir Dogs', 99, N'R', CAST(N'1992-10-23 00:00:00.000' AS DateTime), 0, 1992)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (71, N'Das Boot', 149, N'R', CAST(N'1982-02-10 00:00:00.000' AS DateTime), 0, 1981)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (72, N'Requiem for a Dream', 102, N'NC-17', CAST(N'2000-10-27 00:00:00.000' AS DateTime), 0, 2000)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (73, N'The Third Man', 93, NULL, CAST(N'1949-08-31 00:00:00.000' AS DateTime), 0, 1949)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (74, N'The Treasure of the Sierra Madre', 126, NULL, CAST(N'1948-01-24 00:00:00.000' AS DateTime), 0, 1948)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (75, N'Eternal Sunshine of the Spotless Mind', 108, N'R', CAST(N'2004-03-19 00:00:00.000' AS DateTime), 0, 2004)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (76, N'Cinema Paradiso', 155, N'PG', CAST(N'1990-02-23 00:00:00.000' AS DateTime), 0, 1988)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (77, N'Once Upon a Time in America', 139, N'R', CAST(N'1984-05-23 00:00:00.000' AS DateTime), 0, 1984)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (78, N'Chinatown', 130, NULL, CAST(N'1974-06-20 00:00:00.000' AS DateTime), 0, 1974)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (79, N'L.A. Confidential', 138, N'R', CAST(N'1997-09-19 00:00:00.000' AS DateTime), 0, 1997)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (80, N'The Lion King', 89, N'G', CAST(N'1994-06-24 00:00:00.000' AS DateTime), 0, 1994)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (81, N'Star Wars: Episode VI - Return of the Jedi', 134, N'PG', CAST(N'1983-05-25 00:00:00.000' AS DateTime), 0, 1983)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (82, N'Full Metal Jacket', 116, N'R', CAST(N'1987-06-26 00:00:00.000' AS DateTime), 0, 1987)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (83, N'Monty Python and the Holy Grail', 91, N'PG', CAST(N'1975-05-25 00:00:00.000' AS DateTime), 0, 1975)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (84, N'Braveheart', 177, N'R', CAST(N'1995-05-24 00:00:00.000' AS DateTime), 1, 1995)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (85, N'Singin'' in the Rain', 103, NULL, CAST(N'1952-04-11 00:00:00.000' AS DateTime), 0, 1952)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (86, N'Oldboy', 120, N'R', CAST(N'2003-11-21 00:00:00.000' AS DateTime), 0, 2003)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (87, N'Some Like It Hot', 120, NULL, CAST(N'1959-03-29 00:00:00.000' AS DateTime), 0, 1959)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (88, N'Amadeus', 160, N'PG', CAST(N'1984-09-19 00:00:00.000' AS DateTime), 1, 1984)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (89, N'Metropolis', 114, NULL, CAST(N'1927-03-13 00:00:00.000' AS DateTime), 0, 1927)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (90, N'Rashomon', 88, NULL, CAST(N'1951-12-26 00:00:00.000' AS DateTime), 0, 1950)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (91, N'Bicycle Thieves', 93, NULL, CAST(N'1949-12-13 00:00:00.000' AS DateTime), 0, 1948)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (92, N'2001: A Space Odyssey', 141, NULL, CAST(N'1968-04-06 00:00:00.000' AS DateTime), 0, 1968)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (93, N'Unforgiven', 131, N'R', CAST(N'1992-08-07 00:00:00.000' AS DateTime), 1, 1992)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (94, N'All About Eve', 138, NULL, CAST(N'1951-01-15 00:00:00.000' AS DateTime), 1, 1950)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (95, N'The Apartment', 125, NULL, CAST(N'1960-09-16 00:00:00.000' AS DateTime), 1, 1960)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (96, N'Indiana Jones and the Last Crusade', 127, N'PG', CAST(N'1989-05-24 00:00:00.000' AS DateTime), 0, 1989)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (97, N'The Sting', 129, NULL, CAST(N'1974-01-10 00:00:00.000' AS DateTime), 1, 1973)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (98, N'Raging Bull', 129, N'R', CAST(N'1980-12-19 00:00:00.000' AS DateTime), 0, 1980)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (99, N'The Bridge on the River Kwai', 161, NULL, CAST(N'1957-12-14 00:00:00.000' AS DateTime), 1, 1957)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (100, N'Die Hard', 131, N'R', CAST(N'1988-07-15 00:00:00.000' AS DateTime), 0, 1988)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (101, N'Witness for the Prosecution', 116, NULL, CAST(N'1958-02-06 00:00:00.000' AS DateTime), 0, 1957)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (102, N'Batman Begins', 140, N'PG-13', CAST(N'2005-06-15 00:00:00.000' AS DateTime), 0, 2005)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (103, N'A Separation', 123, N'PG-13', CAST(N'2011-03-16 00:00:00.000' AS DateTime), 0, 2011)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (104, N'Grave of the Fireflies', 89, NULL, CAST(N'1988-04-16 00:00:00.000' AS DateTime), 0, 1988)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (105, N'Pan''s Labyrinth', 118, N'R', CAST(N'2007-01-19 00:00:00.000' AS DateTime), 0, 2006)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (106, N'Downfall', 156, N'R', CAST(N'2004-09-16 00:00:00.000' AS DateTime), 0, 2004)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (107, N'Mr. Smith Goes to Washington', 129, NULL, CAST(N'1939-10-19 00:00:00.000' AS DateTime), 0, 1939)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (108, N'Yojimbo', 75, N'TV-MA', CAST(N'1961-09-13 00:00:00.000' AS DateTime), 0, 1961)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (109, N'The Great Escape', 172, NULL, CAST(N'1963-07-04 00:00:00.000' AS DateTime), 0, 1963)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (110, N'For a Few Dollars More', 132, N'R', CAST(N'1967-05-10 00:00:00.000' AS DateTime), 0, 1965)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (111, N'Snatch.', 102, N'R', CAST(N'2001-01-19 00:00:00.000' AS DateTime), 0, 2000)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (112, N'Inglourious Basterds', 153, N'R', CAST(N'2009-08-21 00:00:00.000' AS DateTime), 0, 2009)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (113, N'On the Waterfront', 108, NULL, CAST(N'1954-06-24 00:00:00.000' AS DateTime), 1, 1954)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (114, N'The Elephant Man', 124, N'PG', CAST(N'1980-10-10 00:00:00.000' AS DateTime), 0, 1980)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (115, N'The Seventh Seal', 96, NULL, CAST(N'1958-10-13 00:00:00.000' AS DateTime), 0, 1957)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (116, N'Toy Story', 81, N'TV-G', CAST(N'1995-11-22 00:00:00.000' AS DateTime), 0, 1995)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (117, N'The Maltese Falcon', 100, NULL, CAST(N'1941-10-18 00:00:00.000' AS DateTime), 0, 1941)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (118, N'Heat', 170, N'R', CAST(N'1995-12-15 00:00:00.000' AS DateTime), 0, 1995)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (119, N'The General', 75, NULL, CAST(N'1927-02-24 00:00:00.000' AS DateTime), 0, 1926)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (120, N'Gran Torino', 116, N'R', CAST(N'2009-01-09 00:00:00.000' AS DateTime), 0, 2008)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (121, N'Rebecca', 130, NULL, CAST(N'1940-04-12 00:00:00.000' AS DateTime), 1, 1940)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (122, N'Blade Runner', 117, N'R', CAST(N'1982-06-25 00:00:00.000' AS DateTime), 0, 1982)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (123, N'The Avengers', 143, N'PG-13', CAST(N'2012-05-04 00:00:00.000' AS DateTime), 0, 2012)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (124, N'Wild Strawberries', 91, NULL, CAST(N'1959-06-22 00:00:00.000' AS DateTime), 0, 1957)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (125, N'Fargo', 98, N'R', CAST(N'1996-04-05 00:00:00.000' AS DateTime), 0, 1996)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (126, N'The Kid', 68, NULL, CAST(N'1921-02-06 00:00:00.000' AS DateTime), 0, 1921)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (127, N'Scarface', 170, N'R', CAST(N'1983-12-09 00:00:00.000' AS DateTime), 0, 1983)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (128, N'Touch of Evil', 108, N'PG-13', CAST(N'1958-06-08 00:00:00.000' AS DateTime), 0, 1958)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (129, N'The Big Lebowski', 117, N'R', CAST(N'1998-03-06 00:00:00.000' AS DateTime), 0, 1998)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (130, N'Ran', 162, N'R', CAST(N'1985-06-01 00:00:00.000' AS DateTime), 0, 1985)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (131, N'The Deer Hunter', 182, N'R', CAST(N'1979-02-23 00:00:00.000' AS DateTime), 1, 1978)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (132, N'Cool Hand Luke', 126, NULL, CAST(N'1967-11-01 00:00:00.000' AS DateTime), 0, 1967)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (133, N'Sin City', 147, N'R', CAST(N'2005-04-01 00:00:00.000' AS DateTime), 0, 2005)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (134, N'The Gold Rush', 72, NULL, CAST(N'1925-06-26 00:00:00.000' AS DateTime), 0, 1925)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (135, N'Strangers on a Train', 101, NULL, CAST(N'1951-06-30 00:00:00.000' AS DateTime), 0, 1951)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (136, N'It Happened One Night', 105, NULL, CAST(N'1934-02-23 00:00:00.000' AS DateTime), 1, 1934)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (137, N'No Country for Old Men', 122, N'R', CAST(N'2007-11-21 00:00:00.000' AS DateTime), 1, 2007)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (138, N'Jaws', 130, N'PG', CAST(N'1975-06-20 00:00:00.000' AS DateTime), 0, 1975)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (139, N'Lock, Stock and Two Smoking Barrels', 107, N'R', CAST(N'1999-03-05 00:00:00.000' AS DateTime), 0, 1998)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (140, N'The Sixth Sense', 107, N'PG-13', CAST(N'1999-08-06 00:00:00.000' AS DateTime), 0, 1999)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (141, N'Hotel Rwanda', 121, N'PG-13', CAST(N'2005-02-04 00:00:00.000' AS DateTime), 0, 2004)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (142, N'High Noon', 85, NULL, CAST(N'1952-07-30 00:00:00.000' AS DateTime), 0, 1952)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (143, N'Platoon', 120, N'R', CAST(N'1986-12-24 00:00:00.000' AS DateTime), 1, 1986)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (144, N'The Thing', 109, N'R', CAST(N'1982-06-25 00:00:00.000' AS DateTime), 0, 1982)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (145, N'Butch Cassidy and the Sundance Kid', 110, N'PG', CAST(N'1969-10-24 00:00:00.000' AS DateTime), 0, 1969)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (146, N'The Wizard of Oz', 101, NULL, CAST(N'1939-08-25 00:00:00.000' AS DateTime), 0, 1939)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (147, N'Casino', 178, N'R', CAST(N'1995-11-22 00:00:00.000' AS DateTime), 0, 1995)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (148, N'Trainspotting', 94, N'R', CAST(N'1996-07-19 00:00:00.000' AS DateTime), 0, 1996)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (149, N'Kill Bill: Vol. 1', 111, N'TV-14', CAST(N'2003-10-10 00:00:00.000' AS DateTime), 0, 2003)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (150, N'Warrior', 140, N'PG-13', CAST(N'2011-09-09 00:00:00.000' AS DateTime), 0, 2011)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (151, N'Annie Hall', 93, N'PG', CAST(N'1977-04-20 00:00:00.000' AS DateTime), 1, 1977)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (152, N'Notorious', 101, NULL, CAST(N'1946-09-06 00:00:00.000' AS DateTime), 0, 1946)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (153, N'The Secret in Their Eyes', 129, N'R', CAST(N'2009-08-13 00:00:00.000' AS DateTime), 0, 2009)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (154, N'Gone with the Wind', 238, N'G', CAST(N'1940-01-17 00:00:00.000' AS DateTime), 1, 1939)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (155, N'Good Will Hunting', 126, N'R', CAST(N'1998-01-09 00:00:00.000' AS DateTime), 0, 1997)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (156, N'The King''s Speech', 118, N'R', CAST(N'2010-12-24 00:00:00.000' AS DateTime), 1, 2010)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (157, N'The Grapes of Wrath', 129, NULL, CAST(N'1940-03-15 00:00:00.000' AS DateTime), 0, 1940)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (158, N'Into the Wild', 148, N'R', CAST(N'2007-09-21 00:00:00.000' AS DateTime), 0, 2007)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (159, N'Life of Brian', 94, N'R', CAST(N'1979-08-17 00:00:00.000' AS DateTime), 0, 1979)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (160, N'Finding Nemo', 100, N'G', CAST(N'2003-05-30 00:00:00.000' AS DateTime), 0, 2003)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (161, N'V for Vendetta', 132, N'R', CAST(N'2006-03-17 00:00:00.000' AS DateTime), 0, 2005)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (162, N'How to Train Your Dragon', 98, N'PG', CAST(N'2010-03-26 00:00:00.000' AS DateTime), 0, 2010)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (163, N'My Neighbor Totoro', 86, N'G', CAST(N'1988-04-16 00:00:00.000' AS DateTime), 0, 1988)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (164, N'The Big Sleep', 114, NULL, CAST(N'1946-08-31 00:00:00.000' AS DateTime), 0, 1946)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (165, N'Dial M for Murder', 105, N'PG', CAST(N'1954-05-29 00:00:00.000' AS DateTime), 0, 1954)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (166, N'Ben-Hur', 212, NULL, CAST(N'1960-03-30 00:00:00.000' AS DateTime), 1, 1959)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (167, N'The Terminator', 107, N'R', CAST(N'1984-10-26 00:00:00.000' AS DateTime), 0, 1984)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (168, N'Network', 121, N'R', CAST(N'1976-11-27 00:00:00.000' AS DateTime), 0, 1976)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (169, N'Million Dollar Baby', 132, N'PG-13', CAST(N'2005-01-28 00:00:00.000' AS DateTime), 1, 2004)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (170, N'Black Swan', 108, N'R', CAST(N'2010-12-17 00:00:00.000' AS DateTime), 0, 2010)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (171, N'The Night of the Hunter', 93, NULL, CAST(N'1955-11-24 00:00:00.000' AS DateTime), 0, 1955)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (172, N'There Will Be Blood', 158, N'R', CAST(N'2008-01-25 00:00:00.000' AS DateTime), 0, 2007)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (173, N'Stand by Me', 89, N'R', CAST(N'1986-08-08 00:00:00.000' AS DateTime), 0, 1986)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (174, N'Donnie Darko', 113, N'R', CAST(N'2002-01-30 00:00:00.000' AS DateTime), 0, 2001)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (175, N'Groundhog Day', 101, N'PG', CAST(N'1993-02-12 00:00:00.000' AS DateTime), 0, 1993)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (176, N'Dog Day Afternoon', 125, N'R', CAST(N'1975-09-21 00:00:00.000' AS DateTime), 0, 1975)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (177, N'Twelve Monkeys', 129, N'R', CAST(N'1996-01-05 00:00:00.000' AS DateTime), 0, 1995)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (178, N'Amores Perros', 154, N'R', CAST(N'2000-06-16 00:00:00.000' AS DateTime), 0, 2000)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (179, N'The Bourne Ultimatum', 115, N'PG-13', CAST(N'2007-08-03 00:00:00.000' AS DateTime), 0, 2007)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (180, N'Mary and Max', 92, NULL, CAST(N'2009-04-09 00:00:00.000' AS DateTime), 0, 2009)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (181, N'The 400 Blows', 99, NULL, CAST(N'1959-11-16 00:00:00.000' AS DateTime), 0, 1959)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (182, N'Persona', 83, NULL, CAST(N'1967-03-16 00:00:00.000' AS DateTime), 0, 1966)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (183, N'The Graduate', 106, NULL, CAST(N'1967-12-22 00:00:00.000' AS DateTime), 0, 1967)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (184, N'Gandhi', 191, N'PG', CAST(N'1983-02-25 00:00:00.000' AS DateTime), 1, 1982)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (185, N'The Killing', 85, NULL, CAST(N'1956-06-06 00:00:00.000' AS DateTime), 0, 1956)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (186, N'Howl''s Moving Castle', 119, N'PG', CAST(N'2005-06-17 00:00:00.000' AS DateTime), 0, 2004)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (187, N'The Artist', 100, N'PG-13', CAST(N'2012-01-20 00:00:00.000' AS DateTime), 1, 2011)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (188, N'The Princess Bride', 98, N'PG', CAST(N'1987-09-25 00:00:00.000' AS DateTime), 0, 1987)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (189, N'Argo', 120, N'R', CAST(N'2012-10-12 00:00:00.000' AS DateTime), 0, 2012)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (190, N'Slumdog Millionaire', 120, N'R', CAST(N'2009-01-23 00:00:00.000' AS DateTime), 1, 2008)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (191, N'Who''s Afraid of Virginia Woolf?', 131, NULL, CAST(N'1966-06-22 00:00:00.000' AS DateTime), 0, 1966)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (192, N'La Strada', 108, N'PG', CAST(N'1956-07-16 00:00:00.000' AS DateTime), 0, 1954)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (193, N'The Manchurian Candidate', 126, NULL, CAST(N'1962-10-24 00:00:00.000' AS DateTime), 0, 1962)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (194, N'The Hustler', 134, NULL, CAST(N'1961-09-25 00:00:00.000' AS DateTime), 0, 1961)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (195, N'A Beautiful Mind', 135, N'PG-13', CAST(N'2002-01-04 00:00:00.000' AS DateTime), 1, 2001)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (196, N'The Wild Bunch', 145, N'R', CAST(N'1969-06-18 00:00:00.000' AS DateTime), 0, 1969)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (197, N'Rocky', 119, N'PG', CAST(N'1976-12-03 00:00:00.000' AS DateTime), 1, 1976)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (198, N'Anatomy of a Murder', 160, N'TV-PG', CAST(N'1959-09-01 00:00:00.000' AS DateTime), 0, 1959)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (199, N'Stalag 17', 120, NULL, CAST(N'1953-08-10 00:00:00.000' AS DateTime), 0, 1953)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (200, N'The Exorcist', 122, N'R', CAST(N'1974-03-16 00:00:00.000' AS DateTime), 0, 1973)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (201, N'Sleuth', 138, N'PG', CAST(N'1972-12-10 00:00:00.000' AS DateTime), 0, 1972)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (202, N'Rope', 80, NULL, CAST(N'1948-08-28 00:00:00.000' AS DateTime), 0, 1948)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (203, N'Barry Lyndon', 184, N'PG', CAST(N'1975-12-18 00:00:00.000' AS DateTime), 0, 1975)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (204, N'The Man Who Shot Liberty Valance', 123, NULL, CAST(N'1962-04-22 00:00:00.000' AS DateTime), 0, 1962)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (205, N'District 9', 112, N'R', CAST(N'2009-08-14 00:00:00.000' AS DateTime), 0, 2009)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (206, N'Stalker', 163, NULL, CAST(N'1980-04-17 00:00:00.000' AS DateTime), 0, 1979)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (207, N'Infernal Affairs', 101, N'R', CAST(N'2002-12-12 00:00:00.000' AS DateTime), 0, 2002)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (208, N'Roman Holiday', 118, NULL, CAST(N'1953-09-02 00:00:00.000' AS DateTime), 0, 1953)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (209, N'The Truman Show', 103, N'PG', CAST(N'1998-06-05 00:00:00.000' AS DateTime), 0, 1998)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (210, N'Ratatouille', 111, N'G', CAST(N'2007-06-29 00:00:00.000' AS DateTime), 0, 2007)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (211, N'Pirates of the Caribbean: The Curse of the Black Pearl', 143, N'PG-13', CAST(N'2003-07-09 00:00:00.000' AS DateTime), 0, 2003)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (212, N'Ip Man', 106, N'R', CAST(N'2008-12-12 00:00:00.000' AS DateTime), 0, 2008)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (213, N'The Diving Bell and the Butterfly', 112, N'PG-13', CAST(N'2007-05-23 00:00:00.000' AS DateTime), 0, 2007)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (214, N'Harry Potter and the Deathly Hallows: Part 2', 130, N'PG-13', CAST(N'2011-07-15 00:00:00.000' AS DateTime), 0, 2011)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (215, N'A Fistful of Dollars', 99, N'R', CAST(N'1967-01-18 00:00:00.000' AS DateTime), 0, 1964)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (216, N'A Streetcar Named Desire', 125, N'PG', CAST(N'1951-12-01 00:00:00.000' AS DateTime), 0, 1951)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (217, N'Monsters, Inc.', 92, N'G', CAST(N'2001-11-02 00:00:00.000' AS DateTime), 0, 2001)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (218, N'In the Name of the Father', 133, N'R', CAST(N'1994-02-25 00:00:00.000' AS DateTime), 0, 1993)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (219, N'Star Trek', 127, N'PG-13', CAST(N'2009-05-08 00:00:00.000' AS DateTime), 0, 2009)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (220, N'Beauty and the Beast', 84, N'G', CAST(N'1991-11-22 00:00:00.000' AS DateTime), 0, 1991)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (221, N'Rosemary''s Baby', 136, N'R', CAST(N'1968-06-12 00:00:00.000' AS DateTime), 0, 1968)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (222, N'Harvey', 104, NULL, CAST(N'1950-10-13 00:00:00.000' AS DateTime), 0, 1950)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (223, N'Nauticaä of the Valley of the Wind', 117, N'PG', CAST(N'1984-03-11 00:00:00.000' AS DateTime), 0, 1984)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (224, N'The Wrestler', 109, N'R', CAST(N'2009-01-30 00:00:00.000' AS DateTime), 0, 2008)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (225, N'All Quiet on the Western Front', 133, NULL, CAST(N'1930-08-24 00:00:00.000' AS DateTime), 1, 1930)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (226, N'La Haine', 98, NULL, CAST(N'1996-02-23 00:00:00.000' AS DateTime), 0, 1995)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (227, N'Rain Man', 133, N'R', CAST(N'1988-12-16 00:00:00.000' AS DateTime), 1, 1988)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (228, N'Battleship Potemkin', 66, NULL, CAST(N'1925-12-24 00:00:00.000' AS DateTime), 0, 1925)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (229, N'Shutter Island', 138, N'R', CAST(N'2010-02-19 00:00:00.000' AS DateTime), 0, 2010)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (230, N'Nosferatu', 81, NULL, CAST(N'1929-06-03 00:00:00.000' AS DateTime), 0, 1922)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (231, N'Spring, Summer, Fall, Winter... and Spring', 103, N'R', CAST(N'2003-09-19 00:00:00.000' AS DateTime), 0, 2003)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (232, N'Manhattan', 96, N'R', CAST(N'1979-04-25 00:00:00.000' AS DateTime), 0, 1979)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (233, N'Mystic River', 138, N'R', CAST(N'2003-10-15 00:00:00.000' AS DateTime), 0, 2003)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (234, N'Bringing Up Baby', 102, NULL, CAST(N'1938-02-18 00:00:00.000' AS DateTime), 0, 1938)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (235, N'Shadow of a Doubt', 108, NULL, CAST(N'1943-01-15 00:00:00.000' AS DateTime), 0, 1943)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (236, N'Big Fish', 125, N'PG-13', CAST(N'2004-01-09 00:00:00.000' AS DateTime), 0, 2003)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (237, N'Castle in the Sky', 124, N'PG', CAST(N'1986-08-02 00:00:00.000' AS DateTime), 0, 1986)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (238, N'Papillon', 151, N'PG', CAST(N'1973-12-16 00:00:00.000' AS DateTime), 0, 1973)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (239, N'The Nightmare Before Christmas', 76, N'PG', CAST(N'1993-10-29 00:00:00.000' AS DateTime), 0, 1993)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (240, N'The Untouchables', 119, N'R', CAST(N'1987-06-03 00:00:00.000' AS DateTime), 0, 1987)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (241, N'Jurassic Park', 127, N'PG-13', CAST(N'1993-06-11 00:00:00.000' AS DateTime), 0, 1993)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (242, N'Let the Right One In', 115, N'R', CAST(N'2008-10-24 00:00:00.000' AS DateTime), 0, 2008)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (243, N'In the Heat of the Night', 109, NULL, CAST(N'1967-10-14 00:00:00.000' AS DateTime), 1, 1967)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (244, N'3 Idiots', 170, N'PG-13', CAST(N'2009-12-24 00:00:00.000' AS DateTime), 0, 2009)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (245, N'Arsenic and Old Lace', 118, NULL, CAST(N'1944-09-23 00:00:00.000' AS DateTime), 0, 1944)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (246, N'The Searchers', 119, NULL, CAST(N'1956-03-13 00:00:00.000' AS DateTime), 0, 1956)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (247, N'In the Mood for Love', 98, N'PG', CAST(N'2000-09-29 00:00:00.000' AS DateTime), 0, 2000)
GO
INSERT [IntIdMovies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year]) VALUES (248, N'Rio Bravo', 141, NULL, CAST(N'1959-04-04 00:00:00.000' AS DateTime), 0, 1959)
GO
SET IDENTITY_INSERT [IntIdMovies] OFF
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 000', N'The Shawshank Redemption', 142, N'R', CAST(N'1994-10-14 00:00:00.000' AS DateTime), 0, 1994, CAST(N'2015-08-05T20:15:56.0224196+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.0224196+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 001', N'The Godfather', 175, N'R', CAST(N'1972-03-24 00:00:00.000' AS DateTime), 1, 1972, CAST(N'2015-08-05T20:15:56.0380396+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.0380396+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 002', N'The Godfather: Part II', 200, N'R', CAST(N'1974-12-20 00:00:00.000' AS DateTime), 1, 1974, CAST(N'2015-08-05T20:15:56.0536657+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.0536657+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 003', N'Pulp Fiction', 168, N'R', CAST(N'1994-10-14 00:00:00.000' AS DateTime), 0, 1994, CAST(N'2015-08-05T20:15:56.0692910+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.0692910+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 004', N'The Good, the Bad and the Ugly', 161, NULL, CAST(N'1967-12-29 00:00:00.000' AS DateTime), 0, 1966, CAST(N'2015-08-05T20:15:56.0849177+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.0849177+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 005', N'12 Angry Men', 96, NULL, CAST(N'1957-04-10 00:00:00.000' AS DateTime), 0, 1957, CAST(N'2015-08-05T20:15:56.0849177+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.0849177+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 006', N'The Dark Knight', 152, N'PG-13', CAST(N'2008-07-18 00:00:00.000' AS DateTime), 0, 2008, CAST(N'2015-08-05T20:15:56.1005192+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.1005192+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 007', N'Schindler''s List', 195, N'R', CAST(N'1993-12-15 00:00:00.000' AS DateTime), 1, 1993, CAST(N'2015-08-05T20:15:56.1161665+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.1161665+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 008', N'The Lord of the Rings: The Return of the King', 201, N'PG-13', CAST(N'2003-12-17 00:00:00.000' AS DateTime), 1, 2003, CAST(N'2015-08-05T20:15:56.1317900+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.1317900+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 009', N'Fight Club', 139, N'R', CAST(N'1999-10-15 00:00:00.000' AS DateTime), 0, 1999, CAST(N'2015-08-05T20:15:56.1474158+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.1474158+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 010', N'Star Wars: Episode V - The Empire Strikes Back', 127, N'PG', CAST(N'1980-05-21 00:00:00.000' AS DateTime), 0, 1980, CAST(N'2015-08-05T20:15:56.1630425+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.1630425+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 011', N'One Flew Over the Cuckoo''s Nest', 133, NULL, CAST(N'1975-11-21 00:00:00.000' AS DateTime), 1, 1975, CAST(N'2015-08-05T20:15:56.1786466+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.1786466+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 012', N'The Lord of the Rings: The Fellowship of the Ring', 178, N'PG-13', CAST(N'2001-12-19 00:00:00.000' AS DateTime), 0, 2001, CAST(N'2015-08-05T20:15:56.1786466+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.1786466+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 013', N'Inception', 148, N'PG-13', CAST(N'2010-07-16 00:00:00.000' AS DateTime), 0, 2010, CAST(N'2015-08-05T20:15:56.1942913+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.1942913+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 014', N'Goodfellas', 146, N'R', CAST(N'1990-09-19 00:00:00.000' AS DateTime), 0, 1990, CAST(N'2015-08-05T20:15:56.2099168+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.2099168+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 015', N'Star Wars', 121, N'PG', CAST(N'1977-05-25 00:00:00.000' AS DateTime), 0, 1977, CAST(N'2015-08-05T20:15:56.2255420+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.2255420+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 016', N'Seven Samurai', 141, NULL, CAST(N'1956-11-19 00:00:00.000' AS DateTime), 0, 1954, CAST(N'2015-08-05T20:15:56.2411679+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.2411679+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 017', N'The Matrix', 136, N'R', CAST(N'1999-03-31 00:00:00.000' AS DateTime), 0, 1999, CAST(N'2015-08-05T20:15:56.2567632+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.2567632+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 018', N'Forrest Gump', 142, N'PG-13', CAST(N'1994-07-06 00:00:00.000' AS DateTime), 1, 1994, CAST(N'2015-08-05T20:15:56.2567632+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.2567632+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 019', N'City of God', 130, N'R', CAST(N'2002-01-01 00:00:00.000' AS DateTime), 0, 2002, CAST(N'2015-08-05T20:15:56.2724228+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.2724228+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 020', N'The Lord of the Rings: The Two Towers', 179, N'PG-13', CAST(N'2002-12-18 00:00:00.000' AS DateTime), 0, 2002, CAST(N'2015-08-05T20:15:56.2880425+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.2880425+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 021', N'Once Upon a Time in the West', 175, N'PG-13', CAST(N'1968-12-21 00:00:00.000' AS DateTime), 0, 1968, CAST(N'2015-08-05T20:15:56.3036671+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.3036671+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 022', N'Se7en', 127, N'R', CAST(N'1995-09-22 00:00:00.000' AS DateTime), 0, 1995, CAST(N'2015-08-05T20:15:56.3192930+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.3192930+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 023', N'The Silence of the Lambs', 118, N'R', CAST(N'1991-02-14 00:00:00.000' AS DateTime), 1, 1991, CAST(N'2015-08-05T20:15:56.3349176+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.3349176+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 024', N'Casablanca', 102, N'PG', CAST(N'1943-01-23 00:00:00.000' AS DateTime), 1, 1942, CAST(N'2015-08-05T20:15:56.3505470+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.3505470+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 025', N'The Usual Suspects', 106, N'R', CAST(N'1995-08-16 00:00:00.000' AS DateTime), 0, 1995, CAST(N'2015-08-05T20:15:56.3505470+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.3505470+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 026', N'Raiders of the Lost Ark', 115, N'PG', CAST(N'1981-06-12 00:00:00.000' AS DateTime), 0, 1981, CAST(N'2015-08-05T20:15:56.3661681+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.3661681+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 027', N'Rear Window', 112, N'PG', CAST(N'1955-01-13 00:00:00.000' AS DateTime), 0, 1954, CAST(N'2015-08-05T20:15:56.3974192+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.3974192+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 028', N'Psycho', 109, N'TV-14', CAST(N'1960-09-08 00:00:00.000' AS DateTime), 0, 1960, CAST(N'2015-08-05T20:15:56.4130172+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.4130172+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 029', N'It''s a Wonderful Life', 130, N'PG', CAST(N'1947-01-06 00:00:00.000' AS DateTime), 0, 1946, CAST(N'2015-08-05T20:15:56.4286689+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.4286689+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 030', N'Léon: The Professional', 110, N'R', CAST(N'1994-11-18 00:00:00.000' AS DateTime), 0, 1994, CAST(N'2015-08-05T20:15:56.4286689+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.4286689+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 031', N'Sunset Blvd.', 110, NULL, CAST(N'1950-08-25 00:00:00.000' AS DateTime), 0, 1950, CAST(N'2015-08-05T20:15:56.4442947+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.4442947+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 032', N'Memento', 113, N'R', CAST(N'2000-10-11 00:00:00.000' AS DateTime), 0, 2000, CAST(N'2015-08-05T20:15:56.4599182+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.4599182+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 033', N'The Dark Knight Rises', 165, N'PG-13', CAST(N'2012-07-20 00:00:00.000' AS DateTime), 0, 2012, CAST(N'2015-08-05T20:15:56.4755473+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.4755473+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 034', N'American History X', 119, N'R', CAST(N'1999-02-12 00:00:00.000' AS DateTime), 0, 1998, CAST(N'2015-08-05T20:15:56.4911693+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.4911693+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 035', N'Apocalypse Now', 153, N'R', CAST(N'1979-08-15 00:00:00.000' AS DateTime), 0, 1979, CAST(N'2015-08-05T20:15:56.5067948+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.5067948+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 036', N'Terminator 2: Judgment Day', 152, N'R', CAST(N'1991-07-03 00:00:00.000' AS DateTime), 0, 1991, CAST(N'2015-08-05T20:15:56.5224210+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.5224210+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 037', N'Dr. Strangelove or: How I Learned to Stop Worrying and Love the Bomb', 95, N'PG', CAST(N'1964-01-29 00:00:00.000' AS DateTime), 0, 1964, CAST(N'2015-08-05T20:15:56.5224210+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.5224210+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 038', N'Saving Private Ryan', 169, N'R', CAST(N'1998-07-24 00:00:00.000' AS DateTime), 0, 1998, CAST(N'2015-08-05T20:15:56.5380427+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.5380427+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 039', N'Alien', 117, N'TV-14', CAST(N'1979-05-25 00:00:00.000' AS DateTime), 0, 1979, CAST(N'2015-08-05T20:15:56.5536694+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.5536694+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 040', N'North by Northwest', 136, NULL, CAST(N'1959-09-26 00:00:00.000' AS DateTime), 0, 1959, CAST(N'2015-08-05T20:15:56.5692929+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.5692929+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 041', N'City Lights', 87, NULL, CAST(N'1931-03-07 00:00:00.000' AS DateTime), 0, 1931, CAST(N'2015-08-05T20:15:56.5849193+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.5849193+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 042', N'Spirited Away', 125, N'PG', CAST(N'2001-07-20 00:00:00.000' AS DateTime), 0, 2001, CAST(N'2015-08-05T20:15:56.6005443+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.6005443+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 043', N'Citizen Kane', 119, N'PG', CAST(N'1941-09-05 00:00:00.000' AS DateTime), 0, 1941, CAST(N'2015-08-05T20:15:56.6005443+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.6005443+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 044', N'Modern Times', 87, NULL, CAST(N'1936-02-25 00:00:00.000' AS DateTime), 0, 1936, CAST(N'2015-08-05T20:15:56.6161478+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.6161478+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 045', N'The Shining', 142, N'R', CAST(N'1980-05-23 00:00:00.000' AS DateTime), 0, 1980, CAST(N'2015-08-05T20:15:56.6317936+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.6317936+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 046', N'Vertigo', 129, NULL, CAST(N'1958-07-21 00:00:00.000' AS DateTime), 0, 1958, CAST(N'2015-08-05T20:15:56.6474203+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.6474203+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 047', N'Back to the Future', 116, N'PG', CAST(N'1985-07-03 00:00:00.000' AS DateTime), 0, 1985, CAST(N'2015-08-05T20:15:56.6630453+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.6630453+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 048', N'American Beauty', 122, N'R', CAST(N'1999-10-01 00:00:00.000' AS DateTime), 1, 1999, CAST(N'2015-08-05T20:15:56.6786720+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.6786720+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 049', N'M', 117, NULL, CAST(N'1931-08-30 00:00:00.000' AS DateTime), 0, 1931, CAST(N'2015-08-05T20:15:56.6942955+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.6942955+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 050', N'The Pianist', 150, N'R', CAST(N'2003-03-28 00:00:00.000' AS DateTime), 0, 2002, CAST(N'2015-08-05T20:15:56.6942955+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.6942955+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 051', N'The Departed', 151, N'R', CAST(N'2006-10-06 00:00:00.000' AS DateTime), 1, 2006, CAST(N'2015-08-05T20:15:56.7099205+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.7099205+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 052', N'Taxi Driver', 113, N'R', CAST(N'1976-02-08 00:00:00.000' AS DateTime), 0, 1976, CAST(N'2015-08-05T20:15:56.7255440+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.7255440+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 053', N'Toy Story 3', 103, N'G', CAST(N'2010-06-18 00:00:00.000' AS DateTime), 0, 2010, CAST(N'2015-08-05T20:15:56.7411689+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.7411689+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 054', N'Paths of Glory', 88, NULL, CAST(N'1957-10-25 00:00:00.000' AS DateTime), 0, 1957, CAST(N'2015-08-05T20:15:56.7723904+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.7723904+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 055', N'Life Is Beautiful', 118, N'PG-13', CAST(N'1999-02-12 00:00:00.000' AS DateTime), 0, 1997, CAST(N'2015-08-05T20:15:56.8036403+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.8036403+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 056', N'Double Indemnity', 107, NULL, CAST(N'1944-04-24 00:00:00.000' AS DateTime), 0, 1944, CAST(N'2015-08-05T20:15:56.8036403+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.8036403+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 057', N'Aliens', 154, N'R', CAST(N'1986-07-18 00:00:00.000' AS DateTime), 0, 1986, CAST(N'2015-08-05T20:15:56.8192644+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.8192644+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 058', N'WALL-E', 98, N'G', CAST(N'2008-06-27 00:00:00.000' AS DateTime), 0, 2008, CAST(N'2015-08-05T20:15:56.8348893+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.8348893+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 059', N'The Lives of Others', 137, N'R', CAST(N'2006-03-23 00:00:00.000' AS DateTime), 0, 2006, CAST(N'2015-08-05T20:15:56.8505143+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.8505143+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 060', N'A Clockwork Orange', 136, N'R', CAST(N'1972-02-02 00:00:00.000' AS DateTime), 0, 1971, CAST(N'2015-08-05T20:15:56.8661736+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.8661736+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 061', N'Amélie', 122, N'R', CAST(N'2001-04-24 00:00:00.000' AS DateTime), 0, 2001, CAST(N'2015-08-05T20:15:56.8817953+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.8817953+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 062', N'Gladiator', 155, N'R', CAST(N'2000-05-05 00:00:00.000' AS DateTime), 1, 2000, CAST(N'2015-08-05T20:15:56.8974244+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.8974244+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 063', N'The Green Mile', 189, N'R', CAST(N'1999-12-10 00:00:00.000' AS DateTime), 0, 1999, CAST(N'2015-08-05T20:15:56.8974244+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.8974244+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 064', N'The Intouchables', 112, N'R', CAST(N'2011-11-02 00:00:00.000' AS DateTime), 0, 2011, CAST(N'2015-08-05T20:15:56.9130200+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.9130200+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 065', N'Lawrence of Arabia', 227, NULL, CAST(N'1963-01-30 00:00:00.000' AS DateTime), 1, 1962, CAST(N'2015-08-05T20:15:56.9286716+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.9286716+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 066', N'To Kill a Mockingbird', 129, NULL, CAST(N'1963-03-16 00:00:00.000' AS DateTime), 0, 1962, CAST(N'2015-08-05T20:15:56.9442972+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.9442972+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 067', N'The Prestige', 130, N'PG-13', CAST(N'2006-10-20 00:00:00.000' AS DateTime), 0, 2006, CAST(N'2015-08-05T20:15:56.9599221+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.9599221+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 068', N'The Great Dictator', 125, NULL, CAST(N'1941-03-07 00:00:00.000' AS DateTime), 0, 1940, CAST(N'2015-08-05T20:15:56.9755468+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.9755468+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 069', N'Reservoir Dogs', 99, N'R', CAST(N'1992-10-23 00:00:00.000' AS DateTime), 0, 1992, CAST(N'2015-08-05T20:15:56.9911788+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.9911788+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 070', N'Das Boot', 149, N'R', CAST(N'1982-02-10 00:00:00.000' AS DateTime), 0, 1981, CAST(N'2015-08-05T20:15:56.9911788+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:56.9911788+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 071', N'Requiem for a Dream', 102, N'NC-17', CAST(N'2000-10-27 00:00:00.000' AS DateTime), 0, 2000, CAST(N'2015-08-05T20:15:57.0068014+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.0068014+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 072', N'The Third Man', 93, NULL, CAST(N'1949-08-31 00:00:00.000' AS DateTime), 0, 1949, CAST(N'2015-08-05T20:15:57.0224211+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.0224211+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 073', N'The Treasure of the Sierra Madre', 126, NULL, CAST(N'1948-01-24 00:00:00.000' AS DateTime), 0, 1948, CAST(N'2015-08-05T20:15:57.0380464+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.0380464+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 074', N'Eternal Sunshine of the Spotless Mind', 108, N'R', CAST(N'2004-03-19 00:00:00.000' AS DateTime), 0, 2004, CAST(N'2015-08-05T20:15:57.0584076+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.0584076+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 075', N'Cinema Paradiso', 155, N'PG', CAST(N'1990-02-23 00:00:00.000' AS DateTime), 0, 1988, CAST(N'2015-08-05T20:15:57.0740607+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.0740607+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 076', N'Once Upon a Time in America', 139, N'R', CAST(N'1984-05-23 00:00:00.000' AS DateTime), 0, 1984, CAST(N'2015-08-05T20:15:57.0896871+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.0896871+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 077', N'Chinatown', 130, NULL, CAST(N'1974-06-20 00:00:00.000' AS DateTime), 0, 1974, CAST(N'2015-08-05T20:15:57.0896871+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.0896871+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 078', N'L.A. Confidential', 138, N'R', CAST(N'1997-09-19 00:00:00.000' AS DateTime), 0, 1997, CAST(N'2015-08-05T20:15:57.1053109+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.1053109+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 079', N'The Lion King', 89, N'G', CAST(N'1994-06-24 00:00:00.000' AS DateTime), 0, 1994, CAST(N'2015-08-05T20:15:57.1209373+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.1209373+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 080', N'Star Wars: Episode VI - Return of the Jedi', 134, N'PG', CAST(N'1983-05-25 00:00:00.000' AS DateTime), 0, 1983, CAST(N'2015-08-05T20:15:57.1365623+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.1365623+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 081', N'Full Metal Jacket', 116, N'R', CAST(N'1987-06-26 00:00:00.000' AS DateTime), 0, 1987, CAST(N'2015-08-05T20:15:57.1521864+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.1521864+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 082', N'Monty Python and the Holy Grail', 91, N'PG', CAST(N'1975-05-25 00:00:00.000' AS DateTime), 0, 1975, CAST(N'2015-08-05T20:15:57.1677879+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.1677879+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 083', N'Braveheart', 177, N'R', CAST(N'1995-05-24 00:00:00.000' AS DateTime), 1, 1995, CAST(N'2015-08-05T20:15:57.1834375+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.1834375+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 084', N'Singin'' in the Rain', 103, NULL, CAST(N'1952-04-11 00:00:00.000' AS DateTime), 0, 1952, CAST(N'2015-08-05T20:15:57.1990636+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.1990636+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 085', N'Oldboy', 120, N'R', CAST(N'2003-11-21 00:00:00.000' AS DateTime), 0, 2003, CAST(N'2015-08-05T20:15:57.2146886+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.2146886+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 086', N'Some Like It Hot', 120, NULL, CAST(N'1959-03-29 00:00:00.000' AS DateTime), 0, 1959, CAST(N'2015-08-05T20:15:57.2303135+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.2303135+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 087', N'Amadeus', 160, N'PG', CAST(N'1984-09-19 00:00:00.000' AS DateTime), 1, 1984, CAST(N'2015-08-05T20:15:57.2476777+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.2476777+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 088', N'Metropolis', 114, NULL, CAST(N'1927-03-13 00:00:00.000' AS DateTime), 0, 1927, CAST(N'2015-08-05T20:15:57.2633285+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.2633285+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 089', N'Rashomon', 88, NULL, CAST(N'1951-12-26 00:00:00.000' AS DateTime), 0, 1950, CAST(N'2015-08-05T20:15:57.2789288+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.2789288+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 090', N'Bicycle Thieves', 93, NULL, CAST(N'1949-12-13 00:00:00.000' AS DateTime), 0, 1948, CAST(N'2015-08-05T20:15:57.2789288+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.2789288+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 091', N'2001: A Space Odyssey', 141, NULL, CAST(N'1968-04-06 00:00:00.000' AS DateTime), 0, 1968, CAST(N'2015-08-05T20:15:57.2945535+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.2945535+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 092', N'Unforgiven', 131, N'R', CAST(N'1992-08-07 00:00:00.000' AS DateTime), 1, 1992, CAST(N'2015-08-05T20:15:57.3102051+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.3102051+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 093', N'All About Eve', 138, NULL, CAST(N'1951-01-15 00:00:00.000' AS DateTime), 1, 1950, CAST(N'2015-08-05T20:15:57.3258298+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.3258298+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 094', N'The Apartment', 125, NULL, CAST(N'1960-09-16 00:00:00.000' AS DateTime), 1, 1960, CAST(N'2015-08-05T20:15:57.3414536+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.3414536+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 095', N'Indiana Jones and the Last Crusade', 127, N'PG', CAST(N'1989-05-24 00:00:00.000' AS DateTime), 0, 1989, CAST(N'2015-08-05T20:15:57.3727065+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.3727065+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 096', N'The Sting', 129, NULL, CAST(N'1974-01-10 00:00:00.000' AS DateTime), 1, 1973, CAST(N'2015-08-05T20:15:57.4195810+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.4195810+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 097', N'Raging Bull', 129, N'R', CAST(N'1980-12-19 00:00:00.000' AS DateTime), 0, 1980, CAST(N'2015-08-05T20:15:57.4352066+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.4352066+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 098', N'The Bridge on the River Kwai', 161, NULL, CAST(N'1957-12-14 00:00:00.000' AS DateTime), 1, 1957, CAST(N'2015-08-05T20:15:57.4508312+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.4508312+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 099', N'Die Hard', 131, N'R', CAST(N'1988-07-15 00:00:00.000' AS DateTime), 0, 1988, CAST(N'2015-08-05T20:15:57.4664550+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.4664550+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 100', N'Witness for the Prosecution', 116, NULL, CAST(N'1958-02-06 00:00:00.000' AS DateTime), 0, 1957, CAST(N'2015-08-05T20:15:57.4664550+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.4664550+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 101', N'Batman Begins', 140, N'PG-13', CAST(N'2005-06-15 00:00:00.000' AS DateTime), 0, 2005, CAST(N'2015-08-05T20:15:57.4820548+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.4820548+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 102', N'A Separation', 123, N'PG-13', CAST(N'2011-03-16 00:00:00.000' AS DateTime), 0, 2011, CAST(N'2015-08-05T20:15:57.4977047+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.4977047+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 103', N'Grave of the Fireflies', 89, NULL, CAST(N'1988-04-16 00:00:00.000' AS DateTime), 0, 1988, CAST(N'2015-08-05T20:15:57.5133346+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.5133346+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 104', N'Pan''s Labyrinth', 118, N'R', CAST(N'2007-01-19 00:00:00.000' AS DateTime), 0, 2006, CAST(N'2015-08-05T20:15:57.5289566+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.5289566+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 105', N'Downfall', 156, N'R', CAST(N'2004-09-16 00:00:00.000' AS DateTime), 0, 2004, CAST(N'2015-08-05T20:15:57.5445804+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.5445804+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 106', N'Mr. Smith Goes to Washington', 129, NULL, CAST(N'1939-10-19 00:00:00.000' AS DateTime), 0, 1939, CAST(N'2015-08-05T20:15:57.5602068+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.5602068+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 107', N'Yojimbo', 75, N'TV-MA', CAST(N'1961-09-13 00:00:00.000' AS DateTime), 0, 1961, CAST(N'2015-08-05T20:15:57.5758344+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.5758344+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 108', N'The Great Escape', 172, NULL, CAST(N'1963-07-04 00:00:00.000' AS DateTime), 0, 1963, CAST(N'2015-08-05T20:15:57.5914579+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.5914579+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 109', N'For a Few Dollars More', 132, N'R', CAST(N'1967-05-10 00:00:00.000' AS DateTime), 0, 1965, CAST(N'2015-08-05T20:15:57.5914579+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.5914579+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 110', N'Snatch.', 102, N'R', CAST(N'2001-01-19 00:00:00.000' AS DateTime), 0, 2000, CAST(N'2015-08-05T20:15:57.6070553+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.6070553+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 111', N'Inglourious Basterds', 153, N'R', CAST(N'2009-08-21 00:00:00.000' AS DateTime), 0, 2009, CAST(N'2015-08-05T20:15:57.6227061+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.6227061+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 112', N'On the Waterfront', 108, NULL, CAST(N'1954-06-24 00:00:00.000' AS DateTime), 1, 1954, CAST(N'2015-08-05T20:15:57.6383325+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.6383325+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 113', N'The Elephant Man', 124, N'PG', CAST(N'1980-10-10 00:00:00.000' AS DateTime), 0, 1980, CAST(N'2015-08-05T20:15:57.6539598+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.6539598+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 114', N'The Seventh Seal', 96, NULL, CAST(N'1958-10-13 00:00:00.000' AS DateTime), 0, 1957, CAST(N'2015-08-05T20:15:57.6695810+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.6695810+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 115', N'Toy Story', 81, N'TV-G', CAST(N'1995-11-22 00:00:00.000' AS DateTime), 0, 1995, CAST(N'2015-08-05T20:15:57.6852062+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.6852062+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 116', N'The Maltese Falcon', 100, NULL, CAST(N'1941-10-18 00:00:00.000' AS DateTime), 0, 1941, CAST(N'2015-08-05T20:15:57.7008329+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.7008329+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 117', N'Heat', 170, N'R', CAST(N'1995-12-15 00:00:00.000' AS DateTime), 0, 1995, CAST(N'2015-08-05T20:15:57.7164579+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.7164579+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 118', N'The General', 75, NULL, CAST(N'1927-02-24 00:00:00.000' AS DateTime), 0, 1926, CAST(N'2015-08-05T20:15:57.7320567+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.7320567+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 119', N'Gran Torino', 116, N'R', CAST(N'2009-01-09 00:00:00.000' AS DateTime), 0, 2008, CAST(N'2015-08-05T20:15:57.7320567+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.7320567+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 120', N'Rebecca', 130, NULL, CAST(N'1940-04-12 00:00:00.000' AS DateTime), 1, 1940, CAST(N'2015-08-05T20:15:57.7477060+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.7477060+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 121', N'Blade Runner', 117, N'R', CAST(N'1982-06-25 00:00:00.000' AS DateTime), 0, 1982, CAST(N'2015-08-05T20:15:57.7633064+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.7633064+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 122', N'The Avengers', 143, N'PG-13', CAST(N'2012-05-04 00:00:00.000' AS DateTime), 0, 2012, CAST(N'2015-08-05T20:15:57.7789568+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.7789568+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 123', N'Wild Strawberries', 91, NULL, CAST(N'1959-06-22 00:00:00.000' AS DateTime), 0, 1957, CAST(N'2015-08-05T20:15:57.7945821+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.7945821+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 124', N'Fargo', 98, N'R', CAST(N'1996-04-05 00:00:00.000' AS DateTime), 0, 1996, CAST(N'2015-08-05T20:15:57.8102085+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.8102085+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 125', N'The Kid', 68, NULL, CAST(N'1921-02-06 00:00:00.000' AS DateTime), 0, 1921, CAST(N'2015-08-05T20:15:57.8258341+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.8258341+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 126', N'Scarface', 170, N'R', CAST(N'1983-12-09 00:00:00.000' AS DateTime), 0, 1983, CAST(N'2015-08-05T20:15:57.8414608+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.8414608+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 127', N'Touch of Evil', 108, N'PG-13', CAST(N'1958-06-08 00:00:00.000' AS DateTime), 0, 1958, CAST(N'2015-08-05T20:15:57.8570837+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.8570837+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 128', N'The Big Lebowski', 117, N'R', CAST(N'1998-03-06 00:00:00.000' AS DateTime), 0, 1998, CAST(N'2015-08-05T20:15:57.8570837+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.8570837+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 129', N'Ran', 162, N'R', CAST(N'1985-06-01 00:00:00.000' AS DateTime), 0, 1985, CAST(N'2015-08-05T20:15:57.8726799+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.8726799+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 130', N'The Deer Hunter', 182, N'R', CAST(N'1979-02-23 00:00:00.000' AS DateTime), 1, 1978, CAST(N'2015-08-05T20:15:57.8883339+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.8883339+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 131', N'Cool Hand Luke', 126, NULL, CAST(N'1967-11-01 00:00:00.000' AS DateTime), 0, 1967, CAST(N'2015-08-05T20:15:57.9039577+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.9039577+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 132', N'Sin City', 147, N'R', CAST(N'2005-04-01 00:00:00.000' AS DateTime), 0, 2005, CAST(N'2015-08-05T20:15:57.9195844+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.9195844+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 133', N'The Gold Rush', 72, NULL, CAST(N'1925-06-26 00:00:00.000' AS DateTime), 0, 1925, CAST(N'2015-08-05T20:15:57.9352088+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.9352088+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 134', N'Strangers on a Train', 101, NULL, CAST(N'1951-06-30 00:00:00.000' AS DateTime), 0, 1951, CAST(N'2015-08-05T20:15:57.9508352+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.9508352+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 135', N'It Happened One Night', 105, NULL, CAST(N'1934-02-23 00:00:00.000' AS DateTime), 1, 1934, CAST(N'2015-08-05T20:15:57.9664581+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.9664581+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 136', N'No Country for Old Men', 122, N'R', CAST(N'2007-11-21 00:00:00.000' AS DateTime), 1, 2007, CAST(N'2015-08-05T20:15:57.9664581+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.9820631+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 137', N'Jaws', 130, N'PG', CAST(N'1975-06-20 00:00:00.000' AS DateTime), 0, 1975, CAST(N'2015-08-05T20:15:57.9820631+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.9820631+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 138', N'Lock, Stock and Two Smoking Barrels', 107, N'R', CAST(N'1999-03-05 00:00:00.000' AS DateTime), 0, 1998, CAST(N'2015-08-05T20:15:57.9976828+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:57.9976828+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 139', N'The Sixth Sense', 107, N'PG-13', CAST(N'1999-08-06 00:00:00.000' AS DateTime), 0, 1999, CAST(N'2015-08-05T20:15:58.0133350+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.0133350+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 140', N'Hotel Rwanda', 121, N'PG-13', CAST(N'2005-02-04 00:00:00.000' AS DateTime), 0, 2004, CAST(N'2015-08-05T20:15:58.0289597+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.0289597+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 141', N'High Noon', 85, NULL, CAST(N'1952-07-30 00:00:00.000' AS DateTime), 0, 1952, CAST(N'2015-08-05T20:15:58.0445852+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.0445852+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 142', N'Platoon', 120, N'R', CAST(N'1986-12-24 00:00:00.000' AS DateTime), 1, 1986, CAST(N'2015-08-05T20:15:58.0602128+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.0602128+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 143', N'The Thing', 109, N'R', CAST(N'1982-06-25 00:00:00.000' AS DateTime), 0, 1982, CAST(N'2015-08-05T20:15:58.0758340+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.0758340+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 144', N'Butch Cassidy and the Sundance Kid', 110, N'PG', CAST(N'1969-10-24 00:00:00.000' AS DateTime), 0, 1969, CAST(N'2015-08-05T20:15:58.0914601+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.0914601+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 145', N'The Wizard of Oz', 101, NULL, CAST(N'1939-08-25 00:00:00.000' AS DateTime), 0, 1939, CAST(N'2015-08-05T20:15:58.0914601+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.0914601+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 146', N'Casino', 178, N'R', CAST(N'1995-11-22 00:00:00.000' AS DateTime), 0, 1995, CAST(N'2015-08-05T20:15:58.1070628+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.1070628+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 147', N'Trainspotting', 94, N'R', CAST(N'1996-07-19 00:00:00.000' AS DateTime), 0, 1996, CAST(N'2015-08-05T20:15:58.1227100+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.1227100+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 148', N'Kill Bill: Vol. 1', 111, N'TV-14', CAST(N'2003-10-10 00:00:00.000' AS DateTime), 0, 2003, CAST(N'2015-08-05T20:15:58.1383344+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.1383344+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 149', N'Warrior', 140, N'PG-13', CAST(N'2011-09-09 00:00:00.000' AS DateTime), 0, 2011, CAST(N'2015-08-05T20:15:58.1539608+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.1539608+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 150', N'Annie Hall', 93, N'PG', CAST(N'1977-04-20 00:00:00.000' AS DateTime), 1, 1977, CAST(N'2015-08-05T20:15:58.1695852+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.1695852+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 151', N'Notorious', 101, NULL, CAST(N'1946-09-06 00:00:00.000' AS DateTime), 0, 1946, CAST(N'2015-08-05T20:15:58.1852113+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.1852113+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 152', N'The Secret in Their Eyes', 129, N'R', CAST(N'2009-08-13 00:00:00.000' AS DateTime), 0, 2009, CAST(N'2015-08-05T20:15:58.2008354+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.2008354+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 153', N'Gone with the Wind', 238, N'G', CAST(N'1940-01-17 00:00:00.000' AS DateTime), 1, 1939, CAST(N'2015-08-05T20:15:58.2164648+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.2164648+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 154', N'Good Will Hunting', 126, N'R', CAST(N'1998-01-09 00:00:00.000' AS DateTime), 0, 1997, CAST(N'2015-08-05T20:15:58.2164648+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.2164648+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 155', N'The King''s Speech', 118, N'R', CAST(N'2010-12-24 00:00:00.000' AS DateTime), 1, 2010, CAST(N'2015-08-05T20:15:58.2320924+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.2320924+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 156', N'The Grapes of Wrath', 129, NULL, CAST(N'1940-03-15 00:00:00.000' AS DateTime), 0, 1940, CAST(N'2015-08-05T20:15:58.2477109+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.2477109+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 157', N'Into the Wild', 148, N'R', CAST(N'2007-09-21 00:00:00.000' AS DateTime), 0, 2007, CAST(N'2015-08-05T20:15:58.2633356+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.2633356+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 158', N'Life of Brian', 94, N'R', CAST(N'1979-08-17 00:00:00.000' AS DateTime), 0, 1979, CAST(N'2015-08-05T20:15:58.2789599+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.2789599+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 159', N'Finding Nemo', 100, N'G', CAST(N'2003-05-30 00:00:00.000' AS DateTime), 0, 2003, CAST(N'2015-08-05T20:15:58.2945869+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.2945869+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 160', N'V for Vendetta', 132, N'R', CAST(N'2006-03-17 00:00:00.000' AS DateTime), 0, 2005, CAST(N'2015-08-05T20:15:58.3102119+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.3102119+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 161', N'How to Train Your Dragon', 98, N'PG', CAST(N'2010-03-26 00:00:00.000' AS DateTime), 0, 2010, CAST(N'2015-08-05T20:15:58.3287641+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.3287641+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 162', N'My Neighbor Totoro', 86, N'G', CAST(N'1988-04-16 00:00:00.000' AS DateTime), 0, 1988, CAST(N'2015-08-05T20:15:58.3287641+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.3287641+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 163', N'The Big Sleep', 114, NULL, CAST(N'1946-08-31 00:00:00.000' AS DateTime), 0, 1946, CAST(N'2015-08-05T20:15:58.3443902+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.3443902+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 164', N'Dial M for Murder', 105, N'PG', CAST(N'1954-05-29 00:00:00.000' AS DateTime), 0, 1954, CAST(N'2015-08-05T20:15:58.3600436+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.3600436+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 165', N'Ben-Hur', 212, NULL, CAST(N'1960-03-30 00:00:00.000' AS DateTime), 1, 1959, CAST(N'2015-08-05T20:15:58.3777061+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.3777061+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 166', N'The Terminator', 107, N'R', CAST(N'1984-10-26 00:00:00.000' AS DateTime), 0, 1984, CAST(N'2015-08-05T20:15:58.3904188+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.3904188+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 167', N'Network', 121, N'R', CAST(N'1976-11-27 00:00:00.000' AS DateTime), 0, 1976, CAST(N'2015-08-05T20:15:58.4060733+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.4060733+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 168', N'Million Dollar Baby', 132, N'PG-13', CAST(N'2005-01-28 00:00:00.000' AS DateTime), 1, 2004, CAST(N'2015-08-05T20:15:58.4216968+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.4216968+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 169', N'Black Swan', 108, N'R', CAST(N'2010-12-17 00:00:00.000' AS DateTime), 0, 2010, CAST(N'2015-08-05T20:15:58.4373224+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.4373224+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 170', N'The Night of the Hunter', 93, NULL, CAST(N'1955-11-24 00:00:00.000' AS DateTime), 0, 1955, CAST(N'2015-08-05T20:15:58.4529491+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.4529491+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 171', N'There Will Be Blood', 158, N'R', CAST(N'2008-01-25 00:00:00.000' AS DateTime), 0, 2007, CAST(N'2015-08-05T20:15:58.4685471+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.4841715+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 172', N'Stand by Me', 89, N'R', CAST(N'1986-08-08 00:00:00.000' AS DateTime), 0, 1986, CAST(N'2015-08-05T20:15:58.4997970+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.4997970+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 173', N'Donnie Darko', 113, N'R', CAST(N'2002-01-30 00:00:00.000' AS DateTime), 0, 2001, CAST(N'2015-08-05T20:15:58.5154222+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.5154222+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 174', N'Groundhog Day', 101, N'PG', CAST(N'1993-02-12 00:00:00.000' AS DateTime), 0, 1993, CAST(N'2015-08-05T20:15:58.5466748+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.5466748+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 175', N'Dog Day Afternoon', 125, N'R', CAST(N'1975-09-21 00:00:00.000' AS DateTime), 0, 1975, CAST(N'2015-08-05T20:15:58.5622980+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.5622980+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 176', N'Twelve Monkeys', 129, N'R', CAST(N'1996-01-05 00:00:00.000' AS DateTime), 0, 1995, CAST(N'2015-08-05T20:15:58.5779194+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.5779194+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 177', N'Amores Perros', 154, N'R', CAST(N'2000-06-16 00:00:00.000' AS DateTime), 0, 2000, CAST(N'2015-08-05T20:15:58.5935450+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.5935450+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 178', N'The Bourne Ultimatum', 115, N'PG-13', CAST(N'2007-08-03 00:00:00.000' AS DateTime), 0, 2007, CAST(N'2015-08-05T20:15:58.6091720+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.6091720+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 179', N'Mary and Max', 92, NULL, CAST(N'2009-04-09 00:00:00.000' AS DateTime), 0, 2009, CAST(N'2015-08-05T20:15:58.6248204+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.6248204+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 180', N'The 400 Blows', 99, NULL, CAST(N'1959-11-16 00:00:00.000' AS DateTime), 0, 1959, CAST(N'2015-08-05T20:15:58.6404448+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.6404448+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 181', N'Persona', 83, NULL, CAST(N'1967-03-16 00:00:00.000' AS DateTime), 0, 1966, CAST(N'2015-08-05T20:15:58.6560783+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.6560783+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 182', N'The Graduate', 106, NULL, CAST(N'1967-12-22 00:00:00.000' AS DateTime), 0, 1967, CAST(N'2015-08-05T20:15:58.6716988+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.6716988+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 183', N'Gandhi', 191, N'PG', CAST(N'1983-02-25 00:00:00.000' AS DateTime), 1, 1982, CAST(N'2015-08-05T20:15:58.6873244+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.6873244+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 184', N'The Killing', 85, NULL, CAST(N'1956-06-06 00:00:00.000' AS DateTime), 0, 1956, CAST(N'2015-08-05T20:15:58.7029490+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.7029490+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 185', N'Howl''s Moving Castle', 119, N'PG', CAST(N'2005-06-17 00:00:00.000' AS DateTime), 0, 2004, CAST(N'2015-08-05T20:15:58.7185775+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.7341737+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 186', N'The Artist', 100, N'PG-13', CAST(N'2012-01-20 00:00:00.000' AS DateTime), 1, 2011, CAST(N'2015-08-05T20:15:58.7497984+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.7497984+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 187', N'The Princess Bride', 98, N'PG', CAST(N'1987-09-25 00:00:00.000' AS DateTime), 0, 1987, CAST(N'2015-08-05T20:15:58.7654225+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.7654225+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 188', N'Argo', 120, N'R', CAST(N'2012-10-12 00:00:00.000' AS DateTime), 0, 2012, CAST(N'2015-08-05T20:15:58.7810486+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.7966791+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 189', N'Slumdog Millionaire', 120, N'R', CAST(N'2009-01-23 00:00:00.000' AS DateTime), 1, 2008, CAST(N'2015-08-05T20:15:58.7966791+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.8123006+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 190', N'Who''s Afraid of Virginia Woolf?', 131, NULL, CAST(N'1966-06-22 00:00:00.000' AS DateTime), 0, 1966, CAST(N'2015-08-05T20:15:58.8279200+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.8279200+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 191', N'La Strada', 108, N'PG', CAST(N'1956-07-16 00:00:00.000' AS DateTime), 0, 1954, CAST(N'2015-08-05T20:15:58.8435464+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.8435464+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 192', N'The Manchurian Candidate', 126, NULL, CAST(N'1962-10-24 00:00:00.000' AS DateTime), 0, 1962, CAST(N'2015-08-05T20:15:58.8591716+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.8591716+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 193', N'The Hustler', 134, NULL, CAST(N'1961-09-25 00:00:00.000' AS DateTime), 0, 1961, CAST(N'2015-08-05T20:15:58.8747963+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.8747963+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 194', N'A Beautiful Mind', 135, N'PG-13', CAST(N'2002-01-04 00:00:00.000' AS DateTime), 1, 2001, CAST(N'2015-08-05T20:15:58.8904471+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.8904471+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 195', N'The Wild Bunch', 145, N'R', CAST(N'1969-06-18 00:00:00.000' AS DateTime), 0, 1969, CAST(N'2015-08-05T20:15:58.9060720+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.9060720+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 196', N'Rocky', 119, N'PG', CAST(N'1976-12-03 00:00:00.000' AS DateTime), 1, 1976, CAST(N'2015-08-05T20:15:58.9216973+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.9216973+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 197', N'Anatomy of a Murder', 160, N'TV-PG', CAST(N'1959-09-01 00:00:00.000' AS DateTime), 0, 1959, CAST(N'2015-08-05T20:15:58.9373369+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.9373369+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 198', N'Stalag 17', 120, NULL, CAST(N'1953-08-10 00:00:00.000' AS DateTime), 0, 1953, CAST(N'2015-08-05T20:15:58.9373369+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.9373369+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 199', N'The Exorcist', 122, N'R', CAST(N'1974-03-16 00:00:00.000' AS DateTime), 0, 1973, CAST(N'2015-08-05T20:15:58.9529258+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.9529258+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 200', N'Sleuth', 138, N'PG', CAST(N'1972-12-10 00:00:00.000' AS DateTime), 0, 1972, CAST(N'2015-08-05T20:15:58.9685768+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.9841813+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 201', N'Rope', 80, NULL, CAST(N'1948-08-28 00:00:00.000' AS DateTime), 0, 1948, CAST(N'2015-08-05T20:15:58.9997980+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:58.9997980+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 202', N'Barry Lyndon', 184, N'PG', CAST(N'1975-12-18 00:00:00.000' AS DateTime), 0, 1975, CAST(N'2015-08-05T20:15:59.0154242+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.0310547+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 203', N'The Man Who Shot Liberty Valance', 123, NULL, CAST(N'1962-04-22 00:00:00.000' AS DateTime), 0, 1962, CAST(N'2015-08-05T20:15:59.0466761+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.0466761+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 204', N'District 9', 112, N'R', CAST(N'2009-08-14 00:00:00.000' AS DateTime), 0, 2009, CAST(N'2015-08-05T20:15:59.0622993+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.0779240+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 205', N'Stalker', 163, NULL, CAST(N'1980-04-17 00:00:00.000' AS DateTime), 0, 1979, CAST(N'2015-08-05T20:15:59.0935536+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.0935536+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 206', N'Infernal Affairs', 101, N'R', CAST(N'2002-12-12 00:00:00.000' AS DateTime), 0, 2002, CAST(N'2015-08-05T20:15:59.1242423+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.1272413+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 207', N'Roman Holiday', 118, NULL, CAST(N'1953-09-02 00:00:00.000' AS DateTime), 0, 1953, CAST(N'2015-08-05T20:15:59.1506322+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.1506322+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 208', N'The Truman Show', 103, N'PG', CAST(N'1998-06-05 00:00:00.000' AS DateTime), 0, 1998, CAST(N'2015-08-05T20:15:59.1662826+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.1662826+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 209', N'Ratatouille', 111, N'G', CAST(N'2007-06-29 00:00:00.000' AS DateTime), 0, 2007, CAST(N'2015-08-05T20:15:59.1819088+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.1819088+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 210', N'Pirates of the Caribbean: The Curse of the Black Pearl', 143, N'PG-13', CAST(N'2003-07-09 00:00:00.000' AS DateTime), 0, 2003, CAST(N'2015-08-05T20:15:59.1819088+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.1819088+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 211', N'Ip Man', 106, N'R', CAST(N'2008-12-12 00:00:00.000' AS DateTime), 0, 2008, CAST(N'2015-08-05T20:15:59.1975413+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.1975413+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 212', N'The Diving Bell and the Butterfly', 112, N'PG-13', CAST(N'2007-05-23 00:00:00.000' AS DateTime), 0, 2007, CAST(N'2015-08-05T20:15:59.2131631+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.2131631+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 213', N'Harry Potter and the Deathly Hallows: Part 2', 130, N'PG-13', CAST(N'2011-07-15 00:00:00.000' AS DateTime), 0, 2011, CAST(N'2015-08-05T20:15:59.2287874+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.2287874+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 214', N'A Fistful of Dollars', 99, N'R', CAST(N'1967-01-18 00:00:00.000' AS DateTime), 0, 1964, CAST(N'2015-08-05T20:15:59.2444127+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.2444127+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 215', N'A Streetcar Named Desire', 125, N'PG', CAST(N'1951-12-01 00:00:00.000' AS DateTime), 0, 1951, CAST(N'2015-08-05T20:15:59.2611491+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.2767740+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 216', N'Monsters, Inc.', 92, N'G', CAST(N'2001-11-02 00:00:00.000' AS DateTime), 0, 2001, CAST(N'2015-08-05T20:15:59.2924010+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.2924010+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 217', N'In the Name of the Father', 133, N'R', CAST(N'1994-02-25 00:00:00.000' AS DateTime), 0, 1993, CAST(N'2015-08-05T20:15:59.3080263+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.3236539+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 218', N'Star Trek', 127, N'PG-13', CAST(N'2009-05-08 00:00:00.000' AS DateTime), 0, 2009, CAST(N'2015-08-05T20:15:59.3392768+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.3392768+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 219', N'Beauty and the Beast', 84, N'G', CAST(N'1991-11-22 00:00:00.000' AS DateTime), 0, 1991, CAST(N'2015-08-05T20:15:59.3705255+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.3705255+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 220', N'Rosemary''s Baby', 136, N'R', CAST(N'1968-06-12 00:00:00.000' AS DateTime), 0, 1968, CAST(N'2015-08-05T20:15:59.4017746+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.4017746+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 221', N'Harvey', 104, NULL, CAST(N'1950-10-13 00:00:00.000' AS DateTime), 0, 1950, CAST(N'2015-08-05T20:15:59.4174034+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.4174034+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 222', N'Nauticaä of the Valley of the Wind', 117, N'PG', CAST(N'1984-03-11 00:00:00.000' AS DateTime), 0, 1984, CAST(N'2015-08-05T20:15:59.4330465+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.4330465+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 223', N'The Wrestler', 109, N'R', CAST(N'2009-01-30 00:00:00.000' AS DateTime), 0, 2008, CAST(N'2015-08-05T20:15:59.4486729+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.4486729+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 224', N'All Quiet on the Western Front', 133, NULL, CAST(N'1930-08-24 00:00:00.000' AS DateTime), 1, 1930, CAST(N'2015-08-05T20:15:59.4642750+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.4642750+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 225', N'La Haine', 98, NULL, CAST(N'1996-02-23 00:00:00.000' AS DateTime), 0, 1995, CAST(N'2015-08-05T20:15:59.4799296+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.4799296+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 226', N'Rain Man', 133, N'R', CAST(N'1988-12-16 00:00:00.000' AS DateTime), 1, 1988, CAST(N'2015-08-05T20:15:59.4955534+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.4955534+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 227', N'Battleship Potemkin', 66, NULL, CAST(N'1925-12-24 00:00:00.000' AS DateTime), 0, 1925, CAST(N'2015-08-05T20:15:59.5111789+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.5111789+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 228', N'Shutter Island', 138, N'R', CAST(N'2010-02-19 00:00:00.000' AS DateTime), 0, 2010, CAST(N'2015-08-05T20:15:59.5268065+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.5268065+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 229', N'Nosferatu', 81, NULL, CAST(N'1929-06-03 00:00:00.000' AS DateTime), 0, 1922, CAST(N'2015-08-05T20:15:59.5594359+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.5604359+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 230', N'Spring, Summer, Fall, Winter... and Spring', 103, N'R', CAST(N'2003-09-19 00:00:00.000' AS DateTime), 0, 2003, CAST(N'2015-08-05T20:15:59.5754341+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.5774365+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 231', N'Manhattan', 96, N'R', CAST(N'1979-04-25 00:00:00.000' AS DateTime), 0, 1979, CAST(N'2015-08-05T20:15:59.5924380+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.5928283+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 232', N'Mystic River', 138, N'R', CAST(N'2003-10-15 00:00:00.000' AS DateTime), 0, 2003, CAST(N'2015-08-05T20:15:59.6084532+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.6084532+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 233', N'Bringing Up Baby', 102, NULL, CAST(N'1938-02-18 00:00:00.000' AS DateTime), 0, 1938, CAST(N'2015-08-05T20:15:59.6240779+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.6240779+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 234', N'Shadow of a Doubt', 108, NULL, CAST(N'1943-01-15 00:00:00.000' AS DateTime), 0, 1943, CAST(N'2015-08-05T20:15:59.6397035+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.6397035+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 235', N'Big Fish', 125, N'PG-13', CAST(N'2004-01-09 00:00:00.000' AS DateTime), 0, 2003, CAST(N'2015-08-05T20:15:59.6553284+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.6553284+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 236', N'Castle in the Sky', 124, N'PG', CAST(N'1986-08-02 00:00:00.000' AS DateTime), 0, 1986, CAST(N'2015-08-05T20:15:59.6750744+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.6770738+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 237', N'Papillon', 151, N'PG', CAST(N'1973-12-16 00:00:00.000' AS DateTime), 0, 1973, CAST(N'2015-08-05T20:15:59.6900827+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.6910612+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 238', N'The Nightmare Before Christmas', 76, N'PG', CAST(N'1993-10-29 00:00:00.000' AS DateTime), 0, 1993, CAST(N'2015-08-05T20:15:59.7040857+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.7040857+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 239', N'The Untouchables', 119, N'R', CAST(N'1987-06-03 00:00:00.000' AS DateTime), 0, 1987, CAST(N'2015-08-05T20:15:59.7101571+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.7101571+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 240', N'Jurassic Park', 127, N'PG-13', CAST(N'1993-06-11 00:00:00.000' AS DateTime), 0, 1993, CAST(N'2015-08-05T20:15:59.7258093+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.7258093+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 241', N'Let the Right One In', 115, N'R', CAST(N'2008-10-24 00:00:00.000' AS DateTime), 0, 2008, CAST(N'2015-08-05T20:15:59.7414352+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.7414352+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 242', N'In the Heat of the Night', 109, NULL, CAST(N'1967-10-14 00:00:00.000' AS DateTime), 1, 1967, CAST(N'2015-08-05T20:15:59.7604774+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.7644776+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 243', N'3 Idiots', 170, N'PG-13', CAST(N'2009-12-24 00:00:00.000' AS DateTime), 0, 2009, CAST(N'2015-08-05T20:15:59.7804759+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.7834767+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 244', N'Arsenic and Old Lace', 118, NULL, CAST(N'1944-09-23 00:00:00.000' AS DateTime), 0, 1944, CAST(N'2015-08-05T20:15:59.8001735+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.8001735+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 245', N'The Searchers', 119, NULL, CAST(N'1956-03-13 00:00:00.000' AS DateTime), 0, 1956, CAST(N'2015-08-05T20:15:59.8314240+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.8470501+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 246', N'In the Mood for Love', 98, N'PG', CAST(N'2000-09-29 00:00:00.000' AS DateTime), 0, 2000, CAST(N'2015-08-05T20:15:59.8626766+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.8626766+00:00' AS DateTimeOffset), 0)
GO
INSERT [Movies] ([Id], [Title], [Duration], [MpaaRating], [ReleaseDate], [BestPictureWinner], [Year], [createdAt], [updatedAt], [deleted]) VALUES (N'Movie 247', N'Rio Bravo', 141, NULL, CAST(N'1959-04-04 00:00:00.000' AS DateTime), 0, 1959, CAST(N'2015-08-05T20:15:59.8844621+00:00' AS DateTimeOffset), CAST(N'2015-08-05T20:15:59.8874627+00:00' AS DateTimeOffset), 0)
GO
SET ANSI_PADDING ON
