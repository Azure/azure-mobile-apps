// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoTestGroupTableViewController.h"
#import "ZumoTestHelpViewController.h"
#import "ZumoTestGlobals.h"
#import "ZumoTestStore.h"
#import "ZumoBlobConnection.h"
#import <MessageUI/MFMailComposeViewController.h>
#import "ZumoTestResultViewController.h"
#import "ZumoTestCallbacks.h"
#import <Google/SignIn.h>

@interface ZumoTestGroupTableViewController () <MFMailComposeViewControllerDelegate>

@property (nonatomic, strong) ZumoBlobConnection *blobConnection;
@property (nonatomic, strong) NSString *runName;
@property (nonatomic, strong) NSString *runId;
@property (nonatomic, strong) NSIndexPath *selectedRow;
@end

@implementation ZumoTestGroupTableViewController

@synthesize testGroup, logUploadUrl;

- (void)viewDidLoad
{
    [super viewDidLoad];
    NSString *groupName = [self.testGroup name];
    
    ZumoTestGlobals *globals = [ZumoTestGlobals sharedInstance];
    
    if (globals.storageURL && ![globals.storageURL isEqualToString:@""]) {
        self.blobConnection = [[ZumoBlobConnection alloc] initWithStorageURL:globals.storageURL token:globals.storageToken];
    }

    [GIDSignIn sharedInstance].uiDelegate = self;

    self.navigationItem.title = groupName;
    
    [self resetTests:self];
    
    if ([groupName hasPrefix:ALL_TESTS_GROUP_NAME]) {
        // Start running the tests
        [self runTests:nil];
    }
}

-(void) viewWillAppear:(BOOL)animated
{
    [self.navigationController setToolbarHidden:NO animated:YES];
}

- (void) viewWillDisappear:(BOOL)animated
{
    [self.navigationController setToolbarHidden:YES animated:animated];
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    
    // Dispose of any resources that can be recreated.
}

