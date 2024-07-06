/*
 Copyright (c) num0005. Some rights reserved
 This software is part of the Osoyoos Launcher.
 Released under the MIT License, see LICENSE.md for more information.
*/


#include "H2ToolHooks.h"
#include "PatternScanner.h"
#include "patches.h"
#include "Debug.h"

bool H2ToolHooks::hook()
{
	/*
	* Perform signature scanning to find the instructions we need to patch
	*/
	PatternScanner scanner;

	std::array<pattern_entry, 7> hs_assert = {
		PAT_BYTES(2, {0x6A, 0x01}), // push    1 (is fatal)
		PAT_BYTE(0x68), PAT_INTEGER_RANGE(uint32_t, 2960 - 400, 2960 + 400), // push c_line_number
		PAT_BYTE(0x68), PAT_ANY(4), // push c_file_name
		PAT_BYTE(0x68), PAT_STRING_XREF("hs_type_valid(definition->return_type)")
	};

	DebugPrintf("Scanning for hs assert");
	const auto hs_match = scanner.find_pattern_in_code(hs_assert);

	

	if (hs_match) {
		const uint32_t hs_assert_call_address = hs_match->offset + hs_match->length;
		DebugPrintf("hs assert offset: %x", hs_assert_call_address);

		bool is_exit_patched = false;

		uint32_t display_assert_offset = get_function_address_from_call(hs_assert_call_address);
		uint32_t system_debugger_present_offset = get_function_address_from_call(hs_assert_call_address + 0x3 + 0x5);

		DebugPrintf("display_assert offset: %x", display_assert_offset);
		DebugPrintf("system_debugger_present offset: %x", system_debugger_present_offset);

		for (auto search_off = hs_assert_call_address + 0x3; search_off < hs_assert_call_address + 0x50; search_off++) {
			auto data = reinterpret_cast<uint8_t*>(search_off);
			// search for push -1, call system_exit
			if (data[0] == 0x6A && data[1] == 0xFF && data[2] == 0xE8) {
				auto system_exit_offset = get_function_address_from_call(search_off + 2);

				// disable system_exit by replacing it with a no-op
				DebugPrintf("Patching system_exit @ %x", system_exit_offset);
				WriteValue<uint8_t>(system_exit_offset, 0xC3);

				is_exit_patched = true;
				break;
			}
		}

		if (!is_exit_patched)
		{
			DebugPrintf("Failed to disable system_exit!");
			return false;
		}

		std::array<pattern_entry, 10> assert_pat = {
			PAT_BYTES(2, { 0x6A, 0x01}), //  push 1 (is fatal)
			PAT_BYTE(0x68), PAT_ANY(4),  //  push c_line
			PAT_BYTE(0x68), PAT_ANY(4),  //  push c_filename
			PAT_BYTE(0x68), PAT_ANY(4),  //  push c_assertion_message
			PAT_CALL(display_assert_offset), // call display_assert
			PAT_BYTES(3, { 0x83, 0xC4, 0x10}), // add esp, 10h
			PAT_CALL(system_debugger_present_offset) // call system_debugger_present
		};
		auto asserts = scanner.find_pattern_in_code_multiple(assert_pat);
		DebugPrintf("Found %d asserts", asserts.size());
		for (auto &assert : asserts) {
			WriteValue<uint8_t>(assert.offset + 1, 0x00); // disable fatal
		}
		DebugPrintf("Patched all asserts found!");

		return true;
	}
	else
	{
		DebugPrintf("Failed to find hs_assert keystone");
	}

	return false;
}