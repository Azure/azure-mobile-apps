// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { expect } from 'chai';
import { BinaryExpression, BinaryOperators, Node } from '../../../src/query/nodes';

describe('src/query/nodes/BinaryExpression', () => {
    describe('#constructor', () => {
        it('sets type in constructor', () => {
            const n = new BinaryExpression(BinaryOperators.Equal);
            expect(n.type).to.equal('BinaryExpression');
            expect(n.operator).to.equal('Equal');
            expect(n.left).to.be.undefined;
            expect(n.right).to.be.undefined;
        });

        it('sets left when non-null', () => {
            const left = new Node();
            const n = new BinaryExpression(BinaryOperators.And, left);
            expect(n.type).to.equal('BinaryExpression');
            expect(n.operator).to.equal('And');
            expect(n.left).to.equal(left);
            expect(n.right).to.be.undefined;
        });

        it('sets right when non-null', () => {
            const right = new Node();
            const n = new BinaryExpression(BinaryOperators.NotEqual, undefined, right);
            expect(n.type).to.equal('BinaryExpression');
            expect(n.operator).to.equal('NotEqual');
            expect(n.left).to.be.undefined;
            expect(n.right).to.equal(right);
        });

        it('sets left and right when non-null', () => {
            const left = new Node();
            const right = new Node();
            const n = new BinaryExpression(BinaryOperators.GreaterThan, left, right);
            expect(n.type).to.equal('BinaryExpression');
            expect(n.operator).to.equal('GreaterThan');
            expect(n.left).to.equal(left);
            expect(n.right).to.equal(right);
        });

        it('can set the operator', () => {
            const n = new BinaryExpression(BinaryOperators.LessThanOrEqual);
            n.operator = BinaryOperators.LessThan;
            expect(n.operator).to.equal('LessThan');
        });

        it('can set left and right', () => {
            const n = new BinaryExpression(BinaryOperators.Subtract);
            n.left = new Node();
            n.right = new Node();
            expect(n.left).to.not.be.undefined;
            expect(n.right).to.not.be.undefined;
            expect(n.operator).to.equal('Subtract');
        });
    });
});