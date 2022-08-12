// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect } from "../helpers/chai-helper";
import { MockHttpClient } from "../helpers/http-client";
import { PipelineRequest, createHttpHeaders } from "@azure/core-rest-pipeline";

import { datasyncClientPolicy } from "../../src/utils/policies";

describe("utils/policies", () => {
    describe("datasyncClientPolicy", () => {
        it("has a name", () => {
            const policy = datasyncClientPolicy({});
            expect(policy.name).to.have.length.greaterThan(1);
        });

        it("sets the protocol version by default", async () => {
            const policy = datasyncClientPolicy({});
            const mock = new MockHttpClient().addResponse(204);

            const request: PipelineRequest = {
                url: "http://localhost",
                method: "GET",
                headers: createHttpHeaders(),
                timeout: 0,
                withCredentials: false,
                requestId: "1"
            };

            await policy.sendRequest(request, r => mock.sendRequest(r));

            expect(mock.requests).to.have.lengthOf(1);
            expect(mock.requests[0].headers.get("zumo-api-version")).to.equal("3.0.0");
        });

        it("sets the protocol version by option", async () => {
            const policy = datasyncClientPolicy({ apiVersion: "2.1.0" });
            const mock = new MockHttpClient().addResponse(204);

            const request: PipelineRequest = {
                url: "http://localhost",
                method: "GET",
                headers: createHttpHeaders(),
                timeout: 0,
                withCredentials: false,
                requestId: "1"
            };

            await policy.sendRequest(request, r => mock.sendRequest(r));

            expect(mock.requests).to.have.lengthOf(1);
            expect(mock.requests[0].headers.get("zumo-api-version")).to.equal("2.1.0");
        });

        it("sets the internal user-agent when user-agent is set", async () => {
            const policy = datasyncClientPolicy({ apiVersion: "2.1.0" });
            const mock = new MockHttpClient().addResponse(204);

            const headers = createHttpHeaders();
            headers.set("user-agent", "test-wumpus");
            const request: PipelineRequest = {
                url: "http://localhost",
                method: "GET",
                headers: headers,
                timeout: 0,
                withCredentials: false,
                requestId: "1"
            };

            await policy.sendRequest(request, r => mock.sendRequest(r));
            expect(mock.requests).to.have.lengthOf(1);
            expect(mock.requests[0].headers.get("x-zumo-version")).to.equal("test-wumpus");
        });

        it("sets the internal user-agent when user-agent is set", async () => {
            const policy = datasyncClientPolicy({ apiVersion: "2.1.0" });
            const mock = new MockHttpClient().addResponse(204);

            const headers = createHttpHeaders();
            const request: PipelineRequest = {
                url: "http://localhost",
                method: "GET",
                headers: headers,
                timeout: 0,
                withCredentials: false,
                requestId: "1"
            };

            await policy.sendRequest(request, r => mock.sendRequest(r));
            expect(mock.requests).to.have.lengthOf(1);
            expect(mock.requests[0].headers.get("x-zumo-version")).to.be.undefined;
        });
    });
});