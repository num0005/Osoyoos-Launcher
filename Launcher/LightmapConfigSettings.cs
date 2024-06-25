namespace ToolkitLauncher
{
    public class LightmapConfigSettings
    {
        public LightmapConfigSettings(string path)
        {
            this.config = new(path);
        }
        public bool IsCheckerboard
        {
            get { return config.Get("is_checkboard", false); }
            set { config.Set("is_checkboard", value); }
        }
        public bool IsDirectOnly
        {
            get { return config.Get("is_direct_only", false); }
            set { config.Set("is_direct_only", value); }
        }
        public bool IsDraft
        {
            get { return config.Get("is_draft", false); }
            set { config.Set("is_draft", value); }
        }
        public int SampleCount
        {
            get { return config.Get("main_monte_carlo_setting", 8); }
            set { config.Set("main_monte_carlo_setting", value); }
        }
        public int PhotonCount
        {
            get { return config.Get("proton_count", 20000000); }
            set { config.Set("proton_count", value); }
        }
        public int AASampleCount
        {
            get { return config.Get("secondary_monte_carlo_setting", 4); }
            set { config.Set("secondary_monte_carlo_setting", value); }
        }
        public float GatherDistance
        {
            get { return config.Get("unk7", 4.0f); }
            set { config.Set("unk7", value); }
        }

        /// <summary>
        /// Reset settings back to default
        /// </summary>
        public void Reset()
        {
            config.Remove("is_checkboard");
            config.Remove("is_direct_only");
            config.Remove("is_draft");
            config.Remove("main_monte_carlo_setting");
            config.Remove("proton_count");
            config.Remove("secondary_monte_carlo_setting");
            config.Remove("unk7");
        }

        /// <summary>
        /// Saves the config to <c>Path</c>
        /// </summary>
        /// <returns>Success</returns>
        public bool Save()
        {
            return config.WriteToFile();
        }

        public string Path { get => config.filePath; }

        readonly private Utility.KeyValueConfig config;
    }
}
