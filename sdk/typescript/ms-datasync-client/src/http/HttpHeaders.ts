// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

/**
 * An interface for HTTP headers.
 */
export interface HttpHeaders {
    /**
     * headerName: The name of the header.
     * headerValue: The value of the header.
     */
    [headerName: string]: string;
}
