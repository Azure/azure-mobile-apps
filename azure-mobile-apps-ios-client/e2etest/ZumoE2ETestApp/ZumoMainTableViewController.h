// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <UIKit/UIKit.h>
#import "ZumoTestGroupCallbacks.h"

@interface ZumoMainTableViewController : UITableViewController <UITextFieldDelegate>

@property (nonatomic, copy) NSArray *testGroups;
@property (nonatomic, strong) NSString *appURL;
@property (nonatomic, strong) NSString *appKey;
@property (nonatomic, strong) NSString *clientId;
@property (nonatomic, strong) NSString *clientSecret;

@end
