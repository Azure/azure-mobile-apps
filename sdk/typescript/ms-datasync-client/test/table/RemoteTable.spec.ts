// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { assert, expect, use } from 'chai';
import chaiString from 'chai-string';
import chaiAsPromised from 'chai-as-promised';
import chaiDateTime from 'chai-datetime';
import { v4 as uuid } from 'uuid';

import { ArgumentError, ConflictError, RestError } from '../../src/errors';
import * as http from '../../src/http';
import * as table from '../../src/table';
import * as mock from '../helpers';

import { expectMoviesToMatch } from '../helpers/movie';

use(chaiString);
use(chaiAsPromised);
use(chaiDateTime);

describe('table/RemoteTable', () => {
    describe('#constructor', () => {
        it('can create a remote table', () => {
            const sut = mock.getMovieTable();

            expect(sut).to.not.be.undefined;
            expect(sut.endpoint.href).to.equal('http://localhost/tables/movies');
            expect(sut.client).to.not.be.undefined;
        });

        it('throw when providing an invalid path', () => {
            const client = new http.ServiceHttpClient('http://localhost', { httpClient: new mock.MockHttpClient() });
            expect(() => { new table.RemoteTable('invalid//path', client); }).to.throw;
        });
    });

    describe('#createItem', () => {
        it('throws when the ID of the entity is invalid', async () => {
            for (const invalidId of mock.invalidIds) {
                const sut = mock.getMovieTable();
                const movie: mock.Movie = {
                    id: invalidId,
                    ...mock.testMovie,
                };

                try {
                    await sut.createItem(movie);
                    assert.fail('expected createItem to throw');
                } catch (err) {
                    expect(err).to.be.instanceOf(ArgumentError);
                }
            }
        });

        it('correctly formulates request and response', async () => {
            const sut = mock.getMovieTable();
            const movieResponse: mock.Movie = {
                id: 'movie-001',
                version: uuid(),
                updatedAt: new Date(),
                deleted: false,
                ...mock.testMovie
            };
            mock.addMovieResponse(sut, 200, JSON.stringify(movieResponse), {
                'ETag': `"${movieResponse.version}"`
            });

            const response = await sut.createItem(movieResponse);

            mock.expectMoviesToMatch(movieResponse, response);

            // Check that request is sent properly
            const requests = mock.getMovieRequests(sut);
            expect(requests).to.have.lengthOf(1);
            const req = requests[0];

            expect(req.headers.get('zumo-api-version')).to.equal('3.0.0');
            expect(req.headers.get('x-zumo-version')).to.startWith('Datasync/5.0.0');
            expect(req.headers.get('content-type')).to.startWith('application/json');
            expect(req.method).to.equal('POST');
            expect(req.url).to.equal('http://localhost/tables/movies');

            if (typeof req.body === 'function') {
                const body = req.body();
                expect(body).to.equal(JSON.stringify(movieResponse));
            }
        });

        it('throws when successful but providing no content', async () => {
            const sut = mock.getMovieTable();
            mock.addMovieResponse(sut, 200);
            const movie: mock.Movie = {
                id: 'movie-001',
                ...mock.testMovie,
            };

            try {
                await sut.createItem(movie);
                assert.fail('createItem should not be successful when no content');
            } catch (err) {
                expect(err).to.be.instanceOf(RestError);
            }
        });

        it('throws when receiving bad JSON content', async () => {
            const sut = mock.getMovieTable();
            mock.addMovieResponse(sut, 200, '{this-is-bad-json');
            const movie: mock.Movie = {
                id: 'movie-001',
                ...mock.testMovie,
            };

            try {
                await sut.createItem(movie);
                assert.fail('createItem should not be successful when returning bad JSON content');
            } catch (err) {
                expect(err).to.be.instanceOf(RestError);
                if (err instanceof RestError) {
                    expect(err.code).to.equal('PARSE_ERROR');
                }
            }
        });

        it('throws a ConflictException when a 409 or 412 is received', async () => {
            const sut = mock.getMovieTable();
            const responseMovie: mock.Movie = {
                id: 'movie-001',
                version: 'abcd',
                ...mock.testMovie
            };

            for (const statusCode of [ 409, 412]) {
                mock.addMovieResponse(sut, statusCode, JSON.stringify(responseMovie), { 'Content-Type': 'application/json', 'ETag': '"abcd"' });
                const movie: mock.Movie = {
                    id: 'movie-001',
                    ...mock.testMovie,
                };
    
                try {
                    await sut.createItem(movie);
                    assert.fail('createItem should not be successful when returning bad JSON content');
                } catch (err) {
                    expect(err).to.be.instanceOf(ConflictError);
                    if (err instanceof ConflictError) {
                        expect(err.request).to.not.be.undefined;
                        expect(err.response).to.not.be.undefined;
                        expectMoviesToMatch(responseMovie, err.serverValue as mock.Movie);
                    }
                }
            }
        });

        it('throws when a conflict without content is received', async () => {
            const sut = mock.getMovieTable();
            const movie: mock.Movie = {
                id: 'movie-001',
                ...mock.testMovie,
            };

            for (const statusCode of [ 409, 412 ]) {
                mock.addMovieResponse(sut, statusCode);

                try {
                    await sut.createItem(movie);
                    assert.fail('createItem should not be successful when returning bad JSON content');
                } catch (err) {
                    expect(err).to.be.instanceOf(RestError);
                }
            }
        });

        it('throws a RestError when any other error is received', async () => {
            const sut = mock.getMovieTable();
            const movie: mock.Movie = {
                id: 'movie-001',
                ...mock.testMovie,
            };

            for (const statusCode of [ 400, 401, 403, 413 ])
            mock.addMovieResponse(sut, statusCode);

            try {
                await sut.createItem(movie);
                assert.fail('createItem should not be successful when an error is returned');
            } catch (err) {
                expect(err).to.be.instanceOf(RestError);
                if (err instanceof RestError) {
                    expect(err.code).to.equal('HTTP_ERROR');
                }
            }
        });
    });

    // describe('#deleteItem', () => {
    // });

    // describe('#getItem', () => {

    // });

    // describe('#getPageOfItems', () => {

    // });

    // describe('#listItems', () => {

    // });

    // describe('#replaceItem', () => {

    // });
});