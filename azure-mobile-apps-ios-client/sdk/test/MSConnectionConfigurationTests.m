// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <XCTest/XCTest.h>
#import "MSConnectionConfiguration.h"

@interface MSConnectionConfigurationTests : XCTestCase

@end

@implementation MSConnectionConfigurationTests

- (void)setUp
{
	[super setUp];
	
	[MSConnectionConfiguration appConfiguration];
}

- (void)tearDown
{
	[super tearDown];
	
	[[MSConnectionConfiguration appConfiguration] revertToDefaultApiEndpoint];
	[[MSConnectionConfiguration appConfiguration] revertToDefaultTableEndpoint];
}

- (void)testUrlSettings_Defaults
{
	MSConnectionConfiguration *settings = [MSConnectionConfiguration appConfiguration];
	
	XCTAssertTrue([settings.tableEndpoint isEqualToString:@"tables"]);
	XCTAssertTrue([settings.apiEndpoint isEqualToString:@"api"]);
}

- (void)testUrlSettings_tableChanges
{
	MSConnectionConfiguration *settings = [MSConnectionConfiguration appConfiguration];
	settings.tableEndpoint = @"api";
	
	XCTAssertTrue([[MSConnectionConfiguration appConfiguration].tableEndpoint isEqualToString:@"api"]);
}

- (void)testUrlSettings_revertTable
{
	MSConnectionConfiguration *settings = [MSConnectionConfiguration appConfiguration];
	settings.tableEndpoint = @"custom";
	
	
	XCTAssertTrue([[MSConnectionConfiguration appConfiguration].tableEndpoint isEqualToString:@"custom"]);
	
	[settings revertToDefaultTableEndpoint];
	
	XCTAssertTrue([[MSConnectionConfiguration appConfiguration].tableEndpoint isEqualToString:@"tables"]);
}

- (void)testUrlSettings_apiChanges
{
	MSConnectionConfiguration *settings = [MSConnectionConfiguration appConfiguration];
	settings.apiEndpoint = @"custom";
	
	XCTAssertTrue([[MSConnectionConfiguration appConfiguration].apiEndpoint isEqualToString:@"custom"]);
}

- (void)testUrlSettings_revertApi
{
	MSConnectionConfiguration *settings = [MSConnectionConfiguration appConfiguration];
	
	settings.apiEndpoint = @"custom";
	
	XCTAssertTrue([[MSConnectionConfiguration appConfiguration].apiEndpoint isEqualToString:@"custom"]);
	
	[settings revertToDefaultApiEndpoint];
	
	XCTAssertTrue([[MSConnectionConfiguration appConfiguration].apiEndpoint isEqualToString:@"api"]);
}

@end
