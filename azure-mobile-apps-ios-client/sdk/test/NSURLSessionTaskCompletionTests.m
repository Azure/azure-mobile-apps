//
//  NSURLSessionTaskCompletionTests.m
//  MicrosoftAzureMobile
//
//  Created by Damien Pontifex on 26/08/2016.
//  Copyright © 2016 Windows Azure. All rights reserved.
//

#import <XCTest/XCTest.h>
#import "NSURLSessionTask+Completion.h"

@interface NSURLSessionTaskCompletionTests : XCTestCase
@property (strong, nonatomic) NSURLSessionTask *task;
@end

@implementation NSURLSessionTaskCompletionTests

- (void)setUp {
    [super setUp];
    
    self.task = [[NSURLSessionTask alloc] init];
}

- (void)tearDown {
    // Put teardown code here. This method is called after the invocation of each test method in the class.
    [super tearDown];
}

- (void)testAlwaysReturnsDataInstance {
    XCTAssertNotNil(self.task.data);
    // Initial construction should have empty data
    XCTAssertEqual(0, self.task.data.length);
}

- (void)testReturningData {
    NSString *testString = @"Hello world";
    NSMutableData *stringData = [[testString dataUsingEncoding:NSUTF8StringEncoding] mutableCopy];
    
    self.task.data = stringData;
    
    NSString *retrievedString = [[NSString alloc] initWithData:self.task.data encoding:NSUTF8StringEncoding];
    XCTAssertTrue([testString isEqualToString:retrievedString]);
}

- (void)testSettingCompletionBlock {
    
    XCTestExpectation *expectation = [self expectationWithDescription:@"Should call completion"];
    
    self.task.completion = ^(NSHTTPURLResponse *response, NSData *data, NSError *error) {
        [expectation fulfill];
    };
    
    self.task.completion(nil, nil, nil);
    
    // If our block was retrieved expectation should be fulfilled
    [self waitForExpectationsWithTimeout:0.1 handler:nil];
}

@end
