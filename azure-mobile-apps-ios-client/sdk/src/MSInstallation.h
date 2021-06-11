// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>

@interface MSInstallation : NSObject 

#pragma mark * Public Properties

/// @name Properties
/// @{

/// Expiration time of the installation.
@property (nonatomic, strong, readonly, nullable) NSDate *expirationTime;

/// Globally unique identifier of the installation.
@property (nonatomic, strong, nonnull) NSString *installationId;

/// Notification platform of the installation.
@property (nonatomic, strong, nonnull) NSString *platform;

/// Registration Id, token or URI obtained from platform-specific notification service.
@property (nonatomic, strong, nonnull) NSString *pushChannel;

/// A collection of push variables.
@property (nonatomic, strong, nullable) NSDictionary *pushVariables;

/// A list of tags.
@property (nonatomic, strong, nullable) NSArray *tags;

/// A collection of templates.
@property (nonatomic, strong, nullable) NSDictionary *templates;

/// Installation expired or not.
@property (nonatomic, readonly) BOOL pushChannelExpired;

/// @}

#pragma  mark * Public Static Initializer Methods

/// @name Initializing the MSInstallation object
/// @{

/// Initializes an |MSInstallation| with the given properties.
+(nonnull id) installationWithInstallationId:(nonnull NSString *)installationId
                                    platform:(nonnull NSString *)platform
                                 pushChannel:(nonnull NSString *)pushChannel
                               pushVariables:(nullable NSDictionary *)pushVariables
                                        tags:(nullable NSArray *) tags
                                   templates:(nullable NSDictionary *) templates
                              expirationTime:(nullable NSDate *) expirationTime
                          pushChannelExpired:(BOOL) pushChannelExpired;

///Initializes an |MSInstallation| with the given properties.
+(nonnull id) installationWithInstallationId:(nonnull NSString *)installationId
                                    platform:(nonnull NSString *)platform
                                 pushChannel:(nonnull NSString *)pushChannel
                               pushVariables:(nullable NSDictionary *)pushVariables
                                        tags:(nullable NSArray *) tags
                                   templates:(nullable NSDictionary *) templates;

/// @}

@end
