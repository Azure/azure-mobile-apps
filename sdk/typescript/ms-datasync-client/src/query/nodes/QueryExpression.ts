// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { Node } from './Node';
import { BinaryExpression } from './BinaryExpression';

export class QueryExpression extends Node {
    constructor() {
        super();
    }

    public groupClauses(operator: string, clauses: Array<Node>) {
        const combine = (left: Node, right: Node) => {
            if (!left) {
                return right;
            } else if (!right) {
                return left;
            } else {
                return new BinaryExpression(operator, left, right);
            }
        }
        return clauses.reduce(combine);
    }
}