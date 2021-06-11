// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoCUDTests.h"
#import "ZumoTest.h"
#import "ZumoTestGlobals.h"

@implementation ZumoCUDTests

+ (NSArray *)createTests {
    NSMutableArray *result = [[NSMutableArray alloc] init];
    [result addObject:[self createDeleteTestWithName:@"[int id] Delete with id" andType:DeleteUsingId]];
    [result addObject:[self createDeleteTestWithName:@"[int id] Delete with object" andType:DeleteUsingObject]];
    
    [result addObject:[self createNegDeleteTestWithName:@"(Neg) Delete with non-existent id" andType:NegDeleteUsingInvalidId]];
    [result addObject:[self createNegDeleteTestWithName:@"(Neg) Delete with object and non-existent id" andType:NegDeleteObjectInvalidId]];
    [result addObject:[self createNegDeleteTestWithName:@"(Neg) Delete with object without 'id' field" andType:NegDeleteObjectNoId]];
    
    NSArray *validStringIds = @[@"iOS with space", @"random number", @"iOS non-english ãéìôü ÇñÑالكتاب على الطاولة这本书在桌子上הספר הוא על השולחן"];
    for (NSString *validId in validStringIds) {
        for (int i = 0; i < 2; i++) {
            BOOL useDeleteWithId = i == 0;
            NSString *testName = [NSString stringWithFormat:@"[string id] Delete (%@), id = %@", useDeleteWithId ? @"by id" : @"by object", validId];
            ZumoTest *test = [ZumoTest createTestWithName:testName andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
                NSString *itemId;
                if ([validId isEqualToString:@"random number"]) {
                    itemId = [NSString stringWithFormat:@"%d", rand()];
                } else {
                    itemId = validId;
                }
                [test addLog:[@"Using id = " stringByAppendingString:itemId]];
                MSClient *client = [[ZumoTestGlobals sharedInstance] client];
                MSTable *table = [client tableWithName:TABLES_ROUND_TRIP_STRING_ID];
                [table insert:@{@"id":itemId,@"name":@"unused"} completion:^(NSDictionary *item, NSError *error) {
                    // it's fine if the insert failed (possible if the item already existed.
                    
                    [test addLog:@"Calling delete"];
                    MSDeleteBlock deleteCompletion = ^(id deletedItemId, NSError *error) {
                        if (error) {
                            [test addLog:[NSString stringWithFormat:@"Error calling delete: %@", error]];
                            completion(NO);
                        } else {
                            [test addLog:[NSString stringWithFormat:@"Delete succeeded for item: %@", deletedItemId]];
                            completion(YES);
                        }
                    };
                    if (useDeleteWithId) {
                        [table deleteWithId:itemId completion:deleteCompletion];
                    } else {
                        [table delete:@{@"id":itemId,@"name":@"unused"} completion:deleteCompletion];
                    }
                }];
            }];
            [test addRequiredFeature:FEATURE_STRING_ID_TABLES];
            [result addObject:test];
        }
    }

    [result addObject:[self createUpdateTestWithName:@"[int id] Update item" andType:UpdateUsingObject]];
    [result addObject:[self createNegUpdateTestWithName:@"(Neg) Update with non-existing id" andType:NegUpdateObjectInvalidId]];
    [result addObject:[self createNegUpdateTestWithName:@"(Neg) Update with no id" andType:NegUpdateObjectNoId]];
    
    for (NSString *validId in validStringIds) {
        NSString *testName = [@"[string id] Update with id = " stringByAppendingString:validId];
        ZumoTest *test = [ZumoTest createTestWithName:testName andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
            NSString *itemId;
            if ([validId isEqualToString:@"random number"]) {
                itemId = [NSString stringWithFormat:@"%d", rand()];
            } else {
                itemId = validId;
            }
            [test addLog:[@"Using id = " stringByAppendingString:itemId]];
            MSClient *client = [[ZumoTestGlobals sharedInstance] client];
            MSTable *table = [client tableWithName:TABLES_ROUND_TRIP_STRING_ID];
            [table insert:@{@"id":itemId,@"name":@"unused"} completion:^(NSDictionary *item, NSError *error) {
                // it's fine if the insert failed (possible if the item already existed.
                NSDictionary *toUpdate = @{@"id":itemId,@"name":@"another value"};
                [table update:toUpdate completion:^(NSDictionary *updated, NSError *error) {
                    BOOL testPassed;
                    if (error) {
                        [test addLog:[NSString stringWithFormat:@"Error calling delete: %@", error]];
                        testPassed = NO;
                    } else {
                        [test addLog:[NSString stringWithFormat:@"Updated: %@", updated]];
                        NSMutableArray *errors = [[NSMutableArray alloc] init];
                        if ([ZumoTestGlobals compareObjects:toUpdate
                                                       with:updated
                                                 ignoreKeys:@[ @"id", @"complex", @"bool", @"date1", @"integer", @"number", MSSystemColumnVersion, MSSystemColumnUpdatedAt, MSSystemColumnCreatedAt, MSSystemColumnDeleted ]
                                                        log:errors]) {
                            [test addLog:@"Object compared successfully"];
                            testPassed = YES;
                        } else {
                            [test addLog:@"Error comparing the objects:"];
                            for (NSString *err in errors) {
                                [test addLog:err];
                            }
                            testPassed = NO;
                        }
                    }
                    
                    [test addLog:@"Cleanup: deleting the item"];
                    [table deleteWithId:itemId completion:^(id itemId, NSError *error) {
                        [test addLog:[@"Delete " stringByAppendingString:(error ? @"failed" : @"succeeded")]];
                        completion(testPassed);
                    }];
                }];
            }];
        }];
        [test addRequiredFeature:FEATURE_STRING_ID_TABLES];
        [result addObject:test];
    }
    
    return result;
}

