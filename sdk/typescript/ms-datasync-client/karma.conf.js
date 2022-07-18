// Karma configuration
// Generated on Fri Jul 15 2022 15:16:33 GMT-0700 (Pacific Daylight Time)

module.exports = function(config) {
  config.set({
    basePath: '',
    frameworks: [
      'mocha', 
      'karma-typescript'
    ],
    plugins: [
      'karma-chrome-launcher',
      'karma-mocha',
      'karma-sourcemap-loader',
      'karma-typescript'
    ],
    files: [
      'src/**/*.ts',
      'test/**/*.ts'
    ],
    exclude: [
    ],
    preprocessors: {
      '**/*.ts': [ 'karma-typescript', 'sourcemap' ]
    },
    reporters: [
      'progress', 
      'karma-typescript'
    ],
    port: 9876,
    colors: true,
    logLevel: config.LOG_INFO,
    autoWatch: false,
    customLaunchers: {
      ChromeDebugging: {
        base: 'Chrome',
        flags: [ '--remote-debugging-port=9333' ]
      }
    },
    browsers: ['ChromeDebugging'],
    singleRun: true,
    concurrency: 1,
    karmaTypescriptConfig: {
      extends: "../tsconfig.json",
      compilerOptions: {
        sourceMap: true,
        esModuleInterop: true
      }
    }
  });
}
