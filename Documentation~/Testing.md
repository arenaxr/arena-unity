# Testing

`arena-unity` has two test suites. They are split by what they need to run, not by
what they cover:

| Suite | Location | Needs | Runs in CI |
| --- | --- | --- | --- |
| Pure .NET unit tests | `Tests~/DotNet/ArenaUnity.PureTests` | A .NET 8 SDK. No Unity, no licence. | Always — this is the pull-request gate. |
| Unity EditMode tests | `Tests/Editor` | A Unity 6 Editor. | Only when Unity licence secrets are configured. |

The split exists because the Unity Test Framework needs a licensed Editor, which a
pull request from a fork cannot have. Anything that can be tested without
`UnityEngine` therefore lives in the .NET suite so that it always gates merges.

## Running the .NET suite

Prerequisites: the .NET 8 SDK (`dotnet --version` should report `8.x`). Nothing else —
you do not need Unity installed.

```sh
dotnet test "Tests~/DotNet/ArenaUnity.PureTests/ArenaUnity.PureTests.csproj"
```

The project does not copy any package source. It links the real files out of
`Runtime/` with `<Compile Include>`, so these tests always compile the shipping code.
Only sources that are free of `UnityEngine` can be linked in; if you add one that
is not, it simply will not compile.

To run a single fixture or a single test, filter by fully qualified name:

```sh
# one fixture
dotnet test "Tests~/DotNet/ArenaUnity.PureTests/ArenaUnity.PureTests.csproj" \
  --filter "FullyQualifiedName~ArenaTopicsTests"

# one test
dotnet test "Tests~/DotNet/ArenaUnity.PureTests/ArenaUnity.PureTests.csproj" \
  --filter "FullyQualifiedName~ArenaTopicsTests.DefaultConstructor"

# everything except one fixture
dotnet test "Tests~/DotNet/ArenaUnity.PureTests/ArenaUnity.PureTests.csproj" \
  --filter "FullyQualifiedName!~ArenaCssColorsTests"
```

The `~` in `Tests~` is what keeps Unity from importing this project, the same
convention `Samples~/` and `Documentation~/` use. Nothing under `Tests~/` needs a
`.meta` file.

## Running the Unity EditMode suite

Prerequisites: Unity **6000.0** or newer, matching the `unity` field in `package.json`.

Because `arena-unity` is a UPM package rather than a Unity project, the Test Runner
needs a project to host it:

1. Create a new Unity 6 project (or open an existing one).
2. `Window > Package Manager`, then `+ > Add package from disk...` and select this
   repository's `package.json`.
3. Edit the project's `Packages/manifest.json`. Two entries are needed: the package
   has to be listed in `testables`, because Unity only discovers tests inside a
   package when it is named there, and the test project has to depend on
   `com.unity.test-framework` itself. `arena-unity` does **not** declare the test
   framework as a package dependency — it is only needed to run the tests, not to use
   the library — so nothing else pulls it in. Pin it to an exact version, with no
   `^`, `~` or `*`:

   ```json
   {
     "dependencies": {
       "io.conix.arena.unity": "file:../../arena-unity",
       "com.unity.test-framework": "1.4.6"
     },
     "testables": [
       "io.conix.arena.unity"
     ]
   }
   ```

   `1.4.6` is the version CI runs; see the harness manifest in
   `.github/workflows/test.yaml`.

4. `Window > General > Test Runner`, choose the **EditMode** tab, and **Run All**.

The same run from the command line, for scripting or for reproducing a CI failure:

```sh
/path/to/Unity \
  -runTests \
  -batchmode \
  -projectPath /path/to/your/harness-project \
  -testPlatform EditMode \
  -testResults ./editmode-results.xml \
  -logFile -
```

`-runTests` implies quitting when the run finishes. The exit code is non-zero if any
test fails, and `editmode-results.xml` is an NUnit3 result file.

## What CI runs

Both suites are wired up in `.github/workflows/test.yaml`, on every `pull_request`
and on pushes to `main`.

**Only the `dotnet-tests` job runs by default.** It needs no secrets, so it also runs
on pull requests from forks, and it is the job that actually gates a merge.

The `unity-editmode-tests` job is **skipped unless the repository has Unity licence
secrets configured**. A preceding `check-unity-license` job probes for them and
publishes a job output, because a `${{ secrets.* }}` expression is not evaluated in a
job-level `if:`. A skipped job does not fail the pull request.

To enable it, a maintainer adds either of the following under
**Settings > Secrets and variables > Actions**:

- `UNITY_LICENSE` — the contents of a personal-licence `.ulf` file; or
- `UNITY_EMAIL`, `UNITY_PASSWORD` and `UNITY_SERIAL` — for a Plus/Pro licence.

The job then creates a throwaway Unity project, adds the checked-out package to it
from disk, adds a pinned `com.unity.test-framework` and sets `testables`, and runs the
EditMode tests through `game-ci/unity-test-runner`. Note that secrets are not exposed
to workflows triggered by a pull request from a fork, so this job stays skipped for
fork contributions by design.

## Adding a test

**To the .NET suite** — add a `.cs` file under
`Tests~/DotNet/ArenaUnity.PureTests/`. It is picked up automatically; there is no
file list to update and no `.meta` file to create. If the code you want to cover
needs `UnityEngine`, it belongs in the Unity suite instead — do not write a stub or
fake for a Unity type, because a test that exercises a reimplementation of `Vector3`
proves nothing about the real one.

**To the Unity suite** — add a `.cs` file under `Tests/Editor/`. Unity imports this
folder, so the file needs a sibling `.meta`. Let the Editor generate it by opening
the project once, then commit both the `.cs` and the `.cs.meta`. Keep assertions
within the NUnit 3.0 API surface: `com.unity.test-framework` brings
`com.unity.ext.nunit`, which is based on NUnit 3.5, so newer helpers such as
`Assert.Multiple` and `Is.AnyOf` are not available there.

## The `PINS CURRENT BEHAVIOUR (bug)` convention

Some tests assert behaviour that is *wrong*. They exist so that a known fault is
recorded and cannot change silently, and they are marked like this:

```csharp
/// PINS CURRENT BEHAVIOUR (bug): <file>:<line> does X.
/// Refs #<issue>
/// CORRECT BEHAVIOUR would be Y:
///     <the fix>
/// When that fix lands, flip this assertion to ...
```

**If you are fixing one of these bugs, flip the assertion — do not delete the
test.** The comment names the file and line, the issue, what the correct behaviour
is, and which assertion to change. Deleting it removes the only coverage of that
code path. Where the corrected expectation was obvious enough to write down in
advance, a ready-made `[Ignore]`d test sits next to the pin; enable it by removing
its `[Ignore]` attribute.

A pin is not an excuse to leave a bug in place. It is a way to keep the test suite
green and honest at the same time while the fix is scheduled separately.
