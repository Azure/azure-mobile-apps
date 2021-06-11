// ----------------------------------------------------------------------------
// Copyright (c) 2015 Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var express = require('express'),
    bodyParser = require('body-parser'),

    authenticate = require('azure-mobile-apps/src/express/middleware/authenticate'),
    authorize = require('azure-mobile-apps/src/express/middleware/authorize');

module.exports = function (configuration) {
    var router = express.Router();

    // Retrieve all records in the specified category
    router.get('/:category', (req, res, next) => {
        req.azureMobile.tables('items')
            .where({ category: req.params.category })
            .read()
            .then(results => res.json(results))
            .catch(next); // it is important to catch any errors and log them
    });

    // A simple API that constructs a record from the POSTed JSON
    router.post('/:category/:id', authenticate(configuration), authorize, bodyParser.json(), (req, res, next) => {
        var context = req.azureMobile;

        var item = {
            id: req.params.id,
            userId: context.user.id,
            category: req.params.category,
            content: JSON.stringify(req.body)
        };

        context.tables('items')
            .insert(item)
            .then(() => res.status(201).send('Success!'))
            .catch(next);
    });

    return router;
};
