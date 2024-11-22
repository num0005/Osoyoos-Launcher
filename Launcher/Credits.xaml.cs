using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using ToolkitLauncher.Utility;
using System.Linq;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Win32;
using System.Runtime.Versioning;

namespace ToolkitLauncher
{
    /// <summary>
    /// Interaction logic for PathSettings.xaml
    /// </summary>
    public partial class Credits : Window
    {
        public Credits()
        {
            InitializeComponent();
            version.Text = Assembly.GetEntryAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        }


        private static async Task DoUpdate(GitHubReleases gitHubReleases, GitHubReleases.Release release, CancelableProgressBarWindow<long> progress)
        {
            CancellationToken token = progress.GetCancellationToken();
            byte[]? newExe = await Task.Run(() => {
                return gitHubReleases.DownloadReleaseAsset(
release.Assets.First(assert => assert.Name == "Osoyoos.exe"),
progress, token);
            });
            if (newExe is null)
            {
                progress.Complete = true;
                _ = MessageBox.Show("Couldn't download release!", "Download Failed!", MessageBoxButton.OK);

            }
            else
            {
                progress.Status = "Applying update";
                // get the name of the host file
                string host = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                string oldHost = host + "_old.exe";

                // delete any old backup files
                try
                {
                    File.Delete(oldHost);
                }
                catch { }

                try
                {
                    // backup current/bypass lock
                    File.Move(host, oldHost);
                } catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                    _ = MessageBox.Show("Couldn't rename running instance!", "Update Failed!", MessageBoxButton.OK);
                    progress.Cancel("couldn't rename running instance");
                    return;
                }

                try
                {

                    // save new file
                    await File.WriteAllBytesAsync(host, newExe, progress.GetCancellationToken());
#if DEBUG
                    await Task.Delay(1000, token); // test
#endif
                    // setup new instance
                    ProcessStartInfo newProcess = new(host);
                    newProcess.ArgumentList.Add(App.DeleteOldCommand);
                    newProcess.ArgumentList.Add(oldHost);

                    // final chance
                    token.ThrowIfCancellationRequested();

                    if (System.Diagnostics.Process.Start(newProcess) is not null)
                    {
                        // startup of new instance worked, terminate ourselves without delay
                        Environment.Exit(0);
                    }
                    // something went wrong
                    throw new OperationCanceledException("launch failure!");
                } catch
                {
                    progress.Cancel("couldn't launch new instance");
                    // delete new copy/rollback
                    try
                    {
                        File.Delete(host);
                    }
                    catch { }
                    // rollback
                    File.Move(oldHost, host);
                    throw; // rethrow
                }
            }
        }

		[SupportedOSPlatform("windows")]
		private static bool CheckDotnet8Installed()
        {
            const string registry_path_string = "SOFTWARE\\dotnet\\Setup\\InstalledVersions\\x64\\sharedfx\\Microsoft.WindowsDesktop.App";

			RegistryKey? sub_key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(registry_path_string);

            if (sub_key is null)
            {
                Trace.TraceWarning("Unable to open subkey to check installed dotnet versions, this shouddn't happen!");
                return false;
            }

            string[] values = sub_key.GetValueNames();
            Trace.WriteLine("Installed dotnet version: " + String.Join(",", values));

			foreach (string value in values)
            {
                // todo parse the version string here one day
                if (value.StartsWith("8."))
                    return true;
            }

			return false;
		}

        private async void update_button_Click(object sender, RoutedEventArgs e)
        {
            update_button.IsEnabled = false; // Make sure we can't click on this multiple times
            try 
            {
                if (OperatingSystem.IsWindows() && !CheckDotnet8Installed())
                {
                    const string dotnet_8_download_url = "https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe";
					MessageBoxResult result = MessageBox.Show(
	                    "You do not have .NET 8 Desktop runtime installed yet, this will be required for future versions.\nPleases do so now, the download link will be opened in your browser.",
	                    "Required .NET version not installed!",
	                    MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.Cancel)
                        return;
                    Utility.Process.OpenURL(dotnet_8_download_url);
				}

                GitHubReleases gitHubReleases = new();
                IReadOnlyList<GitHubReleases.Release> list = await gitHubReleases.GetReleasesForRepo("num0005", "Osoyoos-Launcher");
                Debug.WriteLine(list.ToString());

                GitHubReleases.Release? selectedRelease = UpdateUIHelper.AskUserToSelectUpdate(list);

                if (selectedRelease is not null)
                {
                    CancelableProgressBarWindow<long> progress = new();
                    progress.Owner = this;
                    progress.Status = "Downloading update";
                    progress.Title = progress.Status;

                    try
                    {
                        await DoUpdate(gitHubReleases, selectedRelease, progress);
                    }
                    catch (OperationCanceledException) { }
                    finally
                    {
                        progress.Complete = true;
                    }
                }
            }
            finally
            {
                update_button.IsEnabled = true; // We're done
            }


        }

        private async void license_info_open_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory(App.TempFolder);
            string license_file_name = Path.Join(App.TempFolder, $"launcher_copyright_{Guid.NewGuid()}.txt");
            Assembly assembly = Assembly.GetExecutingAssembly();
            var test = assembly.GetManifestResourceNames();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("LicenseInfoFull.txt"));
            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            using (StreamWriter fileStream = File.CreateText(license_file_name))
            {
                stream.CopyTo(fileStream.BaseStream);
            }
            ProcessStartInfo startInfo = new(license_file_name)
            {
                UseShellExecute = true
            };
            var process = System.Diagnostics.Process.Start(startInfo);
            await process.WaitForExitAsync();

            try
            {
                File.Delete(license_file_name);
            }
            catch 
            {
                Trace.WriteLine($"Failed to delete temporary file {license_file_name}");
            }
        }
    }
}
