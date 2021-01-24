# How to use the iOS client library for Azure Mobile Apps

This guide teaches you to perform common scenarios using the latest [Azure Mobile Apps iOS SDK](https://cocoapods.org/pods/MicrosoftAzureMobile). If you are new to Azure Mobile Apps, first complete [Azure Mobile Apps Quick Start](../../quickstarts/ios/index.md) to create a backend, create a table, and download a pre-built iOS Xcode project. In this guide, we focus on the client-side iOS SDK. To learn more about the server-side SDK for the backend, see the Server SDK HOWTOs.

## Reference documentation

The reference documentation for the iOS client SDK is located here: [Azure Mobile Apps iOS Client Reference](https://azure.github.io/azure-mobile-apps-ios-client/).

## Supported Platforms

The iOS SDK supports Objective-C projects, Swift 2.2 projects, and Swift 2.3 projects for iOS versions 8.0 or later.

The "server-flow" authentication uses a WebView for the presented UI.  If the device is not able to present a WebView UI, then another method of authentication is required that is outside the scope of the product. This SDK is thus not suitable for Watch-type or similarly restricted devices.

## <a name="Setup"></a>Setup and Prerequisites

This guide assumes that you have created a backend with a table. This guide assumes that the table has the same schema as the tables in those tutorials. This guide also assumes that in your code, you reference `MicrosoftAzureMobile.framework` and import `MicrosoftAzureMobile/MicrosoftAzureMobile.h`.  

### Install the Azure Mobile Apps SDK with Cocoapods

We recommend that you install Azure Mobile Apps in your project with CocoaPods.  First, [install CocoaPods](https://guides.cocoapods.org/using/getting-started.html#installation) and [initialize the project](https://guides.cocoapods.org/using/using-cocoapods.html#installation).  Then add the following to your `Podfile`:

``` text
    pod 'MicrosoftAzureMobile', '~> 3.4'
```

Finally, run `pod install` to install the framework in your project.

## <a name="create-client"></a>Create Client

To access an Azure Mobile Apps backend in your project, create an `MSClient`. Replace `AppUrl` with the app URL. You may leave `gatewayURLString` and `applicationKey` empty. If you set up a gateway for authentication, populate `gatewayURLString` with the gateway URL.

=== "Objective-C"

    ```objc
    MSClient *client = [MSClient clientWithApplicationURLString:@"AppUrl"];
    ```

=== "Swift"

    ```swift
    let client = MSClient(applicationURLString: "AppUrl")
    ```

## <a name="table-reference"></a>Create Table Reference

To access or update data, create a reference to the backend table. Replace `TodoItem` with the name of your table

=== "Objective-C"

    ```objc
    MSTable *table = [client tableWithName:@"TodoItem"];
    ```

=== "Swift"

    ```swift
    let table = client.tableWithName("TodoItem")
    ```

## <a name="querying"></a>Query Data

To create a database query, query the `MSTable` object. The following query gets all the items in `TodoItem` and logs the text of each item.

=== "Objective-C"

    ```objc
    [table readWithCompletion:^(MSQueryResult *result, NSError *error) {
            if(error) { // error is nil if no error occurred
                    NSLog(@"ERROR %@", error);
            } else {
                    for(NSDictionary *item in result.items) { // items is NSArray of records that match query
                            NSLog(@"Todo Item: %@", [item objectForKey:@"text"]);
                    }
            }
    }];
    ```

=== "Swift"

    ```swift
    table.readWithCompletion { (result, error) in
        if let err = error {
            print("ERROR ", err)
        } else if let items = result?.items {
            for item in items {
                print("Todo Item: ", item["text"])
            }
        }
    }
    ```

## <a name="filtering"></a>Filter Returned Data

To filter results, there are many available options.

To filter using a predicate, use an `NSPredicate` and `readWithPredicate`. The following filters returned data to find only incomplete Todo items.

=== "Objective-C"

    ```objc
    // Create a predicate that finds items where complete is false
    NSPredicate * predicate = [NSPredicate predicateWithFormat:@"complete == NO"];
    // Query the TodoItem table
    [table readWithPredicate:predicate completion:^(MSQueryResult *result, NSError *error) {
            if(error) {
                    NSLog(@"ERROR %@", error);
            } else {
                    for(NSDictionary *item in result.items) {
                            NSLog(@"Todo Item: %@", [item objectForKey:@"text"]);
                    }
            }
    }];
    ```

=== "Swift"

    ```swift
    // Create a predicate that finds items where complete is false
    let predicate =  NSPredicate(format: "complete == NO")
    // Query the TodoItem table
    table.readWithPredicate(predicate) { (result, error) in
        if let err = error {
            print("ERROR ", err)
        } else if let items = result?.items {
            for item in items {
                print("Todo Item: ", item["text"])
            }
        }
    }
    ```

## <a name="query-object"></a>Use MSQuery

To perform a complex query (including sorting and paging), create an `MSQuery` object, directly or by using a predicate:

=== "Objective-C"

    ```objc
    MSQuery *query = [table query];
    MSQuery *query = [table queryWithPredicate: [NSPredicate predicateWithFormat:@"complete == NO"]];
    ```

=== "Swift"

    ```swift
    let query = table.query()
    let query = table.queryWithPredicate(NSPredicate(format: "complete == NO"))
    ```

`MSQuery` lets you control several query behaviors.

* Specify order of results
* Limit which fields to return
* Limit how many records to return
* Specify total count in response
* Specify custom query string parameters in request
* Apply additional functions

Execute an `MSQuery` query by calling `readWithCompletion` on the object.

## <a name="sorting"></a>Sort Data with MSQuery

To sort results, let's look at an example. To sort by field 'text' ascending, then by 'complete' descending, invoke `MSQuery` like this:

=== "Objective-C"

    ```objc
    [query orderByAscending:@"text"];
    [query orderByDescending:@"complete"];
    [query readWithCompletion:^(MSQueryResult *result, NSError *error) {
            if(error) {
                    NSLog(@"ERROR %@", error);
            } else {
                    for(NSDictionary *item in result.items) {
                            NSLog(@"Todo Item: %@", [item objectForKey:@"text"]);
                    }
            }
    }];
    ```

=== "Swift"

    ```swift
    query.orderByAscending("text")
    query.orderByDescending("complete")
    query.readWithCompletion { (result, error) in
        if let err = error {
            print("ERROR ", err)
        } else if let items = result?.items {
            for item in items {
                print("Todo Item: ", item["text"])
            }
        }
    }
    ```

## <a name="selecting"></a><a name="parameters"></a>Limit Fields and Expand Query String Parameters with MSQuery

To limit fields to be returned in a query, specify the names of the fields in the **selectFields** property. This example returns only the text and completed fields:

=== "Objective-C"

    ```objc
    query.selectFields = @[@"text", @"complete"];
    ```

=== "Swift"

    ```swift
    query.selectFields = ["text", "complete"]
    ```

To include additional query string parameters in the server request (for example, because a custom server-side script uses them), populate `query.parameters` like so:

=== "Objective-C"

    ```objc
    query.parameters = @{
        @"myKey1" : @"value1",
        @"myKey2" : @"value2",
    };
    ```

=== "Swift"

    ```swift
    query.parameters = ["myKey1": "value1", "myKey2": "value2"]
    ```

## <a name="paging"></a>Configure Page Size

With Azure Mobile Apps, the page size controls the number of records that are pulled at a time from the backend tables. A call to `pull` data would then batch up data, based on this page size, until there are no more records to pull.

It's possible to configure a page size using **MSPullSettings** as shown below. The default page size is 50, and the example below changes it to 3.

You could configure a different page size for performance reasons. If you have a large number of small data records, a high page size reduces the number of server round-trips.

This setting controls only the page size on the client side. If the client asks for a larger page size than the Mobile Apps backend supports, the page size is capped at the maximum the backend is configured to support.

This setting is also the *number* of data records, not the *byte size*.

If you increase the client page size, you should also increase the page size on the server. 

=== "Objective-C"

    ```objc
    MSPullSettings *pullSettings = [[MSPullSettings alloc] initWithPageSize:3];
    [table  pullWithQuery:query queryId:@nil settings:pullSettings
                            completion:^(NSError * _Nullable error) {
                                if(error) {
                        NSLog(@"ERROR %@", error);
                    }
                            }];
    ```

=== "Swift"

    ```swift
    let pullSettings = MSPullSettings(pageSize: 3)
    table.pullWithQuery(query, queryId:nil, settings: pullSettings) { (error) in
        if let err = error {
            print("ERROR ", err)
        }
    }
    ```

## <a name="inserting"></a>Insert Data

To insert a new table row, create a `NSDictionary` and invoke `table insert`. If dynamic schema is enabled, the Azure App Service mobile backend automatically generates new columns based on the `NSDictionary`.

If `id` is not provided, the backend automatically generates a new unique ID. Provide your own `id` to use email addresses, usernames, or your own custom values as ID. Providing your own ID may ease joins and business-oriented database logic.

The `result` contains the new item that was inserted. Depending on your server logic, it may have additional or modified data compared to what was passed to the server.

=== "Objective-C"

    ```objc
    NSDictionary *newItem = @{@"id": @"custom-id", @"text": @"my new item", @"complete" : @NO};
    [table insert:newItem completion:^(NSDictionary *result, NSError *error) {
        if(error) {
            NSLog(@"ERROR %@", error);
        } else {
            NSLog(@"Todo Item: %@", [result objectForKey:@"text"]);
        }
    }];
    ```

=== "Swift"

    ```swift
    let newItem = ["id": "custom-id", "text": "my new item", "complete": false]
    table.insert(newItem) { (result, error) in
        if let err = error {
            print("ERROR ", err)
        } else if let item = result {
            print("Todo Item: ", item["text"])
        }
    }
    ```

## <a name="modifying"></a>Modify Data

To update an existing row, modify an item and call `update`:

=== "Objective-C"

    ```objc
    NSMutableDictionary *newItem = [oldItem mutableCopy]; // oldItem is NSDictionary
    [newItem setValue:@"Updated text" forKey:@"text"];
    [table update:newItem completion:^(NSDictionary *result, NSError *error) {
        if(error) {
            NSLog(@"ERROR %@", error);
        } else {
            NSLog(@"Todo Item: %@", [result objectForKey:@"text"]);
        }
    }];
    ```

=== "Swift"

    ```swift
    if let newItem = oldItem.mutableCopy() as? NSMutableDictionary {
        newItem["text"] = "Updated text"
        table2.update(newItem as [NSObject: AnyObject], completion: { (result, error) -> Void in
            if let err = error {
                print("ERROR ", err)
            } else if let item = result {
                print("Todo Item: ", item["text"])
            }
        })
    }
    ```

Alternatively, supply the row ID and the updated field:

=== "Objective-C"

    ```objc
    [table update:@{@"id":@"custom-id", @"text":"my EDITED item"} completion:^(NSDictionary *result, NSError *error) {
        if(error) {
            NSLog(@"ERROR %@", error);
        } else {
            NSLog(@"Todo Item: %@", [result objectForKey:@"text"]);
        }
    }];
    ```

=== "Swift"

    ```swift
    table.update(["id": "custom-id", "text": "my EDITED item"]) { (result, error) in
        if let err = error {
            print("ERROR ", err)
        } else if let item = result {
            print("Todo Item: ", item["text"])
        }
    }
    ```

At minimum, the `id` attribute must be set when making updates.

## <a name="deleting"></a>Delete Data

To delete an item, invoke `delete` with the item:

=== "Objective-C"

    ```objc
    [table delete:item completion:^(id itemId, NSError *error) {
        if(error) {
            NSLog(@"ERROR %@", error);
        } else {
            NSLog(@"Todo Item ID: %@", itemId);
        }
    }];
    ```

=== "Swift"

    ```swift
    table.delete(newItem as [NSObject: AnyObject]) { (itemId, error) in
        if let err = error {
            print("ERROR ", err)
        } else {
            print("Todo Item ID: ", itemId)
        }
    }
    ```

Alternatively, delete by providing a row ID:

=== "Objective-C"

    ```objc
    [table deleteWithId:@"37BBF396-11F0-4B39-85C8-B319C729AF6D" completion:^(id itemId, NSError *error) {
        if(error) {
            NSLog(@"ERROR %@", error);
        } else {
            NSLog(@"Todo Item ID: %@", itemId);
        }
    }];
    ```

=== "Swift"

    ```swift
    table.deleteWithId("37BBF396-11F0-4B39-85C8-B319C729AF6D") { (itemId, error) in
        if let err = error {
            print("ERROR ", err)
        } else {
            print("Todo Item ID: ", itemId)
        }
    }
    ```

At minimum, the `id` attribute must be set when making deletes.

## <a name="customapi"></a>Call Custom API

With a custom API, you can expose any backend functionality. It doesn't have to map to a table operation. Not only do you gain more control over messaging, you can even read/set headers and change the response body format.

To call a custom API, call `MSClient.invokeAPI`. The request and response content are treated as JSON. To use other media types, [use the other overload of `invokeAPI`][5].  To make a `GET` request instead of a `POST` request, set parameter `HTTPMethod` to `"GET"` and parameter `body` to `nil` (since GET requests do not have message bodies.) If your custom API supports other HTTP verbs, change `HTTPMethod` appropriately.

=== "Objective-C"

    ```objc
    [self.client invokeAPI:@"sendEmail"
                    body:@{ @"contents": @"Hello world!" }
                HTTPMethod:@"POST"
                parameters:@{ @"to": @"bill@contoso.com", @"subject" : @"Hi!" }
                headers:nil
                completion: ^(NSData *result, NSHTTPURLResponse *response, NSError *error) {
                    if(error) {
                        NSLog(@"ERROR %@", error);
                    } else {
                        // Do something with result
                    }
                }];
    ```

=== "Swift"

    ```swift
    client.invokeAPI("sendEmail",
                body: [ "contents": "Hello World" ],
                HTTPMethod: "POST",
                parameters: [ "to": "bill@contoso.com", "subject" : "Hi!" ],
                headers: nil)
                {
                    (result, response, error) -> Void in
                    if let err = error {
                        print("ERROR ", err)
                    } else if let res = result {
                            // Do something with result
                    }
            }
    ```

## <a name="errors"></a>Handle Errors

When you call an Azure App Service mobile backend, the completion block contains an `NSError` parameter. When an error occurs, this parameter is non-nil. In your code, you should check this parameter and handle the error as needed, as demonstrated in the preceding code snippets.

The file `<WindowsAzureMobileServices/MSError.h>` defines the constants `MSErrorResponseKey`, `MSErrorRequestKey`, and `MSErrorServerItemKey`. To get more data related to the error:

=== "Objective-C"

    ```objc
    NSDictionary *serverItem = [error.userInfo objectForKey:MSErrorServerItemKey];
    ```

=== "Swift"

    ```swift
    let serverItem = error.userInfo[MSErrorServerItemKey]
    ```

In addition, the file defines constants for each error code:

=== "Objective-C"

    ```objc
    if (error.code == MSErrorPreconditionFailed) {
    ```

=== "Swift"

    ```swift
    if (error.code == MSErrorPreconditionFailed) {
    ```

## <a name="adal"></a>Authenticate users with the Azure Active Directory

> **NOTE**  ADAL is now deprecated, and should be replaced by MSAL.  You must configure MSAL
> as directed [in the instructions](https://docs.microsoft.com/azure/active-directory/develop/quickstart-v2-ios) then use the instructions below.

You must [acquire a token](https://docs.microsoft.com/azure/active-directory/develop/tutorial-v2-ios#acquire-tokens) with the appropriate permissions (scopes).  This will provide an `access_token`

=== "Objective-C"

    ```objc
    NSDictionary *payload = @{
        @"access_token" : result.accessToken
    };
    [client loginWithProvider:@"aad" token:payload completion:completionBlock];
    ```

=== "Swift"

    ```swift
    let payload: [String: String] = ["access_token": result.accessToken]
    client.loginWithProvider("aad", token: payload, completion: completion)
    ```

## Authenticate users with other providers

All providers that are supported (including Facebook, Google, and any other OAuth2 provider) follow the same pattern:

1. [Configure Azure App Service authentication](https://docs.microsoft.com/azure/app-service/app-service-authentication-how-to) for the provider you wish to use.
2. Integrate the provider SDK (for example, [Facebook](https://developers.facebook.com/docs/facebook-login/ios) or [Google](https://developers.google.com/identity/sign-in/ios/sign-in)) to get an access token (or, in some cases, an ID token - consult the [Azure App Service documentation](https://docs.microsoft.com/azure/app-service/app-service-authentication-how-to#validate-tokens-from-providers) for the information you need)
3. Call `loginWithProvider` (as above) with an appropriate payload.

All future calls to the backend by the Azure Mobile Apps client will be authenticated.
