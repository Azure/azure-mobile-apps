// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

function defineTableGenericFunctionalTestsNamespace() {
    var tests = [],
        stringIdTableName = 'roundtriptable',
        intIdTableName = 'intidroundtriptable',
        globalTest = null,
        globaldone = null;

    //Helper functions
    function newLink() {
        var def = $.Deferred();
        return def;
    }
    var failFn = function (err) {
        if (err != undefined) {
            globalTest.addLog('Error' + JSON.stringify(err));
        }
        globaldone(false);
    }

    function emptyTable(table) {
        return table.read().then(function (results) {
            if (!results || results.length == 0) {
                return null;
            }

            var deletePromiseChain;
            results.forEach(function (result) {
                var delPromise = table.del(result);

                if (!deletePromiseChain) {
                    deletePromiseChain = delPromise;
                } else {
                    deletePromiseChain = deletePromiseChain.then(function (result) {
                        return delPromise;
                    });
                }
            });
            
            return deletePromiseChain;
        });
    }

    function populateTable(table, recordIds, callback) {
        var promise,            
            insertedRecords = 0;

        recordIds.forEach(function (recordId) {
            var insertPromise = table.insert({ id: recordId, name: 'Hey' }).then(function (item) {
                insertedRecords++;
            });

            if (!promise) {
                promise = insertPromise;
            } else {
                promise = promise.then(function (count) { return insertPromise; });
            }
        });
    
        if (!promise) { return 0; }
        return promise.then(function () {
            return insertedRecords;
        })
    }

    tests.push(new zumo.Test('Identify enabled runtime features for functional tests',
           function (test, done) {
               var client = zumo.getClient();
               client.invokeApi('runtimeInfo', {
                   method: 'GET'
               }).done(function (response) {
                   var runtimeInfo = response.result;
                   test.addLog('Runtime features: ', runtimeInfo);
                   var features = runtimeInfo.features;
                   zumo.util.globalTestParams[zumo.constants.RUNTIME_FEATURES_KEY] = features;
                   if (runtimeInfo.runtime.type.indexOf("node") > -1) {
                       validStringIds = baseValidStringIds();
                       validStringIds.push("...");
                       validStringIds.push("id with 255 characters " + new Array(257 - 24).join('A'));
                       validStringIds.push("id with allowed ascii characters  !#$'(),-.0123456789;=@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_abcdefghijklmnopqrstuvwxyz{|}");
                       validStringIds.push("id with allowed extended ascii characters ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþ");
                   }
                   if (runtimeInfo.runtime.type.indexOf("NET") > -1) {
                       validStringIds = baseValidStringIds();
                       validStringIds.push("id with 128 characters " + new Array(129 - 24).join('A'));
                       validStringIds.push("id with allowed ascii characters  !#$'(),-.0123456789;=@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_abcdefghijklmnopqrstuvwxyz{|}".substr(0, 127));
                       validStringIds.push("id with allowed extended ascii characters ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþ".substr(0, 127));
                   }
                   done(true);
               }, function (err) {
                   test.addLog('Error retrieving runtime info: ', err);
                   done(false);
               });
           }));

    tests.push(new zumo.Test('UpdateAsyncWithWithMergeConflict', function (test, done) {
        globalTest = test;
        globaldone = done;

        var client = zumo.getClient(),
            table = client.getTable(stringIdTableName),
            savedVersion,
            correctVersion,
            newItem;

        emptyTable(table).then(function () {
            return table.insert({ id: 'an id', name: 'a value' });
        }).then(function (item) {
            savedVersion = item.version;
            item.name = 'Hello!';
            return table.update(item);
        }).then(function (item) {
            var a1 = assert.areNotEqual(item.version, savedVersion);
            item.name = 'But Wait!';
            correctVersion = item.version;
            item.version = savedVersion;
            newItem = item;
            return table.update(item);
        }).then(function (items) { 
            done(false); 
        }, function (error) {            
            if (assert.areEqual(test, 412, error.request.status) &&
                        error.message.indexOf("Precondition Failed") > -1 &&
                        assert.areEqual(test, error.serverInstance.version, correctVersion) &&
                        assert.areEqual(test, error.serverInstance.name, 'Hello!')) {
                newItem.version = correctVersion;
                return table.update(newItem);
            } else {
                throw new Error('Incorrect error returned from update');
            }
        }).then(function (item) {
            if (assert.areNotEqual(item.version, correctVersion)) {
                done(true);
            } else {
                throw new Error('Incorrect version found');
            }
        }, failFn);
    }));

    tests.push(new zumo.Test('DeleteAsyncWithNosuchItemAgainstStringIdTable', function (test, done) {
        globalTest = test;
        globaldone = done;

        var table = zumo.getClient().getTable(stringIdTableName),
            lookupData = function (recordIds) {
                var promise;
                recordIds.forEach(function (recordId) {
                    lookupPromise = table.lookup(recordId);

                    if (!promise) {
                        promise = lookupPromise;
                    } else {
                        promise = promise.then(function () { return lookupPromise; });
                    }
                });

                return promise;
            };
            deleteData = function (recordIds, initialAttempt) {
                var promise;
                recordIds.forEach(function (recordId) {
                    deletePromise = table.del({ id: recordId }).then(function (result) {
                        if (!initialAttempt) throw new Error('Expected delete to fail');
                        return;
                    }, function (error) {
                        if (initialAttempt) throw error;
                        return;
                    });

                    if (!promise) {
                        promise = deletePromise;
                    } else {
                        promise = promise.then(function () { return deletePromise });
                    }
                });

                return promise;
            };

        emptyTable(table).then(function () {
            test.addLog('Initialize our data');
            return populateTable(table, validStringIds);
        }).then(function () {
            test.addLog('Lookup our data');
            return lookupData(validStringIds);
        }).then(function () {
            test.addLog('Delete our data');
            return deleteData(validStringIds, true);
        }).then(function() {
            test.addLog('Delete our data again');
            return deleteData(validStringIds, false);
        }).then(function () {
            done(true);
        }, failFn);
    }));
    
    tests.push(new zumo.Test('FilterReadAsyncWithEmptyStringIdAgainstStringIdTable', function (test, done) {
        globalTest = test;
        globaldone = done;

        var table = zumo.getClient().getTable(stringIdTableName),
            readData = function (recordIds) {
                var promise;
                recordIds.forEach(function (recordId) {
                    readPromise = table.where({ id: recordId }).read().then(function (results) {
                        if (results.length > 0) {
                            throw new Error('Results found for id: ' + recordId);
                        }
                    });

                    if (!promise) {
                        promise = readPromise;
                    } else {
                        promise = promise.then(function () { return readPromise; });
                    }
                });

                return promise;
            };

        emptyTable(table).then(function () {
            test.addLog('Initialize our data');
            return populateTable(table, validStringIds);
        }).then(function () {
            test.addLog('verify no results for ids we didn\'t use');

            var testIds = emptyStringIds.concat(invalidStringIds).concat(null);
            return readData(testIds);
        }).then(function () {
            done(true);
        }, failFn);
    }));

    tests.push(new zumo.Test('RefreshAsyncWithNoSuchItemAgainstStringIdTable', function (test, done) {
        globalTest = test;
        globaldone = done;

        var table = zumo.getClient().getTable(stringIdTableName),
            lookupData = function (recordIds) {
                var promise;
                recordIds.forEach(function (recordId) {
                    lookupPromise = table.lookup(recordId).then(function (item) {
                        if (!assert.areEqual(test, item.id, recordId)) {
                            throw new Error('Wrong record found');
                        }
                    });

                    if (!promise) {
                        promise = lookupPromise;
                    } else {
                        promise = promise.then(function () { return lookupPromise; });
                    }
                });

                return promise;
            },
            deleteData = function (recordIds) {
                var promise;
                recordIds.forEach(function (recordId) {
                    delPromise = table.del({ id: recordId });
                    if (!promise) {
                        promise = delPromise;
                    } else {
                        promise = promise.then(function () { return delPromise; });
                    }
                });

                return promise;
            },
            refreshData = function (recordIds) {
                var promise;
                recordIds.forEach(function (recordId) {
                    refreshPromise = table.refresh({ id: recordId, Name: 'Hey' }).then(function (result) {
                        throw new Error('Refresh was supposed to fail for: ' + recordId);
                    }, function (error) {
                        return;
                    });

                    if (!promise) {
                        promise = refreshPromise;
                    } else {
                        promise = promise.then(function () { return refreshPromise; });
                    }
                });

                return promise;
            };

        emptyTable(table).then(function() {
            test.addLog('Initialize our data');
            return populateTable(table, validStringIds);
        }).then(function () {
            test.addLog('Read our data');
            return lookupData(validStringIds);
        }).then(function () {
            test.addLog('Delete our data');
            return deleteData(validStringIds);
        }).then(function () {
            test.addLog('Refresh our data');
            return refreshData(validStringIds);
        }).then(function () {
            done(true);
        }, failFn);
    }));

    // BUG #2133523: .NET runtime should respond 400 if query requests invalid system properties
    //tests.push(new zumo.Test('AsyncTableOperationsWithInvalidSystemPropertiesQuerystring', function (test, done) {
    //    var client = zumo.getClient(),
    //        savedItem;
    //    var table = client.getTable(stringIdTableName);

    //    var forLink = [newLink()]
    //    var i = 0;
    //    forLink[i].resolve(i);

    //    testInvalidSystemPropertyQueryStrings.forEach(function (systemProperties) {
    //        i = i + 1;
    //        forLink[i] = newLink();
    //        forLink[i - 1].then(function (i2) {
    //            systemProperties = testInvalidSystemPropertyQueryStrings[i2];
    //            var systemPropertiesKeyValue = systemProperties.split('='),
    //                userParams = {};

    //            userParams[systemPropertiesKeyValue[0]] = systemPropertiesKeyValue[1];
    //            test.addLog('querystring: ' + systemProperties);
    //            var insertPromise = newLink();
    //            table.insert({ id: 'an id', name: 'a value' }, userParams).then(function (item) {
    //                forLink[i2 + 1].reject();
    //            }, function (error) {
    //                insertPromise.resolve();
    //            });

    //            var readPromise = newLink();
    //            insertPromise.then(function () {
    //                return table.read('', userParams).then(function (items) {
    //                    forLink[i2 + 1].reject();
    //                }, function (error) {
    //                    readPromise.resolve();
    //                })
    //            })

    //            var wherePromise = newLink();
    //            readPromise.then(function () {
    //                return table.where({ version: 'AAA' }).read(userParams).then(function (items) {
    //                    forLink[i2 + 1].reject();
    //                }, function (error) {
    //                    wherePromise.resolve();
    //                })
    //            });

    //            var lookupPromise = newLink();
    //            readPromise.then(function () {
    //                return table.lookup('an id', userParams).then(function (items) {
    //                    forLink[i2 + 1].reject();
    //                }, function (error) {
    //                    lookupPromise.resolve();
    //                })
    //            });


    //            lookupPromise.then(function () {
    //                table.update({ id: 'an id', name: 'new value' }, userParams).then(function (items) {
    //                    forLink[i2 + 1].reject();
    //                }, function (error) {
    //                    forLink[i2 + 1].resolve(i2 + 1);
    //                })
    //            });
    //        })
    //    })
    //    $.when.apply($, forLink).then(function () {
    //        done(true);
    //    }, function () {
    //        done(false);
    //    });
    //}, [zumo.runtimeFeatureNames.NodeRuntime_Only]));

    // BUG #1706815 (OData query for version field (string <--> byte[] mismatch)
    //tests.push(new zumo.Test('AsyncTableOperationsWithAllSystemPropertiesUsingCustomSystemParameters', function (test, done) {
    //    var client = zumo.getClient(),
    //        savedItem;
    //    var table = client.getTable(stringIdTableName);

    //    var forLink = [newLink()]
    //    var i = 0;
    //    forLink[i].resolve(i);

    //    testValidSystemPropertyQueryStrings.forEach(function (systemProperties) {
    //        i = i + 1;
    //        forLink[i] = newLink();
    //        forLink[i - 1].then(function (i2) {
    //            systemProperties = testValidSystemPropertyQueryStrings[i2];
    //            var systemPropertiesKeyValue = systemProperties.split('='),
    //                                userParams = {
    //                                },
    //                                savedVersion;

    //            userParams[systemPropertiesKeyValue[0]] = systemPropertiesKeyValue[1];

    //            var lowerCaseSysProperties = systemProperties.toLowerCase();
    //            shouldHaveCreatedAt = lowerCaseSysProperties.indexOf('created') !== -1,
    //            shouldHaveUpdatedAt = lowerCaseSysProperties.indexOf('updated') !== -1,
    //            shouldHaveVersion = lowerCaseSysProperties.indexOf('version') !== -1;

    //            if (lowerCaseSysProperties.indexOf('*') !== -1) {
    //                shouldHaveCreatedAt = shouldHaveUpdatedAt = shouldHaveVersion = true;
    //            }

    //            table.read().then(function (results) {
    //                var readPromise = newLink();
    //                var promises = [];
    //                results.forEach(function (result) {
    //                    var delProm = newLink();
    //                    promises.push(delProm);
    //                    table.del(result).then(function () { delProm.resolve(); });
    //                });
    //                $.when.apply($, promises).done(function () {
    //                    readPromise.resolve();
    //                });
    //                return readPromise;
    //            }, function () { forLink[i2 + 1].reject(); }).then(function () {
    //                var insertPromise = newLink();
    //                table.insert({ id: 'an id', name: 'a value' }, userParams).then(function (item) {
    //                    insertPromise.resolve(item);
    //                });
    //                return insertPromise;
    //            }, function () { forLink[i2 + 1].reject(); })
    //           .then(function (item) {
    //               var insertPromise2 = newLink();
    //               var a1 = assert.areEqual(test, shouldHaveCreatedAt, item.createdAt !== undefined);
    //               var a2 = assert.areEqual(test, shouldHaveUpdatedAt, item.updatedAt !== undefined);
    //               var a3 = assert.areEqual(test, shouldHaveVersion, item.version !== undefined);
    //               savedItem = item;
    //               if (a1 && a2 && a3) {
    //                   table.read('', userParams).then(function (item) {
    //                       insertPromise2.resolve(item);
    //                   },
    //                   function () { forLink[i2 + 1].reject(); });
    //               }
    //               else {
    //                   insertPromise2.reject();
    //               }
    //               return insertPromise2;
    //           }, function () { forLink[i2 + 1].reject(); })
    //           .then(function (items) {
    //               var wherePromise = newLink();
    //               var a4 = assert.areEqual(test, 1, items.length);
    //               var item = items[0];
    //               var create = assert.areEqual(test, shouldHaveCreatedAt, item.createdAt !== undefined);
    //               var update = assert.areEqual(test, shouldHaveUpdatedAt, item.updatedAt !== undefined);
    //               var version = assert.areEqual(test, shouldHaveVersion, item.version !== undefined);


    //               var whereCriteria = null;
    //               if (shouldHaveCreatedAt) {
    //                   whereCriteria = { createdAt: savedItem.createdAt }
    //               }

    //               if (shouldHaveUpdatedAt) {
    //                   whereCriteria = { updatedAt: savedItem.updatedAt }
    //               }

    //               if (shouldHaveVersion) {
    //                   whereCriteria = { version: savedItem.version }
    //               }

    //               if (create && update && version && a4) {
    //                   table.where(whereCriteria).read(userParams).then(function (item) {
    //                       wherePromise.resolve(item);
    //                   }, function () { forLink[i2 + 1].reject(); });
    //               }
    //               else {
    //                   wherePromise.reject();
    //               }
    //               return wherePromise;
    //           }, function () { forLink[i2 + 1].reject(); })
    //           .then(function (items) {
    //               var a4 = assert.areEqual(test, 1, items.length);
    //               var item = items[0];
    //               var create = assert.areEqual(test, shouldHaveCreatedAt, item.createdAt !== undefined);
    //               var update = assert.areEqual(test, shouldHaveUpdatedAt, item.updatedAt !== undefined);
    //               var version = assert.areEqual(test, shouldHaveVersion, item.version !== undefined);

    //               var lookupPromise = newLink();
    //               if (create && update && version && a4) {
    //                   table.lookup(savedItem.id, userParams).then(function (item) {
    //                       lookupPromise.resolve(item);
    //                   }, function () {
    //                       forLink[i2 + 1].reject();
    //                   })
    //               }
    //               else {
    //                   forLink[i2 + 1].reject();
    //               }
    //               return lookupPromise;
    //           }, function () { forLink[i2 + 1].reject(); })
    //           .then(function (item) {
    //               var updatePromise = newLink();
    //               var a1 = (assert.areEqual(test, shouldHaveCreatedAt, item.createdAt !== undefined) &&
    //                         assert.areEqual(test, shouldHaveUpdatedAt, item.updatedAt !== undefined) &&
    //                     assert.areEqual(test, shouldHaveVersion, item.version !== undefined));

    //               savedItem.name = 'Hello!';
    //               savedVersion = item.version;

    //               if (a1) {
    //                   table.update(savedItem, userParams).then(function (item) {
    //                       updatePromise.resolve(item);
    //                   }, function () {
    //                       forLink[i2 + 1].reject();
    //                   })
    //               }
    //               else {
    //                   updatePromise.reject();
    //               }
    //               return updatePromise;
    //           }, function () { forLink[i2 + 1].reject(); })
    //           .then(function (item) {
    //               var a1 = (assert.areEqual(test, shouldHaveCreatedAt, item.createdAt !== undefined) &&
    //                                    assert.areEqual(test, shouldHaveUpdatedAt, item.updatedAt !== undefined) &&
    //                                    assert.areEqual(test, shouldHaveVersion, item.version !== undefined));
    //               if (shouldHaveVersion) {
    //                   a1 = a1 && assert.areNotEqual(item.version, savedVersion);
    //               }

    //               if (a1) {
    //                   table.del(item).then(function (item) {
    //                       forLink[i2 + 1].resolve(i2 + 1);
    //                   }, function () {
    //                       forLink[i2 + 1].reject();
    //                   })
    //               } else {
    //                   forLink[i2 + 1].reject();
    //               }
    //           })
    //        })
    //    })
    //    $.when.apply($, forLink).then(function () {
    //        done(true);
    //    }, function () {
    //        done(false);
    //    });
    //}, [zumo.runtimeFeatureNames.NodeRuntime_Only]));

    tests.push(new zumo.Test('LookupAsyncWithNosuchItemAgainstStringIdTable', function (test, done) {
        globaldone = done;
        globalTest = test;

        var table = zumo.getClient().getTable(stringIdTableName);
        var insertPromises = [newLink()];

        emptyTable(table).then(function () {
            test.addLog('Initialize our data');
            validStringIds.forEach(function (testId) {
                var insertPromise = newLink();
                insertPromises.push(insertPromise)
                table.insert({
                    id: testId, name: 'Hey'
                }).then(function () {
                    insertPromise.resolve();
                }, function (err) {
                    test.addLog('Error' + JSON.stringify(err));
                    insertPromise.reject();
                });
            });

            insertPromises[0].resolve();

            var deleteStart = newLink();
            $.when.apply($, insertPromises).then(function () {
                deleteStart.resolve();
            }, function (err) {
                done(false);
            });

            var deletePromises = [newLink()];
            deleteStart.then(function () {
                test.addLog('Delete our data');
                validStringIds.forEach(function (testId) {
                    var def = newLink();
                    deletePromises.push(def);
                    table.del({ id: testId }).then(function () {
                        def.resolve();
                    }, function (err) {
                        test.addLog('Should have suceeded');
                        test.addLog('Error' + JSON.stringify(err));
                        def.reject();
                    });
                });

                deletePromises[0].resolve();
                var lookupStart = newLink();
                $.when.apply($, deletePromises).then(function () {
                    lookupStart.resolve();
                }, function () {
                    done(false);
                });

                var lookupPromises = [newLink()];
                lookupStart.then(function () {
                    validStringIds.forEach(function (testId) {
                        var def = newLink();
                        lookupPromises.push(def);
                        table.lookup(testId).then(function (item) {
                            test.addLog('Should have failed');
                            return def.reject();
                        }, function (error) {
                            def.resolve();
                        });
                    });
                    lookupPromises[0].resolve();
                    $.when.apply($, lookupPromises).then(function () {
                        done(true);
                    }, function () {
                        done(false);
                    });
                })
            })
        }, failFn);
    }));

    tests.push(new zumo.Test('InsertAsyncWithExistingItemAgainstStringIdTable', function (test, done) {
        globaldone = done;
        globalTest = test;

        var table = zumo.getClient().getTable(stringIdTableName);
        var insertPromises = [newLink()];

        emptyTable(table).then(function () {
            test.addLog('Initialize our data');
            validStringIds.forEach(function (testId) {
                var insertPromise = newLink();
                insertPromises.push(insertPromise)
                table.insert({ id: testId, name: 'Hey' }).then(function () {
                    insertPromise.resolve();
                }, function (error) {
                    if (error != undefined) { test.addLog('Error' + JSON.stringify(error)); }
                    done(false);
                });
            });

            insertPromises[0].resolve();
            var lookupStart = newLink();
            $.when.apply($, insertPromises).then(function () {
                lookupStart.resolve();
            }, function (err) {
                done(false);
            });

            var lookupPromises = [newLink()];
            lookupStart.then(function () {
                validStringIds.forEach(function (testId) {
                    var def = newLink();
                    lookupPromises.push(def);
                    table.lookup(testId).then(function (item) {
                        if (assert.areEqual(test, item.id, testId)) {
                            def.resolve();
                        }
                        else {
                            def.reject();
                        }
                    }, function (error) {
                        if (error != undefined) { test.addLog('Error' + JSON.stringify(error)); }
                        done(false);
                    });
                });

                lookupPromises[0].resolve();

                var insertStart = newLink();
                $.when.apply($, lookupPromises).then(function () {
                    insertStart.resolve();
                }, function (err) {
                    done(false);
                });

                var insertPromises = [newLink()];
                insertStart.then(function () {
                    test.addLog('Insert duplicates into our data');
                    validStringIds.forEach(function (testId) {
                        var def = newLink();
                        insertPromises.push(def);
                        return table.insert({ id: testId, name: 'I should really not do this' }).then(function (item) {
                            test.addLog('Should have failed');
                            def.reject();
                        }, function (error) {
                            def.resolve();
                        });
                    });
                    insertPromises[0].resolve();
                    $.when.apply($, insertPromises).then(function () {
                        done(true);
                    }, function () {
                        done(false);
                    });
                })
            });
        }, failFn);
    }));

    tests.push(new zumo.Test('UpdateAsyncWithNosuchItemAgainstStringIdTable', function (test, done) {
        globaldone = done;
        globalTest = test;

        var client = zumo.getClient();
        var table = client.getTable(stringIdTableName);
        var insertPromises = [newLink()];

        emptyTable(table).then(function () {
            test.addLog('Initialize our data');
            validStringIds.forEach(function (testId) {
                var insertPromise = newLink();
                insertPromises.push(insertPromise)
                table.insert({
                    id: testId, name: 'Hey'
                }).then(function () {
                    insertPromise.resolve();
                }, function (err) {
                    insertPromise.reject();
                });
            });

            insertPromises[0].resolve();

            var deleteStart = newLink();
            $.when.apply($, insertPromises).then(function () {
                deleteStart.resolve();
            }, function (err) {
                done(false);
            });

            var deletePromises = [newLink()];
            deleteStart.then(function () {
                test.addLog('Delete our data');
                validStringIds.forEach(function (testId) {
                    var def = newLink();
                    deletePromises.push(def);
                    table.del({ id: testId }).then(function () {
                        def.resolve();
                    }, function (err) {
                        test.addLog('Should have suceeded');
                        test.addLog('Error' + JSON.stringify(err));
                        def.reject();
                    });
                });

                deletePromises[0].resolve();
                var updateStart = newLink();
                $.when.apply($, deletePromises).then(function () {
                    updateStart.resolve();
                }, function () {
                    done(false);
                });

                var updatePromises = [newLink()];
                updateStart.then(function () {
                    test.addLog('Update records the don\'t exist');
                    validStringIds.forEach(function (testId) {
                        var def = newLink();
                        updatePromises.push(def);
                        return table.update({ id: testId, name: 'Alright!' }).then(function (item) {
                            test.addLog('Should have failed');
                            return def.reject();
                        }, function (error) {
                            def.resolve();
                        });
                    });
                    updatePromises[0].resolve();
                    $.when.apply($, updatePromises).then(function () {
                        done(true);
                    }, function () {
                        done(false);
                    });
                })
            })
        }, failFn);
    }));

    tests.push(new zumo.Test('AsyncTableOperationsWithIntegerAsStringIdAgainstIntIdTable', function (test, done) {
        globalTest = test;
        globaldone = done;

        var client = zumo.getClient();
        var table = client.getTable(intIdTableName);

        emptyTable(table).then(function () {
            test.addLog('Insert record');
            var insertPromise = newLink();
            table.insert({
                name: 'Hey'
            }).then(function (item) {
                insertPromise.resolve();
                testId = item.id;
            }, failFn);

            insertPromise.then(function (item) {
                var readPromise = newLink();
                test.addLog('read table');
                table.read().then(function (items) {
                    readPromise.resolve(items);
                }, failFn);
                return readPromise;
            }).then(function (items) {
                var selectPromise = newLink();
                var a1 = assert.areEqual(test, 1, items.length);
                var a2 = assert.areEqual(test, testId, items[0].id);
                var a3 = assert.areEqual(test, "Hey", items[0].name);
                if (a1 && a2 && a3) {
                    test.addLog('perform select');
                    table.select(function () {
                        this.xid = this.id; this.xname = this.name; return this;
                    }).read().then(function (item) {
                        selectPromise.resolve(item);
                    }, failFn);
                }
                else {
                    selectPromise.reject();
                }
                return selectPromise;
            }).then(function (items) {
                var lookupPromise = newLink();
                var a1 = assert.areEqual(test, 1, items.length);
                var a2 = assert.areEqual(test, testId, items[0].xid);
                var a3 = assert.areEqual(test, 'Hey', items[0].xname);
                if (a1 && a2 && a3) {
                    test.addLog('perform lookup');
                    table.lookup(items[0].xid).then(function (items) {
                        lookupPromise.resolve(items)
                    }, failFn);
                }
                else {
                    lookupPromise.reject();
                }
                return lookupPromise;
            }).then(
                 function (item) {
                     var updatePromise = newLink();
                     var a1 = assert.areEqual(test, testId, item.id);
                     var a2 = assert.areEqual(test, 'Hey', item.name);
                     if (a1 && a2) {
                         test.addLog('perform update');
                         item.name = 'What?';
                         table.update(item).then(function (items) {
                             updatePromise.resolve(items)
                         }, failFn);
                     }
                     else {
                         updatePromise.reject();
                     }
                     return updatePromise;
                 }).then(
                 function (item) {
                     var refreshPromise = newLink();
                     var a1 = assert.areEqual(test, testId, item.id);
                     var a2 = assert.areEqual(test, 'What?', item.name);
                     if (a1 && a2) {
                         test.addLog('perform refresh');
                         table.refresh({
                             id: item.id, name: 'Hey'
                         }).then(function (item) {
                             refreshPromise.resolve(item)
                         }, failFn);
                     }
                     else {
                         refreshPromise.reject();
                     }
                     return refreshPromise;
                 }).then(
                 function (item) {
                     var readPromise = newLink();
                     var a1 = assert.areEqual(test, testId, item.id);
                     var a2 = assert.areEqual(test, 'What?', item.name);
                     if (a1 && a2) {
                         test.addLog('perform read again');
                         table.read().then(function (items) {
                             readPromise.resolve(items);
                         }, failFn);
                     }
                     else {
                         readPromise.reject();
                     }
                     return readPromise;
                 }).then(
                function (items) {
                    var a1 = assert.areEqual(test, 1, items.length);
                    var a2 = assert.areEqual(test, testId, items[0].id);
                    var a3 = assert.areEqual(test, 'What?', items[0].name);
                    if (a1 && a2 && a3) {
                        done(true);
                    }
                    else {
                        done(false);
                    }
                });
        }, failFn);
    }));

    tests.push(new zumo.Test('DeleteAsyncWithWithMergeConflict', function (test, done) {
        globalTest = test;
        globaldone = done;
        test.addLog('test delete with conflict')
        var client = zumo.getClient();
        var table = client.getTable(stringIdTableName),
                savedVersion,
                correctVersion;

        var insertPromise = newLink();
        emptyTable(table).then(function () {
            table.insert({
                id: 'an id', name: 'a value'
            }).then(function (item) {
                insertPromise.resolve(item)
            }, failFn)
        }, failFn);

        var updatePromise = newLink();
        insertPromise.then(function (item) {
            savedVersion = item.version;
            item.name = 'Hello!';
            table.update(item).then(function (items) {
                updatePromise.resolve(items);
            }, failFn)
        })

        var deletePromise = newLink();
        updatePromise.then(function (item) {
            var a1 = assert.areNotEqual(item.version, savedVersion);
            item.name = 'But Wait!';
            correctVersion = item.version;
            item.version = savedVersion;
            table.del(item).then(function (item) {
                deletePromise.reject();
            }, function (error) {
                var a1 = (assert.areEqual(test, 412, error.request.status) &&
                          error.message.indexOf("Precondition Failed") > -1 &&
                          assert.areEqual(test, error.serverInstance.version, correctVersion) &&
                          assert.areEqual(test, error.serverInstance.name, 'Hello!'));
                if (a1) {
                    item.version = correctVersion;
                    deletePromise.resolve(item);
                }
                else {
                    deletePromise.reject();
                }

            });
        });

        deletePromise.then(function (item) {
            table.del(item).then(function () {
                done(true);
            }, failFn)
        }, function () {
            done(false);
        });
    }));

    tests.push(new zumo.Test('ReadAsyncWithValidIntIdAgainstIntIdTable', function (test, done) {
        globaldone = done;
        globalTest = test;

        var table = zumo.getClient().getTable(intIdTableName);

        emptyTable(table).then(function () {
            test.addLog('Insert record');
            var insertPromise = newLink();
            table.insert({
                name: 'Hey'
            }).then(function (item) {
                insertPromise.resolve(item);
                testId = item.id;
            }, failFn);

            insertPromise.then(function (item) {
                var readPromise = newLink();
                table.read().then(function (item) {
                    readPromise.resolve(item);
                }, failFn);
                return readPromise;
            }).then(function (items) {
                var a1 = assert.areEqual(test, 1, items.length);
                var a2 = items[0].id > 0;
                var a3 = assert.areEqual(test, 'Hey', items[0].name);
                if (a1 && a2 && a3) {
                    done(true);
                }
                else {
                    done(false);
                }
            });
        });
    }));

    // BUG #1706815 (OData query for version field (string <--> byte[] mismatch)
    //tests.push(new zumo.Test('AsyncTableOperationsWithAllSystemProperties', function (test, done) {
    //    globalTest = test;
    //    globaldone = done;

    //    var table = zumo.getClient().getTable(stringIdTableName),
    //        savedItem,
    //        savedVersion,
    //        savedUpdatedAt;

    //    table.systemProperties = WindowsAzure.MobileServiceTable.SystemProperties.All;  //All

    //    var insertPromise = emptyTable(table).then(function () {
    //        return table.insert({ id: 'an id', Name: 'a value' });
    //    });

    //    var readPromise = newLink();
    //    insertPromise.then(function (item) {
    //        if (item.createdAt != null && item.updatedAt != null && item.version != null) {
    //            table.read().then(function (item) {
    //                readPromise.resolve(item);
    //            }, failFn);
    //        }
    //        else {
    //            readPromise.reject();
    //        }
    //        return readPromise;
    //    }, failFn);


    //    var wherePromise = newLink();
    //    readPromise.then(function (items) {
    //        assert.areEqual(test, 1, items.length)
    //        var item = items[0];
    //        if (item.createdAt != null && item.updatedAt != null && item.version != null) {
    //            savedItem = item;
    //        }
    //        else {
    //            wherePromise.reject;
    //        }
    //        table.where(function (value) {
    //            return this.version == value
    //        }, item.version).read().then(
    //            function (items) {
    //                wherePromise.resolve(items);
    //            }, failFn);
    //        return wherePromise;
    //    }, failFn);

    //    var wherePromise2 = newLink();
    //    wherePromise.then(function (items) {
    //        assert.areEqual(test, 1, items.length)
    //        var item = items[0];
    //        if (item.createdAt == null || item.updatedAt == null || item.version == null) {
    //            wherePromise2.reject();
    //        }
    //        table.where(function (value) {
    //            return this.createdAt == value
    //        }, savedItem.createdAt).read().then(
    //            function (items) {
    //                wherePromise2.resolve(items);
    //            }, failFn);
    //        return wherePromise2;
    //    }, failFn);

    //    var wherePromise3 = newLink();
    //    wherePromise2.then(function (items) {
    //        assert.areEqual(test, 1, items.length)
    //        var item = items[0];
    //        if (item.createdAt == null || item.updatedAt == null || item.version == null) {
    //            wherePromise3.reject();
    //        }
    //        table.where(function (value) {
    //            return this.updatedAt == value
    //        }, savedItem.updatedAt).read().then(
    //            function (items) {
    //                wherePromise3.resolve(items);
    //            }, failFn);
    //        return wherePromise3;
    //    }, failFn);

    //    var lookupPromise = newLink();
    //    wherePromise3.then(function (items) {
    //        assert.areEqual(test, 1, items.length)
    //        var item = items[0];
    //        if (item.createdAt == null || item.updatedAt == null || item.version == null) {
    //            lookupPromise.reject();
    //        }
    //        table.lookup(savedItem.id).then(function (items) {
    //            lookupPromise.resolve(items);
    //        }, failFn);
    //        return lookupPromise;
    //    }, failFn);

    //    var updatePromise = newLink();
    //    lookupPromise.then(function (item) {
    //        var a1 = assert.areEqual(test, item.id, savedItem.id);
    //        var a2 = assert.areEqual(test, item.updatedAt.valueOf(), savedItem.updatedAt.valueOf());
    //        var a3 = assert.areEqual(test, item.createdAt.valueOf(), savedItem.createdAt.valueOf());
    //        var a4 = assert.areEqual(test, item.version, savedItem.version);

    //        savedItem.name = 'Hello';
    //        savedVersion = savedItem.version; //WinJS Mutates
    //        savedUpdatedAt = savedItem.updatedAt.valueOf();

    //        if (a1 && a2 && a3 && a4) {
    //            table.update(savedItem).then(function (items) {
    //                updatePromise.resolve(items);
    //            }, failFn);
    //        }
    //        else {
    //            updatePromise.reject();
    //        }
    //        return updatePromise;
    //    }, failFn);

    //    var readPromise2 = newLink();
    //    updatePromise.then(function (item) {
    //        var a1 = assert.areEqual(test, item.id, savedItem.id);
    //        var a2 = assert.areEqual(test, item.createdAt.valueOf(), savedItem.createdAt.valueOf());
    //        var a3 = assert.areNotEqual(test, item.version, savedVersion);
    //        var a4 = (test, item.updatedAt.valueOf() != savedUpdatedAt);
    //        savedItem = item;

    //        if (a1 && a2 && a3 && a4) {
    //            table.read().then(function (items) {
    //                readPromise2.resolve(items);
    //            }, failFn);
    //        }
    //        else {
    //            readPromise2.reject();
    //        }

    //        return readPromise2;
    //    }, failFn);

    //    readPromise2.then(function (items) {
    //        var item = items[0];
    //        var a1 = assert.areEqual(test, item.id, savedItem.id);
    //        var a2 = assert.areEqual(test, item.updatedAt.valueOf(), savedItem.updatedAt.valueOf());
    //        var a3 = assert.areEqual(test, item.createdAt.valueOf(), savedItem.createdAt.valueOf());
    //        var a4 = assert.areEqual(test, item.version, savedItem.version);

    //        if (a1 && a2 && a3 && a4) {
    //            table.del(savedItem).then(
    //                function () {
    //                    done(true);
    //                },
    //                function (error) {
    //                    done(false);
    //                });
    //        }
    //        else {
    //            done(false);
    //        }
    //    }, failFn);

    //}, [zumo.runtimeFeatureNames.NodeRuntime_Only]));

    //RDBug 1734883:SDK: JS: Account for broken client-server contract when talking to .NET based runtimes
    //tests.push(new zumo.Test('AsyncTableOperationsWithSystemPropertiesSetExplicitly', function (test, done) {
    //    globalTest = test;
    //    globaldone = done;

    //    var table = zumo.getClient().getTable(stringIdTableName),
    //        props = WindowsAzure.MobileServiceTable.SystemProperties;

    //    table.systemProperties = props.Version | props.CreatedAt | props.UpdatedAt;

    //    emptyTable(table).then(function () {
    //        return table.insert({ name: 'a value' });
    //    }).then(function (item) {
    //        if (item.createdAt != null && item.updatedAt != null && item.version != null) {
    //            table.systemProperties = props.Version | props.CreatedAt;
    //            return table.insert({ name: 'a value' });
    //        } else {
    //            throw new Error('Incorrect system properties');
    //        }
    //    }).then(function (item) {
    //        if (item.createdAt != null && item.updatedAt == null && item.version != null) {
    //            table.systemProperties = props.UpdatedAt | props.CreatedAt;
    //            return table.insert({ name: 'a value' });
    //        } else {
    //            throw new Error('Incorrect system properties');
    //        }
    //    }).then(function (item) {
    //        if (item.createdAt != null && item.updatedAt != null && item.version == null) {
    //            table.systemProperties = props.UpdatedAt;
    //            return table.insert({ name: 'a value' });
    //        } else {
    //            throw new Error('Incorrect system properties');
    //        }
    //    }).then(function (item) {
    //        if (!(item.createdAt == null && item.updatedAt != null && item.version == null)) {
    //            throw new Error('Incorrect system properties');
    //        }
    //    }).then(function () {
    //        done(true);
    //    }, failFn);
    //}, [zumo.runtimeFeatureNames.NodeRuntime_Only]));

    return {
        name: 'MobileServiceTableGenericFunctional',
        tests: tests
    };
}
zumo.tests.tableGenericFunctional = defineTableGenericFunctionalTestsNamespace();

