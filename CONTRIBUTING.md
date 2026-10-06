# Contributing

Thanks for helping improve SAP Wialon Fleet Audit! Bug reports, ideas and pull requests are welcome.

## Before you start

* Read [docs/development.md](docs/development.md) for how to build and test. You don't need a Windows machine: every pull request
  is built, unit-tested and UI smoke-tested on a GitHub-hosted Windows runner, and the app and screenshots are uploaded as
  artifacts.
* For a larger change, open an issue first so we can agree on the approach.

## Pull requests

1. Keep each pull request focused on one change, with a clear description of what changed and why.
2. All UI text goes through `IntegrationSolution.Localization/Resources/Strings.resx` **and** its `uk` and `ru` translations.
   The localization tests fail otherwise. See [docs/localization.md](docs/localization.md).
3. Follow the existing MVVM and Prism structure: views without logic, view models derived from `ViewModelBase`, services
   registered in their module. See [docs/architecture.md](docs/architecture.md).
4. Tests that need external systems (Wialon, local files) get `[TestCategory("Integration")]`, so CI can skip them.
5. **Windows CI must be green.** If a change affects the UI, a maintainer adds the `update-screenshots` label to refresh
   `docs/screenshots`. This works for branches of this repository only.

## Reporting a bug

Describe what you did, what you expected and what happened. Attach the relevant part of `log\log-file.log` (in the app's working folder)
and a screenshot if you can. Never post Wialon tokens or real waybill data.
