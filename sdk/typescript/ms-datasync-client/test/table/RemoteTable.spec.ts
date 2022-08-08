// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect, use } from 'chai';
import chaiString from 'chai-string';
import chaiAsPromised from 'chai-as-promised';

import * as http from '../../src/http';
import * as table from '../../src/table';
import { MockHttpClient } from '../helpers/MockHttpClient';

use(chaiString);
use(chaiAsPromised);

describe('table/RemoteTable', () => {
    describe('#constructor', () => {
        it('can create a remote table', () => {
            const mock = new MockHttpClient();
            const client = new http.ServiceHttpClient('http://localhost', { httpClient: mock });
            const sut = new table.RemoteTable('/tables/todoitem', client);

            expect(sut).to.not.be.undefined;
            expect(sut.endpoint.href).to.equal('http://localhost/tables/todoitem');
        });

        it('throw when providing an invalid path', () => {
            const mock = new MockHttpClient();
            const client = new http.ServiceHttpClient('http://localhost', { httpClient: mock });

            expect(() => { new table.RemoteTable('invalid//path', client); }).to.throw;
        });
    });

    describe('#createItem', async () => {

    });

    describe('#deleteItem', async () => {
    });

    describe('#getItem', async () => {

    });

    describe('#getPageOfItems', async () => {

    });

    describe('#listItems', async () => {

    });

    describe('#replaceItem', async () => {

    });
});