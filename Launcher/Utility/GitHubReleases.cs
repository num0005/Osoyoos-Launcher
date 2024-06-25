using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ToolkitLauncher.Utility
{
    class GitHubReleases
    {
        private static string GetExeVersion()
        {
            return Assembly.GetEntryAssembly()
                .GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
        }

        private readonly GitHubClient gitHubClient = new(new ProductHeaderValue("num0005-Osoyoos-Launcher", GetExeVersion()));

        private static readonly HttpClient client = new();
        /*public GitHubReleases()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github.v3+json");
            client.BaseAddress = new Uri(@"https://api.github.com");
        }
        */

        public async Task<IReadOnlyList<Release>> GetReleasesForRepo(string owner, string repo)
        {
            List<Release> releases = new();

            foreach (Octokit.Release release in await gitHubClient.Repository.Release.GetAll(owner, repo))
            {
                List<Asset> assets = new();
                foreach (ReleaseAsset releaseAsset in release.Assets)
                    assets.Add(new Asset(
                        Name: releaseAsset.Name,
                        ID: releaseAsset.Id,
                        DownloadURL: releaseAsset.BrowserDownloadUrl,
                        Size: releaseAsset.Size
                        ));
                releases.Add(new Release(
                    ID: release.Id,
                    NodeID: release.NodeId,
                    Name: release.Name,
                    Description: release.Body,
                    IsPreRelease: release.Draft || release.Prerelease,
                    CreationTime: release.CreatedAt,
                    PublishingTime: release.PublishedAt,
                    Assets: assets
                    ));
            }
            return releases;
        }

        public async Task<byte[]?> DownloadReleaseAsset(Asset asset,
            ICancellableProgress<long> progress,
            CancellationToken cancellationToken = default)
        {
            var response = await client.GetAsync(asset.DownloadURL, HttpCompletionOption.ResponseHeadersRead);
            if (!response.IsSuccessStatusCode)
            {
                Debug.Print($"Failed to download Github asset \"{asset.Name}\" from \"{asset.DownloadURL}\"");
                Debug.Print(response.ToString());
                return null;
            }
            if (cancellationToken.IsCancellationRequested)
                return null;

            long length = Math.Max(response.Content.Headers.ContentLength ?? 0, asset.Size);
            progress.MaxValue += length;

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            {
                if (cancellationToken.IsCancellationRequested)
                    return null;
                if (progress is null || length == 0)
                {
                    byte[] result = await stream.ReadBytesToEndAsync();
                    if (progress is not null)
                        progress.CurrentProgress += length;
                    return result;
                }

                using (MemoryStream ms = new())
                {

                    int totalRead = 0;
                    int count;
                    byte[] buffer = new byte[32768];
                    while ((count = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
                    {
#if DEBUG
                        await Task.Delay(10); // test
#endif
                        ms.WriteAsync(buffer.AsMemory(0, count), cancellationToken).AsTask().Wait(cancellationToken);
                        totalRead += count;
                        progress.Report(count);
                        if (cancellationToken.IsCancellationRequested)
                            return null;
                    }

                    return ms.ToArray();
                }
            }       
        }

        public record Asset(
            string Name,
            int ID,
            string DownloadURL,
            long Size
            );

        public record Release(
            int ID,
            string NodeID,
            string Name,
            string Description,
            bool IsPreRelease,
            DateTimeOffset CreationTime,
            DateTimeOffset? PublishingTime,
            IReadOnlyList<Asset> Assets
            );
    }
}
