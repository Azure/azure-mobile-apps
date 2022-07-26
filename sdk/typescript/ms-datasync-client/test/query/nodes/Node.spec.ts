// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { assert } from 'chai';
import { Node } from '../../../src/query/nodes';

class Expression extends Node {
    constructor() { super(); }
}

class BinaryExpression extends Expression {
    constructor() { super(); }
}

describe('src/query/nodes/Node', () => {
    describe('#constructor', () => {
        it('sets type in constructor', () => {
            const n = new Node();
            assert.equal(n.type, 'Node');
        });

        it('sets type in derived constructor', () => {
            const n = new Expression();
            assert.equal(n.type, 'Expression');
        });

        it('sets type in derived heirarchy', () => {
            const n = new BinaryExpression();
            assert.equal(n.type, 'BinaryExpression');
        });
    });
});