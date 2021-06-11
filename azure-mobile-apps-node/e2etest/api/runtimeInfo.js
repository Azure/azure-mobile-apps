module.exports = function (req, res, next) {
    res.status(200).json({
        runtime: { type: 'node.js', version: process.version },
        features: {
            intIdTables: true,
            stringIdTables: true,
            nhPushEnabled: !!(req.azureMobile.push),
            queryExpandSupport: false,
            userEnabled: /\"users\"/i.test(process.env.MS_PreviewFeatures),
            liveSDKLogin: true,
            azureActiveDirectoryLogin: true,
            singleSignOnLogin: true,
            stringReplace: true,
            nodeRuntimeOnly: true,
            dotNetRuntimeOnly: false
        }
    });
}
