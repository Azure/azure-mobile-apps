## Azure Mobile Apps Cordova Client Quickstart Instructions ##

You can run the quickstart either using Cordova CLI or using [Visual Studio Tools for Apache Cordova](https://www.visualstudio.com/en-us/features/cordova-vs.aspx). Running the quickstart using VS Tools for Apache Cordova provides a seamless experience by letting you debug and run the quickstart on different platforms (Android, iOS, Windows and Windows Phone) from within Visual Studio.

### Running Quickstart Using Cordova CLI

  1. Edit ./ZUMOAPPNAME/www/js/index.js and replace the **_ZUMOAPPURL_** placeholder with your Mobile App cloud backend URL.
  2. Change to the Cordova quickstart directory:

        cd ./ZUMOAPPNAME
        
  3. Add the platform you want to build the quickstart for:

        cordova platform add [android | ios | windows | wp8]
  4. Run the quickstart:

        cordova run [android | ios | windows | wp8]

#### Prerequisites

* [Cordova CLI](https://cordova.apache.org/docs/en/latest/guide/cli/index.html)
* Target platform SDK.

### Running Quickstart Using Visual Studio Tools For Apache Cordova

  1. Edit ./ZUMOAPPNAME/www/js/index.js and replace the **_ZUMOAPPURL_** placeholder with your Mobile App cloud backend URL.
  2. Open ./ZUMOAPPNAME/ZUMOAPPNAME.sln in Visual Studio
  3. From within Visual Studio, choose the platform and the emulator/device
  4. Build the project. _Skipping this step while debugging/running the project for the first time may fail to load the required cordova plugins._
  5. Run (Ctrl + F5) or Debug (F5).

#### Prerequisites

* [Visual Studio Tools For Apacha Cordova](https://www.visualstudio.com/en-us/features/cordova-vs.aspx)
* Refer [Visual Studio documentation](https://taco.visualstudio.com/en-us/docs/run-app-apache/) for platform specific setup needed for building, debugging and running your app.
