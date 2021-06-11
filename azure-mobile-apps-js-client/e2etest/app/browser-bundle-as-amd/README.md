# End to end tests using the SDK bundle as an AMD module 

This directory contains end to end tests for the Azure Mobile Apps Javascript SDK **bundle**. The default behavior is to use the locally built bundle, but this behavior can be changed to use the officially published bundle. For details, refer `index.js`.

Run the following commands to get started.
```
cd <root of this repository>
npm install
npm run build
npm run e2esetup
```

The end to end website is ready for use. Publish _this directory_ to a website and open it in browser.

To start a web server locally (without having to publish), run:
```
cd <this directory>
http-server
```

In the textbox, type the URL of the Azure Mobile Apps backend that hosts the _end to end tests_ and run the tests.

**Note** that you will have to configure the CORS settings in the Azure portal for the Azure Mobile Apps backend.

