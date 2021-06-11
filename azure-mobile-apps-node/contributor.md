# Contribute to the Mobile Apps Node.js SDK

## Ways to contribute

You can contribute to the Azure Mobile Apps Node.js SDK in a number of ways:
 - Help answer questions on [StackOverflow](http://stackoverflow.com/questions/new/azure-mobile-services?show=all&sort=recentlyactive) and [MSDN](https://social.msdn.microsoft.com/forums/azure/en-US/home?forum=azuremobile)
 - [Submit an issue](https://github.com/Azure/azure-mobile-apps-node/issues) with steps to reproduce the issue
 - Give us feedback at [feedback.azure.com](http://feedback.azure.com/forums/216254-mobile-apps-formerly-mobile-services)
 - Suggest changes/help fix our docs on [azure-content](https://github.com/Azure/azure-content)
 - Directly contribute to the source code
 - Create a package for the community to consume

Check out the central [Azure Contributor Guidlines](http://azure.github.io/guidelines.html) page to learn more on how you can participate in our community.

## Submitting an issue

Before you submit an issue, please consider the following:
 - For questions, we ask that you please use [StackOverflow](http://stackoverflow.com/questions/new/azure-mobile-services?show=all&sort=recentlyactive) and [MSDN](https://social.msdn.microsoft.com/forums/azure/en-US/home?forum=azuremobile).
 - Please check that the issue doesn't already exist to prevent having your issue closed as a duplicate.

When you're ready to submit an issue, we ask that you please attempt to document the steps and settings that can lead to a reproduction of that issue. Some things to consider:
 - Node.js & npm version `node -v`/`npm -v`
 - Server SDK version (see package.json)
 - Client SDK name and version (i.e. iOS 2.x.x)
 - Turn on all levels of logs and include the output
 - A list of your environment settings
    - Be sure to censor sensitive information
    - These settings can be gotten through the Web Portal or via Kudu
 - If your project is Open Sourced, feel free to include a link
 - A discrete list of steps that leads to a reproduction

We will try and respond to issues in a timely fashion. It is our goal to triage and priotize defects in a transparent way based on their technical merit and impact. If a defect that is blocking your product isn't being prioritized appropriately, feel free to get a hold of the team via the [Contact Us](./README.md#contact-us) section of our README and we can have a more in depth discussion about the priority.

## Contributing to the SDK git repository

### Contributing code

To contibute to this project you will need to be familar with Node.js and JavaScript. It will help if you are familiar with [Express](http://expressjs.com/). For testing, please be familar with [BDD](https://en.wikipedia.org/wiki/Behavior-driven_development) and, specifically, the [mocha](https://mochajs.org/) test framework and its BDD patterns. You also must be familiar with git and proper git contibution etiquette. You can check out these resources for more information:
 - [Open Source Contribution Etiquette](http://tirania.org/blog/archive/2010/Dec-31.html) by Miguel de Icaza
 - [Don't "Push" Your Pull Requests](https://www.igvita.com/2011/12/19/dont-push-your-pull-requests/) by Ilya Grigorik
 - [A Successful Git Branching Model](http://nvie.com/posts/a-successful-git-branching-model/) by Vincent Driessen

Before you start work on any feature or large contribution, please follow our guidelines for [submitting an issue](#submitting-an-issue) and the Azure Mobile Engineering team and other community members will engage you in a discussion of the scope and strategy for your contibution. If your feature doesn't make sense for our utilities/promises SDK, it may make sense as a separate piece of middleware that the community can add. We will do our best to guide you to success. Please be aware that agreeing that your feature sounds like a good idea before you start does not assure that it will be accepted when you make a PR. We have high standards for utilities/promises SDK and will be defensive both in terms of keeping quality high and the functional scope managable.

Please send Pull Requests that are matched against the 'master' branch in the azure/azure-mobile-apps-node repository.

Once you have made a pull request, the Azure team and the community will engage with you in a review of your code, tests, design decisions, and more. Your code is expected to pass JS Hint with our JS Hint rules as well as generally meet our guidelines and coding conventions. It is also expected that any changes you make include new and updated tests to validate that your code works and that others who change the code will not break your features. We may make numerous change requests of you to meet our standards, but we will try to be transparent about our requirements. If anything seems unclear, please reach out to us and we will attempt to provide clarifying information.

Overall, please try to make your contributions in as focused a means as possible. If you want to contribute two feature adds, please create two pull requests.

### Legal

Azure has a common set of contributor guidelines found here: [http://azure.github.io/guidelines.html](http://azure.github.io/guidelines.html). One of the requirements before we can accept your PR is that you follow the directions for the [Contributor License Agreement](http://azure.github.io/guidelines.html#cla) found in the Azure Contributor Guidelines.

### Getting started with Contributing

#### Set up development environment

0. Fork the Azure Mobile Apps repository
0. Clone the repository - `git clone https://github.com/Azure/azure-mobile-apps-node.git`
0. Provision a database either in Azure SQL DB (x-plat) or a local SQL Express instance (windows only)
0. Execute the [azure-mobile-apps-test.sql](./node_modules/azure-mobile-apps.data.sql/test/azure-mobile-apps-test.sql) script against your target database.
0. Install mocha globally - `npm install -g mocha`
0. Set your environment settings
 - `ms_tableconnectionstring` should be your Azure SQL DB connection string (omit this if using a local SQL Express instance instead)
 - `ms_signingkey` comes from your Azure Gateway (auth only)
 - `ms_mobileappname` is a name given to your test Apps (optional)

 In powershell, you can set these settings via ``$env:<SETTINGNAME> = <SETTINGVALUE>``

 In the future, you'll need to provide additional configuration for Push
0. You should run tests prior to development to ensure you've properly set up your environment. To run tests, run `npm test`.

#### Apply your changes

1. Make the changes you need to the code. Be sure that you've created tests and checked that all past tests continue to pass. (In the future, we may also have some clean up steps like running jshint).
2. Add your changes with `git add <file names>` and create a commit with `git commit -m 'meaningful commit message here'`
3. Now push your changes with `git push`.

Once you've gotten all your work assembled to commit back to the main repository, create a pull request in GitHub. Resolve any conflicts, and then a discussion can begin over the pull request. This may take a few iterations, so please be patient. We want to make sure the code and test quality is good and may have some lingering functional questions left.
