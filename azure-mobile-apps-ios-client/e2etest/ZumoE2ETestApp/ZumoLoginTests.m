// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoLoginTests.h"
#import "ZumoTest.h"
#import "ZumoTestGlobals.h"
#import "ZumoAppDelegate.h"
#import <ADAL/ADAuthenticationContext.h>
#import <ADAL/ADAuthenticationSettings.h>
#import <ADAL/ADAuthenticationResult.h>
#import <ADAL/ADTokenCacheItem.h>

@interface TimerTarget : NSObject
{
    ZumoTest *test;
    ZumoTestCompletion completion;
}

- (id)initWithTest:(ZumoTest *)test completion:(ZumoTestCompletion)completion;
- (void)completeTest:(NSTimer *)timer;

@end

@implementation TimerTarget

- (id)initWithTest:(ZumoTest *)theTest completion:(ZumoTestCompletion)completionBlock {
    self = [super init];
    if (self) {
        self->test = theTest;
        self->completion = completionBlock;
    }
    
    return self;
}

- (void)completeTest:(NSTimer *)timer {
    [test addLog:@"Timer fired, completing the test"];
    [test setTestStatus:TSPassed];
    completion(YES);
}

@end

@implementation ZumoLoginTests

static NSString *lastUserIdentityObjectKey = @"lastUserIdentityObject";

typedef enum { ZumoTableAnonymous, ZumoTableAuthenticated } ZumoTableType;

+ (NSArray *)createTests {
    NSMutableArray *result = [[NSMutableArray alloc] init];
    [result addObject:[self createClearAuthCookiesTest]];
    [result addObject:[self createLogoutTest]];
    [result addObject:[self createCRUDTestForProvider:nil forTable:@"public" ofType:ZumoTableAnonymous andAuthenticated:NO]];
    [result addObject:[self createCRUDTestForProvider:nil forTable:@"authenticated" ofType:ZumoTableAuthenticated andAuthenticated:NO]];

    NSInteger indexOfLastUnattendedTest = [result count];
    
    result = [self createServerLoginFlowAndClientLoginFlowForProvider:@"facebook" tests:result];

    result = [self createServerLoginFlowAndClientLoginFlowForProvider:@"twitter" tests:result];
    
    result = [self createServerLoginRefreshTokenFlowForProvider:@"microsoftaccount" tests:result];

    result = [self createAADLoginTests:result];

    result = [self createGoogleLoginTests:result];

    for (NSInteger i = indexOfLastUnattendedTest; i < [result count]; i++) {
        ZumoTest *test = result[i];
        [test setCanRunUnattended:NO];
    }
    
    [result addObject:[self createLogoutTest]];

    return result;
}

+ (NSMutableArray *)createServerLoginFlowAndClientLoginFlowForProvider:(NSString *)provider tests:(NSMutableArray *)tests
{
    [tests addObject:[self createLogoutTest]];
    [tests addObject:[self createSleepTest:1]];
    
    // Server Login Flow - simplified
    [tests addObject:[self createServerFlowLoginTestForProvider:provider usingSimplifiedMode:YES]];
    [tests addObject:[self createCRUDTestForProvider:provider forTable:@"public" ofType:ZumoTableAnonymous andAuthenticated:YES]];
    [tests addObject:[self createCRUDTestForProvider:provider forTable:@"authenticated" ofType:ZumoTableAuthenticated andAuthenticated:YES]];
    
    [tests addObject:[self createLogoutTest]];
    [tests addObject:[self createSleepTest:3]];
    
    // Server Login Flow - non-simplified
    [tests addObject:[self createServerFlowLoginTestForProvider:provider usingSimplifiedMode:NO]];
    [tests addObject:[self createCRUDTestForProvider:provider forTable:@"public" ofType:ZumoTableAnonymous andAuthenticated:YES]];
    [tests addObject:[self createCRUDTestForProvider:provider forTable:@"authenticated" ofType:ZumoTableAuthenticated andAuthenticated:YES]];
    
    [tests addObject:[self createLogoutTest]];
    [tests addObject:[self createSleepTest:1]];
    
    // Client Login Flow
    [tests addObject:[self createClientSideLoginWithProvider:provider]];
    [tests addObject:[self createCRUDTestForProvider:provider forTable:@"authenticated" ofType:ZumoTableAuthenticated andAuthenticated:YES]];
    
    return tests;
}

