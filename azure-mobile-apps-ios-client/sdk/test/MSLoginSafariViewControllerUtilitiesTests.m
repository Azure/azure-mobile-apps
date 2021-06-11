// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <XCTest/XCTest.h>
#import <Security/SecRandom.h>
#import <CommonCrypto/CommonDigest.h>
#import "MSLoginSafariViewControllerUtilities.h"

@interface MSLoginSafariViewControllerUtilitiesTests : XCTestCase

@end

@implementation MSLoginSafariViewControllerUtilitiesTests

- (void)setUp {
    [super setUp];
    // Put setup code here. This method is called before the invocation of each test method in the class.
}

- (void)tearDown {
    // Put teardown code here. This method is called after the invocation of each test method in the class.
    [super tearDown];
}

- (void)testLoginURLWithParameters
{
    NSURL *loginURL = [[NSURL alloc] initWithString:@"https://example.azurewebsites.net/.auth/login"];
    NSString *provider = @"aad";
    NSString *urlScheme = @"ZumoE2ETest";
    NSDictionary *parameters = @{ @"param1":@"value1", @"param2":@"value2", @"param3":@"value3" };
    NSString *codeVerifier = @"67890";
    NSString *codeChallengeMethod = @"S256";
    
    NSURL *expectedLoginURL = [[NSURL alloc] initWithString:@"https://example.azurewebsites.net/.auth/login/aad?param2=value2&post_login_redirect_url=ZumoE2ETest%3A%2F%2Feasyauth.callback&code_challenge=4iF9Pk4SDGozcqGJDwPiMrNa1lnXH3piUBpO4gSj5m0%3D&param3=value3&code_challenge_method=S256&param1=value1"];
    
    XCTAssertEqualObjects(expectedLoginURL, [MSLoginSafariViewControllerUtilities
                         fullURLFromLoginURL:loginURL
                         provider:provider
                         urlScheme:urlScheme
                         parameters:parameters
                         codeVerifier:codeVerifier
                         codeChallengeMethod:codeChallengeMethod]);
}

- (void)testLoginURLWithoutParameters
{
    NSURL *loginURL = [[NSURL alloc] initWithString:@"https://example.azurewebsites.net/.auth/login"];
    NSString *provider = @"aad";
    NSString *urlScheme = @"ZumoE2ETest";
    NSString *codeVerifier = @"67890";
    NSString *codeChallengeMethod = @"S256";
    
    NSURL *expectedLoginURL = [[NSURL alloc] initWithString:@"https://example.azurewebsites.net/.auth/login/aad?code_challenge=4iF9Pk4SDGozcqGJDwPiMrNa1lnXH3piUBpO4gSj5m0%3D&post_login_redirect_url=ZumoE2ETest%3A%2F%2Feasyauth.callback&code_challenge_method=S256"];
    
    XCTAssertEqualObjects(expectedLoginURL, [MSLoginSafariViewControllerUtilities
                                             fullURLFromLoginURL:loginURL
                                             provider:provider
                                             urlScheme:urlScheme
                                             parameters:nil
                                             codeVerifier:codeVerifier
                                             codeChallengeMethod:codeChallengeMethod]);

}

- (void)testValidRedirectURL
{
    NSURL *customSchemeURL = [[NSURL alloc] initWithString:@"ZumoE2ETest://easyauth.callback"];
    XCTAssertTrue([MSLoginSafariViewControllerUtilities isRedirectURLValid:customSchemeURL withUrlScheme:@"ZumoE2ETest"]);
    
    customSchemeURL = [[NSURL alloc] initWithString:@"ZumoE2ETest://easyauth.callback/#authorization_code=12345"];
    XCTAssertTrue([MSLoginSafariViewControllerUtilities isRedirectURLValid:customSchemeURL withUrlScheme:@"ZumoE2ETest"]);

    customSchemeURL = [[NSURL alloc] initWithString:@"com.example.ZumoE2ETest://easyauth.callback/#authorization_code=12345"];
    XCTAssertTrue([MSLoginSafariViewControllerUtilities isRedirectURLValid:customSchemeURL withUrlScheme:@"com.example.ZumoE2ETest"]);
}

