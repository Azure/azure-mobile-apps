var combine = require('../../../../src/data/sqlite/columns/combine'),
    expect = require('chai').expect;

describe('azure-mobile-apps.data.sqlite.columns.combine', function () {
    it("constructs array of columns from item", function () {
        expect(combine(undefined, undefined, { string: '', number: 1, boolean: false, date: new Date() }))
            .to.deep.equal([
                { name: 'string', type: 'string' },
                { name: 'number', type: 'number' },
                { name: 'boolean', type: 'boolean' },
                { name: 'date', type: 'date' },
                { name: 'id', type: 'string' },
                { name: 'createdAt', type: 'date' },
                { name: 'updatedAt', type: 'date' },
                { name: 'version', type: 'string' }
            ]);
    });

    it("constructs array of columns from predefined columns and item", function () {
        expect(combine(
            undefined,
            { columns: { 'p1': 'string', 'string': 'string', 'number': 'string' }, softDelete: true },
            { string: '', number: 1, boolean: false, date: new Date() }
        )).to.deep.equal([
            { name: 'p1', type: 'string' },
            { name: 'string', type: 'string' },
            { name: 'number', type: 'string' },
            { name: 'boolean', type: 'boolean' },
            { name: 'date', type: 'date' },
            { name: 'id', type: 'string' },
            { name: 'createdAt', type: 'date' },
            { name: 'updatedAt', type: 'date' },
            { name: 'version', type: 'string' },
            { name: 'deleted', type: 'boolean' }
        ]);
    });

    it("constructs array of columns from predefined columns, item and existingColumns", function () {
        expect(combine(
            [
                { name: 'p1', type: 'string' },
                { name: 'id', type: 'string' },
                { name: 'createdAt', type: 'date' },
                { name: 'updatedAt', type: 'date' },
                { name: 'version', type: 'string' },
                { name: 'deleted', type: 'boolean' }
            ],
            { columns: { 'p1': 'string', 'string': 'string', 'number': 'string' } },
            { string: '', number: 1, boolean: false, date: new Date() }
        )).to.deep.equal([
            { name: 'p1', type: 'string' },
            { name: 'id', type: 'string' },
            { name: 'createdAt', type: 'date' },
            { name: 'updatedAt', type: 'date' },
            { name: 'version', type: 'string' },
            { name: 'deleted', type: 'boolean' },
            { name: 'string', type: 'string' },
            { name: 'number', type: 'string' },
            { name: 'boolean', type: 'boolean' },
            { name: 'date', type: 'date' },
        ]);
    });
});
