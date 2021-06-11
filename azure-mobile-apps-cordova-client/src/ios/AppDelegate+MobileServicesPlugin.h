// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
#import "AppDelegate.h"
#import <MicrosoftAzureMobile/MicrosoftAzureMobile.h>

@interface AppDelegate (MobileServicesPlugin)

- (MSClient *)msClient;
- (void)setMSClient:(MSClient *)msClient;

- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options;

@end
