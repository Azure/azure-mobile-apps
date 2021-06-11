// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
//
// This body parser is intended to be used in the request pipeline *after*
// the default express body parser. It will handle any content types not parsed
// by the default parser.

exports = module.exports = function bodyParser() {
    return function (req, res, next) {
        if (req._body) {
            // this flag is set by the default Express body parser
            // to indicate the body has already been parsed
            return next();
        }
        req.body = req.body || '';

        // flag as parsed
        req._body = true;

        var buf = '';
        req.setEncoding('utf8');
        req.on('data', function (chunk) {
            buf += chunk;
        });
        req.on('end', function () {
            try {
                // currently this parser just takes the raw string value
                // as the body
                req.body = buf.length ? buf : '';
                next();
            } catch (err) {
                err.body = buf;
                next(err);
            }
        });
    };
};
