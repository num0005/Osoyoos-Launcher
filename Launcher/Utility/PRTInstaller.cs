using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ToolkitLauncher.Properties;
using Path = System.IO.Path;

namespace ToolkitLauncher.Utility
{
    internal class PRTSimInstaller
    {
        const string repoOwner = "digsite";
        const string repoName = "prt_sim";
        const string redist_dll_name = "d3dx9_43.dll";
        const string redist_package_name = "d3d9x_43_redist.exe";
        public const string prt_executable_file_path = "prt_sim.exe";

        /// <summary>
        /// Get the 32-bit windows directory
        /// </summary>
        /// <returns></returns>
        private static string _32bit_windows_folder()
        {
            // https://stackoverflow.com/a/28448869
            if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
            {
                return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SysWOW64");
            }
            else
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.System);
            }
        }

        /// <summary>
        /// Check if the d3d9 redist package is installed already
        /// </summary>
        /// <returns></returns>
        static public bool IsRedistInstalled()
        {
            string windows_folder = _32bit_windows_folder();
            string redist_dll = Path.Join(windows_folder, redist_dll_name);

            return File.Exists(redist_dll);
        }

        static public async Task<IReadOnlyList<GitHubReleases.Release>> GetReleases()
        {
            GitHubReleases gitHubReleases = new();
            return await gitHubReleases.GetReleasesForRepo(repoOwner, repoName);
        }

        static public async Task<GitHubReleases.Release?> GetLatestRelease()
        {
            IReadOnlyList<GitHubReleases.Release> list = await GetReleases();

            return list.FirstOrDefault(r => !r.IsPreRelease);
        }

        private static async Task<byte[]?> DownloadRedistRuntime(CancelableProgressBarWindow<long> progress)
        {
            GitHubReleases gitHubReleases = new();
            IReadOnlyList<GitHubReleases.Release> list = await gitHubReleases.GetReleasesForRepo(repoOwner, repoName);

            GitHubReleases.Release? redist_release = list.FirstOrDefault(r => r.Assets.Any(a => a.Name == redist_package_name));

            if (redist_release == null)
            {
                Debug.Print("Failed to find any release with redist package!");
                // no redist found
                return null;
            }

            GitHubReleases.Asset redist_asset = redist_release.Assets.First(a => a.Name == redist_package_name);
            return await gitHubReleases.DownloadReleaseAsset(redist_asset, progress);
        }

        private static async Task<bool> DoUpdate(string prt_install_path, GitHubReleases.Release release, CancelableProgressBarWindow<long> progress)
        {
            GitHubReleases gitHubReleases = new();
            CancellationToken token = progress.GetCancellationToken();
            progress.Status = "Downloading prt_sim";
            byte[]? newExe = await Task.Run(() => {
                return gitHubReleases.DownloadReleaseAsset(
release.Assets.First(assert => assert.Name == prt_executable_file_path),
progress, token);
            });
            if (newExe is null)
            {
                progress.Complete = true;
                _ = MessageBox.Show("Couldn't download release!", "Download Failed!", MessageBoxButton.OK);
                return false;

            }
            else
            {
                progress.Status = "Applying update";
                if (File.Exists(prt_install_path))
                {
                    File.Delete(prt_install_path);
                }

                await File.WriteAllBytesAsync(prt_install_path, newExe, progress.GetCancellationToken());

                if (!IsRedistInstalled())
                {
                    progress.Status = "Downloading D3DX (Direct3D 9) redistributable package";

                    byte[]? redist_package = await DownloadRedistRuntime(progress);

                    if (redist_package is null)
                    {
                        progress.Complete = true;
                        progress.Status = "Failed to download redist package!";
                    } else
                    {
                        string temp_folder = Path.Join(Path.GetTempPath(), "Osoyoos_" + Path.GetRandomFileName());
                        Directory.CreateDirectory(temp_folder);

                        try
                        {
                            string redist_executable_path = Path.Join(temp_folder, redist_package_name);
                            await File.WriteAllBytesAsync(redist_executable_path, redist_package, progress.GetCancellationToken());
                            progress.Status = "Installing redist package!";
                            
                            await Process.StartProcess(temp_folder, redist_executable_path, new(), progress.GetCancellationToken(), admin:true);

                            progress.Complete = true;
                            progress.Status = IsRedistInstalled() ? "Installed redist package!" : "Failed to install redist package!";
                        } finally
                        {
                            Directory.Delete(temp_folder, true);
                        }
                    }


                }

                progress.Complete = true;

                return true;
            }
        }

        static public async Task<int?> Install(string prt_tool_path, GitHubReleases.Release? targetRelease = null)
        {
            CancelableProgressBarWindow<long> progress = new();
            progress.Status = "Fetching prt_sim version information";
            progress.Title = progress.Status;

            // fetch latest release
            if (targetRelease is null)
            {
                GitHubReleases.Release latestRelease = await GetLatestRelease();
                if (latestRelease is null)
                {
                    MessageBox.Show("Unable to fetch PRT release list", "Install error!", MessageBoxButton.OK);
                    progress.Complete = true;
                    return null;
                }
                targetRelease = latestRelease;
            }

            Debug.Print(targetRelease.ToString());

            bool success = false;
            try
            {
                success = await DoUpdate(prt_tool_path, targetRelease, progress);
            }
            catch (OperationCanceledException) { }
            finally
            {
                progress.Complete = true;
            }

            if (success)
            {
                if (Settings.Default.newest_prt_sim_version is null || 
                    (targetRelease.ID > Settings.Default.newest_prt_sim_version && !targetRelease.IsPreRelease))
                {
                    Settings.Default.newest_prt_sim_version = targetRelease.ID;
                    Settings.Default.Save();
                }
                return targetRelease.ID;
            }
            else
            {
                return null;
            }
        }
    }
}
