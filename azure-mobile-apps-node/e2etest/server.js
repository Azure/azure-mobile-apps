// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var path = require('path'),
    app = require('express')(),
    mobileApps = require('azure-mobile-apps'),
    configuration = require('azure-mobile-apps/src/configuration'),
    mobileApp,

    config = {
        skipVersionCheck: true,
        pageSize: 1000,
        auth: { secret: 'secret' }
    };

mobileApp = mobileApps(config);

// tables
mobileApp.tables.add('application', { softDelete: true });
mobileApp.tables.add('authorized', { authorize: true });
mobileApp.tables.add('admin', { authorize: true, softDelete: true });
mobileApp.tables.add('public');
mobileApp.tables.add('blog_comments', { columns: { postId: 'string', commentText: 'string', name: 'string', test: 'number' } });
mobileApp.tables.add('blog_posts', { columns: { title: 'string', commentCount: 'number', showComments: 'boolean', data: 'string' } });
mobileApp.tables.add('dates', { columns: { date: 'date', dateOffset: 'date' } });
mobileApp.tables.add('movies', { columns: { title: 'string', duration: 'number', mpaaRating: 'string', releaseDate: 'date', bestPictureWinner: 'boolean' } });
mobileApp.tables.add('IntIdRoundTripTable', { autoIncrement: true, columns: { name: 'string', date1: 'date', bool: 'boolean', integer: 'number', number: 'number' }});
mobileApp.tables.add('intIdMovies', { autoIncrement: true, columns: { title: 'string', duration: 'number', mpaaRating: 'string', releaseDate: 'date', bestPictureWinner: 'boolean' } });
mobileApp.tables.add('OfflineReady');
mobileApp.tables.add('OfflineReadyNoVersionAuthenticated', { authorize: true });
mobileApp.tables.add('StringIdRoundTripTableSoftDelete', { softDelete: true });
mobileApp.tables.import('tables');

app.use(mobileApp);

// custom APIs
app.get('/api/jwtTokenGenerator', require('./api/jwtTokenGenerator')(mobileApp.configuration));
app.get('/api/runtimeInfo', require('./api/runtimeInfo'));
require('./api/applicationPermission').register(app);
require('./api/movieFinder').register(app);
require('./api/push').register(app);

mobileApp.tables.initialize().then(function () {
    app.listen(process.env.PORT || 3000);
});
