/*
 Copyright (c) num0005. Some rights reserved
 This software is part of the Osoyoos Launcher.
 Released under the MIT License, see LICENSE.md for more information.
*/


#include "H2ToolHooks.h"
#include "PatternScanner.h"
#include "patches.h"

struct worker_info_offsets
{
	const uint32_t count_offset;
	const uint32_t index_offset;
};

std::optional<worker_info_offsets> find_worker_info_offsets(PatternScanner &scanner)
{
	const std::array<pattern_entry, 6> merging_worker_bitmaps_push_count = {
		PAT_PUSH_STRING_XREF("merging worker bitmaps"),
		PAT_BYTE(0xE8), // any call
		PAT_ANY(4),
		PAT_BYTE(0x59), // pop ECX
		PAT_BYTES(2, {0xFF, 0x35}) // push value of
		// lightmap.worker_count
	};

	std::optional<PatternScanner::Match> worker_count_match = scanner.find_pattern_in_code(merging_worker_bitmaps_push_count);
	if (!worker_count_match.has_value())
		return {};

	const uint32_t worker_count_offset = ReadFromAddress<uint32_t>(worker_count_match->offset + worker_count_match->length);

	const std::array<pattern_entry, 5> worker_index_pattern = {
		PAT_BYTE(0x99), // cdq
		PAT_BYTES(2, {0xF7, 0x3D}), PAT_POD_TYPE(worker_count_offset), // idiv lightmap.worker_count
		PAT_BYTES(2, {0x3B, 0x15}), PAT_INTEGER_RANGE(uint32_t, worker_count_offset - 0x200, worker_count_offset + 0x200) // cmp     edx, lightmap.worker_index
		// we assume the two values will be close to each other in memory
	};

	std::optional<PatternScanner::Match> worker_index_match = scanner.find_pattern_in_code(worker_index_pattern);
	if (!worker_index_match.has_value())
		return {};

	const uint32_t worker_index_offset = ReadFromAddress<uint32_t>(worker_index_match->offset + worker_index_match->length - 4);

	return worker_info_offsets{ worker_count_offset ,worker_index_offset };

}


bool H2ToolHooks::hook()
{
	/*
	* Perform signature scanning to find the instructions we need to patch
	*/
	PatternScanner scanner;

	std::optional<worker_info_offsets> worker_info = find_worker_info_offsets(scanner);

	if (!worker_info.has_value())
	{
		printf("[DLL FIX] Failed to find worker config\n");
		return false;
	}

	const std::array<pattern_entry, 14> editable_structure_lightmap_pattern = {
		PAT_BYTE(0xA3), PAT_INTEGER_RANGE(uint32_t, worker_info->count_offset - 0x1000, worker_info->count_offset + 0x1000), // mov lightmap.editable_structure_lightmap, eax
		PAT_ANY_RANGE(2, 10), // accept between 2 and 10 bytes in case this changes
		// pop     ecx, pop     ecx - in current build
		// cmp     eax, edi in current build
		PAT_BYTE(0x75), PAT_ANY(1), // jnz assert_not_failed
		PAT_BYTES(2, {0x6A, 0x01}), // push    1
		PAT_BYTE(0x68), PAT_INTEGER_RANGE(uint32_t, 600, 700), // 642 for current build
		PAT_BYTE(0x68), PAT_ANY(4), // any push (filename)
		PAT_PUSH_STRING_XREF("global_lightmap_control.editable_structure_lightmap.create_new(lightmap_tag_path) failed"),
		PAT_BYTE(0xE8), PAT_ANY(4), // any call (call to assertion failed function)
	};

	auto editable_structure_lightmap_match = scanner.find_pattern_in_code(editable_structure_lightmap_pattern);
	if (!editable_structure_lightmap_match.has_value())
	{
		printf("[DLL FIX] Failed to find editable structure offset\n");
		return false;
	}

	uint32_t editable_structure_lightmap_offset = ReadFromAddress<uint32_t>(editable_structure_lightmap_match->offset + 1);


	const std::array<pattern_entry, 14> tag_save_lightmap_pattern = {
		PAT_BYTES(2, {0x83, 0x3D}), PAT_INTEGER_RANGE(uint32_t, worker_info->count_offset - 0x1000, worker_info->count_offset + 0x1000), PAT_BYTE(0x02), // cmp lightmap.mode, 2 # check if the mode is farm
		PAT_PUSH_STRING_XREF("lightmap merge succeeded, palettized and truecolor bitmaps outputted\n"),
		PAT_BYTE(0xE8), PAT_ANY(4), // any call
		PAT_BYTE(0xEB), PAT_ANY(1), // jmp <???>


		PAT_ANY_RANGE(0, 0x40), // in the current build there is just a tag save call in between this and our target call
		PAT_BYTES(2, {0xFF, 0x35}), PAT_POD_TYPE(editable_structure_lightmap_offset), // push editable_structure_lightmap_offset
		PAT_BYTE(0xE8), PAT_ANY(4), // call tag_save
	};

	auto tag_save_lightmap_match = scanner.find_pattern_in_code(tag_save_lightmap_pattern);

	if (!tag_save_lightmap_match)
	{
		printf("[DLL FIX] Failed to find tag_save call for lightmaps tag\n");
		return false;
	}

	uint32_t tag_save_address = get_function_address(tag_save_lightmap_match->offset + tag_save_lightmap_match->length - 5);

	// if we found the lightmap structure save call, lets see if we can find the scenario save logic. This is not strictly required so a failure is not fatal.

	const std::array<pattern_entry, 14> tag_save_scenario_pattern = {
		PAT_BYTES(2, {0xFF, 0xB5}), PAT_ANY(2), PAT_BYTES(2, {0xFF, 0xFF}), // push    [ebp+scenario_tag_index], we can assume the upper bytes of the variable index will not change
		PAT_CALL(tag_save_address),
		PAT_ANY_RANGE(0, 0x10),
		PAT_PUSH_STRING_XREF("###WARN: scenario is not writable and has bad lightmap references!"),
	};

	auto tag_save_scenario_match = scanner.find_pattern_in_code(tag_save_scenario_pattern);

	/*
	* APPLY PATCHES
	* 
	* Since this will be injected by the laucnher we can get away with just nopfilling the code we don't want to run, instead of properly detouring. At least for now we can.
	*/

	NopFill(tag_save_lightmap_match->offset + tag_save_lightmap_match->length - 5, 5); // disable call to save lightmap

	if (tag_save_scenario_match)
	{
		NopFill(tag_save_scenario_match->offset + 6, 5); // disable scenario save
	}

	return true;
}