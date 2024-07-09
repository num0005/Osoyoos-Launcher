/*
 Copyright (c) num0005. Some rights reserved
 This software is part of the Osoyoos Launcher.
 Released under the MIT License, see LICENSE.md for more information.
*/

#pragma once
#include "Debug.h"
#include <string>
#include <fstream>
#include <iostream>
#include <filesystem>
#include <map>
#include <optional>


class KeyValueError : public std::runtime_error
{
private:
	const std::string error_msg;
public:
	KeyValueError(const std::string& error) : std::runtime_error(error),
		error_msg(error)
	{
	};
	const char* what()
	{
		return error_msg.c_str();
	}
};

class KeyValueFile
{
public:
	KeyValueFile(std::filesystem::path path, bool autosave = false):
		autosave(autosave),
		settings_filename(path)
	{
		std::ifstream file(path);

		while (file.good())
		{
			std::string line;
			std::getline(file, line);

			size_t cut_point = line.find_first_of('=');
			if (cut_point == std::string::npos)
				continue;

			// remove trailing and leading spaces.
			std::string setting_name = line.substr(line.find_first_not_of(' '), line.find_last_not_of(' ', cut_point)); 
			std::string setting_value = line.substr(line.find_first_of(' ', cut_point + 1), line.find_last_not_of(' '));
			if (validate_setting_name(setting_name))
				key_value_pairs[setting_name] = setting_value;
			else
				DebugPrintf("Skipping invalid setting name \"%s\" in configuration file \"%s\"", setting_name.c_str(), path.generic_u8string().c_str());
		}
	}

	~KeyValueFile()
	{
		if (autosave)
			Save();
	}

	void Save(bool force = false)
	{
		if (!settings_edited || force)
			return;
		std::ofstream settings_file(settings_filename);
		for (auto i : key_value_pairs)
			settings_file << i.first << " = " << i.second << std::endl;
		settings_file.close();
	}

	const std::optional<std::string&> getStringOptional(const std::string& setting)
	{
		if (validate_setting_name(setting)) {
			auto ilter = key_value_pairs.find(setting);
			if (ilter != key_value_pairs.end())
				return ilter->second;
			else
				return std::optional<std::string&>(); // empty
		}
		else {
			throw KeyValueError("Invalid setting name");
		}
	}

	/* Throws an expection in case of an error */
	const std::string& getString(const std::string& setting)
	{
		auto string = getStringOptional(setting);
		if (!string.has_value())
		{
			throw KeyValueError("No such string");
		}

		return string.value();
	}

	/* Returns success and throws and expection if the name is invalid */
	bool getString(const std::string& setting, std::string& value)
	{
		if (validate_setting_name(setting)) {
			auto ilter = key_value_pairs.find(setting);
			if (ilter != key_value_pairs.end()) {
				value = ilter->second;
				return true;
			}
			else {
				return false;
			}
		}
		throw KeyValueError("Invalid setting name");
	}


	/// string setters

	/* Throws an error if the setting name is invalid */
	void setString(const std::string& setting, const std::string& value)
	{
		if (validate_setting_name(setting)) {
			if (key_value_pairs[setting] != value) {
				key_value_pairs[setting] = value;
				settings_edited = true;
			}
		}
		else {
			throw KeyValueError("Invalid setting name");
		}
	}

	/// Util function

	/* Returns if a setting exists */
	inline bool exists(const std::string& setting)
	{
		if (!validate_setting_name(setting))
			throw std::invalid_argument("Invalid setting name!");

		return key_value_pairs.find(setting) != key_value_pairs.end();
	}

	/* Checks if the string is a valid setting name */
	inline bool is_setting_name_valid(const std::string_view& name)
	{
		return validate_setting_name(name);
	}

	/* Throws an error if the setting name is invalid */
	template <typename NumericType>
	NumericType getNumber(const std::string& setting, NumericType default_value)
	{
		static_assert(std::is_arithmetic<NumericType>::value, "NumericType must be numeric");

		std::string value;
		if (!getString(setting, value)) {
			setNumber(setting, default_value);
			return default_value;
		}
		try {
			if (std::is_integral<NumericType>::value) {
				if (std::is_signed<NumericType>::value)
					return static_cast<NumericType>(std::stoll(value, 0, get_string_base(value)));
				else
					return static_cast<NumericType>(std::stoull(value, 0, get_string_base(value)));
			}
			else if (std::is_floating_point<NumericType>::value) {
				return static_cast<NumericType>(std::stold(value));
			}
			else {
				throw std::runtime_error("Settings::getNumber: unknown NumericType");
			}
		}
		catch (std::invalid_argument) {
			setNumber(setting, default_value);
			return default_value;
		}
		catch (std::out_of_range) {
			setNumber(setting, default_value);
			return default_value;
		}
	}

	/* Throws an error if the setting name is invalid */
	template <typename NumericType>
	inline void setNumber(const std::string& setting, NumericType value)
	{
		static_assert(std::is_arithmetic<NumericType>::value, "NumericType must be numeric");
		setString(setting, std::to_string(value));
	}

	/* Throws an error if the setting name is invalid */
	bool getBoolean(const std::string& setting, bool default_value = false)
	{
		std::string value;
		if (!getString(setting, value) || value.empty()) {
			setBoolean(setting, default_value);
			return default_value;
		}
		try {
			return stol(value);
		}
		catch (std::invalid_argument) {
		}
		catch (std::out_of_range) {
		}
		if (case_insensitive_equal(value, "true") || case_insensitive_equal(value, "on")) {
			return true;
		}
		else if (case_insensitive_equal(value, "false") || case_insensitive_equal(value, "off")) {
			return false;
		}
		else {
			setBoolean(setting, default_value);
			return default_value;
		}
	}
	/* Throws an error if the setting name is invalid */
	void setBoolean(const std::string& setting, bool value)
	{
		setString(setting, value ? "true" : "false");
	}

private:

	bool case_insensitive_equal(std::string_view a, std::string_view b)
	{
		return _strcmpi(a.data(), b.data()) == 0;
	}

	enum radix
	{
		octal = 8,
		decimal = 10,
		hexadecimal = 16
	};


	int get_string_base(std::string_view value)
	{
		auto first_not_whitespace = value.find_first_not_of(" ");

		// default to decimal if we cannot parse the value
		if (first_not_whitespace == std::string_view::npos)
			return decimal;

		auto number_str = value.substr(first_not_whitespace);

		if (!number_str.empty() && number_str[0] == '0')
		{
			if (value.size() >= 2 && (number_str[1] == 'x' || number_str[1] == 'X'))
				return hexadecimal;
			return octal;
		}
		return decimal;
	}

	/* check if the setting name is valid */
	inline bool validate_setting_name(const std::string_view& setting)
	{
		return !setting.empty() && isalpha(setting[0])
			&& std::find_if(setting.begin(), setting.end(), [](char c) {return !isalnum(c) && c != '_' && c != '-';  }) == setting.end();
	}

	std::filesystem::path settings_filename;
	std::map<std::string, std::string> key_value_pairs;
	bool settings_edited = false;
	bool autosave = false;
};