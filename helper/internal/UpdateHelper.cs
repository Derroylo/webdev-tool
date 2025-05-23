using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Spectre.Console;
using Octokit;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.IO.Compression;
using Semver;
using WebDev.Tool.Helper.Internal.Config.Sections;

namespace WebDev.Tool.Helper.Internal
{
    internal class UpdateHelper
    {  
        public static string CurrentVersion {
            get {
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

                string preReleaseName = Assembly.GetExecutingAssembly()
                    ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
                
                // Include PreRelease Version info if it exists
                if (!String.IsNullOrEmpty(preReleaseName) && preReleaseName != currentVersion)
                {
                    currentVersion += preReleaseName;
                }

                return currentVersion;
            }
        }
        
        private static async Task UpdateCacheFile()
        {
            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;
            bool allowPreReleases = GeneralConfig.AllowPreReleases;

            GitHubClient client = new GitHubClient(new ProductHeaderValue("SomeName"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("Derroylo", "webdev-tool");
            
            Release lastRelease = null;

            foreach (Release release in releases) {
                if (release.Draft) {
                    continue;
                }

                if (!allowPreReleases && release.Prerelease) {
                    continue;
                }

                if (null == lastRelease) {
                    lastRelease = release;
                }
            }

            IReadOnlyList<ReleaseAsset> assets = lastRelease.Assets;

            JObject tmp = new JObject(
                new JProperty("last_check", DateTime.Now.ToString()),
                new JProperty("last_release", lastRelease.TagName.Replace("v", "")),
                new JProperty("download_url", assets[0].BrowserDownloadUrl)
            );

            File.WriteAllText(applicationDir + "releases.json", tmp.ToString());
        }

        public static async Task<string> GetLatestVersion(bool forceUpdate = false)
        {
            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;

            if (!File.Exists(applicationDir + "releases.json") || forceUpdate) {
                await UpdateCacheFile();
            }

            JObject cacheFile = JObject.Parse(File.ReadAllText(applicationDir + "releases.json"));

            // Check if we need to update the cache file
            var lastCheck = (DateTime) cacheFile["last_check"];
            var diffHours = (DateTime.Now - lastCheck).TotalHours;

            // If the last check is older then 4 hours, update the cache
            if (diffHours > 4) {
                await UpdateCacheFile();

                cacheFile = JObject.Parse(File.ReadAllText(applicationDir + "releases.json"));
            }
            
            return (string) cacheFile["last_release"];
        }

        public static bool IsUpdateAvailable()
        {
            var currentVersion = CurrentVersion;

            var latestVersion  = GetLatestVersion().Result;

            SemVersion localVersion = SemVersion.Parse(currentVersion, SemVersionStyles.Strict);
            SemVersion latestRelease = SemVersion.Parse(latestVersion, SemVersionStyles.Strict);

            int versionComparison = localVersion.CompareSortOrderTo(latestRelease);

            if (versionComparison < 0) {
                return true;
            }
            
            return false;
        }

        public static async Task<bool> UpdateToLatestRelease()
        {
            var applicationDir = AppDomain.CurrentDomain.BaseDirectory;

            JObject cacheFile = JObject.Parse(File.ReadAllText(applicationDir + "releases.json"));

            try {
                var httpClient = new HttpClient();
                var httpResult = await httpClient.GetAsync((string) cacheFile["download_url"]);
                using var resultStream = await httpResult.Content.ReadAsStreamAsync();
                using var fileStream = File.Create(applicationDir + "webdev-tool.zip");

                resultStream.CopyTo(fileStream);
            } catch (Exception e) {
                AnsiConsole.WriteException(e);

                return false;
            }
            
            if (!File.Exists(applicationDir + "webdev-tool.zip")) {
                AnsiConsole.WriteLine("Downloading the latest release failed");

                return false;
            }

            try {
                ZipFile.ExtractToDirectory(applicationDir + "webdev-tool.zip", applicationDir + "update", true);
            } catch (Exception e) {
                AnsiConsole.WriteException(e);

                return false;
            }

            return true;
        }
    }
}