- (void)testInvalidRedirectURL
{
    NSURL *nilURL = nil;
    XCTAssertFalse([MSLoginSafariViewControllerUtilities isRedirectURLValid:nilURL withUrlScheme:@"ZumoE2ETest"]);
    
    NSURL *malformedURL = [[NSURL alloc] initWithString:@"ZumoE2ETest:///easyauth.callback/#authorization_code=12345"];
    XCTAssertFalse([MSLoginSafariViewControllerUtilities isRedirectURLValid:malformedURL withUrlScheme:@"ZumoE2ETest"]);
    
    NSURL *incorrectRedirectURL = [[NSURL alloc] initWithString:@"ZumoE2ETest://other.callback/#authorization_code=12345"];
    XCTAssertFalse([MSLoginSafariViewControllerUtilities isRedirectURLValid:incorrectRedirectURL withUrlScheme:@"ZumoE2ETest"]);
    
    NSURL *incorrectSchemeURL = [[NSURL alloc] initWithString:@"forbar://easyauth.callback/#authorization_code=12345"];
    XCTAssertFalse([MSLoginSafariViewControllerUtilities isRedirectURLValid:incorrectSchemeURL withUrlScheme:@"ZumoE2ETest"]);

    NSURL *invalidSchemeURL = [[NSURL alloc] initWithString:@"ZumoE2ETest_12345://easyauth.callback/#authorization_code=12345"];
    XCTAssertFalse([MSLoginSafariViewControllerUtilities isRedirectURLValid:invalidSchemeURL withUrlScheme:@"ZumoE2ETest_12345"]);    
}

- (void)testAuthorizationCodeFromRedirectURL
{
    NSURL *customSchemeURL = [[NSURL alloc] initWithString:@"ZumoE2ETest://easyauth.callback/#authorization_code=12345"];
    XCTAssertEqualObjects(@"12345", [MSLoginSafariViewControllerUtilities authorizationCodeFromRedirectURL:customSchemeURL]);
}

- (void)testAuthorizationCodeNotFoundFromRedirectURL
{
    NSURL *nilURL = nil;
    XCTAssertNil([MSLoginSafariViewControllerUtilities authorizationCodeFromRedirectURL:nilURL]);
    
    NSURL *URLWithoutCode = [[NSURL alloc] initWithString:@"ZumoE2ETest://easyauth.callback/"];
    XCTAssertNil([MSLoginSafariViewControllerUtilities authorizationCodeFromRedirectURL:URLWithoutCode]);
}

- (void)testCodeExchangeURL
{
    NSURL *applicationURL = [[NSURL alloc] initWithString:@"https://example.azurewebsites.net"];
    NSString *provider = @"aad";
    NSString *authorizationCode = @"12345";
    NSString *codeVerifier = @"67890";
    
    NSURL *expectedCodeExchangeURL = [[NSURL alloc] initWithString:@"https://example.azurewebsites.net/.auth/login/aad/token?authorization_code=12345&code_verifier=67890"];
    
    XCTAssertEqualObjects(expectedCodeExchangeURL, [MSLoginSafariViewControllerUtilities
                           codeExchangeURLFromApplicationURL:applicationURL
                           provider:provider
                           authorizationCode:authorizationCode
                           codeVerifier:codeVerifier]);
}

- (void)testValidURLScheme
{
    NSString *urlScheme = @"ZumoE2ETest";
    XCTAssertTrue([MSLoginSafariViewControllerUtilities isValidURLScheme:urlScheme]);
    
    urlScheme = @"com.example.ZumoE2ETest";
    XCTAssertTrue([MSLoginSafariViewControllerUtilities isValidURLScheme:urlScheme]);
}

