// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "ZumoTest.h"

@interface ZumoLoginTests : NSObject

+ (NSArray *)createTests;
+ (NSString *)groupDescription;

+ (ZumoTest *)createServerFlowLoginTestForProvider:(NSString *)provider usingSimplifiedMode:(BOOL)useSimplified;
+ (ZumoTest *)createLogoutTest;

@end
