// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { HttpMessageHandler  } from "./client";

/**
 * The HTTP client options
 */
export interface ServiceClientOptions {
    /** The list of message handlers in the HTTP pipeline */
    httpPipeline?: HttpMessageHandler[],

    /** The timeout to wait - default is 100s */
    httpTimeout?: number;

    /** The installation ID for this app on this device */
    installationId?: string;

    /** Set this to a blank string to avoid sending library details */
    userAgent?: string;
}