// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import <SafariServices/SafariServices.h>
#import "ZumoTest.h"
#import "ZumoTestGlobals.h"
#import "ZumoLoginSafariViewControllerTests.h"
#import "ZumoAppDelegate.h"

@implementation ZumoLoginSafariViewControllerTests

+ (ZumoTest *)createSafariLoginTestForProvider:(NSString *)provider animated:(BOOL)animated
{
    ZumoTest *result = [ZumoTest createTestWithName:[NSString stringWithFormat:@"SafariVC Login of %@", provider] andExecution:^(ZumoTest *test, UIViewController *viewController, ZumoTestCompletion completion)
    {
        MSClient *client = [[ZumoTestGlobals sharedInstance] client];
        
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
        };

        [client loginWithProvider:provider urlScheme:@"ZumoE2ETestApp" controller:viewController animated:animated completion:loginBlock];
    }];
    
    return result;
}

+ (NSArray *)createTests
{
    NSMutableArray *result = [[NSMutableArray alloc] init];

    [result addObject:[self createSafariLoginTestForProvider:@"google" animated:YES]];

    [result addObject:[self createSafariLoginTestForProvider:@"aad" animated:NO]];

    [result addObject:[self createSafariLoginTestForProvider:@"microsoftaccount" animated:NO]];

    [result addObject:[self createSafariLoginTestForProvider:@"facebook" animated:NO]];

    [result addObject:[self createSafariLoginTestForProvider:@"twitter" animated:NO]];

    return result;
}

+ (NSString *)groupDescription {
    return @"Tests to validate Login via SafariViewController in the client SDK.";
}


@end
