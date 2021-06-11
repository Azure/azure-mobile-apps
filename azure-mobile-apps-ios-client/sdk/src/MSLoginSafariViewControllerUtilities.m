// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import <SafariServices/SafariServices.h>
#import <Security/SecRandom.h>
#import <CommonCrypto/CommonDigest.h>
#import "MSLoginSafariViewControllerUtilities.h"
#import "MSURLBuilder.h"

#pragma mark * Azure App Service Easy Auth Redirect URL Fragment

NSString *const EasyAuthRedirectURLFragment = @"easyauth.callback";

#pragma mark * Authorization Code URL Fragment

NSString *const AuthorizationCodeURLFragment = @"#authorization_code=";

#pragma mark * Number of Bytes in Code Verifier

NSUInteger const ByteLength = 32;

@implementation MSLoginSafariViewControllerUtilities

+ (NSURL *)fullURLFromLoginURL:(NSURL *)loginURL
                      provider:(NSString *)provider
                     urlScheme:(NSString *)urlScheme
                    parameters:(nullable NSDictionary *)parameters
                  codeVerifier:(NSString *)codeVerifier
           codeChallengeMethod:(NSString *)codeChallengeMethod
{
    NSURL *fullURL = nil;

    if ([self isValidURLScheme:urlScheme]) {

        NSString *redirectURLString = [NSString stringWithFormat:@"%@://%@", urlScheme, EasyAuthRedirectURLFragment];
        
        NSString *codeChallenge = [self sha256Base64EncodeWithString:codeVerifier];
        
        NSMutableDictionary *params = [[NSMutableDictionary alloc] init];
        [params setObject:redirectURLString forKey:@"post_login_redirect_url"];
        [params setObject:codeChallenge forKey:@"code_challenge"];
        [params setObject:codeChallengeMethod forKey:@"code_challenge_method"];
        if (parameters) {
            [params addEntriesFromDictionary:parameters];
        }
      
        fullURL = [loginURL URLByAppendingPathComponent:provider];
        
        fullURL = [MSURLBuilder URLByAppendingQueryParameters:params toURL:fullURL];
    }
    return fullURL;
}

+ (BOOL)isRedirectURLValid:(NSURL *)URL withUrlScheme:(NSString *)urlScheme
{
    if (URL && urlScheme) {
        
        NSURLComponents *urlComponents = [[NSURLComponents alloc] initWithURL:URL resolvingAgainstBaseURL:YES];
        
        if (urlComponents) {
            // Redirect URL is not malformed
            if ([[urlComponents.scheme lowercaseString] isEqualToString:[urlScheme lowercaseString]]) {
                // Redirect URL matches url scheme
                if ([[urlComponents.host lowercaseString] isEqualToString:EasyAuthRedirectURLFragment]) {
                    // Redirect URL matches "easyauth.callback"
                    return YES;
                }
            }
        }
    }
    return NO;
}


+ (NSString *)authorizationCodeFromRedirectURL:(NSURL *)URL
{
    NSString *authorizationCode = nil;
    
    if (URL) {
        NSString *URLString = URL.absoluteString;
        NSInteger match = [URLString rangeOfString:AuthorizationCodeURLFragment].location;
        
        if (match != NSNotFound) {
            authorizationCode = [URLString substringFromIndex:(match + AuthorizationCodeURLFragment.length)];
        }
    }
    
    return authorizationCode;
}

+ (NSURL *)codeExchangeURLFromApplicationURL:(NSURL *)applicationURL
                                    provider:(NSString *)provider
                           authorizationCode:(NSString *)authorizationCode
                                codeVerifier:(NSString *)codeVerifier
{
    NSURL *codeExchangeURL = [applicationURL URLByAppendingPathComponent:[NSString stringWithFormat:@".auth/login/%@/token", provider]];

    NSMutableString *codeExchangeString = [[NSString stringWithFormat:@"%@?authorization_code=%@", codeExchangeURL.absoluteString, authorizationCode] mutableCopy];
    
    [MSURLBuilder appendParameterName:@"code_verifier" andValue:codeVerifier toQueryString:codeExchangeString];
    
    codeExchangeURL = [NSURL URLWithString:codeExchangeString];

    return codeExchangeURL;
}

+ (BOOL)isValidURLScheme:(NSString *)urlScheme
{
    if (urlScheme) {
        NSURLComponents *redirectURLComponents = [[NSURLComponents alloc] init];
        
        // Attempting to set the property scheme with urlScheme.
        // An invalid scheme string will cause an exception.
        @try {
            redirectURLComponents.scheme = urlScheme;
        }
        @catch (NSException *exception) {
            // Ignore any exception
        }
        if (redirectURLComponents.scheme) {
            return YES;
        }
    }
    
    return NO;
}

+ (NSString *)normalizeProvider:(NSString *)provider
{
    // Microsoft Azure Active Directory can be specified either in
    // full or with the 'aad' abbreviation. The service REST API
    // expects 'aad' only.
    if ([[provider lowercaseString] isEqualToString:@"windowsazureactivedirectory"]) {
        return @"aad";
    } else {
        return provider;
    }
}

+ (NSString *)generateCodeVerifier
{
    NSMutableData *randomData = [NSMutableData dataWithLength:ByteLength];
    
    int result = SecRandomCopyBytes(kSecRandomDefault, randomData.length, randomData.mutableBytes);
    
    if (result != 0) {
        NSLog(@"Unable to generate random bytes: %d", errno);
        
        return nil;
    }
    
    NSString *base64EncodedRandomString = [randomData base64EncodedStringWithOptions:0];
    
    return base64EncodedRandomString;
}

+ (NSString *)sha256Base64EncodeWithString:(NSString *)string
{
    NSData *verifierData = [string dataUsingEncoding:NSASCIIStringEncoding];
    
    return [self sha256Base64EncodeWithData:verifierData];
}

+ (NSString *)sha256Base64EncodeWithData:(NSData *)data
{
    NSMutableData *sha256Verifier = [NSMutableData dataWithLength:CC_SHA256_DIGEST_LENGTH];
    
    CC_SHA256(data.bytes, (CC_LONG)data.length, sha256Verifier.mutableBytes);
    
    NSString *sha256VerifierString = [sha256Verifier base64EncodedStringWithOptions:0];
    
    return sha256VerifierString;
}

@end
