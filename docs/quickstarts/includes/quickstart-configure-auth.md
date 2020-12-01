## Configure your backend for authentication

To configure your backend for authentication, you must:

* Create an app registration.
* Configure Azure App Service Authentication and Authorization.
* Add your app to the Allowed External Redirect URLs.

During this tutorial, we will configure your app to use Microsoft authentication, which uses configuration within Azure Active Directory.  An Azure Active Directory tenant has been configured automatically in your Azure subscription.

To complete this tutorial, you will need to know the Backend URL for your application.  This was provided when you created your project.

### Create an app registration

1. Sign in to the [Azure portal](https://portal.azure.com).
2. Select **Azure Active Directory** > **App registrations** > **New registration**.
3. In the **Register an application** page, enter `zumoquickstart` in the **Name** field.
4. Under **Supported account types**, select **Accounts in any organizational directory (Any Azure AD directory - multitenant) and personal Microsoft accounts (e.g. Skype, Xbox)**.
5. In **Redirect URI**, select **Web** and type `<backend-url>/.auth/login/aad/callback`.  For example, if your backend URL is `https://web-abcd1234.azurewebsites.net`, you would enter `https://web-abcd1234.azurewebsites.net/.auth/login/aad/callback`.
6. Press the **Register** button at the bottom of the form.
7. Copy the **Application (client) ID**.  You will need it later.
8. From the left pane, select **Certificates & secrets** > **New client secret**.  Enter a suitable description, select a validity duration, then select **Add**.
9. Copy the value that appears on the **Certificates & secrets** page.  You will need it later and it won't be displayed again.

> **Important**
> The client secret value (password) is an important security credential.  Do not share the password with anyone or distribute it within a client application.

### Configure Azure App Service Authentication and Authorization

1. In the [Azure portal](https://portal.azure.com), select [**All Resources**](https://portal.azure.com/#blade/HubsExtension/BrowseAll), then your App Service.
2. Select **Settings** > **Authentication / Authorization**. 
3. Ensure that **App Service Authentication** is **On**.
4. Under **Authentication Providers**, select **Azure Active Directory**.
5. Select **Advanced** under **Management mode**.
6. Paste the Application (client) ID that you obtained earlier.
7. Enter `https://login.microsoftonline.com/9188040d-6c67-4c5b-b112-36a304b66dad/v2.0` into the **Issuer Url** field.  This is a "magic tenant url" for Microsoft logins.
8. Press **Show secret**.  Paste the client secret value into the field that appears.
9. Select **OK**.
10. To restrict access to Microsoft account users, set **Action to take when request is not authenticated** to **Log in with Azure Active Directory**.
11. In the **Allowed External Redirect URLs**, enter `zumoquickstart://easyauth.callback`. 
12. Select **Save**.

Step 10 requires that all users are authenticated prior to accessing your backend.  You can provide more fine-grained controls by adding code to your backend.  For more details on this, consult the Server SDK How-to for [Node.js](../../howto/server/nodejs.md) or [ASP.NET Framework](../../howto/server/dotnet-framework.md).

> **DID YOU KNOW?**
> You can also authenticate users with organizational accounts in Azure Active Directory, Facebook, Google, Twitter, or any OpenID Connect compatible provider.  Follow the instructions in the [Azure App Service documentation](https://docs.microsoft.com/en-us/azure/app-service/app-service-authentication-how-to).
