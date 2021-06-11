// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoMainTableViewController.h"
#import "ZumoTestGroup.h"
#import "ZumoTestGlobals.h"
#import "ZumoTestGroupTableViewController.h"
#import "ZumoTestHelpViewController.h"

@interface ZumoMainTableViewController ()
@property (nonatomic, strong) NSIndexPath *selected;
@end

@implementation ZumoMainTableViewController

@synthesize testGroups;

- (id)init
{
    return [super initWithStyle:UITableViewStyleGrouped];
}

- (void)viewDidLoad
{
    [super viewDidLoad];

    NSArray *urlPieces = [self.appURL componentsSeparatedByString:@"."];
    [self.navigationController setTitle:urlPieces[0]];
    self.title = urlPieces[0];
    
}

-(void)viewWillAppear:(BOOL)animated
{
    self.navigationController.navigationBar.hidden = NO;
}

- (NSString *)savedAppsArchivePath {
    NSArray *documentDirectories = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    NSString *documentDirectory = [documentDirectories objectAtIndex:0];
    return [documentDirectory stringByAppendingPathComponent:@"savedapps.archive"];
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    
    // Dispose of any resources that can be recreated.
}


#pragma mark - Table view data source


- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView
{
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section
{
    return [testGroups count];
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath
{
    static NSString *CellIdentifier = @"UITableViewCell";
    
    UITableViewCell *cell = [tableView dequeueReusableCellWithIdentifier:CellIdentifier];
    if (!cell) {
        cell = [[UITableViewCell alloc] initWithStyle:UITableViewCellStyleDefault reuseIdentifier:CellIdentifier];
    }

    ZumoTestGroup *testGroup = [[self testGroups] objectAtIndex:[indexPath row]];
    [[cell textLabel] setText:[NSString stringWithFormat:@"%ld. %@", (long)[indexPath row] + 1, [testGroup name]]];
    
    return cell;
}


#pragma mark - Table view delegate


- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath
{
    self.selected = indexPath;
    [self performSegueWithIdentifier:@"RunTest" sender:self];
}

- (BOOL)textFieldShouldReturn:(UITextField *)textField {
    [textField resignFirstResponder];
    return YES;
}

-(void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender
{
    if ([segue.identifier isEqualToString:@"RunTest"]) {
        ZumoTestGroupTableViewController *controller = (ZumoTestGroupTableViewController *) segue.destinationViewController;
        
        controller.testGroup = [[self testGroups] objectAtIndex:self.selected.row];
    }
    
}
@end
