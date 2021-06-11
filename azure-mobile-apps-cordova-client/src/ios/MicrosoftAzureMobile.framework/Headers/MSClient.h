// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#if TARGET_OS_IPHONE
#import <UIKit/UIKit.h>
#endif
#import "MSError.h"
#import "MSBlockDefinitions.h"

@class MSTable;
@class MSUser;
@class MSSyncTable;
@class MSPush;
@class MSSyncContext;
@class MSLoginController;
@class MSLoginSafariViewController;

@protocol MSFilter;

NS_ASSUME_NONNULL_BEGIN

#pragma  mark * MSClient Public Interface

/// The MSClient class is the starting point for working with a Microsoft Azure
/// Mobile Service on a client device. An instance of the *MSClient* class is
/// created with a URL pointing to a Microsoft Azure Mobile Service. The *MSClient*
/// allows the developer to get MSTable instances, which are used to work with
/// the data of the Microsoft Azure Mobile Service, as well as login and logout an
/// end user.
@interface MSClient : NSObject <NSCopying>

#pragma mark * Public Readonly Properties

/// @name Properties
/// @{

/// The URL of the Microsoft Azure Mobile App
@property (nonatomic, strong, readonly, nonnull) NSURL *applicationURL;

/// If set, overrides the host used during all login operations, primarily intended
/// for use when running your server locally
@property (nonatomic, strong, nonnull) NSURL *loginHost;

/// If set, overrides the path used during a login request, defaults to '.auth/login'
/// For legacy usage, this can be set to 'login'
@property (nonatomic, strong, nonnull) NSString *loginPrefix;

/// A collection of MSFilter instances to apply to use with the requests and
/// responses sent and received by the client. The property is readonly and the
/// array is not-mutable. To apply a filter to a client, use the withFilter:
/// method.
@property (nonatomic, strong, readonly, nullable) NSArray<id<MSFilter>> *filters;

/// A sync context that defines how offline data is synced and allows for manually
/// syncing data on demand
@property (nonatomic, strong, nullable) MSSyncContext *syncContext;

/// @name Registering and unregistering for push notifications

/// The property to use for registering and unregistering for notifications via *MSPush*.
@property (nonatomic, strong, readonly, nullable) MSPush *push;

/// @}

#pragma mark * Public ReadWrite Properties


/// The currently logged in user. While the currentUser property can be set
/// directly, the login* and logout methods are more convenient and
/// recommended for non-testing use.
@property (nonatomic, strong, nullable) MSUser *currentUser;

/// An instance of |MSLoginSafariViewController|
@property (nonatomic, strong, nonnull) MSLoginSafariViewController *loginSafariViewController;

/// @}

#pragma  mark * Public Static Constructor Methods

/// @name Initializing the MSClient Object
/// @{

/// Creates a client with the given URL for the Microsoft Azure Mobile Service.
+(nonnull MSClient *)clientWithApplicationURLString:(nonnull NSString *)urlString;

/// Creates a client with the given URL for the Microsoft Azure Mobile Service.
+(nonnull MSClient *)clientWithApplicationURL:(nonnull NSURL *)url;

#pragma  mark * Public Initializer Methods

/// Initializes a client with the given URL for the Microsoft Azure Mobile Service.
-(nonnull instancetype)initWithApplicationURL:(nonnull NSURL *)url;

#pragma mark * Public Filter Methods

/// Creates a clone of the client with the given filter applied to the new client.
-(nonnull MSClient *)clientWithFilter:(nonnull id<MSFilter>)filter;

///@}

/// @name Authenticating Users
/// @{

#pragma mark * Public Login and Logout Methods

#if TARGET_OS_IPHONE
/// Logs in the current end user with the given provider by presenting the
/// MSLoginController with the given controller.
-(void)loginWithProvider:(nonnull NSString *)provider
              controller:(nonnull UIViewController *)controller
                animated:(BOOL)animated
              completion:(nullable MSClientLoginBlock)completion
DEPRECATED_MSG_ATTRIBUTE("Deprecated. Use SFSafariViewController-based login method loginWithProvider:urlScheme:controller:animated:completion instead");

/// Logs in the current end user with the given provider by presenting the
/// MSLoginController with the given controller.
-(void)loginWithProvider:(nonnull NSString *)provider
              parameters:(nullable NSDictionary *)parameters
              controller:(nonnull UIViewController *)controller
                animated:(BOOL)animated
              completion:(nullable MSClientLoginBlock)completion
