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
                .GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
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
                    Debug.WriteLine(ex.ToString());
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

        private async void update_button_Click(object sender, RoutedEventArgs e)
        {
            update_button.IsEnabled = false; // Make sure we can't click on this multiple times
            try 
            {
                GitHubReleases gitHubReleases = new();
                IReadOnlyList<GitHubReleases.Release> list = await gitHubReleases.GetReleasesForRepo("num0005", "Osoyoos-Launcher");
                Debug.Print(list.ToString());

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
            string license_file_name = Path.Combine(Path.GetTempPath(), $"launcher_copyright_{Guid.NewGuid()}.txt");
            Assembly assembly = Assembly.GetExecutingAssembly();
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
                Debug.Print($"Failed to delete temporary file {license_file_name}");
            }
        }
    }
}
