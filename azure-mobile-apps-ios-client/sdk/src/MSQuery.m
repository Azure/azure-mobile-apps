// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSQuery.h"
#import "MSPredicateTranslator.h"
#import "MSURLBuilder.h"
#import "MSSyncContextInternal.h"
#import "MSTableInternal.h"
#import "MSSyncTable.h"
#import "MSClient.h"

#pragma mark * MSQuery Implementation


@implementation MSQuery

#pragma mark * Public Initializer Methods


-(id) initWithTable:(MSTable *)table;
{
    return [self initWithTable:table predicate:nil];
}

-(id) initWithTable:(MSTable *)table predicate:(NSPredicate *)predicate
{
    return [self initWithAnyTable:table predicate:predicate];
}

-(id) initWithSyncTable:(MSSyncTable *)table;
{
    return [self initWithSyncTable:table predicate:nil];
}

-(id) initWithSyncTable:(MSSyncTable *)table predicate:(NSPredicate *)predicate
{
    return [self initWithAnyTable:table predicate:predicate];
}

- (id) initWithAnyTable:(id)table predicate:(NSPredicate *)predicate
{
    self = [super init];
    if (self)
    {
        if ([table isKindOfClass:[MSSyncTable class]]) {
            _syncTable = table;
        } else {
            _table = table;
        }
        
        _predicate = predicate;
        _fetchLimit = -1;
        _fetchOffset = -1;
    }
    return self;
}

#pragma mark * Public OrderBy Methods


-(void) orderByAscending:(NSString *)field
{
    [self orderBy:field isAscending:YES];
}

-(void) orderByDescending:(NSString *)field
{
    [self orderBy:field isAscending:NO];
}


#pragma mark * Public Read Methods


-(void) readWithCompletion:(MSReadQueryBlock)completion;
{
    return [self readInternalWithFeatures:MSFeatureNone completion:completion];
}


#pragma mark * Private Methods

-(void)readInternalWithFeatures:(MSFeatures)features completion:(MSReadQueryBlock)completion {
    // Get the query string
    NSError *error = nil;
    
    features |= MSFeatureTableReadQuery;

    // query execution logic depends on if its against a sync or remote table
    if (self.syncTable != nil) {
        [self.syncTable.client.syncContext readWithQuery:self completion:completion];
    } else {
        NSString *queryString = [self queryStringOrError:&error];
        
        if (!queryString) {
            // Query string is invalid, so call error handler
            if (completion) {
                completion(nil, error);
            }
        }
        else {
            // Call read with the query string
            [self.table readWithQueryStringInternal:queryString features:features completion:completion];
        }
    }
}

-(void) orderBy:(NSString *)field isAscending:(BOOL)isAscending
{
    NSAssert(field != nil, @"'field' can not be nil.");
    
    NSMutableArray *currentOrderBy = [self.orderBy mutableCopy];
    if (currentOrderBy == nil) {
        currentOrderBy = [NSMutableArray array];
    }
    
    NSSortDescriptor *sort = [NSSortDescriptor sortDescriptorWithKey:field
                                                           ascending:isAscending];

    [currentOrderBy addObject:sort];
    self.orderBy = currentOrderBy;
}


-(NSString *) queryStringOrError:(NSError **)error
{
    return [MSURLBuilder queryStringFromQuery:self orError:error];
}


#pragma mark * Overridden Methods


-(NSString *) description
{
    return [self queryStringOrError:nil];
}

-(id)copyWithZone:(NSZone *)zone
{
    MSQuery *query = [[MSQuery allocWithZone:zone] init];
    query.predicate = [self.predicate copyWithZone:zone];
    query.parameters = [self.parameters copyWithZone:zone];
    query.selectFields = [query.selectFields copyWithZone:zone];
    query.fetchLimit = self.fetchLimit;
    query.fetchOffset = self.fetchOffset;
    query.orderBy = [self.orderBy copyWithZone:zone];
    query.includeTotalCount = self.includeTotalCount;
    
    if (self.syncTable) {
        query.syncTable = [[MSSyncTable alloc] initWithName:self.syncTable.name
                                                     client:self.syncTable.client];
    }
    if (self.table) {
        query.table = [[MSTable alloc] initWithName:self.table.name
                                             client:self.table.client];
    }
    
    return query;
}

@end
