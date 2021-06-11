// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoTest.h"
#import <Google/SignIn.h>

typedef void (^AzureLoginBlock)(GIDGoogleUser*);

@interface ZumoTestGoogleSignInDelegate : NSObject <GIDSignInDelegate>

- (void)setZumoTest:(ZumoTest *)test completion:(ZumoTestCompletion)completion andAzureLoginBlock:(AzureLoginBlock)loginBlock;

@end
