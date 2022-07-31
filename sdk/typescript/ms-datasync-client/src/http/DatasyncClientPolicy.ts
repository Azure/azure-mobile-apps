// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import {
    PipelinePolicy,
    PipelineRequest,
    PipelineResponse,
    SendRequest,
} from '@azure/core-rest-pipeline';
import * as ServiceHeaders from './ServiceHeaders';

/**
 * The default protocol version.
 */
export const PROTOCOLVERSION = '3.0.0';

/**
 * The options that this version takes.
 */  
export interface DatasyncClientPolicyOptions {
    apiVersion?: string
}

/**
 * The pipeline policy for the datasync client.
 * 
 * @param options The options for this client.
 * @returns The pipeline policy.
 */
export function datasyncClientPolicy(options: DatasyncClientPolicyOptions): PipelinePolicy {
    return {
        name: 'datasyncClientPolicy',
        async sendRequest(request: PipelineRequest, next: SendRequest): Promise<PipelineResponse> {
            // Set protocol version.
            request.headers.set(ServiceHeaders.ProtocolVersion, options.apiVersion || PROTOCOLVERSION);

            // Set internal user agent to be the same as the requested user agent.
            const userAgent = request.headers.get(ServiceHeaders.UserAgent);
            if (typeof userAgent !== 'undefined') {
                request.headers.set(ServiceHeaders.InternalUserAgent, userAgent);
            }

            return next(request);
        }
    };
}