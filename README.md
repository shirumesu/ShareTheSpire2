# ShareTheSpire2

Maintenance source for the hidden Slay the Spire 2 mod "Share the Spire" (Nexus ID: 157).

This mod lets multiplayer teammates send potions, relics, cards, and gold to each other without transfer fees. The original mod was hidden after game updates broke compatibility, so this repository keeps a maintainable source tree for continued compatibility work.

## Download and Install

If you only want to use the mod, download `ShareTheSpire2.zip` from the latest GitHub Release assets.

Install it like this:

1. Make sure `BaseLib` is installed for Slay the Spire 2.
2. Create this folder if it does not already exist:

```text
<Slay the Spire 2>/mods/ShareTheSpire2/
```

3. Extract `ShareTheSpire2.zip` into that folder.

The final folder should contain these files directly:

- `ShareTheSpire2.dll`
- `ShareTheSpire2.json`

## For Maintainers

## Provenance

The initial source was reconstructed from the original mod DLL with `ilspycmd`.

This is not the original project. Credit for the original mod belongs to its original author. The original Nexus Mods page was `https://www.nexusmods.com/slaythespire2/mods/157`, but it is currently hidden.

## Requirements

- .NET SDK 9.x
- `BaseLib` installed in Slay the Spire 2 for runtime use

## Build

From the repository root:

```powershell
dotnet build .\ShareTheSpire2.csproj -c Release -p:Sts2Path="<path-to-Slay the Spire 2>"
```

On Windows, the project can usually infer the Steam install location from the Steam registry entry. If that works, this is enough:

```powershell
dotnet build .\ShareTheSpire2.csproj -c Release
```

## Package

Create `artifacts/ShareTheSpire2.zip`:

```powershell
dotnet build .\ShareTheSpire2.csproj -c Release -t:PackageMod -p:Sts2Path="<path-to-Slay the Spire 2>"
```

The zip contains:

- `ShareTheSpire2.dll`
- `ShareTheSpire2.json`

## GitHub Actions

`.github/workflows/build-mod.yml` builds `artifacts/ShareTheSpire2.zip`, uploads it as a workflow artifact, and attaches it to GitHub Releases when the workflow runs for a published release or a `v*` tag.

Because the build requires local Slay the Spire 2 assemblies, the package workflow uses a self-hosted Windows runner. Configure the runner on a machine with Slay the Spire 2 installed. If the Steam registry lookup is not available to the runner service account, set a repository variable named `STS2_PATH` to the game root.

## Maintenance Notes

- Keep `mod_manifest.json` as the source manifest. The package target renames it to `ShareTheSpire2.json` for distribution.
- Use Release builds for packages intended for players.

## Notice

This is an unofficial maintenance project for a hidden mod. No ownership of the original mod is claimed here. If the original author requests a valid change or takedown, affected content may be removed.

If you are the original author and want this repository or any release artifact changed or removed, please open an issue or contact the maintainer.
