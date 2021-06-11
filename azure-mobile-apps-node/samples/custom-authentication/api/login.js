// ----------------------------------------------------------------------------
// Copyright (c) 2015 Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var auth = require('azure-mobile-apps/src/auth'),
    bcrypt = require('bcrypt');

module.exports = {
    // validates a username and password and returns a JWT token if successful
    post: function (req, res, next) {
        var context = req.azureMobile,
            // the sign function creates a signed JWT token from provided claims
            sign = auth(context.configuration.auth).sign;

        context.tables('users')
            .where({ id: req.body.username })
            .read()
            .then(function (users) {
                if(users.length === 1 && validatePassword(req.body.password, users[0].password))
                    res.json(createResponse(sign, users[0]));
                else
                    res.status(401).send("Incorrect username or password");
            })
            .catch(next);
    },

    // create a new user with the specified username and password and return a JWT token
    put: function (req, res, next) {
        var context = req.azureMobile,
            sign = auth(context.configuration.auth).sign;

        context.tables('users')
            .insert({
                id: req.body.username,
                password: hashPassword(req.body.password)
            })
            .then(function (user) {
                res.json(createResponse(sign, user));
            })
            .catch(next);
    }
}

function createResponse(sign, user) {
    return {
        // this JWT token must be applied on the Mobile Apps client using the appropriate client APIs
        token: sign({
            // sub is the only required property. this becomes context.user.id
            // you can add other claims here. they become available as context.user.claims
            sub: user.username
        })
    };
}

function hashPassword(password) {
    return bcrypt.hashSync(password, 8);
}

function validatePassword(password, hashed) {
    return bcrypt.compareSync(password, hashed)
}