//assert

assert.areEqual = function (test, objectA, objectB) {
    var errors = [];
    if (!zumo.util.compare(objectA, objectB, errors)) {
        errors.forEach(function (error) {
            test.addLog(error);
        })
        return false;
    } else {
        return true;
    }
};

assert.areNotEqual = function (test, objectA, objectB) {
    var errors = [];
    if (zumo.util.compare(objectA, objectB, errors)) {
        errors.forEach(function (error) {
            test.addLog(error);
        })
        return false;
    } else {
        return true;
    }
};
function assert() {
}

testValidSystemPropertyQueryStrings = [
    "__systemProperties=*",
    "__systemProperties=createdAt",
    "__systemProperties=createdAt,updatedAt",
    "__systemProperties=createdAt,version",
    "__systemProperties=createdAt,updatedAt,version",
    "__systemProperties=createdAt,version,updatedAt",
    "__systemProperties=updatedAt",
    "__systemProperties=updatedAt,createdAt",
    "__systemProperties=updatedAt,createdAt,version",
    "__systemProperties=updatedAt,version",
    "__systemProperties=updatedAt,version, createdAt",
    "__systemProperties=version",
    "__systemProperties=version,createdAt",
    "__systemProperties=version,createdAt,updatedAt",
    "__systemProperties=version,updatedAt",
    "__systemProperties=version,updatedAt, createdAt",

    // Trailing commas, extra commas
    "__systemProperties=createdAt,",
    "__systemProperties=createdAt,updatedAt,",
    "__systemProperties=createdAt,updatedAt,version,",
    "__systemProperties=,createdAt",
    "__systemProperties=createdAt,,updatedAt",
    "__systemProperties=createdAt, ,updatedAt,version",
    "__systemProperties=createdAt,,",
    "__systemProperties=createdAt, ,",

    // Trailing, leading whitespace
    "__systemProperties= *",
    "__systemProperties=\t*\t",
    "__systemProperties= createdAt ",
    "__systemProperties=\tcreatedAt,\tupdatedAt\t",
    "__systemProperties=\rcreatedAt,\rupdatedAt,\tversion\r",
    "__systemProperties=\ncreatedAt\n",
    "__systemProperties=createdAt,\nupdatedAt",
    "__systemProperties=createdAt, updatedAt, version",

    // Different casing
    "__SystemProperties=*",
    "__SystemProperties=createdAt",
    "__SYSTEMPROPERTIES=createdAt,updatedAt",
    "__systemproperties=createdAt,updatedAt,version",
    "__SystemProperties=CreatedAt",
    "__SYSTEMPROPERTIES=createdAt,UPDATEDAT",
    "__systemproperties=createdat,UPDATEDAT,version",

    // Sans __ prefix
    "__systemProperties=createdAt",
    "__systemProperties=updatedAt,createdAt",
    "__systemProperties=UPDATEDAT,createdat",
    "__systemProperties=updatedAt,version,createdAt",

    // Combinations of above
    "__SYSTEMPROPERTIES=createdAt, updatedat",
    "__systemProperties=CreatedAt,,\tVERSION",
    "__systemProperties= updatedat ,,"
];

