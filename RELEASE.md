# Release Process

This document describes the automated release process for `arena-unity`. This package is distributed via the Unity Package Manager (UPM) connected to Git tags. The process uses **release-please** to automate version bumping, changelog generation, and GitHub release creation.

## Overview

```text
Finalize Code on main → Merge release-please PR → Automated Tag & Release
```

## Prerequisites

1. **Test Compilation**: Ensure the project compiles successfully in both Unity 2022.3 and Unity 6 (if supported).
2. **Test Runtime**: Hit "Play" in the Editor and verify connection, object synchronization, and GLTF export functionality.

## Release Steps

### 1. Finalize Code on `main`

Ensure all features and fixes for this release are merged into `main` using [Conventional Commits](https://www.conventionalcommits.org/) format:

```text
feat: add gaussian splatting component support
fix: correct physx API differences for Unity 6
chore: update test dependencies
```

> **Note:** Only `feat:`, `fix:`, and similar functional commits generate changelog entries and trigger version bumps. `chore:`, `docs:`, `ci:` etc. are usually excluded from the changelog or trigger smaller patch bumps depending on configuration.

### 2. Merge the release-please PR

After pushing to `main`, the **release-please** GitHub Action automatically creates (or updates) a release PR titled something like:

```text
chore(main): release 1.6.1
```

This PR contains:
- Updated `CHANGELOG.md` with all new entries generated from your commit history.
- Updated `package.json` with the new semantic version.

**Review the changelog**, then **merge the PR**. 

### 3. Automated Tag and Release

Merging the PR triggers release-please to automatically:
- Create an annotated **git tag** (e.g., `v1.6.1`).
- Create a **GitHub Release** populated with the changelog.

### 4. Downstream Updates (Unity Projects)

Unity projects referencing the GitHub URL directly as a dependency will now be able to fetch or upgrade to the newly tagged version using the Unity Package Manager.

## Troubleshooting

### release-please PR not updating
If you push new commits to `main` and the open release PR doesn't update, check the GitHub Actions tab for any failing `release-please` workflow runs. Sometimes a manually triggered workflow dispatch is needed if the action is stuck.
