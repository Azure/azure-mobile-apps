// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoTestGoogleSignInDelegate.h"

@interface ZumoTestGoogleSignInDelegate()

@property (nonatomic, strong) ZumoTest *test;
@property (nonatomic, strong) ZumoTestCompletion completion;
@property (nonatomic, strong) AzureLoginBlock loginBlock;

@end

@implementation ZumoTestGoogleSignInDelegate

- (void)setZumoTest:(ZumoTest *)test completion:(ZumoTestCompletion)completion andAzureLoginBlock:(AzureLoginBlock)loginBlock{
    self.test = test;
    self.loginBlock = loginBlock;
    self.completion = completion;
}

- (void)signIn:(GIDSignIn *)signIn didSignInForUser:(GIDGoogleUser *)user withError:(NSError *)error {
    if (error){
        [self.test addLog:[NSString stringWithFormat:@"Error authenticating with Google: %@", error.localizedDescription]];
        self.completion(NO);
    } else {
        self.loginBlock(user);
    }
}

@end
