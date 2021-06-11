// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

#import "MSInstallation.h"

@implementation MSInstallation

+(id) installationWithInstallationId:(NSString *)installationId
                            platform:(NSString *)platform
                         pushChannel:(NSString *)pushChannel
                       pushVariables:(NSDictionary *)pushVariables
                                tags:(NSArray *)tags
                           templates:(NSDictionary *)templates
                      expirationTime:(NSDate *)expirationTime
                  pushChannelExpired:(BOOL)pushChannelExpired
{
    return [[MSInstallation alloc] initWithInstallationId:installationId platform:platform pushChannel:pushChannel pushVariables:pushVariables tags:tags templates:templates expirationTime:expirationTime pushChannelExpired:pushChannelExpired];
}

+(id) installationWithInstallationId:(NSString *)installationId
                            platform:(NSString *)platform
                         pushChannel:(NSString *)pushChannel
                       pushVariables:(NSDictionary *)pushVariables
                                tags:(NSArray *)tags
                           templates:(NSDictionary *)templates
{
    return [[MSInstallation alloc] initWithInstallationId:installationId platform:platform pushChannel:pushChannel pushVariables:pushVariables tags:tags templates:templates expirationTime:nil pushChannelExpired:NO];
}

-(id) initWithInstallationId:(NSString *)installationId
                    platform:(NSString *)platform
                 pushChannel:(NSString *)pushChannel
               pushVariables:(NSDictionary *)pushVariables
                        tags:(NSArray *)tags
                   templates:(NSDictionary *)templates
              expirationTime:(NSDate *)expirationTime
          pushChannelExpired:(BOOL)pushChannelExpired
{
    self = [super init];
    if (self) {
        _installationId = installationId;
        _platform = platform;
        _pushChannel = pushChannel;
        _pushVariables = pushVariables;
        _tags = tags;
        _templates = templates;
        _expirationTime = expirationTime;
        _pushChannelExpired = pushChannelExpired;
    }
    return self;
}

@end
