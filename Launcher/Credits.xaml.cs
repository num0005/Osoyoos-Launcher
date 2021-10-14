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
            GitHubReleases gitHubReleases = new();
            IReadOnlyList<GitHubReleases.Release> list = await gitHubReleases.GetReleasesForRepo("num0005", "Osoyoos-Launcher");
            Debug.Print(list.ToString());
            MessageBoxResult result = MessageBox.Show(
                "Do you want to use prerelease builds?\nPrerelease buidlds can be less stable than final builds", 
                "Use prerelease?", 
                MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Cancel)
                return;
            bool usePrerelease = result == MessageBoxResult.Yes;
            GitHubReleases.Release latestRelease = list.FirstOrDefault(r => !r.IsPreRelease || usePrerelease);
            if (latestRelease is null)
            {
                _ = MessageBox.Show("No matching release found, can't update", "No matching release", MessageBoxButton.OK);
            }

            Debug.Print(latestRelease.ToString());
            if (MessageBox.Show($"Do you want to update to {latestRelease.Name} created at {latestRelease.CreationTime}?", "Update?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                CancelableProgressBarWindow<long> progress = new();
                progress.Status = "Downloading update";
                progress.Title = progress.Status;

                try {
                    await DoUpdate(gitHubReleases, latestRelease, progress);
                } catch (OperationCanceledException) {}
                finally
                {
                    progress.Complete = true;
                }
            }
        }
    }
}
