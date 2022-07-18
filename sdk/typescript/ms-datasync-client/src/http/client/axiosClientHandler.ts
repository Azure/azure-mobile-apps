// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import axios, { 
    AxiosInstance, 
    AxiosInterceptorOptions, 
    AxiosRequestConfig,
    AxiosRequestHeaders,
    AxiosResponse
} from 'axios';
import {  
    HttpMethod, 
    HttpRequestMessage, 
    HttpResponseMessage 
} from './clientModels';
import {
    HttpClientHandler
} from './clientHandlers';

/**
 * The definition of an Axios Interceptor set.
 */
export interface AxiosInterceptors {
    request?: {
        onFulfilled?: ((value: AxiosRequestConfig) => AxiosRequestConfig | undefined);
        onRejected?: ((error: any) => any), // eslint-disable-line @typescript-eslint/no-explicit-any
        options?: AxiosInterceptorOptions;
    }
    response?: {
        onFulfilled?: ((value: AxiosResponse) => AxiosResponse | Promise<AxiosResponse> | undefined);
        onRejected?: ((error: any) => any), // eslint-disable-line @typescript-eslint/no-explicit-any
        options?: AxiosInterceptorOptions;
    }
}
/**
 * An implementation of the HttpClientHandler that uses the axios library
 * @see https://axios-http.com
 */
export class AxiosClientHandler extends HttpClientHandler {
    private _client: AxiosInstance;

    /**
     * Creates a new client handler with the specified configuration.
     * 
     * @param axiosConfiguration The Axios configuration.
     * @param interceptors The interceptors for the request/response pipeline.
     */
    constructor(axiosConfiguration?: AxiosRequestConfig, interceptors?: AxiosInterceptors) {
        super();
        this._client = axios.create(axiosConfiguration);
        if (typeof interceptors !== 'undefined') {
            if (typeof interceptors.request !== 'undefined') {
                this._client.interceptors.request.use(interceptors.request.onFulfilled, interceptors.request.onRejected, interceptors.request.options);
            }
            if (typeof interceptors.response !== 'undefined') {
                this._client.interceptors.response.use(interceptors.response.onFulfilled, interceptors.response.onRejected, interceptors.response.options);
            }
        }
    }

    /**
     * Converts the request headers in the HttpRequestMessage to the format needed by axios.
     * 
     * @param request The HttpRequestMessage holding the request headers
     * @returns The Axios version of the request headers
     */
    private getRequestHeaders(request: HttpRequestMessage): AxiosRequestHeaders {
        const headers: AxiosRequestHeaders = {};
        request.headers.forEach((value, key) => { headers[key] = value; });
        return headers;
    }

    /**
     * Converts the response headers provided by axios into the format for a HttpResponseMessage
     * 
     * @param response The Axios Response.
     * @returns The map of headers.
     */
    private getResponseHeaders(response: AxiosResponse): Map<string, string> {
        const headers: Map<string, string> = new Map<string, string>();
        for (const header in response.headers) {
            headers.set(header, response.headers[header]);
        }
        return headers;
    }

    /**
     * Sends a request to the remote service, and returns the result.
     * 
     * @param request The HttpRequestMessage to send.
     * @param signal An abort signal to monitor.
     * @returns A promise that resolves to the HttpResponseMessage. 
     */
    public async sendAsync(request: HttpRequestMessage, signal?: AbortSignal): Promise<HttpResponseMessage> {
        const axiosRequest: AxiosRequestConfig = {
            url: request.requestUri.href,
            method: HttpMethod[request.method].toLowerCase(),
            headers: this.getRequestHeaders(request),
            data: request.content,
            responseType: 'arraybuffer',
            signal: signal
        };
        const axiosResponse = await this._client.request(axiosRequest);
        const response = new HttpResponseMessage(
            axiosResponse.status,
            axiosResponse.statusText, 
            request, 
            this.getResponseHeaders(axiosResponse),
            Buffer.from(axiosResponse.data).toString('utf8')
        );
        return response;
    }
}