-(void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender
{
    if ([segue.identifier isEqualToString:@"details"]) {
        ZumoTestResultViewController *vc = (ZumoTestResultViewController *) segue.destinationViewController;
        vc.test = self.testGroup.tests[self.selectedRow.row];
    }
}

#pragma mark - Table view data source

-(void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath
{
    
    self.selectedRow = indexPath;
    [self performSegueWithIdentifier:@"details" sender:self];
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView
{
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section
{
    return self.testGroup.tests.count;
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath
{
    static NSString *CellIdentifier = @"test";
    
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:CellIdentifier];
    if (!cell) {
        cell = [[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:CellIdentifier];
    }
    
    ZumoTest *test = [[[self testGroup] tests] objectAtIndex:[indexPath row]];
    UIColor *textColor;
    if ([test testStatus] == TSFailed) {
        textColor = [UIColor redColor];
    } else if ([test testStatus] == TSPassed) {
        textColor = [UIColor greenColor];
    } else if ([test testStatus] == TSRunning) {
        textColor = [UIColor grayColor];
    } else if ([test testStatus] == TSSkipped) {
        textColor = [UIColor magentaColor];
    } else {
        textColor = [UIColor blackColor];
    }
    
    cell.textLabel.textColor = textColor;
    cell.textLabel.text = test.description;
    cell.textLabel.minimumScaleFactor = 0.5;
    return cell;
}

- (BOOL)textFieldShouldReturn:(UITextField *)textField {
    [textField resignFirstResponder];
    return YES;
}

- (IBAction)runTests:(id)sender {
    NSLog(@"Start running tests!");
    
    self.testGroup.delegate = self;

    void (^runTestBlock)(NSString *stringId);
    runTestBlock = ^(NSString *runId) {
        __weak UIViewController *weakSelf = self;
        self.runId = runId;
        [self.testGroup startExecutingFrom:weakSelf];        
    };
    
    runTestBlock(@"Local");
}

- (IBAction)resetTests:(id)sender {
    ZumoTest *test;
    for (test in [[self testGroup] tests]) {
        [test resetStatus];
    }
    [self.tableView reloadData];
}

- (void)zumoTestGroupFinished:(NSString *)groupName withPassed:(int)passedTests andFailed:(int)failedTests andSkipped:(int)skippedTests {
    
    NSMutableArray *testResults = [NSMutableArray new];
    
    UIDevice *device = [UIDevice currentDevice];
    NSString *shortId =  [device.identifierForVendor.UUIDString substringToIndex:8];
    NSString *platform = [NSString stringWithFormat:@"%@_%@_%@_%@", device.systemName, device.systemVersion, device.model, shortId];
    
    dispatch_group_t group;
    if (self.blobConnection) {
        group = dispatch_group_create();
    }
    
    for (ZumoTest *test in self.testGroup.tests) {
        NSArray *log = test.formattedLog;
        if (log) {
            NSString *logFileName = [NSString stringWithFormat:@"%@/%@.txt", platform, [[NSUUID UUID] UUIDString]];
            
            if (self.blobConnection) {
                dispatch_group_enter(group);
                [self.blobConnection uploadLog:log withFileName:logFileName completion:^(NSError *error) {
                    dispatch_group_leave(group);
                }];
            }
            
            NSString *groupName = test.groupName ? test.groupName : self.testGroup.name;
            [testResults addObject:@{
                @"full_name" : test.name,
                @"source" : groupName,
                @"outcome" : [ZumoTest testStatusToString:test.testStatus],
                @"start_time" : [NSString stringWithFormat:@"%lld", [ZumoBlobConnection fileTime:test.startTime]],
                @"end_time" : [NSString stringWithFormat:@"%lld", [ZumoBlobConnection fileTime:test.endTime]],
                @"reference_url" : logFileName
            }];
        }
    }
    
    TestStatus finalTestStatus = self.testGroup.testsFailed > 0 ? TSFailed : TSPassed;
    
    if (self.blobConnection) {
        NSString *detailBlobName = [platform stringByAppendingString:@"-detail.json"];
        dispatch_group_enter(group);
        [self.blobConnection uploadJson:testResults withFileName:detailBlobName completion:^(NSError *error) {
            dispatch_group_leave(group);
        }];
        
        dispatch_group_wait(group, DISPATCH_TIME_FOREVER);

        ZumoTestGlobals *globals = [ZumoTestGlobals sharedInstance];
        NSString *serverVersion = globals.globalTestParameters[RUNTIME_VERSION_TAG];
        if (serverVersion == nil) {
            serverVersion = @"Unknown";
        }
        
        
        NSDictionary *finalResult = @{
            @"full_name" : platform,
            @"outcome" : [ZumoTest testStatusToString:finalTestStatus],
            @"total_count" : [NSNumber numberWithInt:self.testGroup.testsPassed + self.testGroup.testsFailed],
            @"passed" : [NSNumber numberWithInt:self.testGroup.testsPassed],
            @"failed" : [NSNumber numberWithInt:self.testGroup.testsFailed],
            @"skipped" : [NSNumber numberWithInt:self.testGroup.testsSkipped],
            @"start_time":[NSString stringWithFormat:@"%lld", [ZumoBlobConnection fileTime:self.testGroup.startTime]],
            @"end_time":[NSString stringWithFormat:@"%lld", [ZumoBlobConnection fileTime:self.testGroup.endTime]],
            @"reference_url": detailBlobName,
            @"server" : serverVersion
        };
        
        [self.blobConnection uploadJson:finalResult withFileName:[platform stringByAppendingString:@"-master.json"] completion:^(NSError *error) {
            dispatch_async(dispatch_get_main_queue(), ^{
                UIAlertView *av = [[UIAlertView alloc] initWithTitle:@"Tests Complete" message:@"reported" delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
                [av show];
            });
        }];
    } else {
        UIAlertView *av = [[UIAlertView alloc] initWithTitle:@"Tests Complete"
                                                     message:[ZumoTest testStatusToString:finalTestStatus]
                                                    delegate:nil
                                           cancelButtonTitle:@"OK"
                                           otherButtonTitles:nil];
        [av show];
    }
}

- (void)zumoTestGroupSingleTestFinished:(int)testIndex withResult:(TestStatus)testStatus {
    [self.testGroup.tests[testIndex] setTestStatus:testStatus];
    [self.tableView reloadData];
}

- (void)zumoTestGroupSingleTestStarted:(int)testIndex {
    [self.testGroup.tests[testIndex] setTestStatus:TSRunning];
    [self.tableView reloadRowsAtIndexPaths:[NSArray arrayWithObject:[NSIndexPath indexPathForRow:testIndex inSection:0]] withRowAnimation:UITableViewRowAnimationAutomatic];
}

- (void)zumoTestGroupStarted:(NSString *)groupName {
    NSLog(@"Test group started: %@", groupName);
}

- (IBAction)mailResults:(id)sender {
    if (![MFMailComposeViewController canSendMail]) {
        return;
    }
    
    MFMailComposeViewController* controller = [[MFMailComposeViewController alloc] init];
    
    controller.mailComposeDelegate = self;
    
    [controller setSubject:@"Test Results"];
    
    NSMutableArray *allLogs = [NSMutableArray array];
    for (ZumoTest *test in self.testGroup.tests) {
        [allLogs addObjectsFromArray:test.formattedLog];
    }
    
    NSString *message = [allLogs componentsJoinedByString:@"\n"];
    
    [controller setMessageBody:message isHTML:NO];
    
    if (controller) {
        [self presentViewController:controller animated:YES completion:nil];
    }
}

- (void)mailComposeController:(MFMailComposeViewController*)controller
          didFinishWithResult:(MFMailComposeResult)result
                        error:(NSError*)error;
{
    if (result == MFMailComposeResultSent) {
        NSLog(@"Logs sent");
    }
    
    [self dismissViewControllerAnimated:YES completion:nil];
}

@end
