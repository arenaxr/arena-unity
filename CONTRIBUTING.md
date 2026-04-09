# Contributing to ARENA Unity

The general Contribution Guide for all ARENA projects can be found [here](https://docs.arenaxr.org/content/contributing.html).

This document covers **development rules and conventions** specific to this repository. These rules are mandatory for all contributors, including automated/agentic coding tools.

## Development Rules

### 1. MQTT Topics — Always Use the `TOPICS` Constructor

**Never hardcode MQTT topic strings.** All topic paths must be constructed using the local `TOPICS` string constructor for ease of future topics modulation. This enables future topic format refactoring without scattered string updates.

### 2. Dependencies — Pin All Versions

**All dependencies must use exact, pegged versions** (no `^`, `~`, or `*` ranges). This prevents version drift across environments and ensures reproducible builds for security.

## Local Development

To develop the `arena-unity` locally:
1. Clone this repo locally.
2. Open `Window > Package Manager` and `+ > Add package from disk...`, use your local repo location selecting `package.json`.
3. Create changes on a development fork or branch and test within a Unity project.

## Code Style
- Follow standard C# styling conventions.
- Maintain Unity Inspector layout cleanliness for `ArenaObject` components.

The `arena-unity` uses [Release Please](https://github.com/googleapis/release-please) to automate CHANGELOG generation and semantic versioning. Your PR titles *must* follow Conventional Commit standards (e.g., `feat:`, `fix:`, `chore:`).
