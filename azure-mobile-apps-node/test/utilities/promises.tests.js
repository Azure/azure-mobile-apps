var promises = require('../../src/utilities/promises'),
    expect = require('chai').expect;

describe('azure-mobile-apps.utilities.promises', function () {
    describe('wrap', function () {
        it('resolves wrapped functions when callback is executed with result', function () {
            var wrappedFunction = promises.wrap(generateFunctionToWrap());
            return wrappedFunction(1, 2).then(function (result) {
                expect(result).to.deep.equal([1, 2]);
            });
        });

        it('rejects wrapped functions when callback is executed with error', function () {
            var wrappedFunction = promises.wrap(generateFunctionToWrap('error'));
            return wrappedFunction(1, 2).catch(function (error) {
                expect(error).to.equal('error');
            });
        });

        it('rejects promise when exception is thrown from wrapped function', function () {
            var wrappedFunction = promises.wrap(function () { throw 'test error'; });
            return wrappedFunction()
                .then(function () {
                    expect.fail('Promise resolved when it should have been rejected');
                })
                .catch(function (error) {
                    expect(error).to.equal('test error');
                });
        });

        function generateFunctionToWrap(error) {
            return function (arg1, arg2, callback) {
                error ? callback(error) : callback(undefined, [arg1, arg2]);
            }
        }
    });

    describe('series', function () {
        it('executes each created promise in series and resolves with an array containing the results', function () {
            return promises.series([1, 2, 3], createPromise)
                .then(function (results) {
                    expect(results).to.deep.equal([1, 2, 3]);
                });

            function createPromise(item) {
                return promises.sleep(0, item);
            }
        });

        it('passes index as second argument to promise factory', function () {
            return promises.series([1, 2, 3], createPromise)
                .then(function (results) {
                    expect(results).to.deep.equal([0, 1, 2]);
                });

            function createPromise(item, index) {
                return promises.sleep(0, index);
            }
        });

        it('rejects after first rejected promise', function () {
            var results = [];

            return promises.series([1, 2, 3], createPromise)
                .catch(function (error) {
                    expect(error).to.equal('error');
                    expect(results).to.deep.equal([1, 2]);
                });

            function createPromise(item) {
                if(item === 3)
                    throw 'error';

                results.push(item);
                return promises.sleep(0);
            }
        });
    });
});
