var wrap = require('../../wrap'),
    expect = require('chai').use(require('chai-subset')).expect

describe('azure-mobile-apps-compatibility.wrap.table', function () {
    it("read returns mobile app compatible function", function () {
        var innerExecuted = false,
            context = {
                tables: {},
                push: {},
                req: {},
                res: {},
                query: {}
            },
            wrapped = wrap.read(function (tables, push, request, user) {
                return function read(query, user, request) {
                    innerExecuted = true;
                    expect(tables).to.equal(context.tables)
                    expect(push).to.equal(context.push)
                    expect(request).to.equal(context.req)
                    expect(query).to.equal(context.query)
                    expect(user.level).to.equal('anonymous')
                }
            })
        wrapped(context)
        expect(innerExecuted).to.equal(true)
    })

    it("wrapped function returns promise", function () {
        var wrapped = wrap.read(function (tables, push, request, user) {
            return function read(query, user, request) { }
        })
        expect(wrapped({ req: {} })).to.be.a('Promise')
    })

    it("returned promise resolves when request.execute promise resolves", function (done) {
        var wrapped = wrap.read(function (tables, push, request, user) {
                return function read(query, user, request) {
                    request.execute()
                }
            }),
            result = wrapped({ req: {}, execute: execute })

        expect(result).to.be.a('Promise')

        result.then(function (returnValue) {
            expect(returnValue).to.equal('test')
            done()
        }).catch(done)

        function execute() {
            return new Promise(function (resolve) {
                setTimeout(function () {
                    resolve('test')
                }, 20)
            })
        }
    })
})
