// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var attach = require('../../src/query/attachOperators'),
    expect = require('chai').use(require('chai-subset')).expect;

describe('azure-mobile-apps.query.attachOperators', function () {
    it('attaches query operators to provided object', function () {
        var target = { };
        attach('table', target);
        expect(target.where).to.be.a('function');
        expect(target.select).to.be.a('function');
        expect(target.orderBy).to.be.a('function');
        expect(target.orderByDescending).to.be.a('function');
        expect(target.skip).to.be.a('function');
        expect(target.take).to.be.a('function');
        expect(target.includeTotalCount).to.be.a('function');
    });

    it('calls read on target object with created query', function () {
        var value,
            result,
            target = {
                read: function (query) {
                    var components = query.getComponents();
                    expect(components.selections).to.deep.equal(['x']);
                    expect(components.filters).to.containSubset({ operator: 'Equal', left: { member: 'x' }, right: { value: 1 } });
                    value = 'value';
                    return 'result';
                }
            };

        attach('table', target);
        result = target.where({ x: 1 }).select('x').read();
        expect(result).to.equal('result');
        expect(value).to.equal('value');
    });
})
