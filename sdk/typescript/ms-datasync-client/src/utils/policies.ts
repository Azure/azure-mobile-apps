// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { PipelinePolicy, PipelineRequest, PipelineResponse, SendRequest } from "@azure/core-rest-pipeline";
import * as ServiceHeaders from "./serviceHeaders";
import { ApiProtocolVersion } from "./constants";
import { DatasyncClientOptions } from "../DatasyncClient";

/**
 * Creates the pipeline policy for communicating with a datasync service.
 * 
 * @param options - the options for this HTTP client.
 * @returns The pipeline policy.
 */
export function datasyncClientPolicy(options: DatasyncClientOptions): PipelinePolicy {
    return {
        name: "datasyncClientPolicy",
        async sendRequest(request: PipelineRequest, next: SendRequest): Promise<PipelineResponse> {
            // ZUMO-API-VERSION header
            request.headers.set(ServiceHeaders.ProtocolVersion, options.apiVersion || ApiProtocolVersion);

            // X-ZUMO-VERSION header (internal user agent)
            const userAgent = request.headers.get(ServiceHeaders.UserAgent);
            if (typeof userAgent !== "undefined") {
                request.headers.set(ServiceHeaders.InternalUserAgent, userAgent);
            }

            return next(request);
        }
    };
}