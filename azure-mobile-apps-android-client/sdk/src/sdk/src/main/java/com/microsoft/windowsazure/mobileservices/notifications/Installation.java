/*
Copyright (c) Microsoft Open Technologies, Inc.
All Rights Reserved
Apache 2.0 License

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
 */

package com.microsoft.windowsazure.mobileservices.notifications;

import java.util.Date;
import java.util.HashMap;
import java.util.List;

/**
 * Represents device installation in Azure Notification Hub (https://msdn.microsoft.com/en-us/library/azure/mt621153.aspx)
 */
public class Installation {

    /**
     * Expiration time of the installation.
     * Note this property is read-only for installation. The value can be set at the NotificationHub
     * level on create or update, and will default to never expire (9999-12-31T23:59:59).
     */
    public Date expirationTime;

    /**
     * Globally unique identifier of the installation
     */
    public String installationId;

    /**
     * Notification platform of the installation
     */
    public String platform;

    /**
     * Registration Id, token or URI obtained from platform-specific notification service
     */
    public String pushChannel;

    /**
     * Installation expired or not.
     * Note this property is read-only for installation. It is ignored in installation create or update.
     */
    public boolean pushChannelExpired;

    /**
     * A collection of push variables
     */
    public HashMap<String, String> pushVariables;

    /**
     * A list of tags
     */
    public List<String> tags;

    /**
     * A collection of templates
     */
    public HashMap<String, InstallationTemplate> templates;

    /**
     * Constructor for Installation
     * @param installationId installation Id
     * @param platform platform
     * @param pushChannel registration Id, token or URI obtained from platform-specific notification service
     * @param pushVariables a collection of push variables
     * @param tags a list of tags
     * @param templates a collection of templates
     * @param expirationTime expiration time
     * @param pushChannelExpired installation expired or not
     */
    public Installation(String installationId, String platform, String pushChannel, HashMap<String, String> pushVariables, List<String> tags, HashMap<String, InstallationTemplate> templates,
                        Date expirationTime, boolean pushChannelExpired) {
        this.installationId = installationId;
        this.platform = platform;
        this.pushChannel = pushChannel;
        this.pushVariables = pushVariables;
        this.tags = tags;
        this.templates = templates;
        this.expirationTime = expirationTime;
        this.pushChannelExpired = pushChannelExpired;
    }

    /**
     * Constructor for Installation
     * @param installationId installation Id
     * @param platform platform
     * @param pushChannel registration Id, token or URI obtained from platform-specific notification service
     * @param pushVariables a collection of push variables
     * @param tags a list of tags
     * @param templates a collection of templates
     */
    public Installation(String installationId, String platform, String pushChannel, HashMap<String, String> pushVariables, List<String> tags, HashMap<String, InstallationTemplate> templates) {
        this(installationId, platform, pushChannel, pushVariables, tags, templates, null, false);
    }
}
