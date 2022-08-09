// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { TableQuery } from './models';
import { ServiceRequest, ServiceResponse } from '../http';
import { ConflictError, EntityNotFoundError, HttpError } from '../errors';

/**
 * Converts a table query into a 
 * @param filter 
 */
export function createQueryString(filter?: TableQuery): string {
    if (typeof filter === 'undefined') {
        return '';
    }

    const params = new URLSearchParams();
    if (typeof filter.filter === 'string') {
        params.append('$filter', filter.filter);
    }

    if (typeof filter.includeCount === 'boolean' && filter.includeCount) {
        params.append('$count', 'true');
    }

    if (typeof filter.includeDeletedItems === 'boolean' && filter.includeDeletedItems) {
        params.append('__includedeleted', 'true');
    }

    if (typeof filter.selection !== 'undefined' && filter.selection.length > 0) {
        params.append('$select', filter.selection.join(','));
    }

    if (typeof filter.skip !== 'undefined' && filter.skip > 0) {
        params.append('$skip', `${filter.skip}`);
    }

    if (typeof filter.top !== 'undefined' && filter.top > 0) {
        params.append('$top', `${filter.top}`);
    }
    return params.toString();
}

/**
 * Converts the request/response set into the appropriate type of HttpError.
 * 
 * @param request The ServiceRequest object.
 * @param response The ServiceResponse object.
 * @returns the Error object that is relevant to the response.
 */
export function  getStandardError(request: ServiceRequest, response: ServiceResponse): Error {
    if (response.statusCode === 404 || response.statusCode === 410) {
        return new EntityNotFoundError('Entity not found', request, response);
    }

    if (response.isConflictStatusCode) {
        return new ConflictError('Conflict', request, response);
    }

    return new HttpError(`Error Status Code ${response.statusCode}`, request, response);
}