﻿using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Reflection;
using Palit.TLSHSharp;
using System;
using System.Linq;

namespace ToolkitLauncher
{
    public static class BuiltinProfiles
    {
        private class TLSHJsonConverter : JsonConverter<TlshHash>
        {
            public override TlshHash Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                string value = reader.GetString();
                if (value is null)
                    return null;
                return TlshHash.FromTlshStr(value);
            }

            public override void Write(Utf8JsonWriter writer, TlshHash value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        private class UpperCaseStringConverter : JsonConverter<string>
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(string);
            }

            public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                string value = reader.GetString();
                if (value is null)
                    return null;
                return value.ToLowerInvariant();
            }

            public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToUpperInvariant());
            }
        }

        public class Profile
        {
            public class Executable
            {
                [JsonConverter(typeof(TLSHJsonConverter))]
                public TlshHash TLSH { get; set; }
                public string[] MD5 { get; set; }
            }

            public string ToolkitName { get; set; }
            public string Description { get; set; }

            public int GameGeneration { get; set; }
            public bool IsMCCBuild { get; set; }

            public bool Community { get; set; }

            public Executable Tool { get; set; }
            public Executable ToolFast { get; set; }
            public Executable Guerilla { get; set; }
            public Executable Sapien { get; set; }
            public Executable Standalone { get; set; }

            private void fixHashCase(Executable executable)
            {
                if (executable is null || executable.MD5 is null)
                    return;
                executable.MD5 = executable.MD5.Select(hash => hash.ToUpperInvariant()).ToArray();
            }
            internal void fixHashCase()
            {
                fixHashCase(Tool);
                fixHashCase(ToolFast);
                fixHashCase(Guerilla);
                fixHashCase(Sapien);
                fixHashCase(Standalone);
            }
        }

        static BuiltinProfiles()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream stream = assembly.GetManifestResourceStream("ToolkitLauncher.BultinProfiles.json");
            using StreamReader reader = new(stream);
            List<Profile> profiles = JsonSerializer.Deserialize<List<Profile>>(reader.ReadToEnd());

            foreach (Profile profile in profiles)
                profile.fixHashCase();

            _profiles = profiles;
        }

        public static IReadOnlyList<Profile> Profiles
        {
            get
            {
                return _profiles;
            }
        }

        private static readonly List<Profile> _profiles;
    }
}
