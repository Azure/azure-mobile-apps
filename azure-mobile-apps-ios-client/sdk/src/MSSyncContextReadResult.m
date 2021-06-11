// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSSyncContextReadResult.h"

@implementation MSSyncContextReadResult

- (id)initWithCount:(NSInteger)count items:(NSArray<NSDictionary *> *)items;
{
    self = [super init];
    if (self) {
        _totalCount = count;
        _items = items;
    }
    
    return self;
}

@end
