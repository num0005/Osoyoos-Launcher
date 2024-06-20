using System.Collections.Generic;
using System;

namespace Bungie
{
    public static class ManagedBlamSystem
    {
        public static void InitializeProject(InitializationType initializationType, string ekPath) { }
    }

    public enum InitializationType
    {
        TagsOnly
    }

    namespace Tags
    {
        public class TagPath
        {
            public static TagPath FromPathAndType(string path, string type) { return null; }
        }

        public class TagFile : IDisposable
        {
            public TagFile(TagPath tagPath) { }
            public TagFile() { }
            public void New(TagPath tagPath) { }
            public void Save() { }
            public void Dispose() { }
            public TagField SelectField(string fieldName) { return null; }
        }

        public abstract class TagField
        {
        }

        public class TagFieldEnum : TagField
        {
            public int Value { get; set; }
        }

        public class TagFieldElementInteger : TagField
        {
            public int Data { get; set; }
        }

        public class TagFieldElementSingle : TagField
        {
            public float Data { get; set; }
        }

        public class TagFieldBlock : TagField
        {
            public IEnumerable<TagFieldElement> Elements { get; set; }
            public void RemoveAllElements() { }
            public void AddElement() { }
        }

        public class TagFieldFlags : TagField
        {
            public int RawValue { get; set; }
        }

        public class TagFieldElement
        {
        }
    }
}