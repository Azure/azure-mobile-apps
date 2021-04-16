module.exports = function (req, res, next) {
    res.set('zumo-compatibility', require('./package.json').version)
    next()
}