+ (NSMutableArray *)createServerLoginRefreshTokenFlowForProvider:(NSString *)provider tests:(NSMutableArray *)tests
{
    [tests addObject:[self createLogoutTest]];
    [tests addObject:[self createSleepTest:1]];
    
    // Server Login Flow - simplified
    [tests addObject:[self createServerFlowLoginTestForProvider:provider usingSimplifiedMode:YES]];
    [tests addObject:[self createCRUDTestForProvider:provider forTable:@"public" ofType:ZumoTableAnonymous andAuthenticated:YES]];
    [tests addObject:[self createCRUDTestForProvider:provider forTable:@"authenticated" ofType:ZumoTableAuthenticated andAuthenticated:YES]];
    
    // Refresh Token
    [tests addObject:[self createRefreshTestForProvider:provider]];

    [tests addObject:[self createLogoutTest]];
    [tests addObject:[self createSleepTest:3]];
    
    // Server Login Flow - non-simplified
    [tests addObject:[self createServerFlowLoginTestForProvider:provider usingSimplifiedMode:NO]];
    [tests addObject:[self createCRUDTestForProvider:provider forTable:@"public" ofType:ZumoTableAnonymous andAuthenticated:YES]];
    [tests addObject:[self createCRUDTestForProvider:provider forTable:@"authenticated" ofType:ZumoTableAuthenticated andAuthenticated:YES]];
    
    return tests;
}

+ (NSMutableArray *)createGoogleLoginTests:(NSMutableArray *)tests
{
    [tests addObject:[self createGoogleLogoutTest]];
    [tests addObject:[self createSleepTest:1]];

    // Client Login Flow
    [tests addObject:[self createGoogleClientFlowAuthTest]];
    [tests addObject:[self createCRUDTestForProvider:@"google" forTable:@"authenticated" ofType:ZumoTableAuthenticated andAuthenticated:YES]];

    return tests;
}

+ (ZumoTest *)createGoogleClientFlowAuthTest {
    return [ZumoTest createTestWithName:@"Login for Google - Client flow" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        ZumoTestGoogleSignInDelegate *delegate = [GIDSignIn sharedInstance].delegate;
        [delegate setZumoTest:test completion:completion andAzureLoginBlock:^(GIDGoogleUser *user){
            if (user.serverAuthCode == nil){
                [test addLog:@"Error authenticating with Google: Server authorization code is nil"];
                completion(NO);
                return;
            }
            NSDictionary *payload = @{ @"id_token":user.authentication.idToken,@"authorization_code":user.serverAuthCode};
            MSClient *client = [[ZumoTestGlobals sharedInstance] client];
            [client loginWithProvider:@"google" token:payload completion:^(MSUser *user, NSError *error) {
                if (error == nil){
                    [test addLog:@"Google authentication successful"];
                    completion(YES);
                } else {
                    [test addLog:[NSString stringWithFormat:@"Error authenticating with Google: %@", error.localizedDescription]];
                    completion(NO);
                }
            }];
        }];
        [[GIDSignIn sharedInstance] signIn];
    }];
}

+ (ZumoTest *)createGoogleLogoutTest {
  ZumoTest *result = [ZumoTest createTestWithName:@"Google logout" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
    MSClient *client = [[ZumoTestGlobals sharedInstance] client];
    [client logoutWithCompletion:^(NSError *error) {
      [test addLog:@"Logged out"];
      [[GIDSignIn sharedInstance] signOut];
      MSUser *loggedInUser = [client currentUser];
      if (loggedInUser == nil) {
        [test setTestStatus:TSPassed];
        completion(YES);
      } else {
        [test addLog:[NSString stringWithFormat:@"Error, user for client is not null: %@", loggedInUser]];
        [test setTestStatus:TSFailed];
        completion(NO);
      }
    }];
  }];

  return result;
}

+ (NSMutableArray *)createAADLoginTests:(NSMutableArray *)tests {
  tests = [self createServerLoginRefreshTokenFlowForProvider:@"aad" tests:tests];
  [tests addObject:[self createClientSideLoginWithAAD]];
  return tests;
}


