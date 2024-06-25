using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ToolkitLauncher.Utility;

namespace ToolkitLauncher
{
    public static class Documentation
    {
        public class Data
        {
            public Uri UrlBase { get; set; }
            
            public class Toolkit
            {
                [JsonPropertyName("Base")]
                public string BaseName { get; set; } = "_base";
                [JsonIgnore]
                public Toolkit Base { get; internal set; }

                public string RunPrograms { get; set; }
                public string Tasks { get; set; }
                public string Settings {get; set; }
                public string General { get; set; }
                public string Packaging { get; set; }
                public string Lighting { get; set; }
                public string Phantom { get; set; }
                public string MultiInstance { get; set; }
                public string H2LOD { get; set; }

                public string _getURL(HelpURL url)
                {
                    switch (url)
                    {
                        case HelpURL.programs:
                            return RunPrograms;
                        case HelpURL.tasks:
                            return Tasks;
                        case HelpURL.settings:
                            return Settings;
                        case HelpURL.lighting:
                            return Lighting;
                        case HelpURL.cache:
                            return Packaging;
                        case HelpURL.phantom:
                            return Phantom;
                        case HelpURL.instances:
                            return MultiInstance;
                        case HelpURL.h2lod:
                            return H2LOD;
                        default:
                            return General;
                    }
                }

                public string GetURL(HelpURL requestedURI)
                {
                    string url = _getURL(requestedURI);
                    if (String.IsNullOrWhiteSpace(url))
                        return _getURL(HelpURL.main);
                    else
                        return url;
                }

                internal void UpdateBaseReference(Data parent)
                {
                    Base = parent.Tookits[BaseName];
                    if (Base == this) // fix circular references 
                        Base = null;
                }

                /// <summary>
                /// Use reflection to fill in any null values using the base toolkit
                /// </summary>
                internal void CopyBaseValues()
                {
                    if (Base is null)
                        return;

                    Base.CopyBaseValues();

                    PropertyInfo[] properties = typeof(Toolkit).GetProperties();
                    foreach (PropertyInfo property in properties)
                    {
                        if (property.GetValue(this) is not null)
                            continue;
                        property.SetValue(this, property.GetValue(Base));
                    }
                }
            }
            public Dictionary<string, Toolkit> Tookits { get; init; }

            internal void UpdateBaseReference()
            {
                foreach (Toolkit toolkit in Tookits.Values)
                {
                    toolkit.UpdateBaseReference(this);
                }
            }

            internal void CopyBaseValues()
            {
                foreach (Toolkit toolkit in Tookits.Values)
                {
                    toolkit.CopyBaseValues();
                }
            }

            public enum HelpURL
            {
                main,
                tasks,
                programs,
                settings,
                lighting,
                cache,
                phantom,
                instances,
                h2lod
            }

            public void OpenURL(string toolkitName, HelpURL url)
            {
                Process.OpenURL(UrlBase + Tookits[toolkitName].GetURL(url));
            }
        }

        private static Data _data;
        public static Data Contents { get => _data; }
        static Documentation()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream stream = assembly.GetManifestResourceStream("ToolkitLauncher.Documentation.json");
            using StreamReader reader = new(stream);
            _data = JsonSerializer.Deserialize<Data>(reader.ReadToEnd());
            _data.UpdateBaseReference();
            _data.CopyBaseValues();
        }
    }
}
