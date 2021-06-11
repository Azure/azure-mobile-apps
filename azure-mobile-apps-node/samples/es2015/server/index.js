// ----------------------------------------------------------------------------
// Copyright (c) 2016 Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

// Basic HTTP Server Listener for an ExpressJS Web application
import application from './app';
import http from 'http';
import process from 'process';

// In this case, the application() code returns a Promise that is resolved
// to the ExpressJS application object.
application().then((app) => {
    let server = http.createServer(app);
    let port = process.env.PORT || 3000;

    server.on('error', (error) => {
        if (error.syscall && error.code)
            console.error(`[http.listen] ${error.syscall} ${error.code} ${error.message}`);
        throw error;
    });

    server.on('listening', () => {
        let port = server.address().port;
        console.info(`[http.listen] Listening on port ${port}`);
    });

    app.set('port', port);
    server.listen(app.get('port'));
});
