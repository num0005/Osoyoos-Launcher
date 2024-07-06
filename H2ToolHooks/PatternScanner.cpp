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

PatternScanner::PatternScanner() {
	MODULEINFO module_info;
	ZeroMemory(&module_info, sizeof(module_info));
	GetModuleInformation(GetCurrentProcess(), GetModuleHandle(NULL), &module_info, sizeof(module_info));
	module_base = size_t(module_info.lpBaseOfDll);
	module_size = module_info.SizeOfImage;
	size_t page_size = 0x1000; // just presume 4k pages
	for (auto offset = module_base & ~(page_size - 1);
			offset < ((module_size + module_base) & ~(page_size - 1)) + page_size;
			offset += page_size) {
		MEMORY_BASIC_INFORMATION memory_info;
		if (VirtualQuery(LPCVOID(offset), &memory_info, sizeof(memory_info))) {
			offset += memory_info.RegionSize - page_size;
			if (memory_info.Protect & PAGE_EXECUTE || memory_info.Protect & PAGE_EXECUTE_READ
					|| memory_info.Protect & PAGE_EXECUTE_READWRITE || memory_info.Protect & PAGE_EXECUTE_WRITECOPY)
				INSERT_INTO_RANGE_LIST(this->code, memory_info)
			if (memory_info.Protect == PAGE_READWRITE || memory_info.Protect == PAGE_WRITECOPY)
				INSERT_INTO_RANGE_LIST(this->data, memory_info)
			if (memory_info.Protect == PAGE_READONLY)
				INSERT_INTO_RANGE_LIST(this->rdata, memory_info)
		}
	}
}

#undef INSERT_INTO_RANGE_LIST