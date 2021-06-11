// ----------------------------------------------------------------------------
// Copyright (c) 2015 Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var promises = require('azure-mobile-apps/src/utilities/promises');

// a custom push notification registration that adds any tags added for this installation
module.exports = {
    post: function (req, res, next) {
        var context = req.azureMobile,
            installationId = req.get('X-ZUMO-INSTALLATION-ID');

        // retrieve the list of tags that this installation has registered for
        context.tables('tags')
            .where({ userId: context.user.id })
            .read()
            .then(function (tagRows) {
                // create an installation object that notification hubs accepts
                var installation = {
                        installationId: installationId,
                        pushChannel: req.body.pushChannel,
                        platform: 'wns',
                        // map the tag from each row in the table into an array
                        tags: tagRows.map(row => row.tag)
                    };

                context.logger.info('Registering for notifications with installation ' + JSON.stringify(installation));

                // wrap the callback style notification hubs function into a promise
                return promises.wrap(context.push.createOrUpdateInstallation, context.push)(installation)
            })
            .then(function (result) {
                res.status(204).end();
            })
            .catch(next);
    }
};

module.exports.post.authenticated = true;
