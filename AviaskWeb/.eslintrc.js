module.exports = {
    env: {
        node: true,
    },
    extends: ["plugin:vue/vue3-recommended", "eslint:recommended", "prettier", "@vue/typescript/recommended"],
    rules: {
        "@typescript-eslint/no-unused-vars": [
            "error",
            {
                argsIgnorePattern: "^_",
                varsIgnorePattern: "^_",
                caughtErrorsIgnorePattern: "^_",
            },
        ],
    },
};
