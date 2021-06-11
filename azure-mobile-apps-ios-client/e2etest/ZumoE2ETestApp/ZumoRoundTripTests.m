﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoRoundTripTests.h"
#import "ZumoTest.h"
#import "ZumoTestGlobals.h"

@implementation ZumoRoundTripTests

static NSString *tableName = @"intIdRoundTripTable";
static NSString *roundTripTable = @"roundTripTable";

typedef enum { RTTString, RTTDouble, RTTBool, RTTInt, RTT8ByteLong, RTTDate } RoundTripTestColumnType;

+ (NSArray *)createTests {
    NSMutableArray *result = [[NSMutableArray alloc] init];

    NSUInteger startOfIntIdTests = [result count];
    [result addObject:[ZumoTest createTestWithName:@"[DynamicSchema] Setup dynamic schema" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client tableWithName:tableName];
        NSDictionary *item = @{@"string1":@"test", @"date1": [ZumoTestGlobals createDateWithYear:2011 month:11 day:11], @"bool1": [NSNumber numberWithBool:NO], @"number": [NSNumber numberWithInt:-1], @"longnum":[NSNumber numberWithLongLong:0LL], @"intnum":[NSNumber numberWithInt:0], @"setindex":@"setindex"};
        [table insert:item completion:^(NSDictionary *inserted, NSError *err) {
            if (err) {
                [test addLog:[NSString stringWithFormat:@"Error inserting data to create schema: %@", err]];
                completion(NO);
            } else {
                [test addLog:@"Inserted item to create schema"];
                completion(YES);
            }
        }];
    }]];
    
    // Negative scenarios
    [result addObject:[ZumoTest createTestWithName:@"[DynamicSchema] (Neg) New column with null value" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client tableWithName:tableName];
        [table insert:@{@"ColumnWhichDoesNotExist":[NSNull null]} completion:^(NSDictionary *item, NSError *err) {
            BOOL passed;
            if (!err) {
                [test addLog:[NSString stringWithFormat:@"Error, adding new column with null element should fail, but insert worked: %@", item]];
                passed = NO;
            } else {
                if (err.code == MSErrorMessageErrorCode) {
                    [test addLog:@"Test passed, got correct error"];
                    passed = YES;
                } else {
                    [test addLog:[NSString stringWithFormat:@"Expected error code %d, got %ld", MSErrorMessageErrorCode, (long)err.code]];
                    passed = NO;
                }
            }
            
            [test setTestStatus:(passed ? TSPassed : TSFailed)];
            completion(passed);
        }];
    }]];
    
    NSUInteger endOfIntIdTests = [result count];
    NSUInteger startOfStringIdTests = [result count];
    
    // Data scenarios (use string id so they run on all runtimes)
    
    [result addObject:[self createRoundTripForType:RTTString withValue:@"" andName:@"Round trip empty string"]];
    NSString *simpleString = [NSString stringWithFormat:@"%c%c%c%c%c",
                              ' ' + (rand() % 95),
                              ' ' + (rand() % 95),
                              ' ' + (rand() % 95),
                              ' ' + (rand() % 95),
                              ' ' + (rand() % 95)];
    [result addObject:[self createRoundTripForType:RTTString withValue:simpleString andName:@"Round trip simple string"]];
    [result addObject:[self createRoundTripForType:RTTString withValue:[NSNull null] andName:@"Round trip nil string"]];
    
    [result addObject:[self createRoundTripForType:RTTString withValue:[@"" stringByPaddingToLength:1000 withString:@"*" startingAtIndex:0] andName:@"Round trip large (1000) string"]];
    [result addObject:[self createRoundTripForType:RTTString withValue:[@"" stringByPaddingToLength:65537 withString:@"*" startingAtIndex:0] andName:@"Round trip large (64k+) string"]];

    [result addObject:[self createRoundTripForType:RTTString withValue:@"ãéìôü ÇñÑ" andName:@"String with non-ASCII characters - Latin"]];
    [result addObject:[self createRoundTripForType:RTTString withValue:@"الكتاب على الطاولة" andName:@"String with non-ASCII characters - Arabic"]];
    [result addObject:[self createRoundTripForType:RTTString withValue:@"这本书在桌子上" andName:@"String with non-ASCII characters - Chinese"]];
    [result addObject:[self createRoundTripForType:RTTString withValue:@"⒈①Ⅻㄨㄩ 啊阿鼾齄 丂丄狚狛 狜狝﨨﨩 ˊˋ˙–〇 㐀㐁䶴䶵" andName:@"String with non-ASCII characters - Chinese 2"]];
    [result addObject:[self createRoundTripForType:RTTString withValue:@"本は机の上に" andName:@"String with non-ASCII characters - Japanese"]];
    [result addObject:[self createRoundTripForType:RTTString withValue:@"הספר הוא על השולחן" andName:@"String with non-ASCII characters - Hebrew"]];

    // Date scenarios
    [result addObject:[self createRoundTripForType:RTTDate withValue:[NSDate date] andName:@"Round trip current date"]];
    [result addObject:[self createRoundTripForType:RTTDate withValue:[ZumoTestGlobals createDateWithYear:2012 month:12 day:12] andName:@"Round trip specific date"]];
    [result addObject:[self createRoundTripForType:RTTDate withValue:[ZumoTestGlobals createDateWithYear:1970 month:1 day:1] andName:@"Round trip unix zero date"]];
    [result addObject:[self createRoundTripForType:RTTDate withValue:[NSDate dateWithTimeIntervalSince1970:-1] andName:@"Round trip before unix zero date"]];
    [result addObject:[self createRoundTripForType:RTTDate withValue:[NSNull null] andName:@"Round trip null date"]];
    
    // Bool scenarios
    [result addObject:[self createRoundTripForType:RTTBool withValue:[NSNumber numberWithBool:YES] andName:@"Round trip (BOOL)YES"]];
    [result addObject:[self createRoundTripForType:RTTBool withValue:[NSNumber numberWithBool:NO] andName:@"Round trip (BOOL)NO"]];
    
    // Number scenarios
    [result addObject:[self createRoundTripForType:RTTInt withValue:[NSNumber numberWithInt:rand()] andName:@"Round trip positive number"]];
    [result addObject:[self createRoundTripForType:RTTInt withValue:[NSNumber numberWithInt:-rand()] andName:@"Round trip negative number"]];
    [result addObject:[self createRoundTripForType:RTTInt withValue:[NSNumber numberWithInt:0] andName:@"Round trip zero"]];
    [result addObject:[self createRoundTripForType:RTTDouble withValue:[NSNumber numberWithDouble:MAXFLOAT] andName:@"Round trip MAXFLOAT"]];
    [result addObject:[self createRoundTripForType:RTT8ByteLong withValue:[NSNumber numberWithLongLong:123456789012345LL] andName:@"Round trip long long number"]];
    [result addObject:[self createRoundTripForType:RTT8ByteLong withValue:[NSNumber numberWithLongLong:-123456789012345LL] andName:@"Round trip negative long long"]];
    long long maxSupportedLong = 0x0020000000000000LL;
    long long maxSupportedNegativeLong = 0xFFE0000000000000LL;
    [result addObject:[self createRoundTripForType:RTT8ByteLong withValue:[NSNumber numberWithLongLong:maxSupportedLong] andName:@"Round trip maximum long long"]];
    [result addObject:[self createRoundTripForType:RTT8ByteLong withValue:[NSNumber numberWithLongLong:maxSupportedNegativeLong] andName:@"Round trip maximum negative long long"]];
    
    // Start of tests for tables with string ids
        
    NSDictionary *validStringIds = @{
                                @"no id": @"",
                                @"ascii": @"id",
                                @"latin": @"ãéìôü ÇñÑ",
                                @"arabic": @"الكتاب على الطاولة",
                                @"chinese": @"这本书在桌子上",
                                @"hebrew": @"הספר הוא על השולחן"
                                };
    NSDictionary *templateItem = @{
                                   @"name":@"ãéìôü ÇñÑ - الكتاب على الطاولة - 这本书在桌子上 - ⒈①Ⅻㄨㄩ 啊阿鼾齄 丂丄狚狛 狜狝﨨﨩 ˊˋ˙–〇 㐀㐁䶴䶵 - 本は机の上に - הספר הוא על השולחן",
                                   @"number":@123.456,
                                   @"integer":@12345,
                                   @"bool":@YES,
                                   @"date1":[NSDate date]
                                   };
    
    for (NSString *key in [validStringIds allKeys]) {
        NSString *testName = [@"String id - insert, id type = " stringByAppendingString:key];
        NSString *testId = [validStringIds objectForKey:key];
        NSMutableDictionary *item = [[NSMutableDictionary alloc] initWithDictionary:templateItem];
        if ([testId length] > 0) {
            NSString *uniqueId = [[NSUUID UUID] UUIDString];
            uniqueId = [@"-" stringByAppendingString:uniqueId];
            [item setValue:[testId stringByAppendingString:uniqueId] forKey:@"id"];
        }
        [result addObject:[self createStringIdRoundTripTestWithName:testName item:item]];
    }
    
    NSArray *invalidStringIds = @[@".",@"..",@"control\tcharacters",[@"large id - " stringByPaddingToLength:260 withString:@"*" startingAtIndex:0]];
    for (NSString *badId in invalidStringIds) {
        NSString *testName = [@"(Neg) String id - invalid id: " stringByAppendingString:badId];
        [result addObject:[ZumoTest createTestWithName:testName andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
            MSClient *client = [[ZumoTestGlobals sharedInstance] client];
            MSTable *table = [client tableWithName:TABLES_ROUND_TRIP_STRING_ID];
            NSDictionary *item = @{@"id":badId,@"name":@"unused"};
            [table insert:item completion:^(NSDictionary *item, NSError *error) {
                if (error) {
                    [test addLog:@"Ok, got expected error"];
                    completion(YES);
                } else {
                    [test addLog:[NSString stringWithFormat:@"Error, insert should not have succeeded. Inserted item: %@", item]];
                    completion(NO);
                }
            }];
        }]];
    }

    NSUInteger endOfStringIdTests = [result count];

    for (NSUInteger i = startOfIntIdTests; i < endOfIntIdTests; i++) {
        ZumoTest *test = [result objectAtIndex:i];
        [test addRequiredFeature:@"intIdTables"];
    }
    
    for (NSUInteger i = startOfStringIdTests; i < endOfStringIdTests; i++) {
        ZumoTest *test = [result objectAtIndex:i];
        [test addRequiredFeature:@"stringIdTables"];
    }

    return result;
}

