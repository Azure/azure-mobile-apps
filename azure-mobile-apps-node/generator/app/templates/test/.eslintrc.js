var OFF = 0, WARN = 1, ERROR = 2;

module.exports = exports = {
    "env": {
<% if(testFramework==='jasmine') { -%>
        "jasmine": true,
<% } else if(testFramework==='mocha') { -%>
        "mocha": true,
<% } -%>
        "node": true
    }
};
