---
name: upgrade-dotnet
description: Upgrade a .NET / ASP.NET Core solution to the latest released .NET SDK and target framework, including package retargets, Docker/CI pins, build-tool rebuilds, and breaking-change fixes. Use when the user asks to upgrade .NET, retarget netX.0, bump the SDK in global.json, or move the app to the newest .NET version.
---

# Upgrade .NET

Upgrade the current repository to the newest .NET release Microsoft supports for production (active GA, or go-live RC when GA is not out yet).

## Quick start

1. Inventory current pins (`TargetFramework`, `global.json`, CI, Docker, Microsoft.* packages).
2. Resolve the latest production release from Microsoft docs/download pages (not training data).
3. Install that SDK locally if missing, then retarget TFM + SDK + framework packages together.
4. Rebuild any checked-in build helpers that target the old TFM.
5. Fix compile/test failures from known runtime breaking changes.
6. `dotnet build` and `dotnet test` the solution before claiming done.

## 1. Inventory

Search the repo for every pin that still names the old major:

| Area | Typical files |
|------|----------------|
| Shared TFM | `Directory.Build.props`, `*.csproj` |
| SDK pin | `global.json` |
| CI | `.github/workflows/*.yml` (`dotnet-version`) |
| Containers | `Dockerfile` (`mcr.microsoft.com/dotnet/sdk|aspnet`) |
| Build helpers | `**/ClearPluginAssemblies.*`, other committed `runtimeconfig.json` |
| Docs / samples | `README.md`, plugin `Notes.txt` templates |
| Framework packages | `Microsoft.AspNetCore.*`, `Microsoft.Extensions.*`, `Microsoft.Data.Sqlite`, `System.Configuration.ConfigurationManager` |

Do **not** rewrite unrelated version strings (npm `package-lock.json`, third-party NuGet majors that are not the .NET shared framework).

## 2. Resolve "latest"

Use live Microsoft sources:

- https://dotnet.microsoft.com/en-us/download/dotnet
- https://github.com/dotnet/core/blob/main/releases.md
- Migration doc for the jump (for example ASP.NET Core 10 → 11)

Rules:

- Prefer the newest **production-supported** build: active GA, else current **go-live** RC.
- Record exact SDK version (for `global.json` / CI) and package version (for NuGet).
- For Docker, probe MCR tags (`N.0-alpine` often tracks the current N.0 build, including RC). Confirm with image labels when unsure.
- If `global.json` pins a prerelease SDK, set `"allowPrerelease": true`.

## 3. Retarget

Apply the major bump everywhere the inventory found it:

1. Shared `TargetFramework` → `net{N}.0` (central props first; then any csproj that hardcodes TFM).
2. `global.json` SDK `version` (+ `allowPrerelease` when needed).
3. CI `dotnet-version` to the same SDK (exact prerelease string if Actions cannot resolve `N.0.x` yet).
4. Dockerfile SDK/runtime tags to `N.0` (or the verified alpine tag).
5. Bump Microsoft / Extensions / matching System packages to the same runtime train.
6. Update human-facing “runs on .NET X” docs and plugin template notes.

Preserve existing line endings (CRLF vs LF) when editing Windows-authored files.

## 4. Build helpers

If the repo ships a prebuilt helper DLL + `runtimeconfig.json` (nopCommerce: `src/Build/ClearPluginAssemblies.*`):

1. Retarget the helper csproj **and** any `.proj` that hardcodes TFM (these may sit outside `Directory.Build.props`).
2. `dotnet build` the helper for the new TFM.
3. Copy the new `.dll` and `.runtimeconfig.json` next to the checked-in tool.
4. Rebuild the main solution; MSB3073 / exit 150 from the helper usually means a stale runtimeconfig or path still pointing at the old `netX.0` output.

## 5. Breaking changes to expect

Read the official breaking-change list for the target major. Recurring themes:

| Symptom | Likely cause | Fix direction |
|---------|----------------|---------------|
| Price / money tests fail with tiny decimal deltas | `double`→`decimal` conversion is exact; NUnit `[TestCase(12.34, …)]` boxes doubles | Convert TestCase doubles with 15 significant digits (`G15` format + parse), or avoid double in monetary paths |
| Cube-root / volume dimensions off by ULP | `Convert.ToDecimal(Math.Pow(...))` after binary float math | Keep domain decimals; when double is required, apply the documented pre-N.0 significant-digit conversion if product behavior depended on it |
| `NU1510` prune warnings | Package now in the shared framework | Remove the redundant `PackageReference` if APIs still compile |
| `ASPDEPR003` Razor runtime compilation | Obsolete in recent ASP.NET | Keep only if the app needs runtime view edits; suppress locally with a short why-comment |

Do **not** blanket-disable warnings. Fix or narrowly suppress with intent.

Also skim ASP.NET migration notes for removed APIs (`SetCompatibilityVersion`, OpenAPI 3.x transformers, etc.) and delete call sites that no longer compile.

## 6. Verify

```bash
dotnet --version   # must match global.json resolution
dotnet build <solution> -c Release
dotnet test <solution> -c Release
```

Required bar:

- Zero build errors.
- Tests green, or failures explained as pre-existing and unrelated to the TFM bump.
- No remaining `net{old}.0` / old SDK pins in project, CI, Docker, or helper runtimeconfig files.

## 7. Ship

Commit on a feature branch, push, and open/update the PR with:

- From → to majors and exact SDK/package versions
- Go-live vs GA status when not on stable yet
- Notable breaking-change fixes
- Build/test counts

## Additional resources

- For the nopCommerce file map and commands used in-repo, see [reference.md](reference.md)
