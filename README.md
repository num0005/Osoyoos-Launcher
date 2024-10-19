# Osoyoos Launcher

[![.NET Core](https://github.com/num0005/Osoyoos-Launcher/actions/workflows/dotnet-6.yml/badge.svg)](https://github.com/num0005/Osoyoos-Launcher/actions/workflows/dotnet-6.yml)

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
 * Halo 4 MCC
 * Halo 2 Anniversary MCC

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

0. Download and install the [.NET 6 Desktop Runtime x64](https://dotnet.microsoft.com/download/dotnet/6.0/runtime). It is *very* important that you download the **64-bit** **Desktop** runtime, otherwise the launcher won't start correctly. For convenience you can use the [direct download link]( https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-6.0.31-windows-x64-installer) but it might point to an older version as this readme is not regularly revised.

1. Download and run the launcher executable [from Github releases](https://github.com/num0005/Osoyoos-Launcher/releases).
2. Use the setup dialog and/or profile wizard to setup the paths for all toolkits you wish to use.

## Non-free content warning
The reference managedblam assembly is not covered by the MIT license and is instead covered by the MCC EULA and/or the fair dealing/fair use exemption. This reference assembly is automatically generated from the public interface of a closed source binary and contains no executable code and is only used for the purpose of interoperability.

## Sponsorship

If you found this software useful and have some spare change feel free to donate using [Github Sponsors](https://github.com/sponsors/num0005) for a one-time donation or via [Liberapay](https://liberapay.com/Osoyoos-Launcher/) for recurring donations (as that platform allows the donations to be split automatically). Donors do not receive anything in return, but any support is appreciated.

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
