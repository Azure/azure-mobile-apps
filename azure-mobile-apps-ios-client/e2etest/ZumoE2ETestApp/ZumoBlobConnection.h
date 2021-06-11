// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>

@interface ZumoBlobConnection : NSObject

-(id) initWithStorageURL:(NSString *)url token:(NSString *)token;

-(void) uploadJson:(id)jsonObject withFileName:(NSString *)name completion:(void(^)(NSError *error))completion;

-(void) uploadLog:(NSArray *)log withFileName:(NSString *)name completion:(void(^)(NSError *error))completion;

+(unsigned long long) fileTime:(NSDate *)time;

@end