typedef enum { UpdateUsingObject, NegUpdateObjectInvalidId, NegUpdateObjectNoId } UpdateTestType;

+ (ZumoTest *)createUpdateTestWithName:(NSString *)name andType:(UpdateTestType)type {
    ZumoTest *result = [ZumoTest createTestWithName:name
                                       andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion)
    {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client tableWithName:TABLES_ROUND_TRIP_INT_ID];
        [table insert:@{ @"name" : @"John Doe", @"age" : @33 }
           completion:^(NSDictionary *inserted, NSError *insertError)
        {
            if (insertError) {
                [test addLog:[NSString stringWithFormat:@"Error inserting data: %@", insertError]];
                test.testStatus = TSFailed;
                completion(NO);
                return;
            }
               
            NSNumber *itemId = [inserted objectForKey:@"id"];
            [test addLog:[NSString stringWithFormat:@"Inserted element %d to be deleted", [itemId intValue]]];
            [table readWithId:itemId
                   completion:^(NSDictionary *roundTripped, NSError *rtError)
            {
                if (rtError) {
                    [test addLog:[NSString stringWithFormat:@"Error retrieving inserted item: %@", rtError]];
                    [test setTestStatus:TSFailed];
                    completion(NO);
                    return;
                }
                
                NSMutableDictionary *itemToUpdate = [inserted mutableCopy];
                NSNumber *updatedValue = @35;
                itemToUpdate[@"age"] = updatedValue;
                
                [table update:itemToUpdate
                   completion:^(NSDictionary *updatedItem, NSError *updateError)
                {
                    BOOL passed = YES;
                    if (updateError) {
                        passed = NO;
                        [test addLog:[NSString stringWithFormat:@"Error updating item: %@", updateError]];
                    } else {
                        if (![updatedValue isEqualToNumber:[updatedItem objectForKey:@"age"]]) {
                            passed = NO;
                            [test addLog:[NSString stringWithFormat:@"Incorrect value for updated object: %@", updatedItem]];
                        }
                    }
                    
                    if (passed) {
                        [self validateUpdateForTest:test andTable:table andId:itemId andExpectedValue:updatedValue withCompletion:completion];
                    } else {
                        [test setTestStatus:(passed ? TSPassed : TSFailed)];
                        completion(passed);
                    }
                }];
            }];
        }];
    }];
    
    [result addRequiredFeature:FEATURE_INT_ID_TABLES];
    return result;
}

