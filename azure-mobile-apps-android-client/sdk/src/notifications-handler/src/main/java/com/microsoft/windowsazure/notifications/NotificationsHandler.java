package com.microsoft.windowsazure.notifications;

import android.content.Context;
import android.os.Handler;
import android.os.Looper;
import android.widget.Toast;

import com.google.firebase.messaging.RemoteMessage;

public class NotificationsHandler  {

    /**
     * Method called when a new token for the default Firebase project is generated.
     *
     * @param token The token used for sending messages to this application instance.
     */
    public void onNewToken(String token) {
    }

    /**
     * Method called after the device is registered for notifications
     *
     * @param context           Application context
     */
    public void onRegistered(Context context, String token) {
    }

    /**
     * Method called after the device is unregistered for notifications
     *
     * @param context           Application context
     */
    public void onUnregistered(Context context, String token) {
    }

    /**
     * Method called after a notification is received.
     * By default, it shows a toast with the value asociated to the "message" key in the bundle
     *
     * @param context Application Context
     * @param remoteMessage  RemoteMessage with notification data
     */
    public void onReceive(final Context context, final RemoteMessage remoteMessage) {
        Handler h = new Handler(Looper.getMainLooper());
        h.post(new Runnable() {

            @Override
            public void run() {
                Toast.makeText(context, remoteMessage.toString(), Toast.LENGTH_SHORT).show();
            }
        });
    }
}
