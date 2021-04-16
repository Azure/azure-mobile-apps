var load = require('../../scaffold/load'),
    expect = require('chai').use(require('chai-subset')).expect

describe('azure-mobile-apps-compatibility.scaffold.load', function () {
    it("loads mobile services table definitions", function () {
        var tables = load(__dirname + '/files/table')
        expect(tables).to.containSubset({
            friends: {
                operations: {},
                permissions: {}
            },
            messages: {
                operations: {},
                permissions: {}
            }
        })
        expect(Object.keys(tables.messages.operations)).to.deep.equal(['insert', 'read'])
    })
})
