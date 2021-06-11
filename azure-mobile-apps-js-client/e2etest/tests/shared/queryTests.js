// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

/// <reference path="../testFramework.js" />

function defineQueryTestsNamespace() {
    var tests = [];
    var serverSideTests = [];
    var i;
    var tableName = 'intIdMovies';
    var tableNameForServerSideFilterTest = 'w8jsServerQueryMovies';
    var stringIdMoviesTableName = 'movies';

    // For server-side filtering tests, we will create a function to add the tests;
    // We'll then compare with the function which we expect from the client, to make
    // sure that we're testing the same thing.
    var getWhereClauseCreatorHeader = 'function getWhereClauseCreator(testName) {\n    switch (testName) {\n';
    var getWhereClauseCreatorFooter = '    }\n    return null;\n}';
    var getWhereClauseSwitchBody = '';

    function addQueryTest(testName, getQueryFunction, filter, options, requiredFeatures) {
        var strRequiredFeatures = requiredFeatures;
        if (requiredFeatures === undefined) {
            requiredFeatures = [];
            strRequiredFeatures = [];
        };

        tests.push(createQueryTest(testName, getQueryFunction, filter, options, false, requiredFeatures));
        tests.push(createQueryTest('String id: ' + testName, getQueryFunction, filter, options, true, strRequiredFeatures));
        options = options || {};

        if (!getQueryFunction || !filter || typeof getQueryFunction === 'string') {
            // Not interesting
            return;
        }

        if (options.isNegativeClientValidation || options.isNegativeServerValidation) {
            // Not interesting
            return;
        }

        if (testName.indexOf('Date: Greater') === 0 || testName.indexOf('String.length - Movie with small names') === 0) {
            // Use captured variables, cannot pass to the server
            return;
        }

        if (testName.indexOf('String.replace, trim') === 0) {
            // String.trim currently not supported on the server
            return;
        }

        serverSideTests.push(createServerSideQueryTest(testName, filter, options));
        var getQueryFunctionBody = getQueryFunction.toString();
        getWhereClauseSwitchBody += '        case \'' + testName + '\':\n';
        getWhereClauseSwitchBody += '            return ' + getQueryFunctionBody + ';\n';
    }

    addQueryTest('GreaterThan and LessThen - Movies from the 90s',
        function (table) { return table.where(function () { return this.year > 1989 && this.year < 2000; }); },
        function (item) { return item.year > 1989 && item.year < 2000; });
    addQueryTest('GreaterEqual and LessEqual - Movies from the 90s',
        function (table) { return table.where(function () { return this.year >= 1990 && this.year <= 1999; }); },
        function (item) { return item.year > 1989 && item.year < 2000; });
    addQueryTest('Compound statement - OR of ANDs - Movies from the 30s and 50s',
        function (table) { return table.where(function () { return (this.year >= 1930 && this.year < 1940) || (this.year >= 1950 && this.year < 1960); }); },
        function (item) { return (item.year >= 1930 && item.year < 1940) || (item.year >= 1950 && item.year < 1960); });

    addQueryTest('Division, equal and different - Movies from the year 2001 with rating other than R',
        function (table) { return table.where(function () { return ((this.year / 1000.5) == 2.0) && this.mpaaRating != 'R'; }); },
        function (item) { return ((item.year / 1000.5) == 2.0) && item.mpaaRating != 'R'; });

    addQueryTest('Addition, subtraction, relational, AND - Movies from the 1980s which last less than 2 hours',
        function (table) { return table.where(function () { return ((this.year - 1900) >= 80) && (this.year + 10 < 2000) && (this.duration < 120); }); },
        function (item) { return ((item.year - 1900) >= 80) && (item.year + 10 < 2000) && (item.duration < 120); });

    addQueryTest('Query via template object - Movies from the year 2000 with rating R',
        function (table) { return table.where({ year: 2000, mpaaRating: 'R' }); },
        function (item) { return item.year == 2000 && item.mpaaRating == 'R'; });

    // String functions
    addQueryTest('String.indexOf - Movies which start with "The"',
        function (table) { return table.where(function () { return this.title.indexOf('The') === 0; }); },
        function (item) { return item.title.indexOf('The') === 0; }, { top: 100 });
    addQueryTest('String.toUpperCase - Finding "THE GODFATHER"',
        function (table) { return table.where(function () { return this.title.toUpperCase() === 'THE GODFATHER'; }); },
        function (item) { return item.title.toUpperCase() === 'THE GODFATHER'; });
    addQueryTest('String.toLowerCase - Finding "pulp fiction"',
        function (table) { return table.where(function () { return this.title.toLowerCase() === 'pulp fiction'; }); },
        function (item) { return item.title.toLowerCase() === 'pulp fiction'; });
    addQueryTest('String.indexOf, String.toLowerCase - Movie which start with "the"',
        function (table) { return table.where(function () { return this.title.toLowerCase().indexOf('the') === 0; }); },
        function (item) { return item.title.toLowerCase().indexOf('the') === 0; }, { top: 100 });
    addQueryTest('String.indexOf, String.toUpperCase - Movie which start with "THE"',
        function (table) { return table.where(function () { return this.title.toUpperCase().indexOf('THE') === 0; }); },
        function (item) { return item.title.toUpperCase().indexOf('THE') === 0; }, { top: 100 });
    for (i = 0; i < 2; i++) {
        var characters = (i + 1) * 6;
        addQueryTest('String.length - Movie with small names (< ' + characters + ' characters)',
            function (table) { return table.where(function (numCharacters) { return this.title.length <= numCharacters; }, characters); },
            function (item) { return item.title.length <= characters; }, { top: 100 });
    }
    addQueryTest('String.indexOf (non-ASCII) - movies containing the "é" character',
        function (table) { return table.where(function () { return this.title.indexOf('é') >= 0; }); },
        function (item) { return item.title.indexOf('é') >= 0; });
    addQueryTest('String.replace, trim - movies starting with "godfather", after removing starting "the"',
        function (table) { return table.where(function () { return this.title.replace('The ', '').trim().indexOf('Godfather') === 0; }); },
        function (item) { return item.title.replace('The ', '').trim().indexOf('Godfather') === 0; }, null, [zumo.runtimeFeatureNames.STRINGREPLACE]);
    addQueryTest('String.substring, length - movies which end with "r"',
        function (table) { return table.where(function () { return this.title.substring(this.title.length - 1, 1) === 'r'; }); },
        function (item) { return /r$/.test(item.title); });
    addQueryTest('String.substr - movies with "father" starting at position 7',
        function (table) { return table.where(function () { return this.title.substr(7, 6) === 'father'; }); },
        function (item) { var result = item.title.substr(7, 6) === 'father'; return result; }, { debug: true });

    // String fields
    addQueryTest('Equals - movies since 1980 with rating PG-13',
        function (table) { return table.where(function () { return this.mpaaRating === 'PG-13' && this.year >= 1980; }); },
        function (item) { return item.mpaaRating == 'PG-13' && item.year >= 1980; }, { top: 100 });
    addQueryTest('Comparison to null - Movies since 1980 without a MPAA rating',
        function (table) { return table.where(function () { return this.mpaaRating === null && this.year >= 1980; }); },
        function (item) { return item.mpaaRating == null && item.year >= 1980; });
    addQueryTest('Comparison to null (not null) - Movies before 1970 with a MPAA rating',
        function (table) { return table.where(function () { return this.mpaaRating !== null && this.year < 1970; }); },
        function (item) { return item.mpaaRating != null && item.year < 1970; });

    // Numeric functions
    addQueryTest('Math.floor - Movies which last more than 3 hours',
        function (table) { return table.where(function () { return Math.floor(this.duration / 60.0) >= 3; }); },
        function (item) { return Math.floor(item.duration / 60.0) >= 3; });

    // Since javascript has only 1 data type number it treats 10.00 and 10 as the same, causing the odata query conversion to 
    // treat and parse 10.00 as 10. Changing the test to verify the double division is consistent across both backends.
    addQueryTest('Math.ceil - Movies which last at most 121 minutes',
        function (table) { return table.where(function () { return Math.ceil(this.duration / 60.5) == 2; }); },
        function (item) { return Math.ceil(item.duration / 60.5) == 2; }, { top: 200 });
    addQueryTest('Math.round - Movies which last between 152 minutes (inclusive) and 3.5 (exclusive) hours',
        function (table) { return table.where(function () { return Math.round(this.duration / 60.5) == 3; }); },
        function (item) { return Math.round(item.duration / 60.5) == 3; });

    // Date fields
    var dateBefore1970 = new Date(Date.UTC(1969, 12 - 1, 31, 23, 59, 59));
    var date1980 = new Date(Date.UTC(1980, 1 - 1, 1));
    var dateBefore1990 = new Date(Date.UTC(1989, 12 - 1, 31, 23, 59, 59));
    var dateOfPulpFiction = new Date(Date.UTC(1994, 10 - 1, 14));
    addQueryTest('Date: Greater than, less than - Movies with release date in the 70s',
        function (table) { return table.where(function (d1, d2) { return this.releaseDate > d1 && this.releaseDate < d2; }, dateBefore1970, date1980); },
        function (item) { return item.releaseDate > dateBefore1970 && item.releaseDate < date1980; });
    addQueryTest('Date: Greater or equal, less or equal - Movies with release date in the 80s',
        function (table) { return table.where(function (d1, d2) { return this.releaseDate >= d1 && this.releaseDate <= d2; }, date1980, dateBefore1990); },
        function (item) { return item.releaseDate >= date1980 && item.releaseDate <= dateBefore1990; });
    addQueryTest('Date: Equals - Movies released on 1994-10-14 (Shawshank Redemption / Pulp Fiction)',
        function (table) { return table.where({ releaseDate: new Date(Date.UTC(1994, 10 - 1, 14)) }); },
        function (item) { return item.releaseDate.getTime() === dateOfPulpFiction.getTime(); });

    // Date functions
    addQueryTest('Date (month): Movies released in November',
        function (table) { return table.where(function () { return this.releaseDate.getUTCMonth() == (11 - 1); }); },
        function (item) { return item.releaseDate.getUTCMonth() == (11 - 1); });
    addQueryTest('Date (day): Movies released in the first day of the month',
        function (table) { return table.where(function () { return this.releaseDate.getUTCDate() == 1; }); },
        function (item) { return item.releaseDate.getUTCDate() == 1; });
    addQueryTest('Date (year): Movies whose year is different than its release year',
        function (table) { return table.where(function () { return this.releaseDate.getUTCFullYear() !== this.year; }); },
        function (item) { return item.releaseDate.getUTCFullYear() !== item.year; }, { debug: true, top: 100 });

    // Boolean fields
    addQueryTest('Boolean: equal to true - Best picture winners before 1950',
        function (table) { return table.where(function () { return this.bestPictureWinner === true && this.year < 1950; }); },
        function (item) { return item.bestPictureWinner === true && item.year < 1950; }, { debug: true });
    addQueryTest('Boolean: not and equal to false - Best picture winners since 2000',
        function (table) { return table.where(function () { return !(this.bestPictureWinner === false) && this.year >= 2000; }); },
        function (item) { return !(item.bestPictureWinner === false) && item.year >= 2000; });
    addQueryTest('Boolean: not equal to false - Best picture winners since 2000',
        function (table) { return table.where(function () { return this.bestPictureWinner !== false && this.year >= 2000; }); },
        function (item) { return item.bestPictureWinner !== false && item.year >= 2000; });

    // In operator
    addQueryTest('In operator: Movies since 2000 with MPAA ratings of "G", "PG" or " PG-13"',
        function (table) { return table.where(function (ratings) { return this.year >= 2000 && this.mpaaRating in ratings; }, ['G', 'PG', 'PG-13']); },
        function (item) { return item.year >= 2000 && ['G', 'PG', 'PG-13'].indexOf(item.mpaaRating) >= 0; });

    // Top and skip
    addQueryTest('Get all using large $top: take(500)', null, null, { top: 500 });
    addQueryTest('Skip all using large skip: skip(500)', null, null, { skip: 500 });
    addQueryTest('Get first ($top) - 10', null, null, { top: 10 });
    addQueryTest('Get last ($skip) - 10', null, null, { skip: zumo.tests.getQueryTestData().length - 10, sortFields: [new OrderByClause('id', true)] });
    addQueryTest('Skip, take and includeTotalCount: movies 11-20, ordered by title', null, null,
        { top: 10, skip: 10, includeTotalCount: true, sortFields: [new OrderByClause('title', true)] });
    addQueryTest('Skip, take and includeTotalCount with filter: movies 11-20, which won the best picture, ordered by release date',
        function (table) { return table.where({ bestPictureWinner: true }); },
        function (item) { return item.bestPictureWinner; },
        { top: 10, skip: 10, includeTotalCount: true, sortFields: [new OrderByClause('releaseDate', false)] });

    // Orderby
    addQueryTest('Order by date and string - 50 movies, ordered by release date, then title', null, null,
        { top: 50, sortFields: [new OrderByClause('releaseDate', false), new OrderByClause('title', true)] });
    addQueryTest('Order by number - 30 shortest movies since 1970',
        function (table) { return table.where(function () { return this.year >= 1970; }); },
        function (item) { return item.year >= 1970; },
        { top: 30, sortFields: [new OrderByClause('duration', true), new OrderByClause('title', true)] });

    // Select
    addQueryTest('Select single field - title of movies since 2000',
        function (table) { return table.where(function () { return this.year >= 2000; }); },
        function (item) { return item.year >= 2000; },
        { top: 200, selectFields: ['title'] });
    addQueryTest('Select multiple fields - title, bestPictureWinner, duration, order by release date, of movies from the 1990s',
        function (table) { return table.where(function () { return this.year >= 1990 && this.year < 2000; }); },
        function (item) { return item.year >= 1990 && item.year < 2000; },
        {
            top: 200,
            selectFields: ['title', 'bestPictureWinner', 'duration'],
            sortFields: [new OrderByClause('releaseDate', true), new OrderByClause('title', true)]
        });

    // Read passing OData query directly
    addQueryTest('Passing OData query directly - movies from the 80s, ordered by title, items 3, 4 and 5',
        '$filter=((year ge 1980) and (year le 1989))&$top=3&$skip=2&$orderby=title asc',
        function (item) { return item.year >= 1980 && item.year <= 1989; },
        { top: 3, skip: 2, sortFields: [new OrderByClause('title', true)] });

    // Negative tests
    addQueryTest('(Neg) Unsupported predicate: lastIndexOf',
        function (table) { return table.where(function () { return this.title.lastIndexOf('r') >= 0; }); },
        function (item) { return item.lastIndexOf('r') == item.length - 1 }, { isNegativeClientValidation: true });
    addQueryTest('(Neg) Unsupported predicate: regular expression',
        function (table) { return table.where(function () { return /Godfather/.test(this.title); }); },
        function (item) { return /Godfather/.test(item.title); }, { isNegativeClientValidation: true });
    addQueryTest('(Neg) Unsupported predicate: function with multiple statements',
        function (table) { return table.where(function () { var year = 2000; return this.year > year; }); },
        function (item) { var year = 2000; return item.title > year; }, { isNegativeClientValidation: true });
    var someVariable = 2000;
    addQueryTest('(Neg) Unsupported predicate: using variables from closure',
        function (table) { return table.where(function () { return this.year > someVariable; }); },
        function (item) { return item.title > someVariable; }, { isNegativeClientValidation: true });
    addQueryTest('(Neg) Very large "top"', null, null, { top: 1001, isNegativeServerValidation: true });

    for (i = -1; i <= 0; i++) {
        var id = i;
        tests.push(new zumo.Test('(Neg) invalid offset: ' + id, function (test, done) {
            var client = zumo.getClient();
            var table = client.getTable(tableName);
            table.lookup(id).done(function (item) {
                test.addLog('Error, lookup for id = ' + id + ' succeeded, it should have failed. Item = ' + JSON.stringify(item));
                done(false);
            }, function (err) {
                test.addLog('Got expected error: ' + JSON.stringify(err));
                done(true);
            });
        }, zumo.runtimeFeatureNames.INT_ID_TABLES));
    }

    tests.push(createQueryTest('String id: query by id',
        function (table) { return table.where(function () { return this.id.indexOf('Movie 12') === 0; }); },
        function (item, index) { return index >= 120 && index <= 129; }, {}, true));

    // Now that all server-side tests have been added, will add one test to validate the server-side script
    var getWhereClauseCreator = getWhereClauseCreatorHeader + getWhereClauseSwitchBody + getWhereClauseCreatorFooter;
    serverSideTests.splice(0, 0, new zumo.Test('Validate getWhereClauseCreator server function', function (test, done) {
        /// <param name="test" type="zumo.Test">The test being executed</param>
        test.addLog('Will validate that the server implementation of the getWhereClauseCreator function is the expected one.');
        var client = zumo.getClient();
        var table = client.getTable(tableNameForServerSideFilterTest);
        table.read({ testName: 'getWhereClauseCreator' }).done(function (results) {
            var actualGetWhereClauseCreator = results[0].code;
            var clientFunction = getWhereClauseCreator.replace(/\r\n/g, '\n');
            var serverFunction = actualGetWhereClauseCreator.replace(/\r\n/g, '\n');
            if (clientFunction === serverFunction) {
                test.addLog('Function matches');
                done(true);
            } else {
                test.addLog('Error, functions do not match. Expected:');
                test.addFullLog(getWhereClauseCreator);
                test.addLog('Actual:');
                test.addFullLog(actualGetWhereClauseCreator);
                done(false);
            }
        }, function (err) {
            test.addLog('Error retrieving the "getWhereClauseCreator" method from the server: ', err);
            done(false);
        });
    }, zumo.runtimeFeatureNames.INT_ID_TABLES));

    function OrderByClause(fieldName, isAscending) {
        this.fieldName = fieldName;
        this.isAscending = isAscending;
    }

    OrderByClause.prototype.getSortFunction = function (a, b) {
        var field1 = a[this.fieldName];
        var field2 = b[this.fieldName];
        var result = 0;
        if (typeof field1 === 'string') {
            result = field1.localeCompare(field2);
        } else if (typeof field1 === 'number') {
            result = field1 - field2;
        } else if (typeof field1 === 'object' && Object.prototype.toString.call(field1) == '[object Date]') {
            result = field1 - field2;
        } else {
            result = 0; // unsupported
        }

        if (!this.isAscending) {
            result = -result;
        }

        return result;
    }

    function createServerSideQueryTest(testName, filter, options) {
        return new zumo.Test('[Server] ' + testName, function (test, done) {
            var client = zumo.getClient();
            var table = client.getTable(tableNameForServerSideFilterTest);
            table.read({ testName: testName, options: JSON.stringify(options) }).done(function (results) {
                test.addLog('Received results from the server');
                var filteredMovies = zumo.tests.getQueryTestData();
                if (filter) {
                    filteredMovies = filteredMovies.filter(filter);
                }
                filteredMovies = applyOptions(filteredMovies, options);
                var errors = [];
                if (!zumo.util.compare(filteredMovies, results, errors)) {
                    errors.forEach(function (error) {
                        test.addLog(error);
                    });
                    test.addLog('Retrieved data is not the expected');
                    test.addLog('Expected: ' + JSON.stringify(filteredMovies));
                    test.addLog('Actual: ' + JSON.stringify(results));
                    done(false);
                } else {
                    done(true);
                }
            }, function (err) {
                test.addLog('Error retrieving data: ', err);
                done(false);
            });
        }, zumo.runtimeFeatureNames.INT_ID_TABLES);
    }

    function applyOptions(filteredMovies, options) {
        /// <param name='movies' type='Array'/>
        /// <param name='options' type='Object'>
        /// {
        ///   top (number): number of elements to 'take'
        ///   skip (number): number of elements to 'skip'
        ///   sortFields (array of OrderByClause): fields to sort the data by
        ///   selectFields (array of string): fields to retrieve
        /// }
        /// </param>
        options = options || {};
        if (options.sortFields && options.sortFields.length) {
            var compareFunction = function (a, b) {
                var i;
                for (i = 0; i < options.sortFields.length; i++) {
                    var temp = options.sortFields[i].getSortFunction(a, b);
                    if (temp !== 0) {
                        return temp;
                    }
                }

                return 0;
            }
            filteredMovies = filteredMovies.sort(compareFunction);
        }

        if (options.skip) {
            filteredMovies = filteredMovies.slice(options.skip, 1000);
        }

        if (options.top) {
            filteredMovies = filteredMovies.slice(0, options.top);
        }

        if (options.selectFields && options.selectFields.length) {
            filteredMovies = filteredMovies.map(function (obj) {
                var result = {};
                var key;
                for (key in obj) {
                    if (obj.hasOwnProperty(key)) {
                        if (options.selectFields.indexOf(key) >= 0) {
                            result[key] = obj[key];
                        }
                    }
                }
                return result;
            });
        }

        return filteredMovies;
    }

    function createQueryTest(testName, getQueryFunction, filter, options, useStringIdTable, requiredFeatures) {
        /// <param name="testName" type="String">Name of the test to be created.</param>
        /// <param name="getQueryFunction" optional="true" type="function or String">Either a function which takes a
        ///    table object and returns a query, or a String with the OData query to be sent to the server.</param>
        /// <param name="filter" optional="true" type="function">Function which takes a movie object and returns a boolean</param>
        /// <param name="options" type="Object" optional="true">Optional object with additional parameters for query.
        ///   Supported fields:
        ///    top (number): number of elements to 'take'
        ///    skip (number): number of elements to 'skip'
        ///    includeTotalCount (boolean): whether to request a total could (wrapped response)
        ///    sortFields (array of OrderByClause): fields to sort the data by
        ///    selectFields (array of string): fields to retrieve
        ///    debug (boolean): whether to log more information than normal
        ///    isNegativeClientValidation (boolean): if an exception should be thrown before the request is sent the server
        ///    isNegativeServerValidation (boolean): if the error callback should be returned by the server
        /// </param>
        if (requiredFeatures === undefined) {
            requiredFeatures = [];
        }
        var x = useStringIdTable ? zumo.runtimeFeatureNames.STRING_ID_TABLES : zumo.runtimeFeatureNames.INT_ID_TABLES;
        requiredFeatures.push(x);
        return new zumo.Test(testName, function (test, done) {
            var i;
            var filteredMovies;
            var query;
            var readODataQuery = null;
            var exception = null;
            options = options || {};
            useStringIdTable = !!useStringIdTable;
            var top = options.top;
            var skip = options.skip;
            var sortFields = options.sortFields;
            var includeTotalCount = options.includeTotalCount;
            var selectFields = options.selectFields;
            var debug = options.debug;
            var isNegativeClientValidation = options.isNegativeClientValidation;
            var isNegativeServerValidation = options.isNegativeServerValidation;

            try {
                var client = zumo.getClient();
                var allMovies = zumo.tests.getQueryTestData();
                var table = useStringIdTable ? client.getTable(stringIdMoviesTableName) : client.getTable(tableName);

                if (getQueryFunction) {
                    if (typeof getQueryFunction === 'string') {
                        readODataQuery = getQueryFunction;
                    } else {
                        query = getQueryFunction(table);
                    }
                } else {
                    query = table;
                }

                if (filter) {
                    filteredMovies = allMovies.filter(filter);
                } else {
                    filteredMovies = allMovies;
                }

                var expectedTotalCount = filteredMovies.length;

                if (includeTotalCount && query) {
                    query = query.includeTotalCount();
                }

                filteredMovies = applyOptions(filteredMovies, options);

                if (!readODataQuery) {
                    if (sortFields && sortFields.length) {
                        for (i = 0; i < sortFields.length; i++) {
                            if (sortFields[i].isAscending) {
                                query = query.orderBy(sortFields[i].fieldName);
                            } else {
                                query = query.orderByDescending(sortFields[i].fieldName);
                            }
                        }
                    }

                    if (skip) {
                        query = query.skip(skip);
                    }

                    if (top) {
                        query = query.take(top);
                    }

                    if (selectFields && selectFields.length) {
                        query.select(selectFields.join(','));
                    }
                }
            } catch (ex) {
                exception = ex;
            }

            if (exception) {
                test.addLog('Exception: ' + JSON.stringify(exception));
                if (isNegativeClientValidation) {
                    test.addLog('Exception is expected.');
                    done(true);
                } else {
                    done(false);
                }
                return;
            } else {
                if (isNegativeClientValidation) {
                    test.addLog('Error, expected an exception before sending the \'read\' call, but none was thrown.');
                    done(false);
                    return;
                }
            }

            var errorFunction = function (err) {
                test.addLog('Read called the error callback: ' + JSON.stringify(err));
                zumo.util.traceResponse(test, err.request);
                if (isNegativeServerValidation) {
                    test.addLog('Error was expected');
                    done(true);
                } else {
                    done(false);
                }
            };

            var successFunction = function (results) {
                test.addLog('Received results from the server');
                if (isNegativeServerValidation) {
                    test.addLog('Error, expected an error response from the server, but received results: ' + JSON.stringify(results));
                    done(false);
                    return;
                }
                if (debug) {
                    test.addLog('Results: ' + JSON.stringify(results));
                }
                var actualTotalCount;
                var actualResults;
                if (includeTotalCount) {
                    actualTotalCount = results.totalCount;
                    actualResults = results;
                    if (actualResults === undefined || actualTotalCount === undefined) {
                        test.addLog('Error, query with \'includeTotalCount()\' should return an object, it did not');
                        test.addLog('Result: ' + JSON.stringify(results));
                        done(false);
                        return;
                    }

                    if (expectedTotalCount !== actualTotalCount) {
                        test.addLog('Error in \'totalCount\'. Expected: ' + expectedTotalCount + ', actual: ' + actualTotalCount);
                        done(false);
                        return;
                    }
                } else {
                    actualTotalCount = -1;
                    actualResults = results;
                }

                var errors = [];
                if (!zumo.util.compare(filteredMovies, actualResults, errors)) {
                    errors.forEach(function (error) {
                        test.addLog(error);
                    });
                    test.addLog('Retrieved data is not the expected');
                    if (debug) {
                        test.addLog('Expected: ' + JSON.stringify(filteredMovies));
                        test.addLog('Actual: ' + JSON.stringify(actualResults));
                    }
                    done(false);
                } else {
                    done(true);
                }
            };

            if (readODataQuery) {
                table.read(readODataQuery).done(successFunction, errorFunction);
            } else {
                query.read().done(successFunction, errorFunction);
            }
        }, requiredFeatures);
    }

    return {
        name: 'Query',
        tests: tests,
        serverSideTests: serverSideTests
    };
}

zumo.tests.query = defineQueryTestsNamespace();