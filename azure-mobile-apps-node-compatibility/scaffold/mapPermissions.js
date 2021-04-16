module.exports = function (source, route) {
    var permissions = source.routes[route || '/'],
        mapping = {
            "public": "anonymous",
            "application": "anonymous",
            "user": "authenticated",
            "admin": "disabled"
        }

    return Object.keys(permissions).reduce(function (target, operationName) {
        target[operationName] = { access: mapping[permissions[operationName].permission] }
        if(operationName === 'insert' && !route)
            target.undelete = { access: mapping[permissions.insert.permission] }
        return target;
    }, { autoIncrement: false })
}
