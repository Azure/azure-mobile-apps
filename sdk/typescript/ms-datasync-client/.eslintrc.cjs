module.exports = {
    root: true,
    parser: '@typescript-eslint/parser',
    parserOptions: {
        project: './tsconfig.json'
    },
    plugins: [
        '@typescript-eslint'
    ],
    extends: [
        'eslint:recommended',
        'plugin:@typescript-eslint/recommended'
    ],
    "rules": {
        "quotes": [ "error", "single", { "avoidEscape": true }],
        "semi": [ "error", "always" ]
    }
};
