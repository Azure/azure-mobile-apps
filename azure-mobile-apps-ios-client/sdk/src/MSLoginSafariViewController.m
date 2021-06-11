// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <UIKit/UIKit.h>
#import <SafariServices/SafariServices.h>
#import "MSClientInternal.h"
#import "MSLoginSafariViewController.h"
#import "MSLoginSafariViewControllerUtilities.h"
#import "MSAuthState.h"
#import "MSClientConnection.h"
#import "MSLoginSerializer.h"
#import "MSJSONSerializer.h"
#import "MSUser.h"
#import "MSURLBuilder.h"

@interface MSLoginSafariViewController() <SFSafariViewControllerDelegate>

#pragma mark * Private Properties

@property (nonatomic, nullable) MSAuthState *authState;

@property (nonatomic, nullable) SFSafariViewController *safariViewController;

@end

@implementation MSLoginSafariViewController

#pragma mark * Public Constructor Methods

- (instancetype)initWithClient:(MSClient *)client
{
    self = [super init];
    
    if (self) {
        _client = client;
    }
    
    return self;
}

#pragma mark * Public Login Methods

- (void)loginWithProvider:(NSString *)provider
                urlScheme:(NSString *)urlScheme
               parameters:(nullable NSDictionary *)parameters
               controller:(UIViewController *)controller
                 animated:(BOOL)animated
               completion:(nullable MSClientLoginBlock)completion
{
    dispatch_async(dispatch_get_main_queue(),^{
        [self loginInternalWithProvider:provider
                          urlScheme:urlScheme
                         parameters:parameters
                         controller:controller
                           animated:animated
                         completion:completion];
    });
}

- (BOOL)resumeWithURL:(NSURL *)URL
{
    if ([NSThread isMainThread]) {
        
        if (self.authState) {
            
            NSURL *codeExchangeRequestURL = [self codeExchangeRequestURLFromRedirectURL:URL];

            if (codeExchangeRequestURL) {
                [self codeExchangeWithURL:codeExchangeRequestURL completion:^(MSUser *user, NSError *error) {
                    if (self.safariViewController) {
                        [self.safariViewController dismissViewControllerAnimated:self.authState.animated completion:^{
                            [self completeLoginWithUser:user responseError:error];
                        }];
                    }
                    else {
                        // For unit testing purpose only - call the completion
                        // regardless whether self.safariViewController is null or not
                        [self completeLoginWithUser:user responseError:error];
                    }
                }];

                return YES;
            }
        }
    }
    return NO;
}

#pragma mark * Private Login Methods

- (void)loginInternalWithProvider:(NSString *)provider
                        urlScheme:(NSString *)urlScheme
                       parameters:(nullable NSDictionary *)parameters
                       controller:(UIViewController *)controller
                         animated:(BOOL)animated
                       completion:(nullable MSClientLoginBlock)completion
{
    if (self.authState) {
        NSError *error = [self errorWithDescriptionKey:@"Login failed because another login operation in progress."
                                          andErrorCode:MSLoginOperationInProgress];
        completion(nil, error);
        return;
    }
    
    self.authState = [[MSAuthState alloc] initWithProvider:[MSLoginSafariViewControllerUtilities normalizeProvider:provider]
                                           loginCompletion:completion
                                              codeVerifier:[MSLoginSafariViewControllerUtilities generateCodeVerifier]
                                                 urlScheme:urlScheme
                                                  animated:animated];
    
    NSURL *fullURL = [MSLoginSafariViewControllerUtilities fullURLFromLoginURL:self.client.loginURL
                                                                      provider:self.authState.provider
                                                                     urlScheme:urlScheme
                                                                    parameters:parameters
                                                                  codeVerifier:self.authState.codeVerifier
                                                           codeChallengeMethod:@"S256"];
    
    // SFSafariViewController is part of SafariServices API, where as
    // SafariServices API is only available on iOS 9 or later, not iOS 8 or prior.
    // When SafariServices is not avaiable, fallback to browser to open the URL.
    
    if ([SFSafariViewController class]) {
        
        self.safariViewController = [[SFSafariViewController alloc] initWithURL:fullURL entersReaderIfAvailable:NO];
        
        self.safariViewController.delegate = self;
        
        [controller presentViewController:self.safariViewController animated:animated completion:nil];
    }
    else {
        
        // Fallback to browser can only happen on older platforms (iOS 8 or prior)
        // where SFSafariViewController is not available.
        // Suppress compiler deprecation warning as openURL method is only used
        // in the context of iOS 8, despite it's already deprecated in iOS 10.

#pragma clang diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
        BOOL openedSafari = [[UIApplication sharedApplication] openURL:fullURL];
#pragma clang diagnostic pop
        
        if (!openedSafari) {
            NSError *error = [self errorWithDescriptionKey:@"Browser cannot be opened." andErrorCode:MSLoginCanceled];
            completion(nil, error);
        }
    }
}

