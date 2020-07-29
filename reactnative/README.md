## Instructions

1. Install rush: `npm install -g @microsoft/rush`
2. From this directory, execute: `rush update`
   1. If you're in a bad state, execute: `rush update --purge`
3. Build solution: `rush rebuild`

More information: [Rush - Everyday commands](https://rushjs.io/pages/developer/everyday_commands/)

## Adding dependency

1. `cd` into your project directory.
2. Add the package using: `rush add --package "@azure/mobile-client"`
   1. In this example, we are adding a local dependency, `@azure/mobile-client`
3. Execute: `rush build` or `rush rebuild`
