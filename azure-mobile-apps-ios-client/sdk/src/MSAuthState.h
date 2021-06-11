// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSBlockDefinitions.h"

NS_ASSUME_NONNULL_BEGIN

@interface MSAuthState : NSObject

@property (nonatomic, readonly) NSString *provider;
@property (nonatomic, readonly, nullable) MSClientLoginBlock loginCompletion;
@property (nonatomic, readonly) NSString *codeVerifier;
@property (nonatomic, readonly) NSString *urlScheme;
@property (nonatomic, readonly) BOOL animated;

/// Initialize an instance of MSAuthState
- (instancetype)initWithProvider:(NSString *)provider
                 loginCompletion:(nullable MSClientLoginBlock)loginCompletion
                    codeVerifier:(NSString *)codeVerifier
                       urlScheme:(NSString *)urlScheme
                        animated:(BOOL)animated;

@end

NS_ASSUME_NONNULL_END
