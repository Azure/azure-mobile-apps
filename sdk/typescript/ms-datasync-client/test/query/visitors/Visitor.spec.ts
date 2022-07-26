// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { assert } from 'chai';
import { Node } from '../../../src/query/nodes';
import { Visitor } from '../../../src/query/visitors';

class BoomNode extends Node { };
class TestNode extends Node { 
    private _value: number;

    constructor(value: number) {
        super();
        this._value = value;
    }

    get intValue(): number { return this._value; }
};

class MockVisitor extends Visitor {
    constructor() {
        super();
        this.registerVisitor('TestNode', (node: TestNode) => node.intValue);
    }

    getSource(node: Node): string {
        if (node instanceof TestNode) {
            return `${node.intValue}`;
        }
        return 'Unrecognized node type';
    }
}

describe('src/query/visitors/Visitor', () => {
    describe('#constructor', () => {
        it('can be instantiated', () => { assert.isDefined(new MockVisitor()); });
    });

    describe('#visit', () => {
        it('returns null for null', () => { 
            const v = new MockVisitor();
            assert.isNull(v.visit(null));
        });

        it('handles non-Node values', () => {
            const v = new MockVisitor();
            assert.equal(v.visit(42), 42);
        });

        it('throws without a handler', () => {
            const v = new MockVisitor();
            assert.throws(() => { v.visit(new BoomNode()); });
        });

        it('can look up a type', () => {
            const v = new MockVisitor();
            assert.equal(v.visit(new TestNode(42)), 42);
        });
    });
});