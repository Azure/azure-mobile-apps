// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var queries = require('azure-mobile-apps/src/query'),
    bodyParser = require('body-parser');

exports.register = function(app) {
    app.get('/api/movieFinder/title/:title', [bodyParser.json(), getByTitle]);
    app.get('/api/movieFinder/date/:year/:month/:day', [bodyParser.json(), getByDate]);
    app.post('/api/movieFinder/moviesOnSameYear', [bodyParser.json(), fetchMoviesSameYear]);
    app.post('/api/movieFinder/moviesWithSameDuration', [bodyParser.json(), fetchMoviesSameDuration]);
}

function getByTitle(req, res) {
    getMovie(req, res, 'Title', req.params.title);
}

function getByDate(req, res) {
    var year = parseInt(req.params.year, 10),
        month = parseInt(req.params.month, 10),
        day = parseInt(req.params.day, 10);

    getMovie(req, res, 'ReleaseDate', new Date(Date.UTC(year, month - 1, day)));
}

function fetchMoviesSameYear(req, res) {
    if (typeof req.body !== 'object')
      return res.status(400).end();

    var movie = req.body,
        table = req.azureMobile.tables('movies'),
        query = queries.create('movies')
            .where({ year: (movie.year || movie.Year) })
            .orderBy(req.query.orderBy || 'Title');

    table.read(query).then(function(results) {
        res.status(200).json({ movies: results });
    }).catch(replyWithError(res));
}

function fetchMoviesSameDuration(req, res) {
    if (typeof req.body !== 'object')
      return res.status(400).end();

    var movie = req.body,
        table = req.azureMobile.tables('movies'),
        query = queries.create('movies')
            .where({ duration: (movie.duration || movie.Duration) })
            .orderBy(req.query.orderBy || 'Title');

    table.read(query).then(function(results) {
        res.status(200).json({ movies: results });
    }).catch(replyWithError(res));
}

function getMovie(req, res, field, value) {
    var table = req.azureMobile.tables('movies');
        filter = {};

    filter[field] = value;

    table.read(queries.create('movies').where(filter)).then(function(results) {
        res.status(200).json({ movies: results });
    }).catch(replyWithError(res));
}

function replyWithError(response) {
    return function(err) {
        response.status(500).json({ error: err });
    }
}
