// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import "MSBlockDefinitions.h"

@class MSSyncContext;
@class MSQuery;

@interface MSQueuePullOperation : NSOperation

- (id) initWithSyncContext:(MSSyncContext *)syncContext
                     query:(MSQuery *)query
                   queryId:(NSString *)queryId
                maxRecords:(NSInteger)maxRecords
             dispatchQueue:(dispatch_queue_t)dispatchQueue
             callbackQueue:(NSOperationQueue *)callbackQueue
                completion:(MSSyncBlock)completion;

@end
