// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <UIKit/UIKit.h>
#import "ZumoTestGoogleSignInDelegate.h"

@interface ZumoAppDelegate : UIResponder <UIApplicationDelegate>

@property (strong, nonatomic, nonnull) UIWindow *window;
@property (strong, nonatomic, nullable) ZumoTestGoogleSignInDelegate *googleDelegate;

@end
