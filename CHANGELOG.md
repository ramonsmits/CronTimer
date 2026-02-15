# Changelog

All notable changes to CronTimer are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/) and this project adheres to [Semantic Versioning](https://semver.org/).

## [Unreleased]

Bug fixes for timezone handling and restart behavior, plus new TimeProvider support for testability on net8.0+.

### Improvements

- Add `TimeProvider` support on `net8.0+` — pass an optional `TimeProvider` to the constructor for deterministic testing with `FakeTimeProvider` (e841bb3)
- Add `net8.0` target framework to the NuGet package (e841bb3)
- Include README in NuGet package via `PackageReadmeFile` (5127b39)

### Bug fixes

- Fix timezone conversion using configured timezone instead of system timezone — `ToUniversalTime()` incorrectly used the host timezone, causing wrong firing times when the host timezone differs from the configured one (522efd4)
- Fix Start/Stop/Start skipping an interval — restarting the timer now recalculates the next occurrence from the current time (84f169f)

### Internal

- Add SourceLink for debugger source mapping (579e119)
- Condition `ContinuousIntegrationBuild` on CI only (579e119)
- Remove broken `PackageProjectUrl` and redundant `IncludeSource` (579e119)
- Migrate to `.slnx` solution format (98a64d7)
- Use MinVer for version derivation from git tags (ef739e7)
- Add justfile with build, test, and pack recipes (7ed9536)
- Add GitHub Actions CI workflow with Ubuntu and Windows jobs (7ed9536, 6e72f0e)
- Add NUnit test project with TimeProvider-based testing (df7ca81)

### Dependencies

- Bump ncrontab from 3.3.1 to 3.3.3 ([#2](https://github.com/ramonsmits/CronTimer/pull/2)) (295c6a5)
- Bump TimeZoneConverter from 6.0.1 to 6.1.0 ([#1](https://github.com/ramonsmits/CronTimer/pull/1)) (e4c335e)

## [2.0.0] - 2022-12-29

Breaking release that drops net35 support due to upstream dependency changes.

### Breaking changes

- Drop `net35` target — TimeZoneConverter no longer supports net35 (a43f847)

### Internal

- No longer pinning external dependencies to a version range (a44cc21)

### Dependencies

- TimeZoneConverter 6.0.1 (4831478)

## [1.0.2] - 2022-12-29

Fixes a bug where the timer could fire multiple times for the same timestamp, and adds event args so subscribers know which occurrence triggered them.

### Improvements

- Add `At` property to `CronTimerEventArgs` so subscribers can retrieve the timestamp for which they were invoked (3a09b25)

### Bug fixes

- Fix timer firing multiple times for the same timestamp when the timer callback ran early — next occurrence is now calculated from the previous scheduled time instead of the current clock value (f17c325)

## [1.0.1] - 2021-03-16

Fixes a crash when calling Stop and allows restarting a stopped timer.

### Bug fixes

- Create timer in constructor so that `Stop()` no longer throws `NullReferenceException`, and allow the instance to be restarted after stopping (3f637d7)

## [1.0.0] - 2020-12-03

Initial release.

[Unreleased]: https://github.com/ramonsmits/CronTimer/compare/2.0.0...HEAD
[2.0.0]: https://github.com/ramonsmits/CronTimer/compare/1.0.2...2.0.0
[1.0.2]: https://github.com/ramonsmits/CronTimer/compare/1.0.1...1.0.2
[1.0.1]: https://github.com/ramonsmits/CronTimer/compare/1.0.0...1.0.1
[1.0.0]: https://github.com/ramonsmits/CronTimer/releases/tag/1.0.0
