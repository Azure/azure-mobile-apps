// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

class ZumoLoginSafariViewControllerSwiftTests: NSObject {

    class func createSafariLoginTest(forProvider provider: String, animated: Bool) -> ZumoTest {

        let result = ZumoTest.createTest(withName: "SafariVC Login for \(provider)", andExecution:
            {(test, viewController, completion) -> Void in
                
                let client: MSClient? = ZumoTestGlobals.sharedInstance().client
                
                let loginBlock: MSClientLoginBlock = {(user, error) -> Void in
                    if (error != nil) {
                        test?.addLog("Error logging in: \(error)")
                        test?.testStatus = TSFailed
                        completion!(false)
                    }
                    else {
                        test?.addLog("Logged in as \(user?.userId)")
                        test?.testStatus = TSPassed
                        completion!(true)
                    }
                }
                
                client?.login(withProvider: provider, urlScheme: "ZumoE2ETestApp", controller: viewController!, animated: animated, completion: loginBlock)
            })
        
        return result!
    }
    
    class func createTests() -> [Any] {
        
        var result = [Any]()
        
        result.append(self.createSafariLoginTest(forProvider: "google", animated: true))
        result.append(self.createSafariLoginTest(forProvider: "aad", animated: false))
        result.append(self.createSafariLoginTest(forProvider: "microsoftaccount", animated: false))
        result.append(self.createSafariLoginTest(forProvider: "facebook", animated: false))
        result.append(self.createSafariLoginTest(forProvider: "twitter", animated: false))
        
        return result
    }
   
    class func groupDescription() -> String {
        return "Tests to validate Login via SafariViewController in the client SDK."
    }
}
