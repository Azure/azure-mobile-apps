// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <XCTest/XCTest.h>
#import "MSClient.h"
#import "MSJsonSerializer.h"
#import "MSLogin.h"
#import "MSTestFilter.h"
#import "MSUser.h"

@interface MSLoginTests : XCTestCase {
}

@end


@implementation MSLoginTests

#pragma mark * Setup and TearDown

- (void)setUp {
    NSLog(@"%@ setUp", self.name);
}

- (void)tearDown {
    NSLog(@"%@ tearDown", self.name);
}

#pragma mark * Refresh User Tests

- (void)testRefreshUserWhenCompletionIsNil
{
    MSClient *client = [MSClient clientWithApplicationURLString:@"http://someURL.com/"];
    
    // Invoke the API
    [client refreshUserWithCompletion:nil];
}

- (void)testRefreshUser
{
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];

    MSClient *client = [MSClient clientWithApplicationURLString:@"http://someURL.com/"];
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200];
    
    testFilter.onInspectResponseData = ^(NSURLRequest *request, NSData *data) {
        NSDictionary *item = @{ @"user" : @{ @"userId" : @"sid:12345678" }, @"authenticationToken" : @"token12345678" };
        return [[MSJSONSerializer JSONSerializer] dataFromItem:item idAllowed:YES ensureDictionary:NO removeSystemProperties:YES orError:nil];
    };
    
    MSClient *filterClient = [client clientWithFilter:testFilter];

    // Invoke the API
    [filterClient refreshUserWithCompletion:
     ^(MSUser *user, NSError *error) {
         XCTAssertNil(error);
         XCTAssertNotNil(user);
         XCTAssertEqualObjects(user.mobileServiceAuthenticationToken, @"token12345678");
         
         [expectation fulfill];
     }];
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

- (void)testRefreshUserWhenLoginHostIsOverriden
{
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];
    
    MSClient *client = [MSClient clientWithApplicationURLString:@"http://someURL.com/"];
    [client setLoginHost:[NSURL URLWithString:@"https://anotherURL.com"]];
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200];
    
    testFilter.onInspectResponseData = ^(NSURLRequest *request, NSData *data) {
        NSDictionary *item = @{ @"user" : @{ @"userId" : @"sid:12345678" }, @"authenticationToken" : @"token12345678" };
        return [[MSJSONSerializer JSONSerializer] dataFromItem:item idAllowed:YES ensureDictionary:NO removeSystemProperties:YES orError:nil];
    };
    
    MSClient *filterClient = [client clientWithFilter:testFilter];
    [filterClient setLoginHost:[NSURL URLWithString:@"https://anotherURL.com"]];
    
    // Invoke the API
    [filterClient refreshUserWithCompletion:
     ^(MSUser *user, NSError *error) {
         XCTAssertNil(error);
         XCTAssertNotNil(user);
         XCTAssertEqualObjects(user.mobileServiceAuthenticationToken, @"token12345678");
         
         [expectation fulfill];
     }];
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

- (void)testRefreshUserWhenResponseContainsNoAuthenticationToken
{
    NSDictionary *item = @{ @"user" : @{ @"userId" : @"sid:12345678" } };
    [self refreshUserWithResponseData:item];
}

- (void)testRefreshUserWhenResponseContainsNoUser
{
    NSDictionary *item = @{ @"authenticationToken" : @"token12345678" };
    [self refreshUserWithResponseData:item];
}

- (void)refreshUserWithResponseData:(NSDictionary *)item
{
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];
    
    MSClient *client = [MSClient clientWithApplicationURLString:@"http://someURL.com/"];
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200];
    
    testFilter.onInspectResponseData = ^(NSURLRequest *request, NSData *data) {
        return [[MSJSONSerializer JSONSerializer] dataFromItem:item idAllowed:YES ensureDictionary:NO removeSystemProperties:YES orError:nil];
    };
    
    MSClient *filterClient = [client clientWithFilter:testFilter];
    
    // Invoke the API
    [filterClient refreshUserWithCompletion:
     ^(MSUser *user, NSError *error) {
         XCTAssertNil(user);
         XCTAssertNotNil(error, @"error should not have been nil.");
         XCTAssertTrue([[error localizedDescription] isEqualToString:
                        @"The token in the login response was invalid. The token must be a JSON object with both a userId and an authenticationToken."],
                       @"error description was: %@", [error localizedDescription]);
         
         [expectation fulfill];
     }];
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

- (void)testRefreshUser400Error
{
    [self refreshUserWithStatusCode:400 withErrorCode:MSRefreshBadRequest];
}

- (void)testRefreshUser401Error
{
    [self refreshUserWithStatusCode:401 withErrorCode:MSRefreshUnauthorized];
}

- (void)testRefreshUser403Error
{
    [self refreshUserWithStatusCode:403 withErrorCode:MSRefreshForbidden];
}

- (void)testRefreshUser500Error
{
    [self refreshUserWithStatusCode:500 withErrorCode:MSRefreshUnexpectedError];
}

- (void)refreshUserWithStatusCode:(int)statusCode withErrorCode:(NSInteger)errorCode
{
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];

    MSClient *client = [MSClient clientWithApplicationURLString:@"http://someURL.com"];
    
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:statusCode];
    
    MSClient *filterClient = [client clientWithFilter:testFilter];
    
    // Invoke the API
    [filterClient refreshUserWithCompletion:
     ^(MSUser *user, NSError *error) {
         XCTAssertNil(user);
         XCTAssertNotNil(error);
         XCTAssertEqual(error.code, errorCode);
         
         [expectation fulfill];
     }];
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

@end
