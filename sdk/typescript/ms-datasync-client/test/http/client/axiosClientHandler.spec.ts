// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { assert } from 'chai';
import * as http from '../../../src/http/client';
import { AxiosRequestConfig, AxiosResponse } from 'axios';
import * as AxiosLogger from 'axios-logger';
import { runBasicHttpTests } from './basicHttpTests';

describe('src/http/client/axiosClientHandler', function () {
    this.slow(1500);        // Web calls take longer

    const getClient = () => { return new http.AxiosClientHandler(); };

    describe('#AxiosClientHandler', () => {
        runBasicHttpTests(getClient);

        it('can handle request or response interceptors', () => {
            const logMessages = new Array<string>();
            const logConfig = { 
                headers: true,
                logger: (value: string) => { logMessages.push(value); }
            };
            const interceptors: http.AxiosInterceptors = {
                request: {
                    onFulfilled: (value: AxiosRequestConfig) => AxiosLogger.requestLogger(value, logConfig),
                    onRejected: (error: any) => AxiosLogger.errorLogger(error, logConfig) // eslint-disable-line @typescript-eslint/no-explicit-any
                },
                response: {
                    onFulfilled: (value: AxiosResponse) => AxiosLogger.responseLogger(value, logConfig),
                    onRejected: (error: any) => AxiosLogger.errorLogger(error, logConfig) // eslint-disable-line @typescript-eslint/no-explicit-any
                }
            };

            const c1 = new http.AxiosClientHandler({}, {});
            assert.isDefined(c1);

            const c2 = new http.AxiosClientHandler({}, { request: interceptors.request });
            assert.isDefined(c2);

            const c3 = new http.AxiosClientHandler({}, { response: interceptors.response });
            assert.isDefined(c3);

            const c4 = new http.AxiosClientHandler({}, interceptors);
            assert.isDefined(c4);
        });
    });
})