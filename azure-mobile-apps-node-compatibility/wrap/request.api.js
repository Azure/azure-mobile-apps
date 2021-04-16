var service = require('./service'),
    user = require('./user')

module.exports = function (context) {
    var request = context.req;
    request.service = service(context)
    request.user = user(context)
    request.respond = function (statusCode, body) {
        context.res.status(statusCode).send(body);
    };

    return request;
}
