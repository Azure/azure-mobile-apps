/* global describe, it */
'use strict';

var expect = require('chai').expect;
var winston = require('winston');

var logger = require('../server/logger');

describe('server/logger.js', function () {
    it('should export a winston object', function () {
        expect(logger).to.be.an.instanceof(winston.Logger);
    });
});
