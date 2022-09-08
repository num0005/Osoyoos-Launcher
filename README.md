# Osoyoos Launcher

[![.NET Core](https://github.com/num0005/Osoyoos-Launcher/actions/workflows/dotnet-5.yml/badge.svg)](https://github.com/num0005/Osoyoos-Launcher/actions/workflows/dotnet-5.yml)

## Launcher Description
The Osoyoos launcher is an application that can be used to easily interact with various HEK toolsets from a simple GUI. Profiles can be created to run released or community modified toolsets.

![A screenshot of the launcher](Osoyoos.png?raw=true "screenshot of the launcher")

## Supported Titles and Features
The following profiles are or will be supported by this launcher:

 * Halo Custom Edition
 * Halo Custom Edition - Open Sauce (W.I.P.)
 * Halo Combat Evolved Anniversary MCC (H1A-MCC)
 * Halo 2 Vista
 * Halo 2 Vista - H2Codez 
 * Halo 2 Classic MCC
 * Halo 3 MCC
 * Halo 3: ODST MCC
 * Halo Reach
 * Halo 4 MCC (Planned)
 * Halo 2 Anniversary MCC (Planned)

Launcher can do the following:

 * Import and light levels
 * Import text tags
 * Import bitmap tags
 * Import model tags
 * Import sound tags
 * Package scenario tags
 * Create profiles to manage many different toolsets from one UI
 * Run a Guerilla or Sapien instance from the UI
 * Run tool commands from the UI or open command prompt in the root of the toolkit
 * Automatically generate blank .shader tags for new models to save you time (H2/H3/ODST)

## Usage

0. Download and install the [.NET 5 Desktop Runtime x64](https://dotnet.microsoft.com/download/dotnet/5.0/runtime). It is *very* important that you download the **64-bit** **Desktop** runtime, otherwise the launcher won't start correctly. For convenience you can use the [direct download link]( https://download.visualstudio.microsoft.com/download/pr/2bfb80f2-b8f2-44b0-90c1-d3c8c1c8eac8/409dd3d3367feeeda048f4ff34b32e82/windowsdesktop-runtime-5.0.13-win-x64.exe) but it might point to an older version as this readme is not regularly revised.

1. Download and run the launcher executable [from Github releases](https://github.com/num0005/Osoyoos-Launcher/releases).
2. Use the setup dialog and/or profile wizard to setup the paths for all toolkits you wish to use.

## Credits

 * Discord user num0005#8646 (https://github.com/num0005)
   * For setting the foundation that this launcher is based on.

 * The contributors behind the Halo 2 Toolkit Launcher.
   * For helping out.
   * [Halo 2 Toolkit Launcher](https://github.com/Project-Cartographer/H2-Toolkit-Launcher)

 * Discord user General_101#9814 (https://github.com/General-101)
   * For contributing to some launcher features, documentation and real world testing.

 * Discord user con#4702 (https://github.com/csauve)
   * For valuable alpha testing and feedback.

 * 343i, Bungie and many others for their work on Halo and MCC (https://www.halowaypoint.com/en-us/games/halo-the-master-chief-collection/credits)
