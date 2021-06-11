// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
#import <Cordova/CDV.h>
#import <MicrosoftAzureMobile/MicrosoftAzureMobile.h>

@interface MobileServicesPlugin : CDVPlugin

- (void)loginWithGoogle:(CDVInvokedUrlCommand *)command;
+ (NSString *)getUriSchemeFromPlist: (NSError **)error;
- (NSDictionary *)getTokenObjectFromUser:(MSUser *)user;
+ (NSError *)errorWithMessage:(NSString *)errorMessage;
- (CDVPluginResult *)getErrorResult:(NSError *)error;

@end
