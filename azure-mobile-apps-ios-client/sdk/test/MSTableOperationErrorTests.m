// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#if TARGET_OS_IPHONE
#import <UIKit/UIKit.h>
#endif
#import <XCTest/XCTest.h>

#import "MSClient.h"
#import "MSCoreDataStore.h"
#import "MSCoreDataStore+TestHelper.h"
#import "MSJSONSerializer.h"
#import "MSOfflinePassthroughHelper.h"
#import "MSSyncContext.h"
#import "MSTableOperationInternal.h"
#import "MSTableOperationError.h"
#import "TodoItem.h"

@interface MSTableOperationErrorTests : XCTestCase {
    MSClient *client;
    BOOL done;
    MSOfflinePassthroughHelper *offline;
    NSManagedObjectContext *context;
}
@end


#pragma mark * Setup and TearDown

@implementation MSTableOperationErrorTests

-(void) setUp
{
    NSLog(@"%@ setUp", self.name);
    
    client = [MSClient clientWithApplicationURLString:@"https://someUrl/"];
    context = [MSCoreDataStore inMemoryManagedObjectContext];
    offline = [[MSOfflinePassthroughHelper alloc] initWithManagedObjectContext:context];
    
    // Enable offline mode
    client.syncContext = [[MSSyncContext alloc] initWithDelegate:offline dataSource:offline callback:nil];
    
    done = NO;
}

-(void) tearDown
{
    NSLog(@"%@ tearDown", self.name);
}

-(void) testBasicInit {
    MSTableOperationError *opError = [self createErrorAndPendingOpForDefaultItem];

    
    XCTAssertNotNil(opError);
    
    XCTAssertEqualObjects(opError.itemId, @"ABC");
    XCTAssertEqualObjects(opError.table, @"TodoItem");
    XCTAssertEqual(opError.code, MSErrorPreconditionFailed);
    XCTAssertEqualObjects(opError.domain, MSErrorDomain);
    XCTAssertEqualObjects(opError.description, @"Fake error...");
}

-(void) testSerializedInit {
    MSJSONSerializer *serializer = [MSJSONSerializer new];
    NSDictionary *details = @{
        @"id": @"1-2-3",
        @"code": @MSErrorPreconditionFailed,
        @"domain": MSErrorDomain,
        @"description": @"Insert error...",
        @"table": @"TodoItem",
        @"operation": [NSNumber numberWithInteger:MSTableOperationInsert],
        @"itemId": @"ABC",
        @"item": @{ @"id":@"ABC", @"text":@"one", @"complete":@NO },
        @"serverItem": @{ @"id":@"ABC", @"text":@"two", @"complete":@YES },
        @"statusCode": @312
    };
    NSData *data = [serializer dataFromItem:details idAllowed:YES ensureDictionary:NO removeSystemProperties:NO orError:nil];
    
    NSDictionary *serializedError = @{
        @"id": @"1-2-3",
        @"properties": data
    };
    
    MSTableOperationError *opError = [[MSTableOperationError alloc] initWithSerializedItem:serializedError
                                                                                   context:client.syncContext];
    
    XCTAssertNotNil(opError);
    XCTAssertEqualObjects(opError.itemId, @"ABC");
    XCTAssertEqualObjects(opError.table, @"TodoItem");
    XCTAssertEqual(opError.code, MSErrorPreconditionFailed);
    XCTAssertEqualObjects(opError.domain, MSErrorDomain);
    XCTAssertEqualObjects(opError.description, @"Insert error...");
}

