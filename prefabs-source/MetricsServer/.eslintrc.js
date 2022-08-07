const { resolve } = require("path");
const prettier = require("./.prettierrc");

module.exports = {
  // https://eslint.org/docs/user-guide/configuring#configuration-cascading-and-hierarchy
  // This option interrupts the configuration hierarchy at this file
  // Remove this if you have an higher level ESLint config file (it usually happens into a monorepos)
  root: true,

  parser: "@typescript-eslint/parser",

  // https://eslint.vuejs.org/user-guide/#how-to-use-a-custom-parser
  parserOptions: {
    project: ["./tsconfig.json"],
    tsconfigRootDir: __dirname,
    ecmaVersion: 2018, // Allows for the parsing of modern ECMAScript features
    sourceType: "module", // Allows for the use of imports
  },

  // Rules order is important, please avoid shuffling them
  extends: [
    "eslint:recommended",

    // https://github.com/typescript-eslint/typescript-eslint/tree/master/packages/eslint-plugin#usage
    // ESLint typescript rules
    "plugin:@typescript-eslint/recommended",
    // consider disabling this class of rules if linting takes too long
    "plugin:@typescript-eslint/recommended-requiring-type-checking",

    "google",
    "plugin:editorconfig/all",
    "plugin:radar/recommended",
    "prettier",
    "standard-with-typescript",

    // Base ESLint recommended rules
    // 'eslint:recommended',
  ],

  plugins: [
    // required to apply rules which need type information
    "@typescript-eslint",

    "editorconfig",
    "optimize-regex",
    "prettier",
    "radar",
  ],

  // add your custom rules here
  rules: {
    // allow async-await
    "generator-star-spacing": "off",
    // allow paren-less arrow functions
    "arrow-parens": "off",
    "one-var": "off",
    "no-void": "off",
    "multiline-ternary": "off",

    "import/first": "off",
    "import/namespace": "error",
    "import/default": "error",
    "import/export": "error",
    "import/extensions": "off",
    "import/no-unresolved": "off",
    "import/no-extraneous-dependencies": "off",
    "prefer-promise-reject-errors": "off",

    // TypeScript
    quotes: ["warn", "single", { avoidEscape: true }],
    "@typescript-eslint/explicit-function-return-type": "off",

    "@typescript-eslint/member-delimiter-style": "off", // -> prettier/prettier

    "operator-linebreak": "off",

    "optimize-regex/optimize-regex": "warn",

    // Redundant rules
    "no-unused-vars": "off", // -> @typescript-eslint/no-unused-vars
    "no-return-await": "off", // -> @typescript-eslint/return-await
    "no-self-compare": "off", // -> radar/no-identical-expressions
    "no-redeclare": "off", // -> @typescript-eslint/no-redeclare
    "no-useless-constructor": "off", // -> @typescript-eslint/no-useless-constructor
    camelcase: "off", // -> @typescript-eslint/naming-convention
    "space-before-function-paren": "off", // -> @typescript-eslint/space-before-function-paren
    quotes: "off", // -> @typescript-eslint/quotes
    "@typescript-eslint/indent": "off", // -> editorconfig
    "operator-linebreak": "off", // -> prettier/prettier

    // Prettier
    "prettier/prettier": ["warn", prettier],
    "arrow-body-style": "off",
    "prefer-arrow-callback": "off",

    // Typescript
    "@typescript-eslint/no-unused-vars": "warn",
    "@typescript-eslint/strict-boolean-expressions": "off",
    "@typescript-eslint/explicit-function-return-type": "off",
    "@typescript-eslint/consistent-type-definitions": ["error", "type"],
    "@typescript-eslint/return-await": "off",
    "@typescript-eslint/no-redeclare": "off",
    "@typescript-eslint/no-misused-promises": "off",
    "@typescript-eslint/restrict-template-expressions": "off",
    "@typescript-eslint/no-useless-constructor": "error",
    "@typescript-eslint/no-invalid-void-type": "off",
    "@typescript-eslint/promise-function-async": "off",
    "@typescript-eslint/explicit-module-boundary-types": "off",
    "@typescript-eslint/no-unsafe-assignment": "off",
    "@typescript-eslint/no-unsafe-call": "off",
    "@typescript-eslint/no-unsafe-member-access": "off",
    "@typescript-eslint/no-unsafe-return": "off",
    "@typescript-eslint/space-before-function-paren": "off", // -> prettier/prettier
    "@typescript-eslint/quotes": [
      "warn",
      "single",
      {
        avoidEscape: true,
        allowTemplateLiterals: true,
      },
    ],
    "@typescript-eslint/member-delimiter-style": "off",

    // Radar
    "radar/prefer-immediate-return": "warn",
    "radar/no-duplicate-string": "off",
    "radar/cognitive-complexity": ["error", 20],

    // Vanilla Eslint - style
    semi: ["warn", "never"],
    "no-template-curly-in-string": "warn",
    "no-unreachable-loop": "warn",
    "require-jsdoc": "off",
    "quote-props": ["warn", "consistent"],
    "comma-dangle": ["warn", "always-multiline"],
    "object-curly-spacing": ["warn", "never"],

    // Vanilla Eslint - logic
    "no-console": "warn",
    "no-await-in-loop": "error",
    "no-unsafe-optional-chaining": "error",
    "no-useless-backreference": "error",
    "array-callback-return": "error",
    "no-void": "off",
    "no-invalid-this": "off",
    "require-atomic-updates": "off",

    // Editorconfig
    // noconflict
    "eol-last": "off",
    indent: "off",
    "linebreak-style": "off",
    "no-trailing-spaces": "off",
    "unicode-bom": "off",

    "editorconfig/charset": "warn",
    "editorconfig/eol-last": "warn",
    "editorconfig/indent": "off",
    "editorconfig/linebreak-style": "warn",
    "editorconfig/no-trailing-spaces": "warn",
  },
};
