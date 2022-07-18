// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { assert } from 'chai';
import * as AxiosLogger from 'axios-logger';
import { AxiosRequestConfig, AxiosResponse } from 'axios';
import * as http from '../../../src/http/client';
import { AxiosInterceptors, AxiosClientHandler } from '../../../src/http/client/axiosClientHandler';

const baseUrl = 'https://jsonplaceholder.typicode.com';

// NOTE:
//  jsonplaceholder is not CORS enabled, so you can't just check headers.
//  We check header availability elsewhere with a real server.

describe('src/http/client/axiosClientHandler', function () {
    this.slow(1000);        // Web calls take longer

    describe('#AxiosClientHandler', () => {
        it('can make a basic web call', async () => {
            const request = new http.HttpRequestMessage(http.HttpMethod.Get, new URL(`${baseUrl}/posts/1`));
            const client = new http.AxiosClientHandler();
            const response = await client.sendAsync(request);

            assert.equal(response.statusCode, 200);
            assert.equal(response.reasonPhrase, "OK");
            assert.strictEqual(response.headers.get('content-type'), 'application/json; charset=utf-8');
            assert.isDefined(response.content);
            assert.match(response.content ?? '', /^{/);

            // Since it's jsonplaceholder - we should convert the content to JSON and check the id == 1
            const obj = JSON.parse(response.content ?? '{}');
            assert.equal(obj.id, 1);
        });

        it('can make a call with headers and interceptors', async () => {
            const headers = new Map<string, string>([[ 'X-ZUMO-REQUEST', '1' ]]);
            const storedHeaders = new Map<string, string>();
            const request = new http.HttpRequestMessage(http.HttpMethod.Post, new URL(`${baseUrl}/posts`), undefined, headers);
            const logMessages = new Array<string>();
            const logConfig = { 
                headers: true,
                logger: (value: string) => { logMessages.push(value); }
            };
            const interceptors: AxiosInterceptors = {
                request: {
                    onFulfilled: (value: AxiosRequestConfig) => AxiosLogger.requestLogger(value, logConfig),
                    onRejected: (error: any) => AxiosLogger.errorLogger(error, logConfig) // eslint-disable-line @typescript-eslint/no-explicit-any
                },
                response: {
                    onFulfilled: (value: AxiosResponse) => AxiosLogger.responseLogger(value, logConfig),
                    onRejected: (error: any) => AxiosLogger.errorLogger(error, logConfig) // eslint-disable-line @typescript-eslint/no-explicit-any
                }
            };
            const clientConfig: AxiosRequestConfig = {
                transformRequest: (data, hdrs) => {
                    storedHeaders.clear();
                    for (const header in hdrs) {
                        storedHeaders.set(header, hdrs[header].toString());
                    }
                    return data;
                }
            };
            const client = new http.AxiosClientHandler(clientConfig, interceptors);
            const response = await client.sendAsync(request);

            assert.isTrue(storedHeaders.has('X-ZUMO-REQUEST'));
            assert.equal(response.statusCode, 201);
            assert.equal(response.reasonPhrase, "Created");
            assert.strictEqual(response.headers.get('content-type'), 'application/json; charset=utf-8');
            assert.isDefined(response.content);
            assert.match(response.content ?? '', /^{/);

            // Since it's jsonplaceholder - we should convert the content to JSON and check the id == 101
            const obj = JSON.parse(response.content ?? '{}');
            assert.equal(obj.id, 101);
        });

        it('can handle request or response interceptors', () => {
            const logMessages = new Array<string>();
            const logConfig = { 
                headers: true,
                logger: (value: string) => { logMessages.push(value); }
            };
            const interceptors: AxiosInterceptors = {
                request: {
                    onFulfilled: (value: AxiosRequestConfig) => AxiosLogger.requestLogger(value, logConfig),
                    onRejected: (error: any) => AxiosLogger.errorLogger(error, logConfig) // eslint-disable-line @typescript-eslint/no-explicit-any
                },
                response: {
                    onFulfilled: (value: AxiosResponse) => AxiosLogger.responseLogger(value, logConfig),
                    onRejected: (error: any) => AxiosLogger.errorLogger(error, logConfig) // eslint-disable-line @typescript-eslint/no-explicit-any
                }
            };

            const c1 = new AxiosClientHandler({}, {});
            assert.isDefined(c1);

            const c2 = new AxiosClientHandler({}, { request: interceptors.request });
            assert.isDefined(c2);

            const c3 = new AxiosClientHandler({}, { response: interceptors.response });
            assert.isDefined(c3);

            const c4 = new AxiosClientHandler({}, interceptors);
            assert.isDefined(c4);
        });
    });
})