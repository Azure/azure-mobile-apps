module.exports = function (configuration) {
    var auth = require('azure-mobile-apps/src/auth')(configuration.auth);

    return function (req, res, next) {
        var payload = {
            "ver": "3",
            "sub": "Facebook:someuserid@hotmail.com",
            "iss": "urn:microsoft:windows-azure:zumo",
            "aud": "urn:microsoft:windows-azure:zumo",
            "exp": 9999999999,
            "nbf": 0
        };

        res.status(200).json({
            "userId": "Facebook:someuserid@hotmail.com",
            "authenticationToken": auth.sign(payload)
        });
    }
};
