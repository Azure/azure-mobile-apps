// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect } from "../helpers/chai-helper";

import * as validate from "../../src/utils/validate";

describe("utils/validate.ts", () => {
    describe("#isDatasyncServiceUrl", () => {
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
            it(`returns ${testCase.expected} for '${testCase.url}'`, () => {
                expect(validate.isDatasyncServiceUrl(new URL(testCase.url))).to.equal(testCase.expected);
            });
        }
    });

    describe("#isEntityId", () => {
        it("returns false for non-string values", () => {
            expect(validate.isEntityId(1)).to.be.false;
            expect(validate.isEntityId(new Date())).to.be.false;
            expect(validate.isEntityId([])).to.be.false;
            expect(validate.isEntityId({})).to.be.false;
            expect(validate.isEntityId(true)).to.be.false;
        });
    });

    describe("#isInCIDR", () => {
        const cidrBlocks = [
            "10.0.0.0/8",
            "172.16.0.0/12",
            "192.168.0.0/8"
        ];

        const testCases = [
            { value: "10.100.8.1",                     expected: true },
            { value: "172.17.200.50",                  expected: true },
            { value: "192.168.0.2",                    expected: true },
            { value: "127.0.0.1",                      expected: false },
            { value: "4.4.4.4",                        expected: false },
            { value: "localhost",                      expected: false },
            { value: "myhost.local",                   expected: false },
            { value: "::1",                            expected: false },
            { value: "[::1]",                          expected: false },
            { value: "mydomain.com",                   expected: false },
            { value: "ds.azurewebsites.net",           expected: false },
            { value: "2001:0db8:85a3::8a2e:0370:7334", expected: false },
            { value: "[2001::7334]",                   expected: false }
        ];

        for (const testCase of testCases) {
            it(`returns ${testCase.expected} for '${testCase.value}'`, () => {
                expect(validate.isInCIDR(testCase.value, cidrBlocks)).to.equal(testCase.expected);
            });
        }
    });

    describe("#isIPv4", () => {
        const testCases = [
            { value: "10.100.8.1",                     expected: true },
            { value: "172.17.200.50",                  expected: true },
            { value: "192.168.0.2",                    expected: true },
            { value: "127.0.0.1",                      expected: true },
            { value: "4.4.4.4",                        expected: true },
            { value: "localhost",                      expected: false },
            { value: "myhost.local",                   expected: false },
            { value: "::1",                            expected: false },
            { value: "[::1]",                          expected: false },
            { value: "mydomain.com",                   expected: false },
            { value: "ds.azurewebsites.net",           expected: false },
            { value: "2001:0db8:85a3::8a2e:0370:7334", expected: false },
            { value: "[2001::7334]",                   expected: false }
        ];

        for (const testCase of testCases) {
            it(`returns ${testCase.expected} for '${testCase.value}'`, () => {
                expect(validate.isIPv4(testCase.value)).to.equal(testCase.expected);
            });
        }
    });

    describe("#isLocalNetwork", () => {
        const testCases = [
            { value: "10.100.8.1",                     expected: true },
            { value: "172.17.200.50",                  expected: true },
            { value: "192.168.0.2",                    expected: true },
            { value: "127.0.0.1",                      expected: true },
            { value: "4.4.4.4",                        expected: false },
            { value: "localhost",                      expected: true },
            { value: "myhost.local",                   expected: true },
            { value: "::1",                            expected: true },
            { value: "[::1]",                          expected: true },
            { value: "mydomain.com",                   expected: false },
            { value: "ds.azurewebsites.net",           expected: false },
            { value: "2001:0db8:85a3::8a2e:0370:7334", expected: false },
            { value: "[2001::7334]",                   expected: false }
        ];

        for (const testCase of testCases) {
            it(`returns ${testCase.expected} for '${testCase.value}'`, () => {
                expect(validate.isLocalNetwork(testCase.value)).to.equal(testCase.expected);
            });
        }
    });
});