- (NSURL *)codeExchangeRequestURLFromRedirectURL:(NSURL *)URL
{
    NSURL *codeExchangeURL = nil;
    
    BOOL isRedirectURLValid = [MSLoginSafariViewControllerUtilities isRedirectURLValid:URL withUrlScheme:self.authState.urlScheme];
    
    if (isRedirectURLValid) {
        NSString *authorizationCode = [MSLoginSafariViewControllerUtilities authorizationCodeFromRedirectURL:URL];
        
        if (authorizationCode) {
            
            codeExchangeURL = [MSLoginSafariViewControllerUtilities codeExchangeURLFromApplicationURL:self.client.applicationURL
                                                                     provider:self.authState.provider
                                                                     authorizationCode:authorizationCode
                                                                codeVerifier:self.authState.codeVerifier];
        }
    }
    
    return codeExchangeURL;
}

- (void)codeExchangeWithURL:(NSURL *)URL
                 completion:(MSClientLoginBlock)completion
{
    NSURLRequest *request = [NSURLRequest requestWithURL:URL];
    
    // Call the token endpoint for code exchange.
    // If response is 200 OK, dismiss safari view controller and call login
    // completion with user.
    // If response is non-400 error, dismiss safari view controller and
    // call login completion with response error.
    // Ignore 400 error. It means something wrong with code exchange, could be a malicious caller
    // with a bogus code verifier.
    
    MSResponseBlock responseCompletion = nil;
    
    if (completion) {
        
        responseCompletion = ^(NSHTTPURLResponse *response, NSData *data, NSError *responseError) {
        
            if (!responseError) {
                if (response.statusCode == 200) {
                    
                    MSUser *user = [[MSLoginSerializer loginSerializer] userFromData:data orError:&responseError];
                    
                    if (user && !responseError) {
                        self.client.currentUser = user;
                        completion(user, responseError);
                    }
                }
                else if (response.statusCode == 400) {
                    
                    // A 400 error can be due to an malformed request to the server by the SDK OR a malicious caller
                    // with an invalid auth code. At this point the server intentionally omit the detailed reason
                    // of the 400. We always assume the 400 is caused by a malicious caller and ignore it silently.
                    // Handling 400 error and notifying the user would mess up the auth flow and make the app
                    // less secure.
                    
                }
                else if (response.statusCode > 400) {
                    
                    // A non-400 error is unlikely to be caused by a malicious caller with an invalid auth code.
                    // So we don't ignore such error.
                    
                    responseError = [[MSJSONSerializer JSONSerializer] errorFromData:data MIMEType:response.MIMEType];
                    completion(nil, responseError);
                }
            }
        };
    }
    
    // Create the connection and start it
    MSClientConnection *connection = [[MSClientConnection alloc] initWithRequest:request
                                                              client:self.client
                                                              completion:responseCompletion];
    [connection start];
}

- (void)completeLoginWithUser:(MSUser *)user
                responseError:(NSError *)responseError
{
    // Saving the loginCompletion callback before resetting safariLoginFlow to nil.
    // Call the loginCompletion at the end.
    
    MSClientLoginBlock loginCompletion = [self.authState.loginCompletion copy];
    
    self.authState = nil;
    
    loginCompletion(user, responseError);
}

#pragma mark * SFSafariViewControllerDelegate Private Implementation

- (void)safariViewControllerDidFinish:(SFSafariViewController *)controller
{
    if (controller != self.safariViewController) {
        // Ignore this call if safari view controller doesn't match
        return;
    }
    
    if (!self.authState) {
        // Ignore this call if there is no pending login flow
        return;
    }
    
    [self completeLoginWithUser:nil responseError:[self errorWithDescriptionKey:@"The login operation was canceled." andErrorCode:MSLoginCanceled]];
}

#pragma mark * Private NSError Generation Methods

- (NSError *) errorWithDescriptionKey:(NSString *)descriptionKey
                        andErrorCode:(NSInteger)errorCode
{
    NSString *description = NSLocalizedString(descriptionKey, nil);
    NSDictionary *userInfo = @{ NSLocalizedDescriptionKey: description };
    
    return [NSError errorWithDomain:MSErrorDomain
                               code:errorCode
                           userInfo:userInfo];
}

- (NSError *)errorWithDescription:(NSString *)description
                             code:(NSInteger)code
                    internalError:(NSError *)error
{
    NSMutableDictionary *userInfo = [@{ NSLocalizedDescriptionKey: description } mutableCopy];
    
    if (error) {
        [userInfo setObject:error forKey:NSUnderlyingErrorKey];
    }
    
    return [NSError errorWithDomain:MSErrorDomain code:code userInfo:userInfo];
}

@end