var baseValidStringIds = function () {
    return [
                 "id",
                 "true",
                 "false",
                 "00000000-0000-0000-0000-000000000000",
                 "aa4da0b5-308c-4877-a5d2-03f274632636",
                 "69C8BE62-A09F-4638-9A9C-6B448E9ED4E7",
                 "{EC26F57E-1E65-4A90-B949-0661159D0546}",
                 "87D5B05C93614F8EBFADF7BC10F7AE8C",
                 "someone@someplace.com",
                 "id with spaces",
                 " .",
                 "'id' with single quotes",
                 "id with Japanese 私の車はどこですか",
                 "id with Arabic أين هو سيارتي؟",
                 "id with Russian Где моя машина"
        ];
    },
    validStringIds = baseValidStringIds(),
    emptyStringIds = [""];

testInvalidSystemPropertyQueryStrings = [
    // Unknown system Properties
    "__invalidSystemProperties=createdAt,version",
    "__systemProperties=created",
    "__systemProperties=updated At",
    "__systemProperties=notASystemProperty",
    "__systemProperties=_version",

// System properties not comma separated
    "__systemProperties=createdAt updatedAt",
    "__systemProperties=createdAt\tversion",
    "__systemProperties=createdAt updatedAt version",
    "__systemProperties=createdAtversion",

// All and individual system properties requested
"__systemProperties=*,updatedAt"
];

