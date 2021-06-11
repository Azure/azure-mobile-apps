// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "ZumoBlobConnection.h"
#import "ZumoTestGlobals.h"

@interface ZumoBlobConnection()

@property (nonatomic, strong) NSString *testRunName;
@property (nonatomic, strong) NSString *projectName;
@property (nonatomic, strong) NSString *clientId;
@property (nonatomic, strong) NSString *clientSecret;
@property (nonatomic, strong) NSString *accessToken;

@property (nonatomic, strong) NSString *blobAccessToken;
@property (nonatomic, strong) NSString *blobUrl;

@end


@implementation ZumoBlobConnection

-(id) initWithStorageURL:(NSString *)url token:(NSString *)token;
{
    self = [super init];
    if (self) {
        _blobUrl = url;
        _blobAccessToken = token;
    }
    return self;
}


#pragma mark Blob Storage Access

-(void) uploadJson:(id)jsonObject withFileName:(NSString *)name completion:(void(^)(NSError *error))completion
{
    NSLog(@"Uploading data to blob");
    
    NSData *data = [NSJSONSerialization dataWithJSONObject:jsonObject
                                                   options:NSJSONWritingPrettyPrinted
                                                     error:nil];

    [self uploadData:data type:@"application/json" withFileName:name completion:completion];
}

-(void) uploadLog:(NSArray *)log withFileName:(NSString *)name completion:(void(^)(NSError *error))completion
{
    NSLog(@"Uploading log to blob");
    NSData *data = [[log componentsJoinedByString:@"\n"] dataUsingEncoding:NSUTF8StringEncoding];
    
    [self uploadData:data type:@"text/plain" withFileName:name completion:completion];
}

-(void) uploadData:(NSData *)data type:(NSString *)type withFileName:(NSString *)name completion:(void (^)(NSError *))completion
{
    NSString *blobUrl = [self.blobUrl stringByTrimmingCharactersInSet:[NSCharacterSet characterSetWithCharactersInString:@"/"]];
    
    NSData *decodedData = [[NSData alloc] initWithBase64EncodedString:self.blobAccessToken options:0];
    NSString *decodedString = [[NSString alloc] initWithData:decodedData encoding:NSUTF8StringEncoding];
    NSURL *url = [NSURL URLWithString:[blobUrl stringByAppendingFormat:@"/%@?%@", [name stringByAddingPercentEncodingWithAllowedCharacters:[NSCharacterSet URLPathAllowedCharacterSet]], decodedString]];
    
    NSMutableURLRequest *request = [[NSMutableURLRequest alloc] initWithURL:url];
    
    request.HTTPMethod = @"PUT";
    
    [request addValue:@"application/json" forHTTPHeaderField:@"Accept"];
    [request addValue:type forHTTPHeaderField:@"content-type"];
    [request addValue:@"BlockBlob" forHTTPHeaderField:@"x-ms-blob-type"];
    
    NSURLSession *session = [NSURLSession sharedSession];
    
    NSURLSessionUploadTask *task = [session uploadTaskWithRequest:request
                                                         fromData:data
                                                completionHandler:^(NSData *data, NSURLResponse *response, NSError *error)
                                    {
                                        NSLog(@"Got response %@ with error %@.\n", response, error);
                                        NSLog(@"DATA:\n%@\nEND DATA\n", [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding]);
                                        completion(error);
                                    }];
    
    [task resume];
}


+(unsigned long long) fileTime:(NSDate *)time
{
    return 116444736000000000LL + (time.timeIntervalSince1970 * 10000000LL);
}



@end
