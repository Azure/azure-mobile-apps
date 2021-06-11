var bodyParser = require('body-parser'),
    promises = require('azure-mobile-apps/src/utilities/promises'),
    expect = require('chai').expect;

module.exports = {
    register: function (app) {
        app.post('/api/push', [bodyParser.json(), push]);
        app.get('/api/verifyRegisterInstallationResult', getVerifyRegisterInstallationResult);
        app.get('/api/verifyUnregisterInstallationResult', getVerifyUnregisterInstallationResult);
        app.delete('/api/deleteRegistrationsForChannel', deleteRegistrationsForChannel);
    }
};

function push(req, res, next) {
    var data = req.body,
        push = req.azureMobile.push;

    switch(data.type) {
        case 'template':
            promises.wrap(push.send, push)(data.tag, data.payload).then(endRequest).catch(next);
            break;
        case 'gcm':
            promises.wrap(push.gcm.send, push.gcm)(data.tag, data.payload).then(endRequest).catch(next);
            break;
        case 'apns':
            promises.wrap(push.apns.send, push.apns)(data.tag, data.payload).then(endRequest).catch(next);
            break;
        case 'wns':
            promises.wrap(push.wns.send, push.wns)(data.tag, data.payload, 'wns/' + data.wnsType).then(endRequest).catch(next);
            break;
    }

    function endRequest() {
        res.status(200).end();
    }
}

function getVerifyRegisterInstallationResult(req, res, next) {
    var installationId = req.get('x-zumo-installation-id'),
        push = req.azureMobile.push;

    retry(function () {
        return promises.wrap(push.getInstallation, push)(installationId).then(function (installation) {
            console.log('Verifying installation');

            if(installation.pushChannel != req.query.channelUri) {
                next('ChannelUri did not match - ' + installation.pushChannel + ', ' + req.query.channelUri);
                return;
            }

            verifyTemplates();
            verifySecondaryTiles();
            return verifyTags().then(function () {
                res.status(200).json(true);
            });
            //.catch(function (error) {
            //    res.status(500).send(error);
            //});

            function verifyTemplates() {
                if(req.query.templates) {
                    var expectedTemplates = JSON.parse(req.query.templates);
                    expect(installation.templates).to.deep.equal(expectedTemplates);
                }
            }

            function verifySecondaryTiles() {
                if(req.query.secondaryTiles) {
                    var secondaryTiles = JSON.parse(req.query.secondaryTiles);
                    expect(installation.secondaryTiles).to.deep.equal(secondaryTiles);
                }
            }

            function verifyTags() {
                var tag = '$InstallationId:{' + installationId + '}';
                return retry(function () {
                    return promises.wrap(push.listRegistrationsByTag, push)(tag).then(function (registrations) {
                        registrations.forEach(function (registration) {
                            expect(registration.Tags).to.contain(tag);
                        });
                    });
                });
            }
        });
    })
    // On validation failures like templates not matching, this catch block executes rather than the one in retry
    // I'm not quite sure why this is, and the consequence is that we can't catch here and report failures correctly,
    // or the retry doesn't get executed.
    .catch(function (error) {
        next(error);
    });
}

function getVerifyUnregisterInstallationResult(req, res, next) {
    var installationId = req.get('x-zumo-installation-id'),
        push = req.azureMobile.push;

    retry(function () {
        return promises.wrap(push.getInstallation, push)(installationId).catch(function () {
            res.status(200).json(true);
        });
    })
    .then(function () {
        res.status(200).json(false);
        // res.status(500).send("Found deleted installation with id " + installationId);
    });
}

function deleteRegistrationsForChannel(req, res, next) {
    var installationId = req.get('x-zumo-installation-id'),
        push = req.azureMobile.push;

    retry(function () {
        return promises.wrap(push.deleteInstallation, push)(installationId).then(function () {
            res.status(200).end();
        });
    });
}

function retry(action, args) {
    return promises.create(function (resolve, reject) {
        var tryCount = 0,
            sleepTimes = [20, 500, 2000, 5000, 10000, 30000];

        tryAction();

        function tryAction() {
            try {
                return promises.sleep(sleepTimes[tryCount])
                    .then(function () {
                        tryCount++;
                        console.log('Attempt ' + tryCount);
                        return action.apply(null, args);
                    })
                    .then(function (result) {
                        resolve(result);
                    })
                    .catch(function (error) {
                        console.log('Attempt ' + tryCount + ' failed: ' + error.message);
                        if(tryCount < sleepTimes.length)
                            return tryAction();
                        reject(error);
                    });
            } catch(ex) {
                reject(ex);
            }
        }
    });
}
