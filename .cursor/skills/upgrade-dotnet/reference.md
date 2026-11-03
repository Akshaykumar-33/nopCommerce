# nopCommerce .NET upgrade reference

Project-specific paths and commands for this repository.

## Canonical pins

| Pin | Path |
|-----|------|
| Shared TFM | `src/Directory.Build.props` |
| SDK | `global.json` (repo root) |
| CI | `.github/workflows/dotnet.yml` |
| Docker | `Dockerfile` (`sdk` + `aspnet` alpine images) |
| Helper source | `src/Build/src/ClearPluginAssemblies/ClearPluginAssemblies.csproj` |
| Helper invoke proj | `src/Build/ClearPluginAssemblies.proj` (hardcodes TFM) |
| Checked-in helper | `src/Build/ClearPluginAssemblies.dll`, `ClearPluginAssemblies.runtimeconfig.json` |
| Framework packages | `src/Libraries/Nop.Core/Nop.Core.csproj`, `Nop.Data.csproj`, `src/Tests/Nop.Tests/Nop.Tests.csproj`, Facebook auth plugin |
| Plugin TFM copies | each `src/Plugins/**/*.csproj` and matching `Notes.txt` |

Core / data / web / test projects inherit TFM from `Directory.Build.props`. Most plugins still declare `<TargetFramework>` explicitly—update both.

## Package train (example: .NET 11 RC 3)

Align these to the same runtime build when retargeting:

- `Microsoft.AspNetCore.Mvc.NewtonsoftJson`
- `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation`
- `Microsoft.Extensions.Caching.StackExchangeRedis`
- `Microsoft.Extensions.Caching.SqlServer`
- `Microsoft.AspNetCore.Authentication.Facebook`
- `Microsoft.Data.Sqlite`
- `System.Configuration.ConfigurationManager`

Leave unrelated third-party packages alone unless restore/build requires a bump.

## Rebuild ClearPluginAssemblies

```bash
dotnet build src/Build/src/ClearPluginAssemblies/ClearPluginAssemblies.csproj -c Release
cp -f src/Build/src/ClearPluginAssemblies/bin/Release/net{N}.0/ClearPluginAssemblies.dll src/Build/
cp -f src/Build/src/ClearPluginAssemblies/bin/Release/net{N}.0/ClearPluginAssemblies.runtimeconfig.json src/Build/
```

Commit both the DLL and runtimeconfig with the TFM change.

## .NET 11 fixes applied in-tree

### Shipping cube root

`ShippingService.GetDimensionsAsync` used `Convert.ToDecimal(Math.Pow(Convert.ToDouble(volume), 1/3))`. Exact double→decimal conversion made perfect volumes (for example 64) land slightly under 4. Keep product behavior by converting with 15 significant digits (`G15` + `decimal.Parse`) when bridging double math back to decimal dimensions.

### Price TestCases

NUnit `[TestCase]` numeric literals are doubles and cannot be `decimal` constants. Accept `double` parameters and convert with the same `G15` helper before calling price APIs / assertions.

### Shared-framework package

`System.IO.FileSystem.AccessControl` is available in the shared framework on modern TFMs—remove the `PackageReference` when `NU1510` appears and ACL APIs still compile.

### Razor runtime compilation

nopCommerce still calls `AddRazorRuntimeCompilation()` for theme/plugin views edited on disk. Keep the call and suppress `ASPDEPR003` locally with a one-line reason.

## Verify commands

```bash
dotnet build src/NopCommerce.sln -c Release
dotnet test src/NopCommerce.sln -c Release
```

## Search leftovers

```bash
rg 'net10\\.0|dotnet-version: 10|sdk:10\\.0|aspnet:10\\.0|\\.NET 10' -g '!**/package-lock.json' -g '!**/bin/**' -g '!**/obj/**'
```

Replace `10` with the previous major when upgrading from a different baseline.