+ (ZumoTest *)createClientSideLoginWithAAD {
  ZumoTest *result = [ZumoTest createTestWithName:[NSString stringWithFormat:@"Login via token for AAD"] andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
    NSString *authority = @"https://login.windows.net/azuremobile.onmicrosoft.com";
    NSString *resourceId = @"8d8b5207-e2d8-4402-a081-53149d056cfd";
    NSString *clientId = @"486a3dbd-b102-4348-b68c-7d0d7deb4dc3";

    // Take last used url.
    NSArray *lastUsedApp = [[ZumoTestGlobals sharedInstance] loadAppInfo];
    NSString *fullUrl = [NSString stringWithFormat:@"%@/.auth/login/done", [lastUsedApp objectAtIndex:0]];
    NSURL *redirectUri = [NSURL URLWithString:fullUrl];

    ZumoAppDelegate *appDelegate = ((ZumoAppDelegate*)(UIApplication.sharedApplication.delegate));
    UIViewController *parentViewController = ((UINavigationController *)(appDelegate.window.rootViewController)).topViewController;
    ADAuthenticationError *error;
    ADAuthenticationContext *authContext = [ADAuthenticationContext authenticationContextWithAuthority:authority error:&error];
    authContext.parentController = parentViewController;
    [ADAuthenticationSettings sharedInstance].enableFullScreen = YES;
    MSClient *client = [[ZumoTestGlobals sharedInstance] client];
    [authContext acquireTokenWithResource:resourceId
                                 clientId:clientId
                              redirectUri:redirectUri
                          completionBlock:^(ADAuthenticationResult *result) {
                            if (result.status != AD_SUCCEEDED) {
                              [test addLog:[NSString stringWithFormat:@"Error logging in: %@", result.error]];
                              [test setTestStatus:TSFailed];
                              completion(NO);
                            } else {
                              NSDictionary *payload = @{ @"access_token" : result.tokenCacheItem.accessToken };
                              [client loginWithProvider:@"aad" token:payload completion:^(MSUser *user, NSError *error) {
                                if (error) {
                                  [test addLog:[NSString stringWithFormat:@"Error logging in: %@", error]];
                                  [test setTestStatus:TSFailed];
                                  completion(NO);
                                } else {
                                  [test addLog:[NSString stringWithFormat:@"Logged in as %@", [user userId]]];
                                  [test setTestStatus:TSPassed];
                                  completion(YES);
                                }
                              }];
                            }
                          }];
  }];
  return result;
}

+ (ZumoTest *)createSleepTest:(int)seconds {
    NSString *testName = [NSString stringWithFormat:@"Sleep for %d seconds", seconds];
    return [ZumoTest createTestWithName:testName andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        [test addLog:@"Starting the timer"];
        TimerTarget *timerTarget = [[TimerTarget alloc] initWithTest:test completion:completion];
        NSTimer *timer = [NSTimer scheduledTimerWithTimeInterval:seconds target:timerTarget selector:@selector(completeTest:) userInfo:nil repeats:NO];
        [test addLog:[NSString stringWithFormat:@"Timer fire date: %@", [timer fireDate]]];
    }];
}

