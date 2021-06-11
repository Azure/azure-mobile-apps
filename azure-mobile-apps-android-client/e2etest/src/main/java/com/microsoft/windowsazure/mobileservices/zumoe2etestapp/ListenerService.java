//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

package com.microsoft.windowsazure.mobileservices.zumoe2etestapp;

import android.content.Context;

import com.google.firebase.messaging.RemoteMessage;
import com.google.gson.JsonElement;
import com.google.gson.JsonParser;
import com.microsoft.windowsazure.mobileservices.zumoe2etestapp.framework.PushMessageManager;
import com.microsoft.windowsazure.notifications.NotificationsHandler;

public class ListenerService extends NotificationsHandler {

    @Override
    public void onReceive(final Context context, final RemoteMessage remoteMessage) {
        String message = remoteMessage.getData().get("message");
        JsonElement msgObj = new JsonParser().parse(message);
        PushMessageManager.instance.AddMessage(msgObj);
    }
}
