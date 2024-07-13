/*
 Copyright (c) num0005. Some rights reserved
 This software is part of the Osoyoos Launcher.
 Released under the MIT License, see LICENSE.md for more information.
*/


#pragma once
namespace H2ToolHooks
{
	enum HookFlags
	{
		None = 0,

		DisableAsserts = 1 << 0,
		PatchLightmapQuality = 1 << 1,
	};
	bool hook(HookFlags flags);
}