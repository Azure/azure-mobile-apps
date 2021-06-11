// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <UIKit/UIKit.h>
#import <XCTest/XCTest.h>
#import <SafariServices/SafariServices.h>
#import "MSLoginSafariViewController.h"
#import "MSClient.h"
#import "MSJsonSerializer.h"
#import "MSTestFilter.h"
#import "MSUser.h"
#import "MSAuthState.h"

@interface MSLoginSafariViewControllerTests : XCTestCase

@end

@interface MSLoginSafariViewController (Test)

// Expose private properties and private methods for testing purpose

@property (nonatomic, nullable) MSAuthState *authState;

@property (nonatomic, nullable) SFSafariViewController *safariViewController;

- (void)loginInternalWithProvider:(NSString *)provider
                                    urlScheme:(NSString *)urlScheme
                                   parameters:(nullable NSDictionary *)parameters
                                   controller:(UIViewController *)controller
                                     animated:(BOOL)animated
                                   completion:(nullable MSClientLoginBlock)completion;

- (NSURL *)codeExchangeRequestURLFromRedirectURL:(NSURL *)URL;

- (void)codeExchangeWithURL:(NSURL *)URL
                 completion:(MSClientLoginBlock)completion;
@end

@implementation MSLoginSafariViewControllerTests

- (void)setUp {
    [super setUp];
}

- (void)tearDown {
    [super tearDown];
}

- (void)testSafariViewControllerLoginWithProviderWhenAnotherLoginInProgress
{
    MSClient *client = [MSClient clientWithApplicationURLString:@"https://ZumoE2ETest.example.com/"];
    
    MSLoginSafariViewController *loginSafariViewController = [[MSLoginSafariViewController alloc] initWithClient:client];
    
    // Setup a Google login in authState of loginSafariViewController first.
    // Then Facebook login come in when Google login is still in progress.
    // Facebook login should fail and complete with a error.
    
    loginSafariViewController.authState = [[MSAuthState alloc] initWithProvider:@"google"
                                                   loginCompletion:^(MSUser *user, NSError *error) {}
                                                      codeVerifier:@"67890"
                                                         urlScheme:@"com.example.ZumoE2ETest" animated:YES];
    
    [loginSafariViewController loginInternalWithProvider:@"facebook"
                                   urlScheme:@"com.example.ZumoE2ETest"
                                  parameters:nil
                                controller:[[UIViewController alloc] initWithNibName:nil bundle:nil]
                                    animated:YES
                                  completion:^(MSUser * _Nullable user, NSError * _Nullable error) {
                                      XCTAssertNil(user);
                                      XCTAssertNotNil(error);
                                      XCTAssertEqual(MSLoginOperationInProgress, error.code);
                                  }];
}

- (void)testSafariViewControllerLoginWithProvider
{
    MSClient *client = [MSClient clientWithApplicationURLString:@"https://ZumoE2ETest.example.com/"];
    
    MSLoginSafariViewController *loginSafariViewController = [[MSLoginSafariViewController alloc] initWithClient:client];
    
    [loginSafariViewController loginInternalWithProvider:@"google"
                                   urlScheme:@"com.example.ZumoE2ETest"
                                  parameters:nil
                                controller:[[UIViewController alloc] initWithNibName:nil bundle:nil]
                                    animated:YES
                                  completion:^(MSUser * _Nullable user, NSError * _Nullable error) {
                                  }];
    
    XCTAssertEqualObjects(@"google", loginSafariViewController.authState.provider);
    XCTAssertEqualObjects(@"com.example.ZumoE2ETest", loginSafariViewController.authState.urlScheme);
    XCTAssertTrue(loginSafariViewController.authState.animated);
    XCTAssertNotNil(loginSafariViewController.authState.codeVerifier);
    XCTAssertNotNil(loginSafariViewController.authState.loginCompletion);
    XCTAssertNotNil(loginSafariViewController.safariViewController);
    
    XCTAssertEqualObjects(loginSafariViewController, loginSafariViewController.safariViewController.delegate);
}

- (void)testCodeExchangeWithURL
{
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];

    MSClient *client = [MSClient clientWithApplicationURLString:@"https://ZumoE2ETest.example.com/"];
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:200];
    
    testFilter.onInspectResponseData = ^(NSURLRequest *request, NSData *data) {
        NSDictionary *item = @{ @"user" : @{ @"userId" : @"sid:12345678" }, @"authenticationToken" : @"token12345678" };
        return [[MSJSONSerializer JSONSerializer] dataFromItem:item idAllowed:YES ensureDictionary:NO removeSystemProperties:YES orError:nil];
    };

    MSClient *filterClient = [client clientWithFilter:testFilter];

    MSLoginSafariViewController *loginSafariViewController = [[MSLoginSafariViewController alloc] initWithClient:filterClient];
    MSClientLoginBlock loginCompletion = ^(MSUser *user, NSError *error) {
        XCTAssertNil(error);
        XCTAssertEqualObjects(@"sid:12345678", user.userId);
        XCTAssertEqualObjects(@"token12345678", user.mobileServiceAuthenticationToken);
        [expectation fulfill];
    };

    loginSafariViewController.authState = [[MSAuthState alloc]
                                           initWithProvider:@"google"
                                           loginCompletion:loginCompletion
                                           codeVerifier:@"67890"
                                           urlScheme:@"com.example.ZumoE2ETest" animated:YES];
    
    [loginSafariViewController codeExchangeWithURL:[[NSURL alloc] initWithString:@"https://ZumoE2ETest.example.com/.auth/login/google/token?authorization_code=12345&code_verifier=67890"] completion:loginCompletion];
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