+ (ZumoTest *)createStringIdRoundTripTestWithName:(NSString *)testName item:(NSDictionary *)item {
    return [ZumoTest createTestWithName:testName andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client tableWithName:TABLES_ROUND_TRIP_STRING_ID];
        NSString *itemId = [item objectForKey:@"id"];
        BOOL hadId = itemId != nil;
        [table insert:item completion:^(NSDictionary *inserted, NSError *error) {
            if (error) {
                [test addLog:[NSString stringWithFormat:@"Error inserting: %@", error]];
                completion(NO);
                return;
            }
            
            [test addLog:[NSString stringWithFormat:@"Inserted item: %@", inserted]];
            id newId = [inserted objectForKey:@"id"];
            if (!newId) {
                [test addLog:@"Error, inserted item does not have an 'id' property"];
                completion(NO);
                return;
            }
            
            if (![newId isKindOfClass:[NSString class]]) {
                [test addLog:@"Error, id should be a string"];
                completion(NO);
                return;
            }

            if (hadId) {
                if (![newId isEqualToString:itemId]) {
                    [test addLog:[NSString stringWithFormat:@"Error, id passed to insert (%@) is not the same as the one returned by the server (%@)", itemId, newId]];
                    completion(NO);
                    return;
                }
            }
            
            [table readWithId:newId completion:^(NSDictionary *retrieved, NSError *error) {
                if (error) {
                    [test addLog:[NSString stringWithFormat:@"Error retrieving: %@", error]];
                    completion(NO);
                    return;
                }

                [test addLog:[NSString stringWithFormat:@"Retrieved item: %@", inserted]];
                NSMutableArray *errors = [[NSMutableArray alloc] init];
                if (![ZumoTestGlobals compareObjects:inserted with:retrieved log:errors]) {
                    [test addLog:@"Error comparing objects:"];
                    for (NSString *err in errors) {
                        [test addLog:err];
                    }
                    completion(NO);
                    return;
                }

                [test addLog:@"Items compare successfully"];
                [test addLog:@"Now trying to insert an item with an existing id (should fail)"];
                NSDictionary *badItem = @{@"id":newId,@"name":@"unused"};
                [table insert:badItem completion:^(NSDictionary *item, NSError *error) {
                    if (error) {
                        [test addLog:@"Ok, got expected error"];
                        completion(YES);
                    } else {
                        [test addLog:[NSString stringWithFormat:@"Error, insert should not have succeeded. Inserted item: %@", item]];
                        completion(NO);
                    }
                }];
            }];
        }];
    }];
}

