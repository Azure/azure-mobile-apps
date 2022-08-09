// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { assert, expect, use } from 'chai';
import chaiDateTime from 'chai-datetime';
import { DataTransferObject } from '../../src/table';
import { MockHttpClient } from './MockHttpClient';
import * as http from '../../src/http';
import * as table from '../../src/table';
import * as msrest from '@azure/core-rest-pipeline';

use(chaiDateTime);

/**
 * Definition of the movie type from the test service.
 */
export interface Movie extends DataTransferObject {
    /** true if the movie won the oscar for best picture. */
    bestPictureWinner: boolean;

    /** The running time of the movie. */
    duration: number;

    /** The rating given to the movie. */
    rating: string;

    /** The release date of the movie. */
    releaseDate: Date;

    /** The title of the movie. */
    title: string;

    /** The year the movie was released. */
    year: number;
}

/**
 * The base movie type is the movie type without the DTO stuff.
 */
export type BaseMovie = Omit<Movie, keyof DataTransferObject>;

/**
 * A test movie type.
 */
export const testMovie: BaseMovie = {
    bestPictureWinner: false,
    duration: 142,
    rating: 'R',
    releaseDate: new Date('1994-10-14'),
    title: 'The Shawshank Redemption',
    year: 1994
};

/**
 * Gets a mocked table.  Use the following:
 * 
 * - tableRef.client to get the service client.
 * - tableRef.client.options.httpClient to get the mock
 * 
 * @returns The mocked table
 */
 export function getMovieTable(): table.RemoteTable<Movie> {
    const mock = new MockHttpClient();
    const client = new http.ServiceHttpClient('http://localhost', { httpClient: mock });
    const tableRef = new table.RemoteTable<Movie>('/tables/movies', client);

    return tableRef;
}

/**
 * Adds a response to the mocked HTTP service.
 * 
 * @param tableRef The table reference being used.
 * @param statusCode The status code to return.
 * @param body The body of the response.
 * @param headers Any headers to return.
 */
export function addMovieResponse(tableRef: table.RemoteTable<Movie>, statusCode: number, body?: string, headers?: http.HttpHeaders): void {
    const mock = tableRef.client.options.httpClient;
    if (mock instanceof MockHttpClient) {
        mock.addResponse(statusCode, body, headers);
    }
}

/**
 * Gets the requests that were sent to the mock HTTP service.
 * 
 * @param tableRef The table reference being used.
 * @returns An array of pipeline requests.
 */
export function getMovieRequests(tableRef: table.RemoteTable<Movie>): Array<msrest.PipelineRequest> {
    const mock = tableRef.client.options.httpClient;
    if (mock instanceof MockHttpClient) {
        return mock.requests;
    }
    return [];
}

/**
 * Check to see if two Dates are equivalent.
 * 
 * @param expected 
 * @param actual 
 */
function expectDatesToMatch(expected?: Date, actual?: Date | string) {
    if (typeof actual === 'undefined') {
        if (typeof expected === 'undefined') {
            return;
        }
    }

    const actualDate = typeof actual === 'string' ? new Date(actual) : actual;
    if (expected instanceof Date && actualDate instanceof Date) {
        expect(actualDate).to.equalDate(expected);
    } else if (expected instanceof Date) {
        assert.fail(`Expected date = '${expected.toISOString()}', but actual date is undefined`);
    } else if (actualDate instanceof Date) {
        assert.fail(`Actual date = '${actualDate.toISOString()}', but expected date is undefined`);
    }
}

/**
 * Expect two DTOs to match.
 * 
 * @param expected 
 * @param actual 
 */
export function expectDTOsToMatch(expected: DataTransferObject, actual: DataTransferObject) {
    expect(actual.id).to.eql(expected.id);
    expectDatesToMatch(expected.updatedAt, actual.updatedAt);
    expect(actual.version).to.eql(expected.version);
    expect(actual.deleted).to.eql(expected.deleted);
}

/**
 * Expect two BaseMovie objects to match.
 * 
 * @param expected 
 * @param actual 
 */
export function expectBaseMoviesToMatch(expected: BaseMovie, actual: BaseMovie) {
    expect(actual.bestPictureWinner).to.eql(expected.bestPictureWinner);
    expect(actual.duration).to.eql(expected.duration);
    expect(actual.rating).to.eql(expected.rating);
    expectDatesToMatch(expected.releaseDate, actual.releaseDate);
    expect(actual.title).to.eql(expected.title);
    expect(actual.year).to.eql(expected.year);
}

/**
 * Expect two Movie objects to match.
 * 
 * @param expected 
 * @param actual 
 */
export function expectMoviesToMatch(expected: Movie, actual: Movie) {
    expectDTOsToMatch(expected, actual);
    expectBaseMoviesToMatch(expected, actual);
}


