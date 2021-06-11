// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSQueryResult.h"

@implementation MSQueryResult

-(id)initWithItems:(NSArray<NSDictionary *> *)items
        totalCount:(NSInteger) totalCount
          nextLink: (NSString *) nextLink
{
    self = [super init];
    if (self) {
        _totalCount = totalCount;
        _items = items;
        _nextLink = nextLink;
    }
    
    return self;
}

@end
