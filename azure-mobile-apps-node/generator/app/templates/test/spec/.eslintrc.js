var OFF = 0, WARN = 1, ERROR = 2;

module.exports = exports = {
    "env": {
        "mocha": true,
        "node": true
    },

    // These rules are turned off because of the spec/test.js file
    // Don't use these in production - they are evil
    "rules": {
        "indent": OFF,
        "strict": OFF,
        "func-names": OFF,
        "prefer-arrow-callback": OFF,
        "wrap-iife": OFF
    }
};
