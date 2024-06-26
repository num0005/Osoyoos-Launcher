using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace ToolkitLauncher.Utility
{
    internal class UpdateUIHelper
    {
        public static GitHubReleases.Release? AskUserToSelectUpdate(IReadOnlyList<GitHubReleases.Release> releases)
        {
            Debug.Print($"releases: {releases}");
            bool has_any_prerelease = releases.Any(r => r.IsPreRelease);

            bool usePrerelease = false;
            if (has_any_prerelease)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Do you want to use prerelease builds?\nPrerelease builds can be less stable than final builds",
                    "Use prerelease?",
                    MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel)
                    return null;
                usePrerelease = result == MessageBoxResult.Yes;
            }
            
            GitHubReleases.Release latestRelease = releases.FirstOrDefault(r => !r.IsPreRelease || usePrerelease);
            if (latestRelease is null)
            {
                _ = MessageBox.Show("No matching release found, can't update", "No matching release", MessageBoxButton.OK);
                return null;
            }

            Debug.Print($"selected release: {latestRelease}");

            if (MessageBox.Show($"Do you want to update to {latestRelease.Name} created at {latestRelease.CreationTime}?", "Update?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                return latestRelease;
            }
            else
            {
                return null;
            }
        }
    }
}
