/* global Promise, describe, it */
'use strict';
<% if(testFramework==='mocha') { -%>
var expect = require('chai').expect;
<% } -%>


var createWebApplication = require('../server/app');

describe('server/app.js', function () {
    it('should export a function', function () {
        expect(createWebApplication).to.be.a('function');
    });

    it('should return a Promise', function () {
        expect(createWebApplication()).to.be.an.instanceof(Promise);
    });

<% if(testFramework==='mocha') { -%>
    it('should resolve to an express Application', function () {
        createWebApplication().then(function (app) {
            expect(app).to.be.a('function');
        });
    });
<% } else if(testFramework==='jasmin') { -%>
    it('should resolve to an express Application', function(done) {
        createWebApplication().then(function (app) {
            expect(app).to.be.a('function');
            done();
        }).catch(done);
    });
<% } -%>
});
