// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { Visitor } from './Visitor';
import * as nodes from '../nodes';

/**
 * The visitor class for QueryExpression nodes.
 */
export class QueryExpressionVisitor extends Visitor {
    /**
     * Creates a new QueryExpressionVisitor
     */
    constructor() {
        super();

        this.registerVisitor('QueryExpression', (node) => node);
        this.registerVisitor('BinaryExpression', (node) => this.visitBinaryExpression(node));
    }

    /**
     * Visitor method for a binary expression.
     * @param node The node being visited.
     * @returns The result node.
     */
    visitBinaryExpression(node: nodes.BinaryExpression): nodes.QueryExpression {
        node.left = this.visit(node.left);
        node.right = this.visit(node.right);
        return node;
    }

    /**
     * Implementation of the getSource method.
     * 
     * @param node The node that needs to be converted.
     */
    public getSource(node: nodes.Node): string {
        if (node.type === 'QueryExpression') {
            throw 'getSource not defined';
        }
        throw 'getSource not implemented';
    }
}