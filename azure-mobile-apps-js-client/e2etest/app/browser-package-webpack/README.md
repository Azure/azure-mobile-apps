# End to end tests using the SDK as an npm package and webpack 

This directory contains end to end tests for the Azure Mobile Apps Javascript SDK **npm package**. The default behavior is to use the local npm package, but this behavior can be changed to use the officially published npm package or the one at github. The bundler used is _webpack_.

Run the following commands to get started.
```
cd <root of this repository>
npm install
npm run build
npm run e2esetup
```

Now build the test directory:
```
cd <this directory>

npm install

// Install the SDK package.
npm install ../../..

// To change the default behavior and install from github or npm, you can run one of the following commands instead:
// npm install azure-mobile-apps-client
// npm install azure/azure-mobile-apps-js-client

npm run build
```

The end to end website is ready for use. Publish _this directory_ to a website and open it in browser.

To start a web server locally (without having to publish), change to this directory and run:
```
http-server
```

In the textbox, type the URL of the Azure Mobile Apps backend that hosts the _end to end tests_ and run the tests.

**Note** that you will have to configure the CORS settings in the Azure portal for the Azure Mobile Apps backend.

