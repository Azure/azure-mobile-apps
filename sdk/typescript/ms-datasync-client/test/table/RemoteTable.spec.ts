// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.


import { assert, expect, expectMockRequestsToBeValid } from "../helpers/chai-helper";
import { datasets, models, MockHttpClient } from "../helpers";
import { v4 as uuid } from "uuid";

import { ConflictError, DatasyncClient, InvalidArgumentError, Page, RemoteTable, RestError, TableQuery } from "../../src";
import { JsonReviver } from "../../src/table";

/**
 * Interface to describe a set of test cases for table queries.
 */
interface TableQueryTestCase {
    query: TableQuery | undefined,
    expected: string
}

/**
 * Test reviver map for the movie database.
 * @param tableName The name of the table
 * @returns The reviver, or undefined.
 */
const movieReviver = (tableName: string) => {
    if (tableName === "movies") {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const reviver: JsonReviver = (propName: string, propValue: unknown): any => {
            if (propName === "releaseDate" && typeof(propValue) === "string") {
                return new Date(propValue);
            }
            return propValue;
        };
        return reviver;
    }
    return undefined;
};

describe("table/RemoteTable", () => {
    describe("#constructor", () => {
        it("can be constructed", () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            expect(sut.clientOptions).to.equal(client.clientOptions);
            expect(sut.serviceClient).to.equal(client.serviceClient);
            expect(sut.tableEndpoint).to.equal("https://localhost/tables/movies");
            expect(sut.tableName).to.equal("movies");
            expect(sut.reviver).to.be.undefined;
        });
        
        it("sets reviver when tableReviver is set and handles the table name", () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            expect(sut.reviver).to.not.be.undefined;
        });

        it("does not set reviver when tableReviver is set and does not handle the table name", () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
            const sut = new RemoteTable(client.serviceClient, "test", new URL("tables/test", client.endpointUrl), client.clientOptions);

            expect(sut.reviver).to.be.undefined;
        });
    });

    describe("#createItem", () => {
        for (const invalidId of datasets.invalidIds) {
            it(`throws when the entity ID is invalid (value: '${invalidId}')`, async () => {
                const mock = new MockHttpClient();
                const client = new DatasyncClient("https://localhost", { httpClient: mock });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

                const movie: models.Movie = { id: invalidId, ...datasets.movie };

                try {
                    await sut.createItem(movie);
                    assert.fail(`expected createItem to throw on invalidId = '${invalidId}'`);
                } catch (err) {
                    expect(err).to.be.instanceOf(InvalidArgumentError);
                }
            });
        }

        it("correctly formulates a request and handles a correct response", async () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movieResponse: models.Movie = {
                id: "movie-001",
                version: uuid(),
                updatedAt: new Date(),
                deleted: false,
                ...datasets.movie
            };
            mock.addResponse(201, JSON.stringify(movieResponse), { "ETag": `"${movieResponse.version}"` });

            const movieRequest: models.Movie = { id: "movie-001", ...datasets.movie };
            const response = await sut.createItem(movieRequest);
            
            // Check that the request is sent properly
            expectMockRequestsToBeValid(mock, 1, "POST", sut.tableEndpoint);
            expect(mock.requests[0].headers.get("if-none-match")).to.equal("*");
            const body = typeof mock.requests[0].body === "function" ? mock.requests[0].body() : mock.requests[0].body;
            expect(body).to.equal(JSON.stringify(movieRequest));

            // Check that the response is correct.
            expect(response).to.eql(movieResponse);
        });

        it("correctly formulates a request and handles a correct response when forced", async () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movieResponse: models.Movie = {
                id: "movie-001",
                version: uuid(),
                updatedAt: new Date(),
                deleted: false,
                ...datasets.movie
            };
            mock.addResponse(201, JSON.stringify(movieResponse), { "ETag": `"${movieResponse.version}"` });

            const movieRequest: models.Movie = { id: "movie-001", ...datasets.movie };
            const response = await sut.createItem(movieRequest, { force: true });
            
            // Check that the request is sent properly
            expectMockRequestsToBeValid(mock, 1, "POST", sut.tableEndpoint);
            expect(mock.requests[0].headers.get("if-none-match")).to.be.undefined;
            const body = typeof mock.requests[0].body === "function" ? mock.requests[0].body() : mock.requests[0].body;
            expect(body).to.equal(JSON.stringify(movieRequest));

            // Check that the response is correct.
            expect(response).to.eql(movieResponse);
        });

        it("throws when successful but no content is returned", async () => {
            const mock = new MockHttpClient().addResponse(200);
            const client = new DatasyncClient("https://localhost", { httpClient: mock });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movie: models.Movie = { id: "movie-001", ...datasets.movie };
            try {
                await sut.createItem(movie);
                assert.fail("createItem should not be successful when there is no content returned");
            } catch (err) {
                expect(err).to.be.instanceOf(RestError);
                const restError = err as RestError;
                expect(restError.code).to.equal("NO_CONTENT");
            }
        });

        it("throws when receiving bad JSON", async () => {
            const mock = new MockHttpClient().addResponse(200, "{this is bad json");
            const client = new DatasyncClient("https://localhost", { httpClient: mock });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movie: models.Movie = { id: "movie-001", ...datasets.movie };
            try {
                await sut.createItem(movie);
                assert.fail("createItem should not be successful when bad JSON is returned");
            } catch (err) {
                expect(err).to.be.instanceOf(RestError);
                const restError = err as RestError;
                expect(restError.code).to.equal("PARSE_ERROR");
            }
        });

        for (const statusCode of [ 409, 412 ]) {
            it(`throws a ConflictError when status code ${statusCode} is returned`, async () => {
                const mock = new MockHttpClient();
                const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

                const responseMovie: models.Movie = { id: "movie-001", version: "abcd", ...datasets.movie };
                mock.addResponse(statusCode, JSON.stringify(responseMovie), { "Content-Type": "application/json", "ETag": `"${responseMovie.version}"` });

                const requestMovie: models.Movie = { id: "movie-001", ...datasets.movie };
                try {
                    await sut.createItem(requestMovie);
                    assert.fail(`createItem should throw ConflictError when statusCode ${statusCode} is returned`);
                } catch(err) {
                    expect(err).to.be.instanceOf(ConflictError);
                    const conflict = err as ConflictError;
                    expect(conflict.request).to.not.be.undefined;
                    expect(conflict.response).to.not.be.undefined;
                    expect(conflict.serverValue as models.Movie).to.eql(responseMovie);
                }
            });

            it(`throws when a statusCode ${statusCode} occurs but no content is returned`, async () => {
                const mock = new MockHttpClient().addResponse(statusCode);
                const client = new DatasyncClient("https://localhost", { httpClient: mock });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);
    
                const movie: models.Movie = { id: "movie-001", ...datasets.movie };
                try {
                    await sut.createItem(movie);
                    assert.fail("createItem should not be successful when there is no content returned");
                } catch (err) {
                    expect(err).to.be.instanceOf(RestError);
                    const restError = err as RestError;
                    expect(restError.code).to.equal("NO_CONTENT");
                }
            });
        }
        
        for (const statusCode of [ 400, 401, 403, 413 ]) {
            it(`throws a RestError when status code ${statusCode} is returned`, async () => {
                const mock = new MockHttpClient().addResponse(statusCode);
                const client = new DatasyncClient("https://localhost", { httpClient: mock });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);
                const requestMovie: models.Movie = { id: "movie-001", ...datasets.movie };

                try {
                    await sut.createItem(requestMovie);
                    assert.fail(`createItem should throw RestError when statusCode ${statusCode} is returned`);
                } catch (err) {
                    expect(err).to.be.instanceOf(RestError);
                    const restError = err as RestError;
                    expect(restError.code).to.equal("HTTP_ERROR");
                }
            });
        }
    });

    describe("#deleteItem", () => {
        for (const invalidId of datasets.invalidIds) {
            it(`throws when the entity ID is invalid (string value: '${invalidId}')`, async () => {
                const mock = new MockHttpClient();
                const client = new DatasyncClient("https://localhost", { httpClient: mock });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

                try {
                    await sut.deleteItem(invalidId);
                    assert.fail(`expected deleteItem to throw on invalidId = '${invalidId}'`);
                } catch (err) {
                    expect(err).to.be.instanceOf(InvalidArgumentError);
                }
            });

            it(`throws when the entity ID is invalid (object value: '${invalidId}')`, async () => {
                const mock = new MockHttpClient();
                const client = new DatasyncClient("https://localhost", { httpClient: mock });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

                const movie: models.Movie = { id: invalidId, ...datasets.movie };

                try {
                    await sut.deleteItem(movie);
                    assert.fail(`expected deleteItem to throw on invalidId = '${invalidId}'`);
                } catch (err) {
                    expect(err).to.be.instanceOf(InvalidArgumentError);
                }
            });
        }

        it("formulates the correct request and handles a correct response (string)", async () => {
            const mock = new MockHttpClient().addResponse(204);
            const client = new DatasyncClient("https://localhost", { httpClient: mock });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movie: models.Movie = { id: "movie-001", ...datasets.movie };
            await sut.deleteItem(movie);

            // Check that the request is sent properly
            expectMockRequestsToBeValid(mock, 1, "DELETE", `${sut.tableEndpoint}/movie-001`);
            expect(mock.requests[0].body).to.be.undefined;
        });

        it("formulates the correct request and handles a correct response (object with version)", async () => {
            const mock = new MockHttpClient().addResponse(204);
            const client = new DatasyncClient("https://localhost", { httpClient: mock });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movie: models.Movie = { id: "movie-001", version: "testversion", ...datasets.movie };
            await sut.deleteItem(movie);

            // Check that the request is sent properly
            expectMockRequestsToBeValid(mock, 1, "DELETE", `${sut.tableEndpoint}/movie-001`);
            expect(mock.requests[0].body).to.be.undefined;
            expect(mock.requests[0].headers.get("if-match")).to.equal(`"${movie.version}"`);
        });

        it("formulates the correct request and handles a correct response (object without version)", async () => {
            const mock = new MockHttpClient().addResponse(204);
            const client = new DatasyncClient("https://localhost", { httpClient: mock });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movie: models.Movie = { id: "movie-001", ...datasets.movie };
            await sut.deleteItem(movie);

            // Check that the request is sent properly
            expectMockRequestsToBeValid(mock, 1, "DELETE", `${sut.tableEndpoint}/movie-001`);
            expect(mock.requests[0].body).to.be.undefined;
            expect(mock.requests[0].headers.get("if-match")).to.be.undefined;
        });

        it("formulates the correct request and handles a correct response (object with version, forced)", async () => {
            const mock = new MockHttpClient().addResponse(204);
            const client = new DatasyncClient("https://localhost", { httpClient: mock });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movie: models.Movie = { id: "movie-001", version: "testversion", ...datasets.movie };
            await sut.deleteItem(movie, { force: true });

            // Check that the request is sent properly
            expectMockRequestsToBeValid(mock, 1, "DELETE", `${sut.tableEndpoint}/movie-001`);
            expect(mock.requests[0].body).to.be.undefined;
            expect(mock.requests[0].headers.get("if-match")).to.be.undefined;
        });

        for (const statusCode of [ 409, 412 ]) {
            it(`throws a ConflictError when status code ${statusCode} is returned`, async () => {
                const mock = new MockHttpClient();
                const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

                const responseMovie: models.Movie = { id: "movie-001", version: "abcd", ...datasets.movie };
                mock.addResponse(statusCode, JSON.stringify(responseMovie), { "Content-Type": "application/json", "ETag": `"${responseMovie.version}"` });

                const requestMovie: models.Movie = { id: "movie-001", ...datasets.movie };
                try {
                    await sut.deleteItem(requestMovie);
                    assert.fail(`deleteItem should throw ConflictError when statusCode ${statusCode} is returned`);
                } catch(err) {
                    expect(err).to.be.instanceOf(ConflictError);
                    const conflict = err as ConflictError;
                    expect(conflict.request).to.not.be.undefined;
                    expect(conflict.response).to.not.be.undefined;
                    expect(conflict.serverValue as models.Movie).to.eql(responseMovie);
                }
            });

            it(`throws when a statusCode ${statusCode} occurs but no content is returned`, async () => {
                const mock = new MockHttpClient().addResponse(statusCode);
                const client = new DatasyncClient("https://localhost", { httpClient: mock });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);
    
                const movie: models.Movie = { id: "movie-001", ...datasets.movie };
                try {
                    await sut.deleteItem(movie);
                    assert.fail("deleteItem should not be successful when there is no content returned");
                } catch (err) {
                    expect(err).to.be.instanceOf(RestError);
                    const restError = err as RestError;
                    expect(restError.code).to.equal("NO_CONTENT");
                }
            });
        }

        for (const statusCode of [ 400, 401, 403, 413 ]) {
            it(`throws a RestError when status code ${statusCode} is returned`, async () => {
                const mock = new MockHttpClient().addResponse(statusCode);
                const client = new DatasyncClient("https://localhost", { httpClient: mock });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);
                const requestMovie: models.Movie = { id: "movie-001", ...datasets.movie };

                try {
                    await sut.deleteItem(requestMovie);
                    assert.fail(`deleteItem should throw RestError when statusCode ${statusCode} is returned`);
                } catch (err) {
                    expect(err).to.be.instanceOf(RestError);
                    const restError = err as RestError;
                    expect(restError.code).to.equal("HTTP_ERROR");
                }
            });
        }
    });

    describe("#getItem", () => {
        for (const invalidId of datasets.invalidIds) {
            it(`throws when the entity ID is invalid (value: '${invalidId}')`, async () => {
                const mock = new MockHttpClient();
                const client = new DatasyncClient("https://localhost", { httpClient: mock });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

                try {
                    await sut.getItem(invalidId);
                    assert.fail(`expected getItem to throw on invalidId = '${invalidId}'`);
                } catch (err) {
                    expect(err).to.be.instanceOf(InvalidArgumentError);
                }
            });
        }

        for (const statusCode of [ 400, 401, 403, 413 ]) {
            it(`throws a RestError when status code ${statusCode} is returned`, async () => {
                const mock = new MockHttpClient().addResponse(statusCode);
                const client = new DatasyncClient("https://localhost", { httpClient: mock });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

                try {
                    await sut.getItem("movie-001");
                    assert.fail(`getItem should throw RestError when statusCode ${statusCode} is returned`);
                } catch (err) {
                    expect(err).to.be.instanceOf(RestError);
                    const restError = err as RestError;
                    expect(restError.code).to.equal("HTTP_ERROR");
                }
            });
        }

        it("correctly formulates a request and handles a correct response", async () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movieResponse: models.Movie = {
                id: "movie-001",
                version: uuid(),
                updatedAt: new Date(),
                deleted: false,
                ...datasets.movie
            };
            mock.addResponse(201, JSON.stringify(movieResponse), { "ETag": `"${movieResponse.version}"` });

            const response = await sut.getItem("movie-001");
            
            // Check that the request is sent properly
            expectMockRequestsToBeValid(mock, 1, "GET", `${sut.tableEndpoint}/movie-001`);
            expect(mock.requests[0].body).to.be.undefined;

            // Check that the response is correct.
            expect(response).to.eql(movieResponse);
        });

        it("correctly formulates a request and handles a correct response (insecure comms)", async () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("http://localhost", { httpClient: mock, tableReviver: movieReviver, allowInsecureConnection: true });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movieResponse: models.Movie = {
                id: "movie-001",
                version: uuid(),
                updatedAt: new Date(),
                deleted: false,
                ...datasets.movie
            };
            mock.addResponse(201, JSON.stringify(movieResponse), { "ETag": `"${movieResponse.version}"` });

            const response = await sut.getItem("movie-001");
            
            // Check that the request is sent properly
            expectMockRequestsToBeValid(mock, 1, "GET", `${sut.tableEndpoint}/movie-001`);
            expect(mock.requests[0].body).to.be.undefined;

            // Check that the response is correct.
            expect(response).to.eql(movieResponse);
        });

        it("allows setting a timeout in clientOptions", async () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("http://localhost", { httpClient: mock, tableReviver: movieReviver, timeout: 10000 });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movieResponse: models.Movie = {
                id: "movie-001",
                version: uuid(),
                updatedAt: new Date(),
                deleted: false,
                ...datasets.movie
            };
            mock.addResponse(201, JSON.stringify(movieResponse), { "ETag": `"${movieResponse.version}"` });

            const response = await sut.getItem("movie-001");
            
            // Check that the request is sent properly
            expectMockRequestsToBeValid(mock, 1, "GET", `${sut.tableEndpoint}/movie-001`);
            expect(mock.requests[0].body).to.be.undefined;
            expect(mock.requests[0].timeout).to.equal(10000);

            // Check that the response is correct.
            expect(response).to.eql(movieResponse);
        });

        it("allows setting a timeout in operation options", async () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("http://localhost", { httpClient: mock, tableReviver: movieReviver });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movieResponse: models.Movie = {
                id: "movie-001",
                version: uuid(),
                updatedAt: new Date(),
                deleted: false,
                ...datasets.movie
            };
            mock.addResponse(201, JSON.stringify(movieResponse), { "ETag": `"${movieResponse.version}"` });

            const response = await sut.getItem("movie-001", { timeout: 10000 });
            
            // Check that the request is sent properly
            expectMockRequestsToBeValid(mock, 1, "GET", `${sut.tableEndpoint}/movie-001`);
            expect(mock.requests[0].body).to.be.undefined;
            expect(mock.requests[0].timeout).to.equal(10000);

            // Check that the response is correct.
            expect(response).to.eql(movieResponse);
        });

        it("correctly formulates a request and handles a correct response (no reviver)", async () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movieResponse: models.Movie = {
                id: "movie-001",
                version: uuid(),
                updatedAt: new Date(),
                deleted: false,
                ...datasets.movie
            };
            mock.addResponse(201, JSON.stringify(movieResponse), { "ETag": `"${movieResponse.version}"` });

            const response = await sut.getItem("movie-001");
            
            // Check that the request is sent properly
            expectMockRequestsToBeValid(mock, 1, "GET", `${sut.tableEndpoint}/movie-001`);
            expect(mock.requests[0].body).to.be.undefined;

            // Check that the response is correct.  Note that the lack of reviver will mean that
            // the response has different semantics.  Specifically, the response.releaseDate is 
            // a string instead of a date, so need to exclude that.
            const mvr = { ...movieResponse, releaseDate: movieResponse.releaseDate.toISOString() };
            expect(response).to.eql(mvr);
        });

        it("throws when successful but no content is returned", async () => {
            const mock = new MockHttpClient().addResponse(200);
            const client = new DatasyncClient("https://localhost", { httpClient: mock });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            try {
                await sut.getItem("movie-001");
                assert.fail("getItem should not be successful when there is no content returned");
            } catch (err) {
                expect(err).to.be.instanceOf(RestError);
                const restError = err as RestError;
                expect(restError.code).to.equal("NO_CONTENT");
            }
        });

        it("throws when successful but blank content is returned", async () => {
            const mock = new MockHttpClient().addResponse(200, "");
            const client = new DatasyncClient("https://localhost", { httpClient: mock });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            try {
                await sut.getItem("movie-001");
                assert.fail("getItem should not be successful when there is no content returned");
            } catch (err) {
                expect(err).to.be.instanceOf(RestError);
                const restError = err as RestError;
                expect(restError.code).to.equal("NO_CONTENT");
            }
        });

        it("throws when receiving bad JSON", async () => {
            const mock = new MockHttpClient().addResponse(200, "{this is bad json");
            const client = new DatasyncClient("https://localhost", { httpClient: mock });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            try {
                await sut.getItem("movie-001");
                assert.fail("getItem should not be successful when bad JSON is returned");
            } catch (err) {
                expect(err).to.be.instanceOf(RestError);
                const restError = err as RestError;
                expect(restError.code).to.equal("PARSE_ERROR");
            }
        });
    });

    describe("#getPageOfItems", () => {
        const testCases: Array<TableQueryTestCase> = [
            { query: undefined, expected: "" },
            { query: {}, expected: "" },
            { query: { filter: "(op eq 'foo')" }, expected: "$filter=(op%20eq%20%27foo%27)" },
            { query: { selection: [ "a", "b", "c" ] }, expected: "$select=a,b,c" },
            { query: { orderBy: [ "op", "ver desc" ] }, expected: "$orderby=op,ver%20desc" },
            { query: { skip: 20 }, expected: "$skip=20" },
            { query: { top: 20 }, expected: "$top=20" },
            { query: { skip: 20, top: 20 }, expected: "$skip=20&$top=20" },
            { query: { includeCount: true }, expected: "$count=true" },
            { query: { includeDeletedItems: true }, expected: "__includedeleted=true"},
            { query: { 
                filter: "(updatedAt gt cast('1994-04-01T13:30:22.044Z',Edm.DateTime) and (userId eq 'furbaz'))",
                selection: [ "id", "updatedAt" ],
                orderBy: [ "updatedAt" ],
                skip: 50,
                top: 25,
                includeCount: true,
                includeDeletedItems: true
              },
              expected: "$filter=(updatedAt%20gt%20cast(%271994-04-01T13:30:22.044Z%27,Edm.DateTime)%20and%20(userId%20eq%20%27furbaz%27))&$orderby=updatedAt&$select=id,updatedAt&$skip=50&$top=25&$count=true&__includedeleted=true"}
        ];

        for (const testCase of testCases) {
            it(`handles test case ${JSON.stringify(testCase.query)}`, async () => {
                const mock = new MockHttpClient();
                const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

                const resultToSend: Page<models.Movie> = {
                    items: [ { id: "movie-001", ...datasets.movie } ]
                };
                mock.addResponse(200, JSON.stringify(resultToSend));

                const response = await sut.getPageOfItems(testCase.query);

                // Check that the request is sent properly
                const expectedUri = testCase.expected !== "" ? `${sut.tableEndpoint}?${testCase.expected}` : sut.tableEndpoint;
                expectMockRequestsToBeValid(mock, 1, "GET", expectedUri);
                expect(mock.requests[0].body).to.be.undefined;

                // Check that the response is sent across
                expect(response).to.deep.equal(resultToSend);
            });
        }

        it("throws when successful but no content is returned", async () => {
            const mock = new MockHttpClient().addResponse(200);
            const client = new DatasyncClient("https://localhost", { httpClient: mock });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            try {
                await sut.getPageOfItems();
                assert.fail("getPageOfItems should not be successful when there is no content returned");
            } catch (err) {
                expect(err).to.be.instanceOf(RestError);
                const restError = err as RestError;
                expect(restError.code).to.equal("NO_CONTENT");
            }
        });

        it("throws when receiving bad JSON", async () => {
            const mock = new MockHttpClient().addResponse(200, "{this is bad json");
            const client = new DatasyncClient("https://localhost", { httpClient: mock });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            try {
                await sut.getPageOfItems();
                assert.fail("getPageOfItems should not be successful when bad JSON is returned");
            } catch (err) {
                expect(err).to.be.instanceOf(RestError);
                const restError = err as RestError;
                expect(restError.code).to.equal("PARSE_ERROR");
            }
        });

        for (const statusCode of [ 400, 401, 403, 413 ]) {
            it(`throws a RestError when status code ${statusCode} is returned`, async () => {
                const mock = new MockHttpClient().addResponse(statusCode);
                const client = new DatasyncClient("https://localhost", { httpClient: mock });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

                try {
                    await sut.getPageOfItems();
                    assert.fail(`getPageOfItems should throw RestError when statusCode ${statusCode} is returned`);
                } catch (err) {
                    expect(err).to.be.instanceOf(RestError);
                    const restError = err as RestError;
                    expect(restError.code).to.equal("HTTP_ERROR");
                }
            });
        }
    });

    describe("#listItems", () => {
        const testCases: Array<TableQueryTestCase> = [
            { query: undefined, expected: "" },
            { query: {}, expected: "" },
            { query: { filter: "(op eq 'foo')" }, expected: "$filter=(op%20eq%20%27foo%27)" },
            { query: { selection: [ "a", "b", "c" ] }, expected: "$select=a,b,c" },
            { query: { orderBy: [ "op", "ver desc" ] }, expected: "$orderby=op,ver%20desc" },
            { query: { skip: 20 }, expected: "$skip=20" },
            { query: { top: 20 }, expected: "$top=20" },
            { query: { skip: 20, top: 20 }, expected: "$skip=20&$top=20" },
            { query: { includeCount: true }, expected: "$count=true" },
            { query: { includeDeletedItems: true }, expected: "__includedeleted=true"},
            { query: { 
                filter: "(updatedAt gt cast('1994-04-01T13:30:22.044Z',Edm.DateTime) and (userId eq 'furbaz'))",
                selection: [ "id", "updatedAt" ],
                orderBy: [ "updatedAt" ],
                skip: 50,
                top: 25,
                includeCount: true,
                includeDeletedItems: true
              },
              expected: "$filter=(updatedAt%20gt%20cast(%271994-04-01T13:30:22.044Z%27,Edm.DateTime)%20and%20(userId%20eq%20%27furbaz%27))&$orderby=updatedAt&$select=id,updatedAt&$skip=50&$top=25&$count=true&__includedeleted=true"}
        ];

        for (const testCase of testCases) {
            it(`handles single page test case ${JSON.stringify(testCase.query)}`, async () => {
                const mock = new MockHttpClient();
                const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

                const page_1: Page<models.Movie> = datasets.getPageOfMovies(1, 10);
                mock.addResponse(200, JSON.stringify(page_1));

                let idx = 1;
                for await (const item of sut.listItems(testCase.query)) {
                    expect(item.id).to.equal(`m-${idx}`);
                    idx++;
                }
                expect(idx).to.equal(11);

                // Check that the request is sent properly
                const expectedUri = testCase.expected !== "" ? `${sut.tableEndpoint}?${testCase.expected}` : sut.tableEndpoint;
                expectMockRequestsToBeValid(mock, 1, "GET", expectedUri);
                expect(mock.requests[0].body).to.be.undefined;
            });

            it(`handles two page test case ${JSON.stringify(testCase.query)}`, async () => {
                const mock = new MockHttpClient();
                const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

                const page_1: Page<models.Movie> = datasets.getPageOfMovies(1, 10, undefined, "https://localhost/foo-page-2");
                mock.addResponse(200, JSON.stringify(page_1));
                const page_2: Page<models.Movie> = datasets.getPageOfMovies(11, 10);
                mock.addResponse(200, JSON.stringify(page_2));

                let idx = 1;
                for await (const item of sut.listItems(testCase.query)) {
                    expect(item.id).to.equal(`m-${idx}`);
                    idx++;
                }
                expect(idx).to.equal(21);

                // Check that the request is sent properly
                const expectedUri = testCase.expected !== "" ? `${sut.tableEndpoint}?${testCase.expected}` : sut.tableEndpoint;
                expectMockRequestsToBeValid(mock, 2, "GET", [ expectedUri, "https://localhost/foo-page-2" ]);
                expect(mock.requests[0].body).to.be.undefined;
            });

            it(`handles single page test case by page ${JSON.stringify(testCase.query)}`, async () => {
                const mock = new MockHttpClient();
                const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

                const page_1: Page<models.Movie> = datasets.getPageOfMovies(1, 10);
                mock.addResponse(200, JSON.stringify(page_1));

                let idx = 1;
                for await (const page of sut.listItems(testCase.query).byPage()) {
                    for (const item of page.items) {
                        expect(item.id).to.equal(`m-${idx}`);
                        idx++;
                    }
                }
                expect(idx).to.equal(11);

                // Check that the request is sent properly
                const expectedUri = testCase.expected !== "" ? `${sut.tableEndpoint}?${testCase.expected}` : sut.tableEndpoint;
                expectMockRequestsToBeValid(mock, 1, "GET", expectedUri);
                expect(mock.requests[0].body).to.be.undefined;
            });

            it(`handles two page test case by page ${JSON.stringify(testCase.query)}`, async () => {
                const mock = new MockHttpClient();
                const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

                const page_1: Page<models.Movie> = datasets.getPageOfMovies(1, 10, undefined, "https://localhost/foo-page-2");
                mock.addResponse(200, JSON.stringify(page_1));
                const page_2: Page<models.Movie> = datasets.getPageOfMovies(11, 10);
                mock.addResponse(200, JSON.stringify(page_2));

                let idx = 1;
                for await (const page of sut.listItems(testCase.query).byPage()) {
                    for (const item of page.items) {
                        expect(item.id).to.equal(`m-${idx}`);
                        idx++;
                    }
                }
                expect(idx).to.equal(21);

                // Check that the request is sent properly
                const expectedUri = testCase.expected !== "" ? `${sut.tableEndpoint}?${testCase.expected}` : sut.tableEndpoint;
                expectMockRequestsToBeValid(mock, 2, "GET", [ expectedUri, "https://localhost/foo-page-2" ]);
                expect(mock.requests[0].body).to.be.undefined;
            });
        }
    });

    describe("#replaceItem", () => {
        for (const invalidId of datasets.invalidIds) {
            it(`throws when the entity ID is invalid (value: '${invalidId}')`, async () => {
                const mock = new MockHttpClient();
                const client = new DatasyncClient("https://localhost", { httpClient: mock });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

                const movie: models.Movie = { id: invalidId, ...datasets.movie };

                try {
                    await sut.replaceItem(movie);
                    assert.fail(`expected replaceItem to throw on invalidId = '${invalidId}'`);
                } catch (err) {
                    expect(err).to.be.instanceOf(InvalidArgumentError);
                }
            });
        }

        it("correctly formulates a request and handles a correct response (no version in request)", async () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movieResponse: models.Movie = {
                id: "movie-001",
                version: uuid(),
                updatedAt: new Date(),
                deleted: false,
                ...datasets.movie
            };
            mock.addResponse(201, JSON.stringify(movieResponse), { "ETag": `"${movieResponse.version}"` });

            const movieRequest: models.Movie = { id: "movie-001", ...datasets.movie };
            const response = await sut.replaceItem(movieRequest);
            
            // Check that the request is sent properly
            expectMockRequestsToBeValid(mock, 1, "PUT", `${sut.tableEndpoint}/movie-001`);
            const body = typeof mock.requests[0].body === "function" ? mock.requests[0].body() : mock.requests[0].body;
            expect(body).to.equal(JSON.stringify(movieRequest));
            expect(mock.requests[0].headers.get("if-match")).to.be.undefined;

            // Check that the response is correct.
            expect(response).to.eql(movieResponse);
        });

        it("correctly formulates a request and handles a correct response (version in request)", async () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movieResponse: models.Movie = {
                id: "movie-001",
                version: uuid(),
                updatedAt: new Date(),
                deleted: false,
                ...datasets.movie
            };
            mock.addResponse(201, JSON.stringify(movieResponse), { "ETag": `"${movieResponse.version}"` });

            const movieRequest: models.Movie = { id: "movie-001", version: "abcd", ...datasets.movie };
            const response = await sut.replaceItem(movieRequest);
            
            // Check that the request is sent properly
            expectMockRequestsToBeValid(mock, 1, "PUT", `${sut.tableEndpoint}/movie-001`);
            const body = typeof mock.requests[0].body === "function" ? mock.requests[0].body() : mock.requests[0].body;
            expect(body).to.equal(JSON.stringify(movieRequest));
            expect(mock.requests[0].headers.get("if-match")).to.equal("\"abcd\"");

            // Check that the response is correct.
            expect(response).to.eql(movieResponse);
        });

        it("correctly formulates a request and handles a correct response (version in request, forced)", async () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movieResponse: models.Movie = {
                id: "movie-001",
                version: uuid(),
                updatedAt: new Date(),
                deleted: false,
                ...datasets.movie
            };
            mock.addResponse(201, JSON.stringify(movieResponse), { "ETag": `"${movieResponse.version}"` });

            const movieRequest: models.Movie = { id: "movie-001", version: "abcd", ...datasets.movie };
            const response = await sut.replaceItem(movieRequest, { force: true });
            
            // Check that the request is sent properly
            expectMockRequestsToBeValid(mock, 1, "PUT", `${sut.tableEndpoint}/movie-001`);
            const body = typeof mock.requests[0].body === "function" ? mock.requests[0].body() : mock.requests[0].body;
            expect(body).to.equal(JSON.stringify(movieRequest));
            expect(mock.requests[0].headers.get("if-match")).to.be.undefined;

            // Check that the response is correct.
            expect(response).to.eql(movieResponse);
        });

        it("throws when successful but no content is returned", async () => {
            const mock = new MockHttpClient().addResponse(200);
            const client = new DatasyncClient("https://localhost", { httpClient: mock });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movie: models.Movie = { id: "movie-001", ...datasets.movie };
            try {
                await sut.replaceItem(movie);
                assert.fail("replaceItem should not be successful when there is no content returned");
            } catch (err) {
                expect(err).to.be.instanceOf(RestError);
                const restError = err as RestError;
                expect(restError.code).to.equal("NO_CONTENT");
            }
        });

        it("throws when receiving bad JSON", async () => {
            const mock = new MockHttpClient().addResponse(200, "{this is bad json");
            const client = new DatasyncClient("https://localhost", { httpClient: mock });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movie: models.Movie = { id: "movie-001", ...datasets.movie };
            try {
                await sut.replaceItem(movie);
                assert.fail("replaceItem should not be successful when bad JSON is returned");
            } catch (err) {
                expect(err).to.be.instanceOf(RestError);
                const restError = err as RestError;
                expect(restError.code).to.equal("PARSE_ERROR");
            }
        });

        for (const statusCode of [ 409, 412 ]) {
            it(`throws a ConflictError when status code ${statusCode} is returned`, async () => {
                const mock = new MockHttpClient();
                const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

                const responseMovie: models.Movie = { id: "movie-001", version: "abcd", ...datasets.movie };
                mock.addResponse(statusCode, JSON.stringify(responseMovie), { "Content-Type": "application/json", "ETag": `"${responseMovie.version}"` });

                const requestMovie: models.Movie = { id: "movie-001", ...datasets.movie };
                try {
                    await sut.replaceItem(requestMovie);
                    assert.fail(`replaceItem should throw ConflictError when statusCode ${statusCode} is returned`);
                } catch(err) {
                    expect(err).to.be.instanceOf(ConflictError);
                    const conflict = err as ConflictError;
                    expect(conflict.request).to.not.be.undefined;
                    expect(conflict.response).to.not.be.undefined;
                    expect(conflict.serverValue as models.Movie).to.eql(responseMovie);
                }
            });

            it(`throws when a statusCode ${statusCode} occurs but no content is returned`, async () => {
                const mock = new MockHttpClient().addResponse(statusCode);
                const client = new DatasyncClient("https://localhost", { httpClient: mock });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);
    
                const movie: models.Movie = { id: "movie-001", ...datasets.movie };
                try {
                    await sut.replaceItem(movie);
                    assert.fail("replaceItem should not be successful when there is no content returned");
                } catch (err) {
                    expect(err).to.be.instanceOf(RestError);
                    const restError = err as RestError;
                    expect(restError.code).to.equal("NO_CONTENT");
                }
            });
        }
        
        for (const statusCode of [ 400, 401, 403, 413 ]) {
            it(`throws a RestError when status code ${statusCode} is returned`, async () => {
                const mock = new MockHttpClient().addResponse(statusCode);
                const client = new DatasyncClient("https://localhost", { httpClient: mock });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);
                const requestMovie: models.Movie = { id: "movie-001", ...datasets.movie };

                try {
                    await sut.replaceItem(requestMovie);
                    assert.fail(`replaceItem should throw RestError when statusCode ${statusCode} is returned`);
                } catch (err) {
                    expect(err).to.be.instanceOf(RestError);
                    const restError = err as RestError;
                    expect(restError.code).to.equal("HTTP_ERROR");
                }
            });
        }
    });

    describe("#updateItem", () => {
        for (const invalidId of datasets.invalidIds) {
            it(`throws when the entity ID is invalid (value: '${invalidId}')`, async () => {
                const mock = new MockHttpClient();
                const client = new DatasyncClient("https://localhost", { httpClient: mock });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

                const movie: models.Movie = { id: invalidId, ...datasets.movie };

                try {
                    await sut.updateItem(movie);
                    assert.fail(`expected updateItem to throw on invalidId = '${invalidId}'`);
                } catch (err) {
                    expect(err).to.be.instanceOf(InvalidArgumentError);
                }
            });
        }

        it("correctly formulates a request and handles a correct response (no version in request)", async () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movieResponse: models.Movie = {
                id: "movie-001",
                version: uuid(),
                updatedAt: new Date(),
                deleted: false,
                ...datasets.movie
            };
            mock.addResponse(200, JSON.stringify(movieResponse), { "ETag": `"${movieResponse.version}"` });

            const movieRequest: models.Movie = { id: "movie-001", ...datasets.movie };
            const response = await sut.updateItem(movieRequest);
            
            // Check that the request is sent properly
            expectMockRequestsToBeValid(mock, 1, "PATCH", `${sut.tableEndpoint}/movie-001`);
            const body = typeof mock.requests[0].body === "function" ? mock.requests[0].body() : mock.requests[0].body;
            expect(body).to.equal(JSON.stringify(movieRequest));
            expect(mock.requests[0].headers.get("if-match")).to.be.undefined;

            // Check that the response is correct.
            expect(response).to.eql(movieResponse);
        });

        it("correctly formulates a request and handles a correct response (version in request)", async () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movieResponse: models.Movie = {
                id: "movie-001",
                version: uuid(),
                updatedAt: new Date(),
                deleted: false,
                ...datasets.movie
            };
            mock.addResponse(200, JSON.stringify(movieResponse), { "ETag": `"${movieResponse.version}"` });

            const movieRequest: models.Movie = { id: "movie-001", version: "abcd", ...datasets.movie };
            const response = await sut.updateItem(movieRequest);
            
            // Check that the request is sent properly
            expectMockRequestsToBeValid(mock, 1, "PATCH", `${sut.tableEndpoint}/movie-001`);
            const body = typeof mock.requests[0].body === "function" ? mock.requests[0].body() : mock.requests[0].body;
            expect(body).to.equal(JSON.stringify(movieRequest));
            expect(mock.requests[0].headers.get("if-match")).to.equal("\"abcd\"");

            // Check that the response is correct.
            expect(response).to.eql(movieResponse);
        });

        it("correctly formulates a request and handles a correct response (version in request, forced)", async () => {
            const mock = new MockHttpClient();
            const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movieResponse: models.Movie = {
                id: "movie-001",
                version: uuid(),
                updatedAt: new Date(),
                deleted: false,
                ...datasets.movie
            };
            mock.addResponse(200, JSON.stringify(movieResponse), { "ETag": `"${movieResponse.version}"` });

            const movieRequest: models.Movie = { id: "movie-001", version: "abcd", ...datasets.movie };
            const response = await sut.updateItem(movieRequest, { force: true });
            
            // Check that the request is sent properly
            expectMockRequestsToBeValid(mock, 1, "PATCH", `${sut.tableEndpoint}/movie-001`);
            const body = typeof mock.requests[0].body === "function" ? mock.requests[0].body() : mock.requests[0].body;
            expect(body).to.equal(JSON.stringify(movieRequest));
            expect(mock.requests[0].headers.get("if-match")).to.be.undefined;

            // Check that the response is correct.
            expect(response).to.eql(movieResponse);
        });

        it("throws when successful but no content is returned", async () => {
            const mock = new MockHttpClient().addResponse(200);
            const client = new DatasyncClient("https://localhost", { httpClient: mock });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movie: models.Movie = { id: "movie-001", ...datasets.movie };
            try {
                await sut.updateItem(movie);
                assert.fail("updateItem should not be successful when there is no content returned");
            } catch (err) {
                expect(err).to.be.instanceOf(RestError);
                const restError = err as RestError;
                expect(restError.code).to.equal("NO_CONTENT");
            }
        });

        it("throws when receiving bad JSON", async () => {
            const mock = new MockHttpClient().addResponse(200, "{this is bad json");
            const client = new DatasyncClient("https://localhost", { httpClient: mock });
            const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

            const movie: models.Movie = { id: "movie-001", ...datasets.movie };
            try {
                await sut.updateItem(movie);
                assert.fail("updateItem should not be successful when bad JSON is returned");
            } catch (err) {
                expect(err).to.be.instanceOf(RestError);
                const restError = err as RestError;
                expect(restError.code).to.equal("PARSE_ERROR");
            }
        });

        for (const statusCode of [ 409, 412 ]) {
            it(`throws a ConflictError when status code ${statusCode} is returned`, async () => {
                const mock = new MockHttpClient();
                const client = new DatasyncClient("https://localhost", { httpClient: mock, tableReviver: movieReviver });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);

                const responseMovie: models.Movie = { id: "movie-001", version: "abcd", ...datasets.movie };
                mock.addResponse(statusCode, JSON.stringify(responseMovie), { "Content-Type": "application/json", "ETag": `"${responseMovie.version}"` });

                const requestMovie: models.Movie = { id: "movie-001", ...datasets.movie };
                try {
                    await sut.updateItem(requestMovie);
                    assert.fail(`updateItem should throw ConflictError when statusCode ${statusCode} is returned`);
                } catch(err) {
                    expect(err).to.be.instanceOf(ConflictError);
                    const conflict = err as ConflictError;
                    expect(conflict.request).to.not.be.undefined;
                    expect(conflict.response).to.not.be.undefined;
                    expect(conflict.serverValue as models.Movie).to.eql(responseMovie);
                }
            });

            it(`throws when a statusCode ${statusCode} occurs but no content is returned`, async () => {
                const mock = new MockHttpClient().addResponse(statusCode);
                const client = new DatasyncClient("https://localhost", { httpClient: mock });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);
    
                const movie: models.Movie = { id: "movie-001", ...datasets.movie };
                try {
                    await sut.updateItem(movie);
                    assert.fail("updateItem should not be successful when there is no content returned");
                } catch (err) {
                    expect(err).to.be.instanceOf(RestError);
                    const restError = err as RestError;
                    expect(restError.code).to.equal("NO_CONTENT");
                }
            });
        }
        
        for (const statusCode of [ 400, 401, 403, 413 ]) {
            it(`throws a RestError when status code ${statusCode} is returned`, async () => {
                const mock = new MockHttpClient().addResponse(statusCode);
                const client = new DatasyncClient("https://localhost", { httpClient: mock });
                const sut = new RemoteTable(client.serviceClient, "movies", new URL("tables/movies", client.endpointUrl), client.clientOptions);
                const requestMovie: models.Movie = { id: "movie-001", ...datasets.movie };

                try {
                    await sut.updateItem(requestMovie);
                    assert.fail(`updateItem should throw RestError when statusCode ${statusCode} is returned`);
                } catch (err) {
                    expect(err).to.be.instanceOf(RestError);
                    const restError = err as RestError;
                    expect(restError.code).to.equal("HTTP_ERROR");
                }
            });
        }
    });
});