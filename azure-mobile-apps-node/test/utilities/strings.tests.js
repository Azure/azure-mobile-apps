var expect = require('chai').expect,
    strings = require('../../src/utilities/strings');

describe('azure-mobile-apps.utilities.strings', function () {
    describe('getVersionFromEtag', function () {
        it('removes starting and finishing double quotes and remove escaping from quotes in the middle', function () {
            expect(strings.getVersionFromEtag('"test \\"inside\\" test"')).to.equal('test "inside" test');
        });
    });

    describe('getEtagFromVersion', function () {
        it('adds starting and finishing double quotes and escapes quotes in the middle', function () {
            expect(strings.getEtagFromVersion('test "inside" test')).to.equal('"test \\"inside\\" test"');
        });
    });
});
