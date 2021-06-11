// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSAuthState.h"

@implementation MSAuthState

- (instancetype)initWithProvider:(NSString *)provider
                 loginCompletion:(nullable MSClientLoginBlock)loginCompletion
                    codeVerifier:(NSString *)codeVerifier
                       urlScheme:(NSString *)urlScheme
                        animated:(BOOL)animated
{
    self = [super init];

    if (self) {
        _provider = [provider copy];
        _loginCompletion = [loginCompletion copy];
        _codeVerifier = [codeVerifier copy];
        _urlScheme = [urlScheme copy];
        _animated = animated;
    }
    
    return self;
}

@end
