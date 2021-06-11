// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import <Foundation/Foundation.h>
#import <MicrosoftAzureMobile/MicrosoftAzureMobile.h>

typedef void (^ZumoHttpRequestCompletion)(NSHTTPURLResponse *response, NSData *responseBody, NSError *error);

extern NSString *const RUNTIME_VERSION_KEY;
extern NSString *const CLIENT_VERSION_KEY;
extern NSString *const RUNTIME_FEATURES_KEY;
extern NSString *const RUNTIME_VERSION_TAG;
extern NSString *const CLIENT_VERSION_TAG;

extern NSString *const FEATURE_STRING_ID_TABLES;
extern NSString *const FEATURE_INT_ID_TABLES;
extern NSString *const FEATURE_NH_PUSH_ENABLED;

extern NSString *const TABLES_ROUND_TRIP_STRING_ID;
extern NSString *const TABLES_ROUND_TRIP_INT_ID;

@protocol PushNotificationReceiver <NSObject>

@required

- (void)pushReceived:(NSDictionary *)userInfo;

@end

@interface ZumoTestGlobals : NSObject
{
    NSMutableDictionary *globalTestParameters;
}

+ (ZumoTestGlobals *)sharedInstance;

@property (nonatomic, strong) MSClient *client;

@property (nonatomic, copy) NSData *deviceToken;
@property (nonatomic, copy) NSString *remoteNotificationRegistrationStatus;
@property (nonatomic, weak) id<PushNotificationReceiver> pushNotificationDelegate;

@property (nonatomic, copy) NSString *storageURL;
@property (nonatomic, copy) NSString *storageToken;

-(void) initializeClientWithAppUrl:(NSString *)url;
- (void)saveAppInfo:(NSString *)appUrl key:(NSString *)appKey;
- (NSArray *)loadAppInfo;
- (NSMutableDictionary *)globalTestParameters;

// Helper methods
+ (NSDate *)createDateWithYear:(NSInteger)year month:(NSInteger)month day:(NSInteger)day;
+ (BOOL)compareDate:(NSDate *)date1 withDate:(NSDate *)date2;
+ (BOOL)compareObjects:(NSDictionary *)obj1 with:(NSDictionary *)obj2 log:(NSMutableArray *)errors;
+ (BOOL)compareObjects:(NSDictionary *)obj1 with:(NSDictionary *)obj2 ignoreKeys:(NSArray *)keys log:(NSMutableArray *)errors;
+ (BOOL)compareJson:(id)json1 with:(id)json2 log:(NSMutableArray *)errors;
+ (NSString *)dateToString:(NSDate *)date;
+ (NSString *)dateToShortString:(NSDate *)date;

@end
