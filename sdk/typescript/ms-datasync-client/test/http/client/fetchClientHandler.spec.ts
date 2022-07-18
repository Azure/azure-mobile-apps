// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import * as http from '../../../src/http/client';
import { runBasicHttpTests } from './basicHttpTests';

describe('src/http/client/fetchClientHandler', function () {
    this.slow(1500);        // Web calls take longer

    const getClient = () => new http.FetchClientHandler();

    describe('#FetchClientHandler', () => {
        runBasicHttpTests(getClient);
    });
})