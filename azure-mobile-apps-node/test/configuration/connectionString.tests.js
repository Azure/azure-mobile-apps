// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
var parse = require('../../src/configuration/connectionString').parse,
    expect = require('chai').expect,
    adoNet = 'Server=tcp:azure-mobile-apps-test.database.windows.net,1433;Database=e2etest-v2-node;User ID=azure-mobile-apps-test@azure-mobile-apps-test;Password=abc123;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;',
    odbc = 'Driver={SQL Server Native Client 11.0};Server=tcp:azure-mobile-apps-test.database.windows.net,1433;Database=e2etest-v2-node;Uid=azure-mobile-apps-test@azure-mobile-apps-test;Pwd=abc123;Encrypt=yes;TrustServerCertificate=no;Connection Timeout=30;',
    expected = {
        provider: 'mssql',
        user: 'azure-mobile-apps-test@azure-mobile-apps-test',
        password: 'abc123',
        server: 'azure-mobile-apps-test.database.windows.net',
        port: 1433,
        database: 'e2etest-v2-node',
        connectionTimeout: 30000,
        options: {
            encrypt: true
        }
    };

describe('azure-mobile-apps.utilities/promises.configuration.connectionString', function () {
    describe('parse', function () {
        it('parses relevant sections of ADO.NET connection strings', function () {
            expect(parse(adoNet)).to.deep.equal(expected);
        });

        it('parses relevant sections of ODBC connection strings', function () {
            expect(parse(odbc)).to.deep.equal(expected);
        });

        it('parses server attributes correctly', function () {
            expect(parse('Server=tcp:azure-mobile-apps-test.database.windows.net,1433').server).to.equal('azure-mobile-apps-test.database.windows.net');
            expect(parse('Server=azure-mobile-apps-test.database.windows.net,1433').server).to.equal('azure-mobile-apps-test.database.windows.net');
            expect(parse('Server=tcp:azure-mobile-apps-test.database.windows.net').server).to.equal('azure-mobile-apps-test.database.windows.net');
            expect(parse('Server=azure-mobile-apps-test.database.windows.net').server).to.equal('azure-mobile-apps-test.database.windows.net');
        });

        it('parses ADO.NET connection strings with spacing issues', function () {
            expect(parse(adoNet.replace(' ', ''))).to.deep.equal(expected);
        });

        it('sets encryption for Azure databases', function () {
            var cs = 'Server=tcp:azure-mobile-apps-test.database.windows.net,1433;Database=e2etest-v2-node;User ID=azure-mobile-apps-test@azure-mobile-apps-test;Password=abc123;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;';
            expect(parse(cs).options.encrypt).to.be.true;
            var cs = 'Server=tcp:azure-mobile-apps-test.database.usgovcloudapi.net,1433;Database=e2etest-v2-node;User ID=azure-mobile-apps-test@azure-mobile-apps-test;Password=abc123;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;';
            expect(parse(cs).options.encrypt).to.be.true;
            var cs = 'Server=tcp:azure-mobile-apps-test.database.cloudapi.de,1433;Database=e2etest-v2-node;User ID=azure-mobile-apps-test@azure-mobile-apps-test;Password=abc123;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;';
            expect(parse(cs).options.encrypt).to.be.true;
            var cs = 'Server=tcp:azure-mobile-apps-test.database.chinacloudapi.cn,1433;Database=e2etest-v2-node;User ID=azure-mobile-apps-test@azure-mobile-apps-test;Password=abc123;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;';
            expect(parse(cs).options.encrypt).to.be.true;
        });
    });
});
