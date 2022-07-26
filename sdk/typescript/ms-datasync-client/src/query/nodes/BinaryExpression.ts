// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { Node } from './Node';
import { QueryExpression } from './QueryExpression';

interface OperatorMap {
    [operator: string]: string
};

/**
 * The list of known binary operators.
 */
export const BinaryOperators: OperatorMap = {
    And: 'And',
    Or: 'Or',
    Add: 'Add',
    Subtract: 'Subtract',
    Multiply: 'Multiply',
    Divide: 'Divide',
    Modulo: 'Modulo',
    GreaterThan: 'GreaterThan',
    GreaterThanOrEqual: 'GreaterThanOrEqual',
    LessThan: 'LessThan',
    LessThanOrEqual: 'LessThanOrEqual',
    NotEqual: 'NotEqual',
    Equal: 'Equal'
};

/**
 * Represents an expression that has a binary operator.
 */
export class BinaryExpression extends QueryExpression {
    private _operator: string;
    private _left?: Node;
    private _right?: Node;

    /**
     * Creates a new binary expression.
     * 
     * @param operator The operator of the binary expression
     * @param left The left operand of the binary expression
     * @param right The right operand of the binary expression
     */
    constructor(operator: string, left?: Node, right?: Node) {
        super();

        this._operator = operator;
        this._left = left;
        this._right = right;
    }

    /**
     * The operator of the binary expression.
     */
    get operator(): string { return this._operator; }
    set operator(value: string) { this._operator = value; }

    /**
     * The left operand of the binary expression.
     */
    get left(): Node | undefined { return this._left; }
    set left(value: Node | undefined) { this._left = value; }

    /**
     * The right operand of the binary expression.
     */
    get right(): Node | undefined { return this._right; }
    set right(value: Node | undefined ) { this._right = value; }
}