/*
 Copyright (c) num0005. Some rights reserved
 This software is part of the Osoyoos Launcher.
 Released under the MIT License, see LICENSE.md for more information.
*/

#include "PatternScanner.h"
#include "platform.h"
#include "psapi.h"

#define INSERT_INTO_RANGE_LIST(list, memory_info) \
	list.push_back(std::pair<uint32_t, uint32_t>(uint32_t(memory_info.BaseAddress), memory_info.RegionSize));

static inline size_t align(size_t value, size_t alignment)
{
	return value & ~(alignment - 1);
}

PatternScanner::PatternScanner() {
	MODULEINFO module_info;
	ZeroMemory(&module_info, sizeof(module_info));
	GetModuleInformation(GetCurrentProcess(), GetModuleHandle(NULL), &module_info, sizeof(module_info));
	module_base = size_t(module_info.lpBaseOfDll);
	module_size = module_info.SizeOfImage;


	size_t page_size = 0x1000; // just presume 4k pages
	size_t module_start = align(module_base, page_size); // align down
	size_t module_end = align((module_size + module_base) + (page_size - 1), page_size); // align up

	DebugPrintf("Module range: %x-%x", module_start, module_end);

	auto offset = module_start;
	while (offset < module_end) {
		MEMORY_BASIC_INFORMATION memory_info;
		if (VirtualQuery(LPCVOID(offset), &memory_info, sizeof(memory_info))) {

			if (memory_info.Protect & PAGE_EXECUTE || memory_info.Protect & PAGE_EXECUTE_READ
					|| memory_info.Protect & PAGE_EXECUTE_READWRITE || memory_info.Protect & PAGE_EXECUTE_WRITECOPY)
				INSERT_INTO_RANGE_LIST(this->code, memory_info)
			if (memory_info.Protect == PAGE_READWRITE || memory_info.Protect == PAGE_WRITECOPY)
				INSERT_INTO_RANGE_LIST(this->data, memory_info)
			if (memory_info.Protect == PAGE_READONLY)
				INSERT_INTO_RANGE_LIST(this->rdata, memory_info)

			offset = (size_t)memory_info.BaseAddress + memory_info.RegionSize;
		}
		else
		{
#if _DEBUG
			DebugPrintf("Failed to get memory info for %x", offset);
#endif
			// try next page anyways
			offset += page_size;
		}
	}
}

#undef INSERT_INTO_RANGE_LIST