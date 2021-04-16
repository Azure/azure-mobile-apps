var azure = require('azure')

module.exports = function (context) {
    var config = context.configuration.notifications
    if(config.hubName && config.connectionString)
        return azure.createNotificationHubService(config.hubName, config.connectionString)
}
