module.exports = {
    entry: "./index.js",
    output: {
        path: __dirname,
        filename: "generated/bundle.js"
    },
    module: {
        loaders: [
            { test: /\.json$/, loader: "json" }
        ]
    }
};
