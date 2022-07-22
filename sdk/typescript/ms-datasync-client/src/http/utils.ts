// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import * as client from './client';
import { version } from '../../package.json';

/**
 * Given an array of handlers, turn it into a set of linked handlers suitable
 * for using as a HTTP pipeline.
 * 
 * @param handlers the array of handlers
 * @returns the root handler
 */
 export function createPipeline(handlers: client.HttpMessageHandler[]): client.HttpMessageHandler {
    // short-circuit - no handlers
    if (handlers.length === 0) {
        return getDefaultClientHandler();
    }

    // if the last handler is not a client handler, add one
    if (!(handlers[handlers.length - 1] instanceof client.HttpClientHandler)) {
        handlers.push(getDefaultClientHandler());
    }

    // Go through the handlers - if it is a DelegatingHandler, then add the next one in the list.
    handlers.forEach((value, index) => {
        if (value instanceof client.DelegatingHandler) {
            value.innerHandler = handlers[index + 1];
        } else if (index !== handlers.length - 1) {
            throw new TypeError('All message handlers except the last one must be a DelegatingHandler');
        }
    });

    return handlers[0];
}

/**
 * The default client handler.
 * 
 * @returns The default client handler.
 */
function getDefaultClientHandler(): client.HttpClientHandler {
    return new client.FetchClientHandler();
}

/**
 * Returns the user agent that requests should use.  Note that some platforms do not allow you to
 * specify the user agent, but we have a second header for those cases.
 * 
 * @returns The user agent.
 */
export function getUserAgent() {
    const baseVersion = version.split(/\./).slice(0, 2).join('.');
    return `Datasync/${baseVersion} (lang=typescript;ver=${version})`;
}