-(void) testCancelAndDiscard {
    MSTableOperationError *opError = [self createErrorAndPendingOpForDefaultItem];

    // Cancel our operation now
    XCTestExpectation *cancelExpectation = [self expectationWithDescription:@"CancelAndDiscard"];
    [opError cancelOperationAndDiscardItemWithCompletion:^(NSError *error) {
        XCTAssertNil(error);
        
        [cancelExpectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    // Verify item is no longer in the local store layer
    NSError *error;
    NSFetchRequest *fetchRequest = [NSFetchRequest fetchRequestWithEntityName:@"TodoItem"];
    NSArray *allTodos = [context executeFetchRequest:fetchRequest error:&error];
    XCTAssertNil(error);
    XCTAssertEqual(allTodos.count, 0);
    
    // Verify operation is not in the local store layer
    fetchRequest = [NSFetchRequest fetchRequestWithEntityName:offline.operationTableName];
    NSArray *allOps = [context executeFetchRequest:fetchRequest error:&error];
    XCTAssertNil(error);
    XCTAssertEqual(allOps.count, 0);
}

-(void) testCancelAndUpdate {
    MSTableOperationError *opError = [self createErrorAndPendingOpForDefaultItem];
    
    // Cancel our pending operation and update the stored value
    XCTestExpectation *expectation = [self expectationWithDescription:@"CancelAndUpdateOperation"];
    NSDictionary *newItem = @{ @"id": @"ABC", @"text": @"value two" };
    [opError cancelOperationAndUpdateItem:newItem completion:^(NSError *error) {
        XCTAssertNil(error);
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    // Verify item is updated in the local store layer
    NSError *error;
    NSFetchRequest *fetchRequest = [NSFetchRequest fetchRequestWithEntityName:@"TodoItem"];
    NSArray *allTodos = [context executeFetchRequest:fetchRequest error:&error];
    XCTAssertNil(error);
    
    XCTAssertEqual(allTodos.count, 1);
    TodoItem *updatedItem = allTodos[0];
    XCTAssertEqualObjects(updatedItem.text, @"value two");
    
    // Verify operation is not in the local store layer
    fetchRequest = [NSFetchRequest fetchRequestWithEntityName:offline.operationTableName];
    NSArray *allOps = [context executeFetchRequest:fetchRequest error:&error];
    XCTAssertNil(error);
    XCTAssertEqual(allOps.count, 0);
    
}

-(void) testCancelAndUpdate_NoItem {
    MSTableOperationError *opError = [self createErrorAndPendingOpForDefaultItem];
    
    // Cancel our pending operation and update the stored value
    XCTestExpectation *expectation = [self expectationWithDescription:@"CancelAndUpdateOperation"];
    
    #pragma clang diagnostic push
    #pragma clang diagnostic ignored "-Wnonnull"

    [opError cancelOperationAndUpdateItem:nil completion:^(NSError *error) {
        XCTAssertNotNil(error);
        [expectation fulfill];
    }];

    #pragma clang diagnostic pop

    [self waitForExpectationsWithTimeout:1.0 handler:nil];
    
    // Verify operation is still in the local store layer
    MSTableOperation *op = [self getOperationFromStore];
    XCTAssertNotNil(op);
}

-(void) testKeepOperationAndUpdateItem_Insert {
    MSTableOperationError *opError = [self createErrorAndPendingOpForDefaultItemOfType:MSTableOperationInsert];
    
    // keep our pending operation and update the stored value
    NSDictionary *newItem = @{ @"id": @"ABC", @"text": @"value two" };
    
    XCTestExpectation *expectation = [self expectationWithDescription:@"KeepOperationAndUpdate"];
    [opError keepOperationAndUpdateItem:newItem completion:^(NSError *error) {
        XCTAssertNil(error);
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1000 handler:nil];
    
    // Check that the item is updated
    NSDictionary *item = [self defaultItemFromStore];
    XCTAssertNotNil(item);
    XCTAssertEqualObjects(@"value two", item[@"text"]);
    
    // Check that the operation is still in the queue
    MSTableOperation *op = [self getOperationFromStore];
    
    XCTAssertNotNil(op);
    XCTAssertEqual(op.type, MSTableOperationInsert);
    XCTAssertNil(op.item);
}

-(void) testKeepOperationAndUpdateItem_Update {
    MSTableOperationError *opError = [self createErrorAndPendingOpForDefaultItemOfType:MSTableOperationUpdate];

    // keep our pending operation and update the stored value
    NSDictionary *newItem = @{ @"id": @"ABC", @"text": @"value two" };
    
    XCTestExpectation *expectation = [self expectationWithDescription:@"KeepOperationAndUpdate"];
    [opError keepOperationAndUpdateItem:newItem completion:^(NSError *error) {
        XCTAssertNil(error);
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1000 handler:nil];

    // Check that the item is updated
    // Check that the item is updated
    NSDictionary *item = [self defaultItemFromStore];
    XCTAssertNotNil(item);
    XCTAssertEqualObjects(@"value two", item[@"text"]);

    // Check that the operation is still in the queue
    MSTableOperation *op = [self getOperationFromStore];
    XCTAssertNotNil(op);
    XCTAssertEqual(op.type, MSTableOperationUpdate);
    XCTAssertNil(op.item);
}

-(void) testKeepOperationAndUpdateItem_Delete {
    MSTableOperationError *opError = [self createErrorAndPendingOpForDefaultItemOfType:MSTableOperationDelete];
    
    // keep our pending operation and update the stored value
    NSDictionary *newItem = @{ @"id": @"ABC", @"text": @"value two" };
    
    XCTestExpectation *expectation = [self expectationWithDescription:@"KeepOperationAndUpdate"];
    [opError keepOperationAndUpdateItem:newItem completion:^(NSError *error) {
        XCTAssertNil(error);
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1000 handler:nil];
    
    // Check that the item is not present
    NSDictionary *item = [self defaultItemFromStore];
    XCTAssertNil(item);
    
    // Check that the operation is still in the queue
    MSTableOperation *op = [self getOperationFromStore];
    XCTAssertNotNil(op);
    XCTAssertNotNil(op.item);
    XCTAssertEqualObjects(@"value two", op.item[@"text"]);
    XCTAssertEqual(op.type, MSTableOperationDelete);
}

-(void) testModifyOperationType_InsertToUpdate
{
    MSTableOperationError *opError = [self createErrorAndPendingOpForDefaultItemOfType:MSTableOperationInsert];
    
    XCTestExpectation *expectation = [self expectationWithDescription:@"ModifyOperation"];
    
    [opError modifyOperationType:MSTableOperationUpdate completion:^(NSError *error) {
        XCTAssertNil(error);
        [expectation fulfill];
    }];
    
    [self waitForExpectationsWithTimeout:1000 handler:nil];
    
    // Check that the operation is still in the queue
    MSTableOperation *op = [self getOperationFromStore];
    XCTAssertNotNil(op);
    XCTAssertEqual(MSTableOperationUpdate, op.type);
    
    // Check that the item is still there
    NSDictionary *item = [self defaultItemFromStore];
    XCTAssertNotNil(item);
    XCTAssertEqualObjects(@"initial value", item[@"text"]);
}

-(void) testModifyOperationType_UpdateToUpdate
{
    MSTableOperationError *opError = [self createErrorAndPendingOpForDefaultItemOfType:MSTableOperationUpdate];
    
    XCTestExpectation *expectation = [self expectationWithDescription:@"ModifyOperation"];
    
    [opError modifyOperationType:MSTableOperationUpdate completion:^(NSError *error) {
        XCTAssertNil(error);
        [expectation fulfill];
    }];
    
    [self waitForExpectationsWithTimeout:1000 handler:nil];
    
    // Check that the operation is still in the queue
    MSTableOperation *op = [self getOperationFromStore];
    XCTAssertNotNil(op);
    XCTAssertEqual(MSTableOperationUpdate, op.type);
    
    // Check that the item is still there
    NSDictionary *item = [self defaultItemFromStore];
    XCTAssertNotNil(item);
    XCTAssertEqualObjects(@"initial value", item[@"text"]);
}

-(void) testModifyOperationType_UpdateToDelete
{
    MSTableOperationError *opError = [self createErrorAndPendingOpForDefaultItemOfType:MSTableOperationUpdate];
    
    XCTestExpectation *expectation = [self expectationWithDescription:@"ModifyOperation"];
    
    [opError modifyOperationType:MSTableOperationDelete completion:^(NSError *error) {
        XCTAssertNil(error);
        [expectation fulfill];
    }];
    
    [self waitForExpectationsWithTimeout:1000 handler:nil];
    
    // Check that the operation is still in the queue
    MSTableOperation *op = [self getOperationFromStore];
    XCTAssertNotNil(op);
    XCTAssertEqual(MSTableOperationDelete, op.type);
    XCTAssertNotNil(op.item);

    // Check that the item is gone
    NSDictionary *item = [self defaultItemFromStore];
    XCTAssertNil(item);
}

-(void) testModifyOperationType_DeleteToUpdate
{
    MSTableOperationError *opError = [self createErrorAndPendingOpForDefaultItemOfType:MSTableOperationDelete];
    
    XCTestExpectation *expectation = [self expectationWithDescription:@"ModifyOperation"];
    
    [opError modifyOperationType:MSTableOperationUpdate completion:^(NSError *error) {
        XCTAssertNil(error);
        [expectation fulfill];
    }];
    
    [self waitForExpectationsWithTimeout:1000 handler:nil];
    
    // Check the item was put back
    NSDictionary *item = [self defaultItemFromStore];
    XCTAssertNotNil(item);
    
    // Check that the operation is still in the queue
    MSTableOperation *op = [self getOperationFromStore];
    XCTAssertNotNil(op);
    XCTAssertEqual(MSTableOperationUpdate, op.type);
}

-(void) testModifyOperationAndItem_InsertToUpdate
{
    MSTableOperationError *opError = [self createErrorAndPendingOpForDefaultItemOfType:MSTableOperationInsert];
    
    // keep our pending operation and update the stored value
    NSDictionary *newItem = @{ @"id": @"ABC", @"text": @"value two" };
    
    XCTestExpectation *expectation = [self expectationWithDescription:@"ModifyOperation"];
    [opError modifyOperationType:MSTableOperationUpdate AndUpdateItem:newItem completion:^(NSError *error) {
        XCTAssertNil(error);
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1000 handler:nil];
    
    // Check that the item is updated
    NSDictionary *item = [self defaultItemFromStore];
    XCTAssertNotNil(item);
    XCTAssertEqualObjects(@"value two", item[@"text"]);
    
    // Check that the operation is still in the queue
    MSTableOperation *op = [self getOperationFromStore];
    XCTAssertNotNil(op);
    XCTAssertEqual(op.type, MSTableOperationUpdate);
}

-(void) testModifyOperationAndItem_UpdateToDelete
{
    MSTableOperationError *opError = [self createErrorAndPendingOpForDefaultItemOfType:MSTableOperationUpdate];
    
    // keep our pending operation and update the stored value
    NSDictionary *newItem = @{ @"id": @"ABC", @"text": @"value two" };
    
    XCTestExpectation *expectation = [self expectationWithDescription:@"ModifyOperation"];
    [opError modifyOperationType:MSTableOperationDelete AndUpdateItem:newItem completion:^(NSError *error) {
        XCTAssertNil(error);
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1000 handler:nil];
    
    // Check that the item is present
    NSDictionary *item = [self defaultItemFromStore];
    XCTAssertNil(item);

    // Check that the operation is still in the queue
    MSTableOperation *op = [self getOperationFromStore];
    XCTAssertNotNil(op);
    XCTAssertEqual(op.type, MSTableOperationDelete);
    XCTAssertNotNil(op.item);
    XCTAssertEqualObjects(@"value two", op.item[@"text"]);
}

-(void) testModifyOperationAndItem_DeleteToInsert
{
    MSTableOperationError *opError = [self createErrorAndPendingOpForDefaultItemOfType:MSTableOperationDelete];
    
    // keep our pending operation and update the stored value
    NSDictionary *newItem = @{ @"id": @"ABC", @"text": @"value two" };
    
    XCTestExpectation *expectation = [self expectationWithDescription:@"ModifyOperation"];
    [opError modifyOperationType:MSTableOperationInsert AndUpdateItem:newItem completion:^(NSError *error) {
        XCTAssertNil(error);
        [expectation fulfill];
    }];
    [self waitForExpectationsWithTimeout:1000 handler:nil];
    
    // Check that the item is present
    NSDictionary *item = [self defaultItemFromStore];
    XCTAssertNotNil(item);
    
    // Check that the operation is still in the queue
    MSTableOperation *op = [self getOperationFromStore];
    XCTAssertNotNil(op);
    XCTAssertEqual(op.type, MSTableOperationInsert);
}

- (MSTableOperationError *) createErrorAndPendingOpForDefaultItem
{
    return [self createErrorAndPendingOpForDefaultItemOfType:MSTableOperationUpdate];
}

- (MSTableOperationError *) createErrorAndPendingOpForDefaultItemOfType:(MSTableOperationTypes)type
{
    NSDictionary *item = @{ @"id": @"ABC", @"text": @"initial value" };
    
    MSTableOperation *tableOp = [self createPendingOperationForItem:item ofType:type];
    
    NSError *error = [NSError errorWithDomain:MSErrorDomain
                                         code:MSErrorPreconditionFailed
                                     userInfo:@{NSLocalizedDescriptionKey: @"Fake error..."}];
    
    return [[MSTableOperationError alloc] initWithOperation:tableOp
                                                       item:item
                                                    context:client.syncContext
                                                      error:error];
}

- (MSTableOperation *) createPendingOperationForItem:(NSDictionary *)item ofType:(MSTableOperationTypes)type
{
    MSSyncTable *table = [client syncTableWithName:@"TodoItem"];
    
    XCTestExpectation *expectation = [self expectationWithDescription:@"UpsertRecord"];
    
    if (type == MSTableOperationInsert) {
        [table insert:item completion:^(NSDictionary *insertedItem, NSError *error) {
            XCTAssertNil(error);
            [expectation fulfill];
        }];
    } else if (type == MSTableOperationUpdate) {
        [table update:item completion:^(NSError *error) {
            XCTAssertNil(error);
            [expectation fulfill];
        }];
    } else {
        [table delete:item completion:^(NSError *error) {
            XCTAssertNil(error);
            [expectation fulfill];
        }];
    }
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];

    // To be safe, verify operation is in the local store layer
    NSError *error;
    NSFetchRequest *fetchRequest = [NSFetchRequest fetchRequestWithEntityName:offline.operationTableName];
    NSArray *allOps = [context executeFetchRequest:fetchRequest error:&error];
    XCTAssertNil(error);
    XCTAssertEqual(allOps.count, 1);
    
    // Create a op that matches what we just did & fake an error
    MSTableOperation *tableOp = [[MSTableOperation alloc] initWithTable:@"TodoItem" type:type itemId:@"ABC"];
    tableOp.operationId = [[allOps[0] valueForKey:@"id"] integerValue];

    return tableOp;
}

- (MSTableOperation *) getOperationFromStore
{
    // Check that the operation is still in the queue
    NSError *error;
    NSFetchRequest *fetchRequest = [NSFetchRequest fetchRequestWithEntityName:offline.operationTableName];
    NSArray *allOps = [context executeFetchRequest:fetchRequest error:&error];
    
    XCTAssertNil(error);
    XCTAssertEqual(allOps.count, 1);
    
    NSDictionary *operation = [offline tableItemFromManagedObject:allOps[0]];
    
    return [[MSTableOperation alloc] initWithItem:operation];
}

-(NSDictionary *) defaultItemFromStore
{
    NSError *error;
    NSFetchRequest *fetchRequest = [NSFetchRequest fetchRequestWithEntityName:@"TodoItem"];
    fetchRequest.predicate = [NSPredicate predicateWithFormat:@"(id == 'ABC')"];
     
    NSArray *todosWithIdABC = [context executeFetchRequest:fetchRequest error:&error];
    XCTAssertNil(error);
    XCTAssertTrue(todosWithIdABC.count < 2);
    
    if (todosWithIdABC.count > 0) {
        return [offline tableItemFromManagedObject:todosWithIdABC[0]];
    }
    
    return nil;
}

@end
