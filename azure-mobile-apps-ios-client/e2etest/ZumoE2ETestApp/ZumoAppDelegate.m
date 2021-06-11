// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoAppDelegate.h"
#import "ZumoMainTableViewController.h"
#import "ZumoTestStore.h"
#import "ZumoTestGlobals.h"

NSString *const ZUMO_E2E_TEST_APP_NAME = @"zumoe2etestapp";

@implementation ZumoAppDelegate

- (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
{
    UIUserNotificationSettings *notificationSettings = [UIUserNotificationSettings settingsForTypes:UIUserNotificationTypeAlert | UIUserNotificationTypeBadge | UIUserNotificationTypeSound categories:nil];
    [application registerUserNotificationSettings:notificationSettings];
    [application registerForRemoteNotifications];
    self.googleDelegate = [ZumoTestGoogleSignInDelegate alloc];
    NSError* configureError;
    [[GGLContext sharedInstance] configureWithError: &configureError];
    [GIDSignIn sharedInstance].serverClientID = @"798089790547-ih5t3frldel62r59bqu3eastnlrl2347.apps.googleusercontent.com" ;
    [GIDSignIn sharedInstance].clientID = @"798089790547-eepuj4sfu96f7rj1bhotvuhpvdsta66b.apps.googleusercontent.com";
    [GIDSignIn sharedInstance].delegate = self.googleDelegate;
    return YES;
}

- (void)application:(UIApplication *)application didFailToRegisterForRemoteNotificationsWithError:(NSError *)error {
    [[ZumoTestGlobals sharedInstance] setRemoteNotificationRegistrationStatus:[NSString stringWithFormat:@"Failed to register for remote notification: %@", error]];
}

- (void)application:(UIApplication *)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken {
    [[ZumoTestGlobals sharedInstance] setRemoteNotificationRegistrationStatus:@"Successfully registered for remote notifications"];
    [ZumoTestGlobals sharedInstance].deviceToken = deviceToken;
}

- (void)application:(UIApplication *)application didReceiveRemoteNotification:(NSDictionary *)userInfo {
    id<PushNotificationReceiver> pushReceiver = [[ZumoTestGlobals sharedInstance] pushNotificationDelegate];
    if (pushReceiver) {
        [pushReceiver pushReceived:userInfo];
    }
}

- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url options:(NSDictionary<UIApplicationOpenURLOptionsKey,id> *)options
{
    if ([[url.scheme lowercaseString] isEqualToString:ZUMO_E2E_TEST_APP_NAME]) {
        // Resume login process from ZumoSafariLoginTests
        return [[ZumoTestGlobals sharedInstance].client resumeWithURL:url];
    }
    else {
        return [[GIDSignIn sharedInstance] handleURL:url
                                   sourceApplication:options[UIApplicationOpenURLOptionsSourceApplicationKey]
                                          annotation:options[UIApplicationOpenURLOptionsAnnotationKey]];
    }
}

- (BOOL)application:(UIApplication *)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation
{
    // This method is deprecated in iOS 10.
    // But it is still the entry point of openURL in iOS 8 and iOS 9.
    return [self application:application
                     openURL:url
                     options:@{}];
}

- (void)applicationWillResignActive:(UIApplication *)application
{
    // Sent when the application is about to move from active to inactive state. This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) or when the user quits the application and it begins the transition to the background state.
    // Use this method to pause ongoing tasks, disable timers, and throttle down OpenGL ES frame rates. Games should use this method to pause the game.
}

- (void)applicationDidEnterBackground:(UIApplication *)application
{
    // Use this method to release shared resources, save user data, invalidate timers, and store enough application state information to restore your application to its current state in case it is terminated later. 
    // If your application supports background execution, this method is called instead of applicationWillTerminate: when the user quits.
}

- (void)applicationWillEnterForeground:(UIApplication *)application
{
    // Called as part of the transition from the background to the inactive state; here you can undo many of the changes made on entering the background.
}

- (void)applicationDidBecomeActive:(UIApplication *)application
{
    // Restart any tasks that were paused (or not yet started) while the application was inactive. If the application was previously in the background, optionally refresh the user interface.
}

- (void)applicationWillTerminate:(UIApplication *)application
{
    // Called when the application is about to terminate. Save data if appropriate. See also applicationDidEnterBackground:.
}

@end
