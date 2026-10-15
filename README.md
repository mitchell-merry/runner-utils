# Runner Utils

This mod adds various features intended to make routing speedruns easier. These include:

- Showing triggers, enemy spawners and enemy viewcones
- Toggleable infinite ammo
- Save/Load keybinds for player location and rotation
- Snowman% time popup
- A camera that follows thrown projectiles
- Time stop
- Auto jump
- Movement information readouts
- An overlay to show what slopes on terrain you can walk up

## Usage

The default binds are:

- Enable all triggers' visibility: `O`
- Disable all triggers' visibility: `I`
- Toggle log visibility: `K`
- Clear log: `J`
- Toggle infinite ammo: `L`
- Toggle infinite health: `Comma`
- Toggle throw cam: `Semicolon`
- Save location: `Left bracket`
- Load location: `Right bracket`
- Clear saved location: `P`
- Toggle timestop: `Right Shift`
- Toggle auto jump: `P`
- Toggle hard fall overlay: `U`
- Toggle enemy viewcones: `Y`

Not bound by default:
- Toggle spawners' visibility: `None`
- Toggle OOB boxes' visibility: `None`
- Toggle all triggers' visibility: `None`
- Toggle timer start triggers' visibility: `None`
- Toggle advanced movement info: `None`

These can be modified in the `BepInEx/config/kestrel.iamyourbeast.runnerutils.cfg` file, which also contains a few other options.

## Building

You need a [.NET SDK](https://dotnet.microsoft.com/download) (8 or newer). On Arch: `sudo pacman -S dotnet-sdk`.

The compiler needs a few DLLs from the game. Either:

1. Point the project at your install (preferred if the game is on this machine). Create `RunnerUtils/RunnerUtils.csproj.user`:

```xml
<Project>
  <PropertyGroup>
    <GameDir>/path/to/I Am Your Beast</GameDir>
  </PropertyGroup>
</Project>
```

Steam Linux default is `~/.local/share/Steam/steamapps/common/I Am Your Beast` and is auto-detected if present. The Windows Steam path is auto-detected too.

2. Or copy these files from `I Am Your Beast_Data/Managed/` into the repo's `Libs/` folder:

- `Assembly-CSharp.dll`
- `Unity.InputSystem.dll`
- `Unity.TextMeshPro.dll`
- `UnityEngine.UI.dll`
- `Unity.RenderPipelines.Universal.Runtime.dll`

Then:

```
dotnet build RunnerUtils/RunnerUtils.csproj
```

Output is `RunnerUtils/bin/Debug/netstandard2.1/RunnerUtils.dll`. If BepInEx is installed in `GameDir`, the DLL is also copied to `BepInEx/plugins/kestrel-RunnerUtils-dev/`.

## Manual Installation

!!! You need [Bepinex 5](https://github.com/BepInEx/BepInEx/releases/latest) for mono !!!
(if you have no idea what the versions mean try BepInEx_win_x64_5.4.23.2 and it might work. maybe)

Once bepinex is installed, put the dll into the BepInEx/plugins folder in the game's root directory.

NOTE: Versions < 2.x.x require bepinex 6.0.0-pre2. I don't know why you'd be using an old version but if you are you need to install that instead of bepinex 5

have fun :3

<img src ="https://files.catbox.moe/fu6dqo.png" width="250" alt="kity">
