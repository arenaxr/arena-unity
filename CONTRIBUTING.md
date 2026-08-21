# Contributing to ARENA Unity

The general Contribution Guide for all ARENA projects can be found [here](https://docs.arenaxr.org/content/contributing.html).

This document covers **development rules and conventions** specific to this repository. These rules are mandatory for all contributors, including automated/agentic coding tools.

## Development Rules

### 1. MQTT Topics — Always Use the `TOPICS` Constructor

**Never hardcode MQTT topic strings.** All topic paths must be constructed using the local `TOPICS` string constructor for ease of future topics modulation. This enables future topic format refactoring without scattered string updates.

### 2. Dependencies — Pin All Versions

**All dependencies must use exact, pegged versions** (no `^`, `~`, or `*` ranges). This prevents version drift across environments and ensures reproducible builds for security.

### 3. Component Instantiation Pattern

When implementing `ApplyRender()` in an `ArenaComponent`, always use the `GetComponent` / `AddComponent` pattern to ensure Unity components are not duplicated on the `GameObject`:

```csharp
Component c = gameObject.GetComponent<Component>();
if (c == null)
{
    c = gameObject.AddComponent<Component>();
}
```
Never blindly use `gameObject.AddComponent<T>()` without first checking if the component already exists.

### 4. Component Schema Conversion Tracking

When implementing a feature in a component that has a list of attributes/properties in the comments:
- **Always make sure the list of properties stays up to date** with the list in the JSON schemas for that component.
- **Update the state** of each property as `TODO` or `DONE`. Do not delete the property from the list when completed, just change its prefix to `DONE`.

### 5. Coordinate Systems — Always Use ArenaUnity.cs Translations

**Incoming and outgoing MQTT messages MUST use the A-Frame coordinate system.** Unity rendering uses the Unity coordinate system. 
- All agents and developers must consult and use the translation utilities in `ArenaUnity.cs` (e.g., `ToUnityPosition`, `ToArenaPosition`, `ToUnityRotationQuat`, `ToArenaRotationQuat`) when passing position, rotation, and scale data between the schema objects and Unity's local transformations.
- **GLTF models** have their own coordinate system spin (LUF). There is an additional translation step to/from GLTF/Unity included in `ArenaUnity.cs` that must be applied when manipulating bones or nodes inside a GLTF hierarchy. Always refer to existing handling to ensure coordinate parity.

### 6. External Libraries — Prefer Bundled Over External

**All dependencies must be freely available** to any user. When integrating an open-source library:
- **Prefer bundling** the compiled library as a native plugin within this package (e.g., `Runtime/AprilTag/Plugin/`, `Runtime/WebP/Plugin/`) over adding an external UPM or NuGet dependency.
- **Use CI cross-compilation** (GitHub Actions) to build native plugins for all target platforms from pinned upstream source tags. This must include an iOS build step (`build-ios-arm64`) that outputs a static library (`.a`), as iOS does not support dynamically loaded external plugins.
- **Avoid external package registries** (OpenUPM, third-party scoped registries) when possible — these add setup friction and availability risk for users.
- External UPM dependencies are acceptable only when the package is published on the **Unity Package Manager** official registry (e.g., `com.unity.cloud.gltfast`, `com.unity.nuget.newtonsoft-json`).

## Local Development

To develop the `arena-unity` locally:
1. Clone this repo locally.
2. Open `Window > Package Manager` and `+ > Add package from disk...`, use your local repo location selecting `package.json`.
3. Create changes on a development fork or branch and test within a Unity project.

## Testing

There are two suites, split by what they need to run. See
[Documentation~/Testing.md](Documentation~/Testing.md) for the full guide.

The pull-request gate needs no Unity installation:
```sh
dotnet test "Tests~/DotNet/ArenaUnity.PureTests/ArenaUnity.PureTests.csproj"
```
It links the real `Runtime/` sources that are free of `UnityEngine` and runs them
under a plain .NET 8 SDK. Add tests here whenever the code under test allows it —
never write a stub or fake for a Unity type to make something testable here.

The Unity EditMode suite in `Tests/Editor` covers what genuinely needs `UnityEngine`
or the Editor. Run it from `Window > General > Test Runner > EditMode` in a project
that has this package added from disk. That project's `Packages/manifest.json` needs
both `"testables": ["io.conix.arena.unity"]` and an exactly pinned
`com.unity.test-framework` dependency (`"1.4.6"`, the version CI uses): the test
framework is not a dependency of this package, so the project running the tests adds
it. In CI this job is **skipped unless the repository has Unity licence secrets**
(`UNITY_LICENSE`, or `UNITY_EMAIL` / `UNITY_PASSWORD` / `UNITY_SERIAL`) configured
under Settings > Secrets and variables > Actions.

New test files under `Tests/Editor` are imported by Unity and so need a sibling
`.meta`; nothing under `Tests~/` does.

Some tests deliberately assert known-wrong behaviour and are marked
`// PINS CURRENT BEHAVIOUR (bug)`. **If you fix one of those bugs, flip the assertion
rather than deleting the test** — the comment names the file, line, issue, and the
correct behaviour.

## Code Style
- Follow standard C# styling conventions.
- Maintain Unity Inspector layout cleanliness for `ArenaObject` components.

The `arena-unity` uses [Release Please](https://github.com/googleapis/release-please) to automate CHANGELOG generation and semantic versioning. Your PR titles *must* follow Conventional Commit standards (e.g., `feat:`, `fix:`, `chore:`).

> [!CAUTION]
> **Never use `BREAKING CHANGE` in commit/PR bodies or the `!` suffix on commit/PR types (e.g., `feat!:`, `fix!:`).** These tokens cause release-please to automatically bump the major version. Major version increments are reserved for the maintainer's explicit decision — contributors and agents do not decide what constitutes a breaking change for semver purposes.

> [!IMPORTANT]
> **Issue and PR References in Commit & PR Messages:**
> Only use `#NN` notation in commit messages, PR titles, and PR descriptions if they correspond to actual GitHub issues or pull requests. Do **not** use `#NN` notation for internal enumerations of planning docs or triage items (e.g., use `Task NN` or plain text instead), as this creates erroneous links and may result in unintended automatic actions.


## CI & Dependency Management Conventions
- **GitHub Actions Tag SHA Pinning**: All GitHub Action references in `.github/workflows/` MUST be pinned to the exact commit SHA of the official release tag (e.g., `uses: actions/checkout@11d5960a326750d5838078e36cf38b85af677262 # v4.4.0`).
- **Inline Version Comments**: The inline comment next to the SHA MUST specify the exact tag version used. This enables Dependabot to recognize the release version, generate human-readable SemVer PR titles (`from X.Y.Z to A.B.C`), and automatically update version comments during upgrades.