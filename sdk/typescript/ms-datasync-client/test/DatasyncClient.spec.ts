// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect } from "./helpers/chai-helper";
import { AccessToken, MockTokenCredential, MockHttpClient } from "./helpers/http-client";
import * as msrest from "@azure/core-rest-pipeline";

import { 
    DatasyncClient, 
    DatasyncClientOptions,
    InvalidArgumentError,
    RemoteTable
} from "../src";
import { models } from "./helpers";

describe("DatasyncClient", () => {
    describe("constructor", () => {
        const testCases = [
            { url: "http://localhost",             expected: true },
            { url: "https://ds.azurewebsites.net", expected: true },
            { url: "http://localhost/api",         expected: true },
            { url: "http://localhost:49133/api",   expected: true },
            { url: "http://ds.azurewebsites.net",  expected: false },
            { url: "file:///foo",                  expected: false },
            { url: "http://localhost#fragment",    expected: false },
            { url: "http://localhost?search",      expected: false }
        ];

        for (const testCase of testCases) {
            if (testCase.expected === true) {
                it(`constructs a new DatasyncClient with a string = '${testCase.url}'`, () => {
                    const client = new DatasyncClient(testCase.url);
                    expect(client.endpointUrl.href).to.startWith(testCase.url);
                    expect(client.clientOptions).to.not.be.undefined;
                    expect(client.credential).to.be.undefined;
                    expect(client.serviceClient).to.not.be.undefined;
                });

                it(`constructs a new DatasyncClient with a URL = '${testCase.url}'`, () => {
                    const client = new DatasyncClient(testCase.url);
                    expect(client.endpointUrl.href).to.startWith(testCase.url);
                    expect(client.clientOptions).to.not.be.undefined;
                    expect(client.credential).to.be.undefined;
                    expect(client.serviceClient).to.not.be.undefined;
                });

                it(`constructs a new DatasyncClient with a string = '${testCase.url}' and custom options`, () => {
                    const options: DatasyncClientOptions = { userAgentOptions: { userAgentPrefix: "foo" } };
                    const client = new DatasyncClient(testCase.url, options);
                    expect(client.endpointUrl.href).to.startWith(testCase.url);
                    expect(client.clientOptions).to.containSubset(options);
                    expect(client.credential).to.be.undefined;
                    expect(client.serviceClient).to.not.be.undefined;
                });

                it(`constructs a new DatasyncClient with a string = '${testCase.url}' and custom options`, () => {
                    const options: DatasyncClientOptions = { userAgentOptions: { userAgentPrefix: "foo" } };
                    const client = new DatasyncClient(new URL(testCase.url), options);
                    expect(client.endpointUrl.href).to.startWith(testCase.url);
                    expect(client.clientOptions).to.containSubset(options);
                    expect(client.credential).to.be.undefined;
                    expect(client.serviceClient).to.not.be.undefined;
                });

                it(`constructs a new DatasyncClient with a string = '${testCase.url}' and a credential`, () => {
                    const credential = new MockTokenCredential();
                    const client = new DatasyncClient(testCase.url, credential);
                    expect(client.endpointUrl.href).to.startWith(testCase.url);
                    expect(client.clientOptions).to.not.be.undefined;
                    expect(client.credential).to.equal(credential);
                    expect(client.serviceClient).to.not.be.undefined;
                });

                it(`constructs a new DatasyncClient with a URL = '${testCase.url}' and a credential`, () => {
                    const credential = new MockTokenCredential();
                    const client = new DatasyncClient(new URL(testCase.url), credential);
                    expect(client.endpointUrl.href).to.startWith(testCase.url);
                    expect(client.clientOptions).to.not.be.undefined;
                    expect(client.credential).to.equal(credential);
                    expect(client.serviceClient).to.not.be.undefined;
                });

                it(`constructs a new DatasyncClient with a string = '${testCase.url}', custom options, and a credential`, () => {
                    const options: DatasyncClientOptions = { userAgentOptions: { userAgentPrefix: "foo" } };
                    const credential = new MockTokenCredential();
                    const client = new DatasyncClient(testCase.url, credential, options);
                    expect(client.endpointUrl.href).to.startWith(testCase.url);
                    expect(client.clientOptions).to.containSubset(options);
                    expect(client.credential).to.equal(credential);
                    expect(client.serviceClient).to.not.be.undefined;
                });

                it(`constructs a new DatasyncClient with a URL = '${testCase.url}', custom options, and a credential`, () => {
                    const options: DatasyncClientOptions = { userAgentOptions: { userAgentPrefix: "foo" } };
                    const credential = new MockTokenCredential();
                    const client = new DatasyncClient(new URL(testCase.url), credential, options);
                    expect(client.endpointUrl.href).to.startWith(testCase.url);
                    expect(client.clientOptions).to.containSubset(options);
                    expect(client.credential).to.equal(credential);
                    expect(client.serviceClient).to.not.be.undefined;
                });
            } else {
                it(`throws an InvalidArgumentException when constructing a new DatasyncClient with a string = '${testCase.url}' argument`, () => {
                    expect(() => {
                        new DatasyncClient(testCase.url);
                    }).to.throw(InvalidArgumentError);
                });

                it(`throws an InvalidArgumentException when constructing a new DatasyncClient with a URL = '${testCase.url}' argument`, () => {
                    expect(() => {
                        new DatasyncClient(new URL(testCase.url));
                    }).to.throw(InvalidArgumentError);
                });
            }
        }
    });

    describe("serviceClient", () => {
        it("properly formulates a service request/response without a credential", async () => {
            const body = JSON.stringify({ id: "1234" });
            const mock = new MockHttpClient().addResponse(200, body);
            const options: DatasyncClientOptions = { httpClient: mock };
            const client = new DatasyncClient("https://localhost", options);

            const request: msrest.PipelineRequest = {
                method: "GET",
                url: "https://localhost/tables/todoitem",
                headers: msrest.createHttpHeaders(),
                timeout: 60000,
                withCredentials: false,
                requestId: "client-sendRequest-without-creds"
            };
            const response = await client.serviceClient.sendRequest(request);

            expect(mock.requests).to.have.lengthOf(1);
            expect(mock.requests[0].method).to.equal("GET");
            expect(mock.requests[0].headers.get("authorization")).to.be.undefined;
            expect(mock.requests[0].headers.get("zumo-api-version")).to.equal("3.0.0");

            expect(response).to.not.be.undefined;
            expect(response.status).to.equal(200);
            expect(response.bodyAsText).to.equal(body);
        });

        it("properly formulates a service request/response with a credential", async () => {
            const body = JSON.stringify({ id: "1234" });
            const token: AccessToken = {
                token: "15d6beb3-4088-4478-a3ec-87116d217bd3",
                expiresOnTimestamp: new Date().getTime() + 100000
            };
            const mock = new MockHttpClient().addResponse(200, body);
            const credential = new MockTokenCredential(token);
            const options: DatasyncClientOptions = { httpClient: mock };
            const client = new DatasyncClient("https://localhost", credential, options);

            const request: msrest.PipelineRequest = {
                method: "GET",
                url: "https://localhost/tables/todoitem",
                headers: msrest.createHttpHeaders(),
                timeout: 60000,
                withCredentials: true,
                requestId: "client-sendRequest-without-creds"
            };
            const response = await client.serviceClient.sendRequest(request);

            expect(mock.requests).to.have.lengthOf(1);
            expect(mock.requests[0].method).to.equal("GET");
            expect(mock.requests[0].headers.get("authorization")).to.equal("Bearer 15d6beb3-4088-4478-a3ec-87116d217bd3");
            expect(mock.requests[0].headers.get("zumo-api-version")).to.equal("3.0.0");

            expect(response).to.not.be.undefined;
            expect(response.status).to.equal(200);
            expect(response.bodyAsText).to.equal(body);
        });
    });

    describe("getRemoteTable", () => {
        it("produces a RemoteTable when provided a table name", () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock });

            const sut = client.getRemoteTable<models.Movie>("movies");
            expect(sut).to.be.instanceOf(RemoteTable);

            const table = sut as RemoteTable<models.Movie>;            
            expect(table.tableName).to.equal("movies");
            expect(table.tableEndpoint).to.equal("https://localhost/tables/movies");
        });

        it("produces a RemoteTable when provided a table name and path", () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock });

            const sut = client.getRemoteTable<models.Movie>("movies", "/api/movies");
            expect(sut).to.be.instanceOf(RemoteTable);

            const table = sut as RemoteTable<models.Movie>;            
            expect(table.tableName).to.equal("movies");
            expect(table.tableEndpoint).to.equal("https://localhost/api/movies");
        });

        it("produces a RemoteTable when provided a table name and path resolver", () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock,
                tablePathResolver: (tableName: string) => `/api/${tableName}`
            });

            const sut = client.getRemoteTable<models.Movie>("movies");
            expect(sut).to.be.instanceOf(RemoteTable);

            const table = sut as RemoteTable<models.Movie>;            
            expect(table.tableName).to.equal("movies");
            expect(table.tableEndpoint).to.equal("https://localhost/api/movies");
        });

        it("throws on an invalid table name", () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock });

            expect(() => { client.getRemoteTable<models.Movie>(""); }).to.throw(InvalidArgumentError);
        });

        it("throws if resolver returns blank string", () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock,
                // eslint-disable-next-line @typescript-eslint/no-unused-vars
                tablePathResolver: (_: string) => ""
            });

            expect(() => { client.getRemoteTable<models.Movie>("movies"); }).to.throw(InvalidArgumentError);
        });
    });
});