DEPRECATED_MSG_ATTRIBUTE("Deprecated. Use SFSafariViewController-based login method loginWithProvider:urlScheme:parameters:controller:animated:completion instead");

/// Returns an MSLoginController that can be used to log in the current
/// end user with the given provider.
-(nonnull MSLoginController *)loginViewControllerWithProvider:(nonnull NSString *)provider
                                                   completion:(nullable MSClientLoginBlock)completion
DEPRECATED_MSG_ATTRIBUTE("Deprecated. Use SFSafariViewController-based login method loginWithProvider:urlScheme:controller:animated:completion instead");

/// Thread-safe login method which can be called from any thread.
/// Logs in the current end user with given provider by presenting the
/// SFSafariViewController with the given controller. The URL scheme of
/// the current application is required for completing login.
/// Login completion will always be called from the main thread.
/// As SFSafariViewController is only available on iOS 9 or later, on old platforms,
/// fallback to browser login. Note that there can be various reasons causing browser
/// fail to complete the login flow (on older platforms) and login flow won't work anymore.
/// In this case, kill & restart the application should make it work again.
-(void)loginWithProvider:(nonnull NSString *)provider
               urlScheme:(nonnull NSString *)urlScheme
              controller:(nonnull UIViewController *)controller
                animated:(BOOL)animated
              completion:(nullable MSClientLoginBlock)completion;

/// Thread-safe login method which can be called from any thread.
/// Logs in the current end user with given provider and given login parameters by presenting the
/// SFSafariViewController with the given controller. The URL scheme of
/// the current application is required for completing login.
/// Login completion will always be called from the main thread.
/// As SFSafariViewController is only available on iOS 9 or later, on old platforms,
/// fallback to browser login. Note that there can be various reasons causing browser
/// fail to complete the login flow (on older platforms) and login flow won't work anymore.
/// In this case, kill & restart the application should make it work again.
-(void)loginWithProvider:(nonnull NSString *)provider
               urlScheme:(nonnull NSString *)urlScheme
              parameters:(nullable NSDictionary *)parameters
              controller:(nonnull UIViewController *)controller
                animated:(BOOL)animated
              completion:(nullable MSClientLoginBlock)completion;

/// Thread-safe method which should only be called from the main thread.
/// Resume login process with the specified URL.
-(BOOL)resumeWithURL:(NSURL *)URL;
#endif

/// Logs in the current end user with the given provider and the given token for
/// the provider.
-(void)loginWithProvider:(nonnull NSString *)provider
                   token:(nonnull NSDictionary *)token
              completion:(nullable MSClientLoginBlock)completion;

/// Logs out the current end user.
-(void)logoutWithCompletion:(nullable MSClientLogoutBlock)completion;

/// Refreshes access token with the identity provider for the logged in user.
-(void)refreshUserWithCompletion:(nullable MSClientLoginBlock)completion;

/// @}

#pragma mark * Public Table Methods

/// @name Querying Tables
/// @{

/// Returns an MSTable instance for a table with the given name.
-(nonnull MSTable *)tableWithName:(nonnull NSString *)tableName;

/// Returns an MSSyncTable instance for a table with the given name.
-(nonnull MSSyncTable *)syncTableWithName:(nonnull NSString *)tableName;

/// @}

#pragma mark * Public invokeAPI Methods

/// @name Invoking Custom APIs
/// @{

/// Invokes a user-defined API of the Mobile Service.  The HTTP request and
/// response content will be treated as JSON.
-(void)invokeAPI:(nonnull NSString *)APIName
            body:(nullable id)body
      HTTPMethod:(nullable NSString *)method
      parameters:(nullable NSDictionary *)parameters
         headers:(nullable NSDictionary *)headers
      completion:(nullable MSAPIBlock)completion;

/// Invokes a user-defined API of the Mobile Service.  The HTTP request and
/// response content can be of any media type.
-(void)invokeAPI:(nonnull NSString *)APIName
            data:(nullable NSData *)data
      HTTPMethod:(nullable NSString *)method
      parameters:(nullable NSDictionary *)parameters
         headers:(nullable NSDictionary *)headers
      completion:(nullable MSAPIDataBlock)completion;

/// @}


#pragma mark * Public Connection Methods


/// @name Controlling connections to the server
/// @{

/// Determines where connections made to the mobile service are run. If set, connection related
/// logic will occur on this queue. Otherwise, the thread that made the call will be used.
@property (nonatomic, strong, nullable) NSOperationQueue *connectionDelegateQueue;

/// @}

@end

NS_ASSUME_NONNULL_END