+ (ZumoTest *)createRoundTripForType:(RoundTripTestColumnType)type withValue:(id)value andName:(NSString *)testName {
    ZumoTest *result = [ZumoTest createTestWithName:testName andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client tableWithName:roundTripTable];
        NSMutableDictionary *item = [[NSMutableDictionary alloc] init];
        NSString *keyName = @"";
        if (type == RTTString) {
            keyName = @"name";
        } else if (type == RTTDouble) {
            keyName = @"number";
        } else if (type == RTTBool) {
            keyName = @"bool";
        } else if (type == RTTInt) {
            keyName = @"integer";
        } else if (type == RTT8ByteLong) {
            keyName = @"number";
        } else if (type == RTTDate) {
            keyName = @"date1";
        }

        [item setObject:value forKey:keyName];
        NSString *valueString = [self toLimitedString:value upTo:100];
        [test addLog:[NSString stringWithFormat:@"Inserting value %@, using key %@", valueString, keyName]];
        [test addLog:[NSString stringWithFormat:@"Full Item: %@", item]];
        
        [table insert:item completion:^(NSDictionary *inserted, NSError *err) {
            if (err) {
                [test addLog:[NSString stringWithFormat:@"Error inserting: %@", err]];
                [test setTestStatus:TSFailed];
                completion(NO);
            } else {
                NSString *itemId = inserted[@"id"];
                [table readWithId:itemId completion:^(NSDictionary *retrieved, NSError *err2) {
                    if (err2) {
                        [test addLog:[NSString stringWithFormat:@"Error retrieving: %@", err2]];
                        [test setTestStatus:TSFailed];
                        completion(NO);
                    } else {
                        NSString *retrievedString = [self toLimitedString:retrieved upTo:200];
                        [test addLog:[NSString stringWithFormat:@"Retrieved item: %@", retrievedString]];
                        BOOL failed = NO;
                        if (type == RTTString) {
                            NSString *rtString = [retrieved objectForKey:@"name"];
                            if ([value isKindOfClass:[NSNull class]] && [rtString isKindOfClass:[NSNull class]]) {
                                // all are null, ok
                            } else {
                                if (![rtString isEqualToString:value]) {
                                    [test addLog:[NSString stringWithFormat:@"Value is incorrect: %@ != %@", value, rtString]];
                                    failed = YES;
                                }
                            }
                        } else if (type == RTTBool) {
                            NSNumber *rtBool = [retrieved objectForKey:@"bool"];
                            if ([rtBool boolValue] != [((NSNumber *)value) boolValue]) {
                                failed = YES;
                                [test addLog:[NSString stringWithFormat:@"Value is incorrect: %@ != %@", value, rtBool]];
                            }
                        } else if (type == RTTDate) {
                            NSDate *rtDate = [retrieved objectForKey:@"date1"];
                            if (![ZumoTestGlobals compareDate:rtDate withDate:((NSDate *)value)]) {
                                failed = YES;
                                [test addLog:[NSString stringWithFormat:@"Value is incorrect: %@ != %@", value, rtDate]];
                            }
                        } else if (type == RTTDouble) {
                            NSNumber *rtNumber = [retrieved objectForKey:@"number"];
                            double dbl1 = [((NSNumber *)value) doubleValue];
                            double dbl2 = [rtNumber doubleValue];
                            double delta = fabs(dbl1 - dbl2);
                            double error = delta / dbl1;
                            if (error > 0.000000001) {
                                failed = YES;
                                [test addLog:[NSString stringWithFormat:@"Value is incorrect: %@ != %@", value, rtNumber]];
                            }
                        } else if (type == RTTInt) {
                            NSNumber *rtNumber = [retrieved objectForKey:@"integer"];
                            if (![rtNumber isEqualToNumber:value]) {
                                failed = YES;
                                [test addLog:[NSString stringWithFormat:@"Value is incorrect: %@ != %@", value, rtNumber]];
                            }
                        } else if (type == RTT8ByteLong) {
                            NSNumber *rtNumber = [retrieved objectForKey:@"number"];
                            [test addLog:[NSString stringWithFormat:@"Retrieved number: %@", rtNumber]];
                            if (![rtNumber isEqualToNumber:value]) {
                                failed = YES;
                                [test addLog:[NSString stringWithFormat:@"Value is incorrect: %@ != %@", value, rtNumber]];
                            }
                        } else {
                            failed = YES;
                            [test addLog:@"Test not implemented for this type"];
                        }
                        
                        if (failed) {
                            [test setTestStatus:TSFailed];
                            completion(NO);
                        } else {
                            if (type == RTTString && value != [NSNull null] && ((NSString *)value).length < 100) {
                                // Additional validation: query for the inserted data
                                NSString *rtString = value;
                                NSPredicate *predicate = [NSPredicate predicateWithFormat:@"id == %@ && name == %@", itemId, rtString];
                                [table readWithPredicate:predicate completion:^(MSQueryResult *result, NSError *err3) {
                                    
                                    BOOL passed = NO;
                                    if (err3) {
                                        [test addLog:[NSString stringWithFormat:@"Error retrieving data: %@", err3]];
                                    } else {
                                        if (result.items.count != 1) {
                                            [test addLog:[NSString stringWithFormat:@"Expected to receive 1 element; received %lu: %@", (unsigned long)result.items.count, result.items]];
                                        } else {
                                            NSString *readId = result.items[0][@"id"];
                                            if ([itemId isEqualToString:readId]) {
                                                [test addLog:@"Test passed"];
                                                passed = YES;
                                            } else {
                                                [test addLog:[NSString stringWithFormat:@"Received invalid item: %@", result.items]];
                                            }
                                        }
                                    }
                                    
                                    [test setTestStatus:(passed ? TSPassed : TSFailed)];
                                    completion(passed);
                                }];
                            } else {
                                [test addLog:@"Test passed"];
                                [test setTestStatus:TSPassed];
                                completion(YES);
                            }
                        }
                    }
                }];
            }
        }];
    }];
    
    return result;
}

+ (NSString *)toLimitedString:(id)value upTo:(int)maxLength {
    NSString *str = [NSString stringWithFormat:@"%@", value];
    if ([str length] > maxLength) {
        str = [NSString stringWithFormat:@"%@ ... (len = %lu)", [str substringToIndex:maxLength], (unsigned long)[str length]];
    }
    
    return str;
}

+ (NSString *)groupDescription {
    return @"Tests for validating the insertion and retrieval of different types of data";
}

@end