invalidStringIds = [
".",
"..",
"id with 256 characters " + new Array(257 - 23).join('A'),
"\r",
"\n",
"\t",
"id\twith\ttabs",
"id\rwith\rreturns",
"id\nwith\n\newline",
"id with backslash \\",
"id with forwardslash \/",
"\"idWithQuotes\"",
"\\",
"\/",
"`",
"+",
" ",
"control character between 0 and 31 " + String.fromCharCode(16),
"control character between 127 and 159" + String.fromCharCode(130)
];

function defineBlogTestsNamespace() {
    var tests = [];
    tests.push(new zumo.Test('UseBlog', function (test, done) {
        test.addLog('Create and manipulate posts/comments');
        var client = zumo.getClient();
        var postTable = client.getTable('blog_posts');
        var commentTable = client.getTable('blog_comments');
        var context = {};
        test.addLog('Add a few posts and a comment');
        postTable.insert({ title: "Windows 8" }).then(
            function (post) {
                context.post = post; context.newItems = 'id ge ' + '\'' + post.id + '\'';
            }, function (err) {
                test.addLog('Error' + JSON.stringify(err));
                done(false);
            }).then(function () {
                return postTable.insert({ title: "ZUMO" })
            }, function (err) {
                test.addLog('Error' + JSON.stringify(err));
                done(false);
            }).then(function (highlight) {
                context.highlight = highlight;
                return commentTable.insert({ postid: context.post.id, name: "Anonymous", commentText: "Beta runs great" })
            }, function (err) {
                test.addLog('Error' + JSON.stringify(err));
                done(false);
            }).then(function () {
                return commentTable.insert({ postid: context.highlight.id, name: "Anonymous", commentText: "Whooooo" })
            }, function (err) {
                test.addLog('Error' + JSON.stringify(err));
                done(false);
            }).then(function () {
                return postTable.where('id eq ' + '\'' + context.post.id + '\' or id eq ' + '\'' + context.highlight.id + '\'').read()
            }, function (err) {
                test.addLog('Error' + JSON.stringify(err));
                done(false);
            }).then(function (items) {
                if (!assert.areEqual(test, 2, items.length)) {
                    done(false);
                }
                else {
                    test.addLog('Add another comment to the first post');
                    return commentTable.insert({ postid: context.post.id, commentText: "Can't wait" })
                }
            }, function (err) {
                test.addLog('Error' + JSON.stringify(err));
                done(false);
            }).then(function (opinion) {
                assert.areNotEqual(test, 0, opinion.id);
                done(true);
            }, function (err) {
                test.addLog('Error' + JSON.stringify(err));
                done(false);
            })
    }))

    return {
        name: 'Blog',
        tests: tests
    };
}

zumo.tests.blog = defineBlogTestsNamespace();