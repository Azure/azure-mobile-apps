// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoTestResultViewController.h"

@interface ZumoTestResultViewController ()

@end

@implementation ZumoTestResultViewController

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        // Custom initialization
    }
    return self;
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    // Do any additional setup after loading the view.
    self.title = self.test.name;
    
    NSMutableAttributedString *fullText = [NSMutableAttributedString new];
                                           
    NSAttributedString *title = [[NSAttributedString alloc] initWithString:self.test.description attributes:@{NSFontAttributeName: [UIFont boldSystemFontOfSize:14.0]}];
    
    [fullText appendAttributedString:title];
    [fullText appendAttributedString:[[NSAttributedString alloc] initWithString:@"\n\n"]];
    
    NSString *logs = [self.test.logs componentsJoinedByString:@"\n"];
    [fullText appendAttributedString:[[NSAttributedString alloc] initWithString:logs]];
    
    self.logDisplay.attributedText = fullText;
    
    // UI Bug?
    self.logDisplay.scrollEnabled = NO;
    self.logDisplay.scrollEnabled = YES;
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
}

@end