- (void)testCodeExchangeWithURLFailedWith400Error
{
    MSClient *client = [MSClient clientWithApplicationURLString:@"https://ZumoE2ETest.example.com/"];
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:400];
    
    MSClient *filterClient = [client clientWithFilter:testFilter];
    
    MSLoginSafariViewController *loginSafariViewController = [[MSLoginSafariViewController alloc] initWithClient:filterClient];
    MSClientLoginBlock loginCompletion = ^(MSUser *user, NSError *error) { };
    
    loginSafariViewController.authState = [[MSAuthState alloc]
                                           initWithProvider:@"google"
                                           loginCompletion:loginCompletion
                                           codeVerifier:@"67890"
                                           urlScheme:@"com.example.ZumoE2ETest" animated:YES];
    
    [loginSafariViewController codeExchangeWithURL:[[NSURL alloc] initWithString:@"https://ZumoE2ETest.example.com/.auth/login/google/token?authorization_code=12345&code_verifier=67890"] completion:loginCompletion];
    
    XCTAssertFalse([self waitForTest:3.0], @"codeExchangeWithURL:completion should hang forever and never finish in the event of 400 error response.");
}

- (void)testCodeExchangeWithURLFailedWith500Error
{
    XCTestExpectation *expectation = [self expectationWithDescription:self.name];

    MSClient *client = [MSClient clientWithApplicationURLString:@"https://ZumoE2ETest.example.com/"];
    MSTestFilter *testFilter = [MSTestFilter testFilterWithStatusCode:500];
    
    MSClient *filterClient = [client clientWithFilter:testFilter];
    
    MSLoginSafariViewController *loginSafariViewController = [[MSLoginSafariViewController alloc] initWithClient:filterClient];
    MSClientLoginBlock loginCompletion = ^(MSUser *user, NSError *error) {
        XCTAssertNil(user);
        XCTAssertNotNil(error);
        
        [expectation fulfill];
    };
    loginSafariViewController.authState = [[MSAuthState alloc]
                                           initWithProvider:@"google"
                                           loginCompletion:loginCompletion
                                           codeVerifier:@"67890"
                                           urlScheme:@"com.example.ZumoE2ETest" animated:YES];
    
    [loginSafariViewController codeExchangeWithURL:[[NSURL alloc] initWithString:@"https://ZumoE2ETest.example.com/.auth/login/google/token?authorization_code=12345&code_verifier=67890"] completion:loginCompletion];
    
    [self waitForExpectationsWithTimeout:1.0 handler:nil];
}

- (void)testCodeExchangeRequestURLFromRedirectURL
{
    MSClient *client = [MSClient clientWithApplicationURLString:@"https://ZumoE2ETest.example.com/"];

    MSLoginSafariViewController *loginSafariViewController = [[MSLoginSafariViewController alloc] initWithClient:client];
    loginSafariViewController.authState = [[MSAuthState alloc]
                                           initWithProvider:@"google"
                                           loginCompletion:nil
                                           codeVerifier:@"67890"
                                           urlScheme:@"com.example.ZumoE2ETest" animated:YES];
    
    NSURL *redirectURL = [[NSURL alloc] initWithString:@"com.example.ZumoE2ETest://easyauth.callback/#authorization_code=12345"];
    
    NSURL *requestURL = [loginSafariViewController codeExchangeRequestURLFromRedirectURL:redirectURL];
    
    XCTAssertEqualObjects(@"https://ZumoE2ETest.example.com/.auth/login/google/token?authorization_code=12345&code_verifier=67890", requestURL.absoluteString);
}

- (void)testCodeExchangeRequestURLFromRedirectURLWithInvalidURLScheme
{
    MSClient *client = [MSClient clientWithApplicationURLString:@"https://ZumoE2ETest.example.com/"];
    
    MSLoginSafariViewController *loginSafariViewController = [[MSLoginSafariViewController alloc] initWithClient:client];
    loginSafariViewController.authState = [[MSAuthState alloc]
                                           initWithProvider:@"google"
                                           loginCompletion:nil
                                           codeVerifier:@"67890"
                                           urlScheme:@"com.example.ZumoE2ETest" animated:YES];
    
    NSURL *redirectURL = [[NSURL alloc] initWithString:@"foobar_url_scheme://easyauth.callback/#authorization_code=12345"];
    
    NSURL *requestURL = [loginSafariViewController codeExchangeRequestURLFromRedirectURL:redirectURL];
    
    XCTAssertNil(requestURL);
}

- (void)testCodeExchangeRequestURLFromRedirectURLWithNoAuthorizationCode
{
    MSClient *client = [MSClient clientWithApplicationURLString:@"https://ZumoE2ETest.example.com/"];
    
    MSLoginSafariViewController *loginSafariViewController = [[MSLoginSafariViewController alloc] initWithClient:client];
    loginSafariViewController.authState = [[MSAuthState alloc]
                                           initWithProvider:@"google"
                                           loginCompletion:nil
                                           codeVerifier:@"67890"
                                           urlScheme:@"com.example.ZumoE2ETest" animated:YES];
    
    NSURL *redirectURL = [[NSURL alloc] initWithString:@"com.example.ZumoE2ETest://easyauth.callback/"];
    
    NSURL *requestURL = [loginSafariViewController codeExchangeRequestURLFromRedirectURL:redirectURL];
    
    XCTAssertNil(requestURL);
}

#pragma mark * Async Test Helper Method

- (BOOL)waitForTest:(NSTimeInterval)testDuration
{
    NSDate *timeoutAt = [NSDate dateWithTimeIntervalSinceNow:testDuration];
    
    BOOL done = NO;
    while (!done) {
        [[NSRunLoop currentRunLoop] runMode:NSDefaultRunLoopMode beforeDate:timeoutAt];
        if([timeoutAt timeIntervalSinceNow] <= 0.0) {
            break;
        }
    }
    
    return done;
}

@end
