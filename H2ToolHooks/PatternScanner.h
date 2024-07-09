/*
 Copyright (c) num0005. Some rights reserved
 This software is part of the Osoyoos Launcher.
 Released under the MIT License, see LICENSE.md for more information.
*/

#pragma once
#include <tuple>
#include <vector>
#include <memory>
#include <array>
#include <optional>
#include "Debug.h"

inline static uint32_t get_function_address_from_call(uint32_t call) {
	return *reinterpret_cast<uint32_t*>(call + 1) + (call + 5);
}


#define PAT_UNI(type, ...) \
	std::make_unique<type>(type(##__VA_ARGS__))

class PatternScanner;

class PatternEntryBase
{
public:
	virtual size_t entry_size() const = 0;
	virtual bool matches(const PatternScanner& scanner, const uint8_t* data) const = 0;
	virtual size_t size_required() const {
		return entry_size();
	}
};

typedef std::unique_ptr<PatternEntryBase> pattern_entry;

class PatternScanner
{
	typedef std::vector<std::pair<uint32_t, uint32_t>> range_list;
public:
	PatternScanner();
	bool is_in_rdata_segment(const uint8_t* pointer) const {
		if (!is_in_module(pointer))
			return false;
		return in_range_list(rdata, pointer);
	}
	bool is_in_module(const uint8_t* pointer) const {
		return in_range(pointer, module_base, module_size);
	}

	struct Match
	{
		uint32_t offset;
		uint32_t length;
	};


	template <size_t pattern_size>
	std::optional<Match> find_pattern_in_code(const std::array<pattern_entry, pattern_size> &pattern) const {
		std::vector<Match> matches = find_pattern_in_code_multiple<pattern_size>(pattern, 1);
		if (matches.empty())
			return std::optional<Match>{};
		return matches[0];
	}

	template <size_t pattern_size>
	std::optional<Match> find_pattern_in_rdata(const std::array<pattern_entry, pattern_size>& pattern) const {

		std::vector<Match> instances;
		instances.reserve(1);

		for (const auto& range : rdata) {
			auto range_end = range.first + range.second;
			if (find_pattern_in_range_internal(instances, range.first, range_end, pattern, 1))
			{
				return instances[0];
			}
		}

		return std::optional<Match>{};
	}

	template <size_t pattern_size>
	std::vector<Match> find_pattern_in_code_multiple(const std::array<pattern_entry, pattern_size>& pattern, size_t max_count = 0) const {
		std::vector<Match> instances;
		for (const auto& range : code) {
			auto range_end = range.first + range.second;
			bool exit_early = find_pattern_in_range_internal<pattern_size>(instances, range.first, range_end, pattern, max_count);
			if (exit_early)
				return instances;
		}
		return instances;
	}

private:

	template <size_t pattern_size>
	bool find_pattern_in_range_internal(std::vector<Match>& instances, uint32_t range_start, uint32_t range_end, const std::array<pattern_entry, pattern_size>& pattern, size_t max_count = 0) const
	{
		for (auto address = range_start; address < range_end; address++) {
			uint32_t offset = 0;
			for (auto pat_idx = 0u; pat_idx < pattern.size(); pat_idx++) {
				auto element_end = address + offset + pattern[pat_idx]->size_required();
				if (element_end > range_end)
					break;
				if (!pattern[pat_idx]->matches(*this, reinterpret_cast<uint8_t*>(offset + address)))
					break;
				offset += pattern[pat_idx]->entry_size();
				if (pat_idx == pattern.size() - 1)
				{
					instances.push_back({ address, offset });

					if (max_count != 0 && instances.size() >= max_count)
						return true;
				}
			}
		}

		return false;
	}

	static bool in_range_list(const range_list& list, const uint8_t* pointer) {
		for (auto range : list) {
			if (in_range(pointer, range.first, range.second))
				return true;
		}
		return false;
	}

	static bool in_range(const uint8_t* pointer, const size_t base, const size_t size) {
		size_t offset = reinterpret_cast<size_t>(pointer);
		return offset >= base && offset < (base + size);
	}

	range_list code;
	range_list data;
	range_list rdata;
	size_t module_base;
	size_t module_size;
};

class PatternEntryByte : public PatternEntryBase
{
public:
	PatternEntryByte(uint8_t value):
		char_value(value)
	{}

	size_t entry_size() const {
		return 1;
	}

	bool matches(const PatternScanner& scanner, const uint8_t* data) const {
		return *data == char_value;
	}

private:
	uint8_t char_value;
};

class PatternEntryCall : public PatternEntryBase
{
public:
	PatternEntryCall(uint32_t value) :
		call_target(value)
	{}

	size_t entry_size() const {
		return 5;
	}

	bool matches(const PatternScanner& scanner, const uint8_t* data) const {
		if (*data != 0xE8)
			return false;
		return *reinterpret_cast<const uint32_t*>(&data[1]) == call_target - (reinterpret_cast<const uint32_t>(data) + 5);
	}

private:
	uint32_t call_target;
};

template <size_t size>
class PatternEntryBytes : public PatternEntryBase
{
public:
	PatternEntryBytes(const std::array<uint8_t, size> &_values) :
		values(_values)
	{}

	size_t entry_size() const {
		return size;
	}

	bool matches(const PatternScanner& scanner, const uint8_t* data) const {
		return memcmp(data, values.data(), values.size()) == 0;
	}

private:
	std::array<uint8_t, size> values;
};

template<typename T>
inline static std::unique_ptr<PatternEntryBytes<sizeof(T)>> pattern_entry_bytes_from_pod(const T value)
{
	static_assert(std::is_pod_v<T>);

	std::array<uint8_t, sizeof(T)> pattern_as_array;
	std::memcpy(pattern_as_array.data(), &value, sizeof(T));

	return std::make_unique<PatternEntryBytes<sizeof(T)>>(pattern_as_array);
}

class PatternEntryAny : public PatternEntryBase
{
public:
	PatternEntryAny(size_t _size = 1):
		size(_size)
	{}

	bool matches(const PatternScanner& scanner, const uint8_t* data) const {
		return true;
	}

	size_t entry_size() const {
		return size;
	}

private:
	size_t size;
};

class PatternEntryStringXREF : public PatternEntryBase
{
public:
	PatternEntryStringXREF(const char* _string) :
		string(_string)
	{}

	bool matches(const PatternScanner& scanner, const uint8_t* data) const {
		const uint8_t* pointer = *reinterpret_cast<const uint8_t* const*>(data);

		if (scanner.is_in_rdata_segment(pointer))
		{
			return strcmp(reinterpret_cast<const char*>(pointer), string) == 0;
		}
		return false;
	}

	size_t entry_size() const {
		return sizeof(string);
	}
private:
	const char* string;
};

template<typename T = int>
class PatternEntryIntegerRange : public PatternEntryBase
{
public:
	PatternEntryIntegerRange(T lower, T upper) :
		lower_bound(lower),
		upper_bound(upper)
	{
	}

	bool matches(const PatternScanner& scanner, const uint8_t* data) const {
		const T value = *reinterpret_cast<const T*>(data);
		return lower_bound <= value && value <= upper_bound;
	}

	size_t entry_size() const {
		return sizeof(T);
	}
private:
	const T lower_bound;
	const T upper_bound;
};

#define PAT_BYTES(size, ...) \
	PAT_UNI(PatternEntryBytes<size>, ##__VA_ARGS__)
#define PAT_ANY(size) \
	PAT_UNI(PatternEntryAny, size)
#define PAT_BYTE(byte) \
	PAT_UNI(PatternEntryByte, byte)
#define PAT_CALL(call_target) \
	PAT_UNI(PatternEntryCall, call_target)
#define PAT_STRING_XREF(string) \
	PAT_UNI(PatternEntryStringXREF, string)
#define PAT_POD_TYPE(pod) \
	pattern_entry_bytes_from_pod(pod)
#define PAT_INTEGER_RANGE(type, lower, upper) \
	PAT_UNI(PatternEntryIntegerRange<type>, lower, upper)

#define PAT_PUSH_STRING_XREF(string) \
	PAT_BYTE(0x68), \
	PAT_STRING_XREF(string)

