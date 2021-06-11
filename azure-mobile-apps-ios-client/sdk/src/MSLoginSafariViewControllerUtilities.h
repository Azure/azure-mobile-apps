// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

NS_ASSUME_NONNULL_BEGIN

@interface MSLoginSafariViewControllerUtilities : NSObject


/**
 Generate full URL for |MSSafariViewController| login from
 login URL, identity provider name, custom URL scheme,
 login parameters, code verifier and code challenge method

 @param loginURL
 @param provider The identity provider
 @param urlScheme The url scheme
 @param parameters NSDictionary representation of extra login parameters
 @param codeVerifier to be used in Proof Key for Code Exchange protocol of OAuth 2.0
 @param codeChallengeMethod to be used in Proof Key for Code Exchange protocol of OAuth 2.0
 @return login URL
 */
+ (NSURL *)fullURLFromLoginURL:(NSURL *)loginURL
                      provider:(NSString *)provider
                     urlScheme:(NSString *)urlScheme
                    parameters:(nullable NSDictionary *)parameters
                  codeVerifier:(NSString *)codeVerifier
           codeChallengeMethod:(NSString *)codeChallengeMethod;


/**
 Determine if redirect URL for |MSSafariViewController| resumeWithURL is valid

 @param URL
 @param urlScheme
 @return YES if URL is valid, NO otherwise
 */
+ (BOOL)isRedirectURLValid:(NSURL *)URL withUrlScheme:(NSString *)urlScheme;


/**
 Extract authorization_code from redirect URL

 @param URL
 @return authorization_code
 */
+ (NSString *)authorizationCodeFromRedirectURL:(NSURL *)URL;


/**
 Generate URL for code exchange for |MSSafariViewController| login
 from application URL, identity provider name, authorization code
 and code verifier

 @param applicationURL
 @param provider The identity provider
 @param authorizationCode to be used in code exchange
 @param codeVerifier to be used in Proof Key for Code Exchange protocol of OAuth 2.0
 @return URL of the authorization_code exchange call
 */
+ (NSURL *)codeExchangeURLFromApplicationURL:(NSURL *)applicationURL
                                   provider:(NSString *)provider
                          authorizationCode:(NSString *)authorizationCode
                               codeVerifier:(NSString *)codeVerifier;


/**
 Determine if custom URL scheme for |MSSafariViewController| login is valid

 @param urlScheme The url scheme
 @return YES if url scheme is valid, NO otherwise
 */
+ (BOOL)isValidURLScheme:(NSString *)urlScheme;

/**
 Normalize identity provider name
 
 @param provider The identity provider
 @return normalized provider name string
 */
+ (NSString *)normalizeProvider:(NSString *)provider;

/**
 Generate codeVerifier to be used in Proof Key for Code Exchange protocol of OAuth 2.0
 
 @return code verifier string
 */
+ (NSString *)generateCodeVerifier;

/**
 SHA256 hashing followed by base64 encoding of the input string
 
 @param string The input string
 @return output string
 */
+ (NSString *)sha256Base64EncodeWithString:(NSString *)string;


/**
 SHA256 hashing followed by base64 encoding of the input data
 
 @param data The input data
 @return output string
 */
+ (NSString *)sha256Base64EncodeWithData:(NSData *)data;

@end

NS_ASSUME_NONNULL_END
