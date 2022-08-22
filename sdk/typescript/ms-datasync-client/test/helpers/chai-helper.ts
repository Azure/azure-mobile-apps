// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect, assert, use } from "chai";
import chaiAsPromised from "chai-as-promised";
import chaiDateTime from "chai-datetime";
import chaiString from "chai-string";
import chaiSubset from "chai-subset";
import { MockHttpClient } from "./http-client";

// Registers the chai plugins into Chai
use(chaiAsPromised);
use(chaiDateTime);
use(chaiString);
use(chaiSubset);

/**
 * Method to check the mock requests.
 * 
 * @param mock The mock object.
 * @param length The expected length.
 * @param method The expected method.
 * @param url The expected URL.
 */
export function expectMockRequestsToBeValid(mock: MockHttpClient, length: number, method: string, url: string | string[]) {
    expect(mock.requests).to.have.length(length);
    for (const req of mock.requests) {
        expect(req.method).to.equal(method);
        const expectedUrl = typeof url === "string" ? url : url.shift();
        expect(req.url).to.equal(expectedUrl);
    }
}

// Re-export expect and assert, except now the plugins are registered.
export { expect, assert };