+ (ZumoTest *)createNegUpdateTestWithName:(NSString *)name andType:(UpdateTestType)type {
    ZumoTest *result = [ZumoTest createTestWithName:name
                                       andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion)
    {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client tableWithName:TABLES_ROUND_TRIP_STRING_ID];
        
        NSDictionary *itemToUpdate;
        if (type == NegUpdateObjectNoId) {
            itemToUpdate = @{ @"name" : @"John Doe", @"age" : @33 };
        } else if (type == NegUpdateObjectInvalidId) {
            itemToUpdate = @{ @"id": @"IdThatCan'tExistHere!", @"name" : @"John Doe" };
        }
        
        [table update:itemToUpdate completion:^(NSDictionary *updatedItem, NSError *updateError) {
            BOOL passed = YES;
            if (!updateError) {
                passed = NO;
                [test addLog:[NSString stringWithFormat:@"Expected error, but update succeeded for item: %@", itemToUpdate]];
            } else if (type == NegUpdateObjectNoId) {
                if (updateError.code != MSMissingItemIdWithRequest) {
                    [test addLog:[NSString stringWithFormat:@"Unexpected error code: %ld", (long)updateError.code]];
                    passed = NO;
                }
            } else if (updateError.code != MSErrorMessageErrorCode) {
                [test addLog:[NSString stringWithFormat:@"Unexpected error code: %ld", (long)updateError.code]];
                passed = NO;
            } else {
                NSHTTPURLResponse *resp = [[updateError userInfo] objectForKey:MSErrorResponseKey];
                if (resp.statusCode != 404) {
                    [test addLog:[NSString stringWithFormat:@"Invalid response status code, expected 404, found %ld", (long)resp.statusCode]];
                    passed = NO;
                }
            }
            
            [test setTestStatus:(passed ? TSPassed : TSFailed)];
            completion(passed);
        }];
    }];
    
    return result;
}

+ (void)validateUpdateForTest:(ZumoTest *)test andTable:(MSTable *)table andId:(NSNumber *)itemId andExpectedValue:(NSNumber *)expectedValue withCompletion:(ZumoTestCompletion)completion {
    [table readWithId:itemId completion:^(NSDictionary *item, NSError *err) {
        BOOL passed = YES;
        if (err) {
            [test addLog:[NSString stringWithFormat:@"Error retrieving updated item: %@", err]];
            passed = NO;
        } else {
            if (![expectedValue isEqualToNumber:[item objectForKey:@"age"]]) {
                [test addLog:[NSString stringWithFormat:@"Update not successful, expected %@, got %@", expectedValue, item]];
                passed = NO;
            } else {
                [test addLog:@"Item updated successfully"];
            }
        }
        
        [test setTestStatus:(passed ? TSPassed : TSFailed)];
        completion(passed);
    }];
}

typedef enum { DeleteUsingId, DeleteUsingObject, NegDeleteUsingInvalidId, NegDeleteObjectInvalidId, NegDeleteObjectNoId } DeleteTestType;

