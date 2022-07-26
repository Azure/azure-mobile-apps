// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

import { Node } from '../nodes';

/**
 * Type that describes a visitor function.
 */
export type VisitFn = (node: any) => any;

/**
 * Type that describes a map of visitor functions.
 */
export interface NodeMap {
    [nodeType: string]: VisitFn;
}

/**
 * Base class for all visitor types.
 */
export abstract class Visitor {
    private _map: NodeMap;

    /**
     * Creates a new Visitor.  All visitor types must call super()
     * before registering types.
     */
    constructor() {
        this._map = {};
    }

    /**
     * Registers a new visitor type.
     * 
     * @param nodeType The type of the node.
     * @param fn The handler function.
     */
    registerVisitor(nodeType: string, fn: VisitFn): void {
        this._map[nodeType] = fn;
    }

    /**
     * Visit a (set of) nodes.
     * 
     * @param node The node or array of nodes that is visited.
     * @returns the nodes visited.
     */
    visit(node: any): any {
        if (Array.isArray(node)) {
            const results = [];
            for (const element of node) {
                results.push(this.visit(element));
            }
            return results;
        } 
        
        if (!(node != null ? node.type : void 0)) {
            return node;
        } 
        
        if (this._map[node.type] !== undefined) {
            return this._map[node.type](node);
        }

        throw `Unsupported expression: ${this.getSource(node)}`;
    }

    /**
     * Gets the source code corresponding to a node.
     * 
     * @param node the node that is being processed.
     * @returns the source code for the node.
     */
    public abstract getSource(node: Node): string;
}
