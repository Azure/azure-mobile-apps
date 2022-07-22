const os = require('os');

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
        // Need to bypass web security for CORS issues
        flags: [ 
          '--remote-debugging-port=9333', 
          '--disable-web-security', 
          '--disable-site-isolation-trials',
          `--user-data-dir=${os.tmpdir()}`,
         ]
      }
    },
    browsers: ['ChromeDebugging'],
    singleRun: true,
    concurrency: 1,
    karmaTypescriptConfig: {
      compilerOptions: {
        sourceMap: true,
        esModuleInterop: true,
        resolveJsonModule: true
      }
    }
  });
}
