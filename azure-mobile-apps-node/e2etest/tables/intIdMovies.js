var movies = require('../movieData');

module.exports = {
    autoIncrement: true,
    seed: movies.intId()
};
