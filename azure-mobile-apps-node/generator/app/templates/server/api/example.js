'use strict';

// Provides a route to /api/example

var dataObject = {
    example: 20
};

// An API object can have any standard Express Verb
var api = {
    get: function (request, response, next) {
        response.status(200).type('application/json').json(dataObject);
        next();
    }
};

api.get.access = 'authenticated';

module.exports = api;
