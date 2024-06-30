using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ToolkitLauncher.Utility
{
    public class KeyValueConfig
    {
        public KeyValueConfig(string filePath, bool readExisting = true)
        {
            this.filePath = filePath;

            if (readExisting)
                ReadFromFile();
        }

        /// <summary>
        /// Read the configuration from <c>filePath</c>
        /// </summary>
        /// <returns>Whatever the file could successfully be read</returns>
        public bool ReadFromFile()
        {
            string[] lines;
            try
            {
                lines = File.ReadAllLines(filePath);
            } catch
            {
                return false;
            }
            foreach (string line in lines)
            {
                string[] pair =  line.Split("=", 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (pair.Length < 1)
                {
                    Trace.WriteLine($"Skipping invalid config line '{line}'");
                    continue;
                }
                if (pair.Length == 2)
                    keyValues[pair[0]] = pair[1];
                else
                    keyValues[pair[0]] = "";
            }
            return true;
        }

        /// <summary>
        /// Save the config to <c>filePath</c> 
        /// </summary>
        /// <returns>Whatever the file was successfully saved</returns>
        public bool WriteToFile()
        {
            try
            {
                using (FileStream file = File.Open(filePath, FileMode.Create))
                    using (var writer = new StreamWriter(file))
                        foreach (KeyValuePair<string, string> keyValuePair in keyValues)
                            writer.WriteLine($"{keyValuePair.Key} = {keyValuePair.Value}");
                return true;
            } catch (IOException ex)
            {
                Trace.WriteLine(ex.ToString()); // log error
            }
            return false;
        }

        /// <summary>
        /// Get a setting
        /// </summary>
        /// <param name="key">Setting name</param>
        /// <returns>setting value</returns>
        public string Get(string key)
        {
            return keyValues[key];
        }

        /// <summary>
        /// Gets the setting value or a default value
        /// </summary>
        /// <param name="key">Setting</param>
        /// <param name="defaultValue">default value</param>
        /// <returns>Setting value or default value</returns>
        public string Get(string key, string defaultValue)
        {
            try
            {
                return Get(key);
            } catch (KeyNotFoundException)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets the setting value or a default value
        /// </summary>
        /// <param name="key">Setting</param>
        /// <param name="defaultValue">default value</param>
        /// <returns>Setting value or default value</returns>
        public T Get<T>(string key, T defaultValue)
        {
            return (T)Convert.ChangeType(Get(key, defaultValue.ToString()), typeof(T));
        }

        /// <summary>
        /// Changes the value of a setting
        /// </summary>
        /// <param name="key">Setting</param>
        /// <param name="value">New value</param>
        public void Set(string key, string value)
        {
            keyValues[key] = value;
        }

        /// <summary>
        /// Changes the value of a setting
        /// </summary>
        /// <param name="key">Setting</param>
        /// <param name="value">New value</param>
        public void Set<T>(string key, T value)
        {
            Set(key, value.ToString());
        }

        /// <summary>
        /// Remove a setting
        /// </summary>
        /// <param name="key">Setting to remove</param>
        public void Remove(string key)
        {
            keyValues.Remove(key);
        }

        private Dictionary<string, string> keyValues = new();

        public readonly string filePath;
    }
}
