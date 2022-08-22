// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.


import { assert, expect, expectMockRequestsToBeValid } from "../helpers/chai-helper";
import { datasets, models, MockHttpClient } from "../helpers";
import { v4 as uuid } from "uuid";

import { ConflictError, DatasyncClient, InvalidArgumentError, RemoteTable, RestError } from "../../src";
import { JsonReviver } from "../../src/table";

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
            expect(sut.tableEndpoint.href).to.equal("https://localhost/tables/movies");
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
            expectMockRequestsToBeValid(mock, 1, "POST", sut.tableEndpoint.href);
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

    // describe("#deleteItem", () => {

    // });

    // describe("#getItem", () => {

    // });

    // describe("#getPageOfItems", () => {

    // });

    // describe("#listItems", () => {
    // });

    // describe("#replaceItem", () => {

    // });
});