+ (ZumoTest *)createCRUDTestForProvider:(NSString *)providerName forTable:(NSString *)tableName ofType:(ZumoTableType)tableType andAuthenticated:(BOOL)isAuthenticated {
    NSString *tableTypeName;
    switch (tableType) {
        case ZumoTableAuthenticated:
            tableTypeName = @"authenticated users";
            break;
            
        default:
            tableTypeName = @"public";
            break;
    }
    
    if (!providerName) {
        providerName = @"no";
    }
    
    NSString *testName = [NSString stringWithFormat:@"CRUD, %@ auth, table with %@ permissions", providerName, tableTypeName];
    BOOL crudShouldWork = tableType == ZumoTableAnonymous || (tableType == ZumoTableAuthenticated && isAuthenticated);
    ZumoTest *result = [ZumoTest createTestWithName:testName andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        MSTable *table = [client tableWithName:tableName];
        [table insert:@{@"name":@"john"} completion:^(NSDictionary *inserted, NSError *insertError) {
            if (![self validateCRUDResultForTest:test andOperation:@"Insert" andError:insertError andExpected:crudShouldWork]) {
                completion(NO);
                return;
            }
            
            NSDictionary *toUpdate = crudShouldWork ? inserted : @{ @"name":@"jane",@"id":@1 };
            [table update:toUpdate completion:^(NSDictionary *updated, NSError *updateError) {
                if (![self validateCRUDResultForTest:test andOperation:@"Update" andError:updateError andExpected:crudShouldWork]) {
                    completion(NO);
                    return;
                }
                
                NSNumber *itemId = crudShouldWork ? [inserted objectForKey:@"id"] : [NSNumber numberWithInt:1];
                [table readWithId:itemId completion:^(NSDictionary *read, NSError *readError) {
                    if (![self validateCRUDResultForTest:test andOperation:@"Read" andError:readError andExpected:crudShouldWork]) {
                        completion(NO);
                        return;
                    }
                    
                    if (!readError && tableType == ZumoTableAuthenticated) {
                        id serverIdentities = [read objectForKey:@"identities"];
                        NSDictionary *identities;
                        if ([serverIdentities isKindOfClass:[NSString class]]) {
                            NSString *identitiesJson = serverIdentities;
                            NSData *identitiesData = [identitiesJson dataUsingEncoding:NSUTF8StringEncoding];
                            NSError *jsonError;
                            identities = [NSJSONSerialization JSONObjectWithData:identitiesData options:0 error:&jsonError];
                            if (jsonError) {
                                [test addLog:[NSString stringWithFormat:@"Identities value is not a valid JSON object: %@", jsonError]];
                                completion(NO);
                                return;
                            }
                        } else if ([serverIdentities isKindOfClass:[NSDictionary class]]) {
                            // it's already a dictionary
                            identities = serverIdentities;
                        } else {
                            [test addLog:@"Server identities is not a dictionary of values"];
                            completion(NO);
                            return;
                        }
                        [[[ZumoTestGlobals sharedInstance] globalTestParameters] setObject:identities forKey:lastUserIdentityObjectKey];
                    }
                    
                    [table deleteWithId:itemId completion:^(NSNumber *deletedId, NSError *deleteError) {
                        if (![self validateCRUDResultForTest:test andOperation:@"Delete" andError:deleteError andExpected:crudShouldWork]) {
                            completion(NO);
                        } else {
                            [test setTestStatus:TSPassed];
                            [test addLog:@"Validation succeeded for all operations"];
                            completion(YES);
                        }
                    }];
                }];
            }];
        }];
    }];
    return result;
}

+ (BOOL)validateCRUDResultForTest:(ZumoTest *)test andOperation:(NSString *)operation andError:(NSError *)error andExpected:(BOOL)shouldSucceed {
    if (shouldSucceed == (error == nil)) {
        if (error) {
            NSHTTPURLResponse *resp = [[error userInfo] objectForKey:MSErrorResponseKey];
            if (resp.statusCode == 401) {
                [test addLog:[NSString stringWithFormat:@"Got expected response code for operation %@: %ld", operation, (long)resp.statusCode]];
                return YES;
            } else {
                [test addLog:[NSString stringWithFormat:@"Got invalid response code for operation %@: %ld", operation, (long)resp.statusCode]];
                return NO;
            }
        } else {
            return YES;
        }
    } else {
        [test addLog:[NSString stringWithFormat:@"Should%@ succeed for %@, but error = %@", (shouldSucceed ? @"" : @" not"), operation, error]];
        [test setTestStatus:TSFailed];
        return NO;
    }
}

+ (ZumoTest *)createClientSideLoginWithProvider:(NSString *)provider {
    return [ZumoTest createTestWithName:[NSString stringWithFormat:@"Login via token for %@", provider] andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        NSDictionary *lastIdentity = [[[ZumoTestGlobals sharedInstance] globalTestParameters] objectForKey:lastUserIdentityObjectKey];
        if (!lastIdentity) {
            [test addLog:@"Last identity is null. Cannot run this test."];
            [test setTestStatus:TSSkipped];
            completion(NO);
            return;
        }
        
        [[[ZumoTestGlobals sharedInstance] globalTestParameters] removeObjectForKey:lastUserIdentityObjectKey];
        
        [test addLog:[NSString stringWithFormat:@"Last user identity object: %@", lastIdentity]];
        NSDictionary *providerIdentity = [lastIdentity objectForKey:provider];
        if (!providerIdentity) {
            [test addLog:@"Don't have identity for specified provider. Cannot run this test."];
            [test setTestStatus:TSSkipped];
            completion(NO);
            return;
        }

        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        NSDictionary *token = providerIdentity;
        [client loginWithProvider:provider token:token completion:^(MSUser *user, NSError *error) {
            if (error) {
                [test addLog:[NSString stringWithFormat:@"Error logging in: %@", error]];
                [test setTestStatus:TSFailed];
                completion(NO);
            } else {
                [test addLog:[NSString stringWithFormat:@"Logged in as %@", [user userId]]];
                [test setTestStatus:TSPassed];
                completion(YES);
            }
        }];
        
    }];
}

#pragma clang diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"

