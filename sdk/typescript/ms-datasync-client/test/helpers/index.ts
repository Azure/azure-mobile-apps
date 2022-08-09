// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

export * from './MockHttpClient';
export * from './movie';

/**
 * A list of invalid IDs for test runners
 */
export const invalidIds: Array<string> = [
    ' ',
    '\t',
    'abcdef gh',
    '!!!',
    '?',
    ';',
    '{EA235ADF-9F38-44EA-8DA4-EF3D24755767}',
    '###'
];