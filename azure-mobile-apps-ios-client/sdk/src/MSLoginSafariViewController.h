// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <UIKit/UIKit.h>
#import "MSClient.h"

NS_ASSUME_NONNULL_BEGIN

@class MSClient;

@interface MSLoginSafariViewController : UIViewController

@property (nonatomic, weak, nullable) MSClient* client;

/// Initializes an |MSLoginSafariViewController| instance with the given client
- (instancetype)initWithClient:(MSClient *)client;


/// Logs in the current end user with given provider by presenting the
/// SFSafariViewController with the given controller.
- (void)loginWithProvider:(NSString *)provider
                urlScheme:(NSString *)urlScheme
               parameters:(nullable NSDictionary *)parameters
               controller:(UIViewController *)controller
                 animated:(BOOL)animated
               completion:(nullable MSClientLoginBlock)completion;


/// Resume login process with the specified URL
- (BOOL)resumeWithURL:(NSURL *)URL;

@end

NS_ASSUME_NONNULL_END
