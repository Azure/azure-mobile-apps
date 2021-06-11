// ----------------------------------------------------------------------------
// Copyright (c) 2016 Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/*
** Sample API Definition - implements a simple configuration endpoint
** that returns a static JSON blob when issued a GET.
*/
function getConfiguration(request, response) {
    let clientConfiguration = {
        auth: {
            details: '/.auth/me',
            login: '/.auth/login/microsoftaccount',
            logout: '/.auth/logout',
            provider: 'microsoftaccount'
        }
    };

    // The response is sent through a standard ExpressJS pipe
    response.status(200).type('application/json').send(clientConfiguration);
}

// The defines the api.
let api = {
    get: getConfiguration
};

// Define the access permissions
api.get.access = 'anonymous';  // Other options are authenticated and disabled

// Export the API
export default api;
