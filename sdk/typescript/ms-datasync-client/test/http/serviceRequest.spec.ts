// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect } from 'chai';
import { ArgumentError } from '../../src/errors';
import { HttpMethod, ServiceRequest } from '../../src/http';

describe('http/ServiceRequest', () => {
    describe('#constructor', () => {
        it('sets default values', () => {
            const req = new ServiceRequest();
            expect(req.method).to.equal(HttpMethod.GET);
        });

        it('can set the method to all values', () => {
            const valuesToTest = [
                HttpMethod.GET,
                HttpMethod.DELETE,
                HttpMethod.POST,
                HttpMethod.PUT,
                HttpMethod.QUERY
            ];

            for (const method of valuesToTest) {
                const req = new ServiceRequest(method);
                expect(req.method).to.equal(method, `method ${HttpMethod[method]} cannot be set in constructor`);
            }
        });

        it('throws when setting an invalid method', () => {
            const value: HttpMethod = 90;
            expect(() => { new ServiceRequest(value); }).to.throw(ArgumentError);
        });
    });

    describe('#withMethod', () => {
        it('can set the method to all values', () => {
            const valuesToTest = [
                HttpMethod.GET,
                HttpMethod.DELETE,
                HttpMethod.POST,
                HttpMethod.PUT,
                HttpMethod.QUERY
            ];

            for (const method of valuesToTest) {
                const req = new ServiceRequest().withMethod(method);
                expect(req.method).to.equal(method, `method ${HttpMethod[method]} cannot be set in withMethod`);
            }
        });

        it('throws when setting an invalid method', () => {
            const value: HttpMethod = 90;
            expect(() => { new ServiceRequest().withMethod(value); }).to.throw(ArgumentError);
        });
    });
});
