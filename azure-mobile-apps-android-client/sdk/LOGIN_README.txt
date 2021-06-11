Background:
	Google recently started to disallow OAuth flow using webview on all Mobile platforms (iOS/Android/Windows) for usability and security reasons. The recommended way on Android is to use Chrome Custom Tabs when they're available, and fallback to a regular browser otherwise. Mobile Apps Android SDK uses webview in server-direct login flow in @{link LoginMananger}. We implements @{link CustomTabsLoginMananger} using Custom Tabs to replace webview login. We also support the PKCE extension (https://tools.ietf.org/html/rfc7636) to OAuth which was created to secure authorization codes in public clients when custom URI scheme redirects are used.

Diagram illustrates the server login flow using Custom Tabs:

 +--------------------------------+             bottom of backstack
 | (a)Your Activity               |                     +
 | (launchmode: any)              |                     |
 +--------------------------------+                     |
         |                    ^                         |
         |(1)                 |(7)                      |
         v                    |                         |
 +---------------------------------+                    |
 |(b)CustomTabsIntermediateActivity|                    |
 | (launchmode: singleTop)         |                    |
 +---------------------------------+                    |
         |                    ^                         |
         |(2)                 |(6)                      |
         v                    |                         |
 +--------------------------------+                     |
 | (c)CustomTabsLoginActivity     |                     |
 | (launchmode: singleTask)       |                     |
 +--------------------------------+                     |
         |                    ^                         |
         |(3)                 |                         |
         v                    |                         |
 +-------------------------+  |                         |
 | (d2)CustomTabs Activity |  |                         |
 | (launchmode: outside    |  |                         |
 |  scope of SDK)          |  |                         |
 | (d2)Browser Tab Activity|  |(5)                      |
 |                         |  |                         |
 +-------------------------+  |                         |
         |                    |                         |
         |(4)                 |                         |
         v                    |                         |
 +--------------------------------+                     |
 | (e)RedirectUrlActivity         |                     |
 | (launchmode: default)          |                     V
 +--------------------------------+                 top of backstack
 
 
 What do activities (a)-(e) mean in the diagram?
 
	(a) YourActivity - This is your activity which uses the Mobile Apps SDK to initiate login
	(b) CustomTabsIntermediateActivity - This is launched by the SDK as the first step for starting a login flow. We need this because we want to return the result via onActivityResult and CustomTabsLoginActivity cannot return a result using onActivityResult as it is has a launch mode of singleTask (this is an Android restriction). So this activity is used as an intermediate activity in the flow so that we can achieve both - returning result via onActivityResult as well as retaining the singleTask launch mode for CustomTabsLoginAcitivty.
	(c) CustomTabsLoginActivity - This is launched by CustomTabsIntermediateActivity initially and again later when the  authorization_code is received in the browser OR custom tab. In the former case, it proceeds to step 4 and in the latter case it relaunches the instance of CustomTabsIntermediateActivity in the task stack (use of SINGLE_TOP | CLEAR_TASK flags ensures this).
	(d1/d2) Chrome Custom Tabs or Browser acitvity - This is not part of the SDK. The SDK launches it and lets the backend perform the login flow. 
	(e) RedirectUrlActivity - This activity is registered to handle the custom scheme in the AndroidManifest.xml. It is launched when the login flow completes by redirecting to the custom scheme. The only thing this activity does is relaunches CustomTabsLoginActivity and returns.

What do steps (1)-(7) mean in the diagram?

	(1) Your activity initiates the login flow by invoking {@link MobileServiceClient.login(String provider, String urlScheme, int requestCode)}. This will launch {@link CustomTabsIntermediateActivity} atop of your activity.
	(2) {@link CustomTabsIntermediateActivity} will launch {@link CustomTabsLoginActivity} in singleTask mode. SingleTask mode makes it possible to re-launch the same instance of {@link CustomTabsLoginActivity} later in the flow. {@link CustomTabsLoginActivity} constructs the proper url for login and calls Azure Mobile Apps backend by launching a Chrome CustomTab. If Chrome CustomTab is not available on the device, a regular browser will be launched instead.
	(3) A native login UI (from the authentication provider) will be presented to the user for username and password. User enters username and password and clicks login button. 
	(4) Azure Mobile Apps backend handles the login with the authentication provider. When login succeeds in the backend, Mobile Apps backend calls the redirectUrl with authorization_code. {@link RedirectUrlActivity} is registered with an intent-filter in AndroidManifest.xml to handle the redirectUrl coming from Mobile Apps backend. authorization_code will be used later for exchange of token.
	(5) {@link RedirectUrlActivity} launchs {@link CustomTabsLoginActivity} again with authorization_code. Because {@link CustomTabsLoginActivity} is a singleTask activity, any activities (RedirectUrlActivity and CustomTabs activity) on top of it will be closed.
	(6) {@link CustomTabsLoginActivity} now has the authorization_code. It will use the authorization_code to exchange for the x-zumo-auth JWT token with the backend. Once the code exchange succeeds and the client received the authenticated MobileServiceUser from the backend, {@link CustomTabsIntermediateActivity} will be launched again using FLAG_ACTIVITY_SINGLE_TOP and FLAG_ACTIVITY_CLEAR_TOP.
	(7) {@link CustomTabsIntermediateActivity} will pass the authenticated MobileServiceUser back to your acitivity that initiates the login flow. If code exchange failed in the previous step, an error message will be sent back to your activity. This comletes the server login flow.
 
 
Implementation details:

	(1) Allow only one login flow at a time. But multiple non-interleaving login flow in a row is okay.
	(2) When Chrome custom tabs is not available on the device, fallback to user's default browser. Tested on Samsung "Internet", Firefox & Opera browser.
	(3) In the event of system killing activities for memory conservation, this login flow still works for Custom Tabs and regular browser.
	(4) Your activity that used to start login flow can be sitting on any task, not limiting to the main/default task of your app.
 