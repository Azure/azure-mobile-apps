package com.microsoft.windowsazure.notifications;

import android.content.Context;

import com.google.firebase.messaging.RemoteMessage;
import com.google.firebase.messaging.FirebaseMessagingService;

public class NotificationMessagingService extends FirebaseMessagingService {

    @Override
    public void onMessageReceived(RemoteMessage remoteMessage) {

        Context context = getApplicationContext();
        NotificationsHandler handler = NotificationsManager.getHandler(context);

        if (handler != null) {
            handler.onReceive(context, remoteMessage);
        }
    }


    @Override
    public void onNewToken(String token) {
        Context context = getApplicationContext();
        NotificationsHandler handler = NotificationsManager.getHandler(context);

        if (handler != null) {
            handler.onNewToken(token);
        }
    }
}
