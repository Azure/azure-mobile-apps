// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
﻿var expect = require('chai').expect,
    setAccess = require('../../src/configuration/importDefinition').setAccess,
    definition;

describe('azure-mobile-apps.configuration.importDefinition', function() {
    describe('setAccess', function () {
        it('sets props to false if nonexistent', function () {
            definition = {
                method: {}
            };
            setAccess(definition, 'method');
            expect(definition.method.authorize).to.equal(false);
            expect(definition.method.disable).to.equal(false);
        });

        it('uses definition level props if no method level', function () {
            definition = {
                authorize: true,
                disable: true,
                method: {}
            };
            setAccess(definition, 'method');
            expect(definition.method.authorize).to.equal(true);
            expect(definition.method.disable).to.equal(true);
        });

        it('overrides definition level props if method level', function () {
            definition = {
                authorize: true,
                disable: false,
                method: {
                    authorize: false,
                    disable: true
                }
            };
            setAccess(definition, 'method');
            expect(definition.method.authorize).to.equal(false);
            expect(definition.method.disable).to.equal(true);
        });

        it('overrides all props if access authenticated', function () {
            definition = {
                authorize: false,
                disable: true,
                method: {
                    authorize: false,
                    disable: true,
                    access: 'authenticated'
                }
            };
            setAccess(definition, 'method');
            expect(definition.method.authorize).to.equal(true);
            expect(definition.method.disable).to.equal(false);
        });

        it('overrides all props if access disabled', function () {
            definition = {
                authorize: true,
                disable: false,
                method: {
                    authorize: true,
                    disable: false,
                    access: 'disabled'
                }
            };
            setAccess(definition, 'method');
            expect(definition.method.authorize).to.equal(false);
            expect(definition.method.disable).to.equal(true);
        });

        it('overrides all props if access anonymous', function () {
            definition = {
                authorize: true,
                disable: true,
                method: {
                    authorize: true,
                    disable: true,
                    access: 'anonymous'
                }
            };
            setAccess(definition, 'method');
            expect(definition.method.authorize).to.equal(false);
            expect(definition.method.disable).to.equal(false);
        });

        it('sets access value on definition', function () {
            definition = {
                authorize: false,
                disable: true,
                access: 'authenticated'
            };
            setAccess(definition);
            expect(definition.authorize).to.equal(true);
            expect(definition.disable).to.equal(false);
        });
    });
});