+ (ZumoTest *)createDeleteTestWithName:(NSString *)name andType:(DeleteTestType)type {
    ZumoTest *result = [ZumoTest createTestWithName:name
                                       andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client tableWithName:TABLES_ROUND_TRIP_INT_ID];
                                           
        [table insert:@{ @"name" : @"John Doe", @"age" : @33 }
           completion:^(NSDictionary *inserted, NSError *insertError) {
            if (insertError) {
                [test addLog:[NSString stringWithFormat:@"Error inserting data: %@", insertError]];
                [test setTestStatus:TSFailed];
                completion(NO);
                return;
            }
               
            id itemId = [inserted objectForKey:@"id"];
            [test addLog:[NSString stringWithFormat:@"Inserted element %@ to be deleted", itemId]];
               
            [table readWithId:itemId completion:^(NSDictionary *roundTripped, NSError *rtError) {
                if (rtError) {
                    [test addLog:[NSString stringWithFormat:@"Error retrieving inserted item: %@", rtError]];
                    [test setTestStatus:TSFailed];
                    completion(NO);
                    return;
                }
                
                id postDeleteCheck = ^(NSNumber *deletedItemId, NSError *deleteError) {
                    BOOL passed = YES;
                    
                    if (deleteError) {
                        passed = NO;
                        [test addLog:[NSString stringWithFormat:@"Error deleting item: %@", deleteError]];
                    } else if (![itemId isEqualToNumber:deletedItemId]) {
                        [test addLog:[NSString stringWithFormat:@"Invalid ID %@ returned after deleting: %@", deletedItemId, itemId]];
                        passed = NO;
                    }
                    
                    if (passed) {
                        [self validateDeletionForTest:test andTable:table andId:itemId withCompletion:completion];
                    } else {
                        [test setTestStatus:(passed ? TSPassed : TSFailed)];
                        completion(passed);
                    }
                };
                
                if (type == DeleteUsingId) {
                    [table deleteWithId:itemId completion:postDeleteCheck];
                } else {
                    NSMutableDictionary *itemToDelete = [[NSMutableDictionary alloc] initWithDictionary:inserted copyItems:YES];
                    [table delete:itemToDelete completion:postDeleteCheck];
                }
            }];
        }];
    }];
    [result addRequiredFeature:FEATURE_INT_ID_TABLES];
    
    return result;
}

+ (ZumoTest *)createNegDeleteTestWithName:(NSString *)name andType:(DeleteTestType)type {
    ZumoTest *result = [ZumoTest createTestWithName:name
                                       andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion)
    {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client tableWithName:TABLES_ROUND_TRIP_STRING_ID];
       
        id postDelete = ^(NSNumber *deletedItemId, NSError *deleteError) {
            BOOL passed = YES;
            if (!deleteError) {
                passed = NO;
                [test addLog:[NSString stringWithFormat:@"Expected error, but delete succeeded for item: %@", deletedItemId]];
            } else if (type == NegDeleteObjectNoId) {
                if (deleteError.code != MSMissingItemIdWithRequest) {
                    [test addLog:[NSString stringWithFormat:@"Unexpected error code: %ld", (long)deleteError.code]];
                    passed = NO;
                }
            } else if ((deleteError.code != MSErrorMessageErrorCode) && (deleteError.code != MSErrorNoMessageErrorCode)) {
                [test addLog:[NSString stringWithFormat:@"Unexpected error code: %ld", (long)deleteError.code]];
                passed = NO;
            } else {
                NSHTTPURLResponse *resp = [[deleteError userInfo] objectForKey:MSErrorResponseKey];
                if (resp.statusCode != 404) {
                    [test addLog:[NSString stringWithFormat:@"Invalid response status code, expected 404, found %ld", (long)resp.statusCode]];
                    passed = NO;
                }
            }
            
            [test setTestStatus:(passed ? TSPassed : TSFailed)];
            completion(passed);
        };
        
        if (type == NegDeleteUsingInvalidId) {
          [table deleteWithId:@"MadeUpIdThatDoesn'tExist!" completion:postDelete];
        } else if (type == NegDeleteObjectNoId) {
            [table delete:@{ @"name" : @"John Doe", @"age" : @33 } completion:postDelete];
        } else {
            [table delete: @{ @"id" : @"MadeUpIdThatDoesn'tExist!", @"name" : @"John Doe", @"age" : @33 } completion:postDelete];
        }
    }];
    
    return result;
}

+ (void)validateDeletionForTest:(ZumoTest *)test andTable:(MSTable *)table andId:(NSNumber *)itemId withCompletion:(ZumoTestCompletion)completion {
    [table readWithId:itemId completion:^(NSDictionary *item, NSError *err) {
        if (!err) {
            [test addLog:[NSString stringWithFormat:@"Delete failed, item %@ still exists: %@", itemId, item]];
        } else {
            [test addLog:@"Item deleted successfully"];
        }
        
        [test setTestStatus:(err ? TSPassed : TSFailed)];
        completion(err ? YES : NO);
    }];
}

+ (NSString *)groupDescription {
    return @"Tests for validating update and delete operations";
}

@end
