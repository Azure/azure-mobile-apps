module.exports = function(promise, callbacks, logger) {
    promise
        .then(function (results) {
            if(callbacks && callbacks.success)
                callbacks.success(results)
        })
        .catch(function (error) {
            if(callbacks && callbacks.error)
                callbacks.error(error)
            else if(logger)
                logger.error(error)
        })
}
