// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect } from 'chai';
import * as http from '../../../src/http/client';
import { AxiosRequestConfig, AxiosResponse } from 'axios';
import * as AxiosLogger from 'axios-logger';
import { runBasicHttpTests } from './basicHttpTests';

describe('src/http/client/axiosClientHandler', function () {
    this.slow(1500);        // Web calls take longer

    const getClient = () => { return new http.AxiosClientHandler(); };

    describe('#AxiosClientHandler', () => {
        runBasicHttpTests(getClient);

        it('uses default headers', async () => {
            const clientConfig: AxiosRequestConfig = {
                headers: {
                    'X-Zumo-Request': 'abcd'
                }
            };
            const client = new http.AxiosClientHandler(clientConfig);

            const request = new http.HttpRequestMessage(
                http.HttpMethod.Get, 
                new URL("https://httpbin.org/anything")
            );
            const response = await client.sendAsync(request);
    
            expect(response).to.be.instanceof(http.HttpResponseMessage);

            // content
            expect(response.content).to.be.a('string').and.to.have.length.greaterThan(0);
            if (typeof response.content === 'string') {
                const obj = JSON.parse(response.content);
                expect(obj.headers).to.haveOwnProperty('X-Zumo-Request');
                expect(obj.headers['X-Zumo-Request']).to.equal('abcd');
            }
        });

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
            expect(c1).to.not.be.undefined;

            const c2 = new http.AxiosClientHandler({}, { request: interceptors.request });
            expect(c2).to.not.be.undefined;

            const c3 = new http.AxiosClientHandler({}, { response: interceptors.response });
            expect(c3).to.not.be.undefined;

            const c4 = new http.AxiosClientHandler({}, interceptors);
            expect(c4).to.not.be.undefined;
        });
    });
})