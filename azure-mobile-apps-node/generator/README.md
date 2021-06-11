# Azure App Service Mobile Apps generator

[Yeoman](http://yeoman.io) generator to scaffold an [Azure App Service Mobile App].

## Getting Started

Install Yeoman and the Yeoman Generator for Azure Mobile Apps

    (sudo) npm install -g yo generator-azure-mobile-apps

Create an Azure Mobile Apps base application

    mkdir myproject
    cd myproject
    yo azure-mobile-apps

Start editing your project!

## Configuring an Azure Mobile Apps project

This project is designed to run inside [Azure App Service].  You will need to hook up a SQL
Azure database and [deploy your app] to Azure.

[Azure App Service]: https://azure.microsoft.com/en-us/documentation/services/app-service/
[Azure App Service Mobile App]: https://azure.microsoft.com/en-us/documentation/articles/app-service-mobile-node-backend-how-to-use-server-sdk/
[deploy your app]: https://azure.microsoft.com/en-us/documentation/articles/web-sites-deploy/

## What's included:

A Boilerplate Azure Mobile Apps service that is suitable for deploying to Azure App Service.
It includes a public web area (that is served statically) and example tables.  It also includes
basic tests for [jasmine] or [mocha] and linting for the entire project using [eslint] with
a set of rules that are fairly strict.

[jasmine]: http://jasmine.github.io/
[mocha]: http://mochajs.org/
[eslint]: http://eslint.org/

You can test the entire service using ```npm test``` - this will also run the linter.

Start the server locally using ```npm start```.
