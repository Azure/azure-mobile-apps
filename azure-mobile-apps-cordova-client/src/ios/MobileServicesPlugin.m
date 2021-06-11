// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Cordova/CDV.h>
#import <Cordova/CDVConfigParser.h>
#import "MobileServicesPlugin.h"
#import "AppDelegate+MobileServicesPlugin.h"
#import <MicrosoftAzureMobile/MicrosoftAzureMobile.h>

@implementation MobileServicesPlugin

static NSString * const PlistRedirectUriSchemeKey = @"AzureMobileAppsRedirectUriScheme";

- (void)loginWithGoogle:(CDVInvokedUrlCommand *)command
{
    NSString *appUrl = [command argumentAtIndex:0 withDefault:nil andClass:[NSString class]];
    
    MSClient *client = [MSClient clientWithApplicationURLString:appUrl];
    AppDelegate *appDelegate = (AppDelegate *)[UIApplication sharedApplication].delegate;
    [appDelegate setMSClient:client];
    
    NSError *error;
    NSString *uriScheme = [MobileServicesPlugin getUriSchemeFromPlist:&error];
    if (error) {
        CDVPluginResult *errorResult = [self getErrorResult:error];
        [self.commandDelegate sendPluginResult:errorResult callbackId:command.callbackId];
        return;
    }
    
    [client loginWithProvider:@"google" urlScheme:uriScheme controller:self.viewController animated:true completion:^(MSUser * _Nullable user, NSError * _Nullable error) {
        if (error) {
            CDVPluginResult *errorResult = [self getErrorResult:error];
            [self.commandDelegate sendPluginResult:errorResult callbackId:command.callbackId];
        }
        else {
            NSDictionary *token = [self getTokenObjectFromUser:user];
            CDVPluginResult *pluginResult = [CDVPluginResult resultWithStatus:CDVCommandStatus_OK messageAsDictionary:token];
            [self.commandDelegate sendPluginResult:pluginResult callbackId:command.callbackId];
        }
    }];
}

+ (NSString *)getUriSchemeFromPlist: (NSError **)error
{
    NSDictionary *infoDictionary = [[NSBundle mainBundle] infoDictionary];
    
    NSString *uriScheme = [infoDictionary objectForKey:PlistRedirectUriSchemeKey];
    
    if (!uriScheme || [uriScheme length] == 0) {
        NSString *message = [NSString stringWithFormat:@"%@%@", PlistRedirectUriSchemeKey, @" key doesn't set or it's value is empty. Please, configure it within your_app-Info.Plist."];
        *error = [self errorWithMessage:message];
        return nil;
    }
    
    return uriScheme;
}

- (NSDictionary *)getTokenObjectFromUser:(MSUser *)user
{
    NSString *authToken = [user mobileServiceAuthenticationToken];
    NSMutableDictionary *tokenDict = [[NSMutableDictionary alloc] initWithObjectsAndKeys:authToken, @"authenticationToken", nil];
    
    NSString *sid = [user userId];
    //remove 'sid:' from the beggining
    sid = [sid substringFromIndex:4];
    NSDictionary *userDict = [[NSDictionary alloc] initWithObjectsAndKeys:sid, @"sid", nil];
    
    [tokenDict setObject:userDict forKey:@"user"];
    
    return tokenDict;
}

+ (NSError *)errorWithMessage:(NSString *)errorMessage
{
    return [NSError errorWithDomain:@"MobileServices"
                               code:-1
                           userInfo:@{ NSLocalizedDescriptionKey: NSLocalizedString(errorMessage, nil) }];
}

- (CDVPluginResult *)getErrorResult:(NSError *)error
{
    NSString *message = [NSString stringWithFormat:@"Couldn't authenticate with google provider. %@", [error localizedDescription]];
    return [CDVPluginResult resultWithStatus:CDVCommandStatus_ERROR messageAsString:message];
}

@end
