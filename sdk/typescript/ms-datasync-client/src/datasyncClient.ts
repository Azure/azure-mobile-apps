// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

export class DatasyncClient {
    private endpointUrl: string;

    constructor(endpointUrl: string) {
        this.endpointUrl = endpointUrl;
    }

    get endpoint(): string {
        return this.endpointUrl;
    }
}