- (void)testInvalidURLScheme
{
    NSString *nilURLScheme = nil;
    XCTAssertFalse([MSLoginSafariViewControllerUtilities isValidURLScheme:nilURLScheme]);
    
    NSString *invalidURLScheme = @"ZumoE2ETest_12345";
    XCTAssertFalse([MSLoginSafariViewControllerUtilities isValidURLScheme:invalidURLScheme]);
    
    invalidURLScheme = @"12345.ZumoE2ETest";
    XCTAssertFalse([MSLoginSafariViewControllerUtilities isValidURLScheme:invalidURLScheme]);
    
    invalidURLScheme = @"12345-ZumoE2ETest";
    XCTAssertFalse([MSLoginSafariViewControllerUtilities isValidURLScheme:invalidURLScheme]);
}

- (void)testGenerateCodeVerifier
{
    NSString *output = [MSLoginSafariViewControllerUtilities generateCodeVerifier];
    
    XCTAssertNotNil(output);
    XCTAssertGreaterThan(output.length, 0);
}

- (void)testSha256EncryptWithString
{
    NSString *inputString = @"abcd";
    NSData *inputData = [inputString dataUsingEncoding:NSASCIIStringEncoding];
    NSMutableData *expectedData = [NSMutableData dataWithLength:32];
    CC_SHA256(inputData.bytes, (CC_LONG)inputData.length, expectedData.mutableBytes);
    
    NSString *expectedString = [expectedData base64EncodedStringWithOptions:0];
    
    NSString *actualString = [MSLoginSafariViewControllerUtilities sha256Base64EncodeWithString:inputString];
    
    XCTAssertEqualObjects(expectedString, actualString);
}

- (void)testSha256EncryptWithEmptyString
{
    NSString *inputString = @"";
    NSData *inputData = [inputString dataUsingEncoding:NSASCIIStringEncoding];
    NSMutableData *expectedData = [NSMutableData dataWithLength:32];
    CC_SHA256(inputData.bytes, (CC_LONG)inputData.length, expectedData.mutableBytes);
    
    NSString *expectedString = [expectedData base64EncodedStringWithOptions:0];
    
    NSString *actualString = [MSLoginSafariViewControllerUtilities sha256Base64EncodeWithString:inputString];
    
    XCTAssertEqualObjects(expectedString, actualString);
}

- (void)testSha256EncryptWithData
{
    NSString *inputString = @"abcd";
    NSData *inputData = [inputString dataUsingEncoding:NSASCIIStringEncoding];
    NSMutableData *expectedData = [NSMutableData dataWithLength:32];
    CC_SHA256(inputData.bytes, (CC_LONG)inputData.length, expectedData.mutableBytes);
    
    NSString *expectedString = [expectedData base64EncodedStringWithOptions:0];
    
    NSString *actualString = [MSLoginSafariViewControllerUtilities sha256Base64EncodeWithData:inputData];
    
    XCTAssertEqualObjects(expectedString, actualString);
}

- (void)testSha256EncryptWithEmptyData
{
    NSString *inputString = @"";
    NSData *inputData = [inputString dataUsingEncoding:NSASCIIStringEncoding];
    NSMutableData *expectedData = [NSMutableData dataWithLength:32];
    CC_SHA256(inputData.bytes, (CC_LONG)inputData.length, expectedData.mutableBytes);
    
    NSString *expectedString = [expectedData base64EncodedStringWithOptions:0];
    
    NSString *actualString = [MSLoginSafariViewControllerUtilities sha256Base64EncodeWithData:inputData];
    
    XCTAssertEqualObjects(expectedString, actualString);
}

- (void)testNormalizeProvider
{
    XCTAssertEqualObjects(@"aad", [MSLoginSafariViewControllerUtilities normalizeProvider:@"windowsazureactivedirectory"]);
    
    XCTAssertEqualObjects(@"AAD", [MSLoginSafariViewControllerUtilities normalizeProvider:@"AAD"]);
}

@end
