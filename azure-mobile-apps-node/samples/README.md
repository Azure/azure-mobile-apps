# Azure Mobile Apps - Node SDK Samples

This directory contains a range of samples covering topics such as table
configuration, custom APIs, authentication and push notifications.

## Installation

To use the samples, obtain the azure-mobile-apps-node repository from github
by executing the following in a console window:

    git clone https://github.com/Azure/azure-mobile-apps-node

Change the current working directory to the sample you want to run and execute:

    npm i

The sample server can then be started by executing:

    node --debug app.js

For extended logging, execute the sample with an additional command line parameter:

    node --debug app.js ---logging.level silly

## Usage

Many samples are designed to work with the client quickstarts available
[here](https://github.com/Azure/azure-mobile-services-quickstarts), or through
the portal for your app, under the Quickstart section. The sample README.md
file will inform you if the sample is intended to work with these clients.

For samples that do not, you can either create a client application to test
the sample server, or use a tool like [postman](https://www.getpostman.com/)
to make HTTP requests against the server.

## Questions / Feedback?

If you are having problems with one of the samples, or have some feedback,
please create a thread or question on StackOverflow or the MSDN forums, or
chat with us on our Gitter channel.

- [Chat on Gitter](https://gitter.im/Azure/azure-mobile-apps-node?utm_source=share-link&utm_medium=link&utm_campaign=share-link)
- [StackOverflow #azure-mobile-services](http://stackoverflow.com/questions/tagged/azure-mobile-services?sort=newest&pageSize=20)
- [MSDN Forums](https://social.msdn.microsoft.com/forums/azure/en-US/home?forum=azuremobile)