+ (ZumoTest *)createServerFlowLoginTestForProvider:(NSString *)provider usingSimplifiedMode:(BOOL)useSimplified {
    ZumoTest *result = [ZumoTest createTestWithName:[NSString stringWithFormat:@"%@Login for %@", useSimplified ? @"Simplified " : @"", provider] andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        BOOL shouldDismissControllerInBlock = !useSimplified;
        MSClientLoginBlock loginBlock = ^(MSUser *user, NSError *error) {
            if (error) {
                [test addLog:[NSString stringWithFormat:@"Error logging in: %@", error]];
                [test setTestStatus:TSFailed];
                completion(NO);
            } else {
                [test addLog:[NSString stringWithFormat:@"Logged in as %@", [user userId]]];
                [test setTestStatus:TSPassed];
                completion(YES);
            }
            if (shouldDismissControllerInBlock) {
                [viewController dismissViewControllerAnimated:YES completion:nil];
            }
        };
        
        if (useSimplified) {
            if ([provider isEqualToString:@"microsoftaccount"] || [provider isEqualToString:@"facebook"] || [provider isEqualToString:@"twitter"]) {
                [client loginWithProvider:provider controller:viewController animated:YES completion:loginBlock];
            } else if ([provider isEqualToString:@"google"]) {
                [client loginWithProvider:provider parameters:@{@"access_type" : @"offline"} controller:viewController animated:YES completion:loginBlock];
            } else if ([provider isEqualToString:@"aad"]) {
                [client loginWithProvider:provider parameters:@{@"response_type" : @"code id_token"} controller:viewController animated:YES completion:loginBlock];
            }
        } else {
            UIViewController *loginController = [client loginViewControllerWithProvider:provider completion:loginBlock];
            [viewController presentViewController:loginController animated:YES completion:nil];
        }
    }];
    
    return result;
}

#pragma clang diagnostic pop

+ (ZumoTest *)createRefreshTestForProvider:(NSString *)provider {
    ZumoTest *result = [ZumoTest createTestWithName:[NSString stringWithFormat:@"Refresh for %@", provider] andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];

        [client refreshUserWithCompletion:^(MSUser *user, NSError *error) {
            if (error) {
                [test addLog:[NSString stringWithFormat:@"Error refreshing: %@", error]];
                [test setTestStatus:TSFailed];
                completion(NO);
            } else {
                [test addLog:[NSString stringWithFormat:@"Refresh succeeded. userId: %@", [user userId]]];
                [test setTestStatus:TSPassed];
                completion(YES);
            }
        }];
    }];
    
    return result;
}

+ (ZumoTest *)createClearAuthCookiesTest {
    ZumoTest *result = [ZumoTest createTestWithName:@"Clear login cookies" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        NSHTTPCookieStorage *cookieStorage = [NSHTTPCookieStorage sharedHTTPCookieStorage];
        NSPredicate *isAuthCookie = [NSPredicate predicateWithFormat:@"domain ENDSWITH '.facebook.com' or domain ENDSWITH '.google.com' or domain ENDSWITH '.live.com' or domain ENDSWITH '.twitter.com' or domain ENDSWITH '.microsoftonline.com' or domain ENDSWITH '.windows.net'"];
        NSArray *cookiesToRemove = [[cookieStorage cookies] filteredArrayUsingPredicate:isAuthCookie];
         for (NSHTTPCookie *cookie in cookiesToRemove) {
            NSLog(@"Removed cookie from %@ %@ %@ %d", [cookie domain], cookie.name, cookie.comment, cookie.isSessionOnly);
            [cookieStorage deleteCookie:cookie];
        }

        [test addLog:@"Removed authentication-related cookies from this app."];
        completion(YES);
    }];
    return result;
}

+ (ZumoTest *)createLogoutTest {
    ZumoTest *result = [ZumoTest createTestWithName:@"Logout" andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion) {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        [client logoutWithCompletion:^(NSError *error) {
            [test addLog:@"Logged out"];
            MSUser *loggedInUser = [client currentUser];
            if (loggedInUser == nil) {
                [test setTestStatus:TSPassed];
                completion(YES);
            } else {
                [test addLog:[NSString stringWithFormat:@"Error, user for client is not null: %@", loggedInUser]];
                [test setTestStatus:TSFailed];
                completion(NO);
            }
        }];
    }];
    
    return result;
}

+ (NSString *)groupDescription {
    return @"Tests to validate all forms of the login operation in the client SDK.";
}

@end
