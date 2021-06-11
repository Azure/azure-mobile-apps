// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>

@interface MSInstallationTemplate : NSObject

#pragma mark * Public Properties

/// @name Properties
/// @{

/// Template body for notification payload which may contain placeholders to be filled in with actual data during the send operation.
@property (nonatomic, strong) NSString *body;

/// Expiry applicable for APNS-targeted notifications
@property (nonatomic, strong) NSString *expiry;

/// A list of tags for a particular template.
@property (nonatomic, strong) NSArray *tags;

/// @}

#pragma  mark * Public Static Initializer Methods

/// @name Initializing the MSInstallationTemplate object
/// @{

///Initializes an |MSInstallationTemplate| with the given properties.
+(id) installationTemplateWithBody:(NSString *)body
                            expiry:(NSString *)expiry
                              tags:(NSArray *)tags;

/// @}
@end
