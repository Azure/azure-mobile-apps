// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
#import "AppDelegate+MobileServicesPlugin.h"
#import "MobileServicesPlugin.h"

@implementation AppDelegate (MobileServicesPlugin)

static MSClient *_msClient;

- (MSClient *)msClient
{
    return _msClient;
}

- (void)setMSClient:(MSClient *)msClient
{
    _msClient = msClient;
}

- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options
{
    NSError *error;
    NSString *uriScheme = [MobileServicesPlugin getUriSchemeFromPlist:&error];
    if (error) {
        NSLog(@"%@%@", @"Could not authenticate with google provider. ", [error localizedDescription]);
        return NO;
    }
    if ([[url.scheme lowercaseString] isEqualToString:uriScheme]) {
        // Resume login flow
        return [_msClient resumeWithURL:url];
    }
    else {
        return NO;
    }
}

@end
