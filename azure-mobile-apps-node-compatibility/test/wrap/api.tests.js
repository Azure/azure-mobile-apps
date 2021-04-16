var wrap = require('../../wrap'),
    expect = require('chai').use(require('chai-subset')).expect

describe('azure-mobile-apps-compatibility.wrap.api', function () {
    it("extracts and wraps exports", function () {
        var wrapped = wrap.api(function (exports) {
            exports.get = function (request, response) { }
            exports.post = function (request, response) { }
        })

        expect(wrapped.get).to.be.a('function')
        expect(wrapped.post).to.be.a('function')
    })
})
