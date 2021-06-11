//
//  ZumoTestResultViewController.h
//  ZumoE2ETestApp
//
//  Created by Phillip Van Nortwick on 8/12/14.
//  Copyright (c) 2014 Microsoft. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "ZumoTest.h"

@interface ZumoTestResultViewController : UIViewController

@property (nonatomic, weak) ZumoTest *test;

@property (weak, nonatomic) IBOutlet UITextView *logDisplay;

@end
