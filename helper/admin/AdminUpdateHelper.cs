using System;
using System.IO;
using System.IO.Compression;
using System.Formats.Tar;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Octokit;
using Spectre.Console;
using Semver;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Helper.Admin;

public class AdminUpdateHelper(IDebugOutputHelper _debugOutputHelper) : IAdminUpdateHelper
{
    private const string WebdevAdminRepoOwner = "Derroylo";
    private const string WebdevAdminRepoName = "webdev-admin";
    private static readonly HttpClient HttpClient = new();

    public async Task<bool> InstallOrUpdateIfNeededAsync()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var adminDir = Path.Combine(baseDir, "admin");
        var composerPath = Path.Combine(adminDir, "composer.json");

        var hasExistingBackend = Directory.Exists(adminDir) && File.Exists(composerPath);

        var doDownload = false;
        if (hasExistingBackend)
        {
            var localVersion = GetLocalVersion(composerPath);
            var (latestRelease, archiveAsset) = await GetLatestReleaseAndArchiveAssetAsync();
            if (latestRelease == null || archiveAsset == null)
            {
                AnsiConsole.MarkupLine("[yellow]Could not check for admin backend updates. Using existing installation.[/]");
                return true;
            }

            var latestVersion = ParseTagToSemVer(latestRelease.TagName);
            if (latestVersion != null && localVersion != null)
            {
                if (latestVersion.CompareSortOrderTo(localVersion) > 0)
                {
                    var update = AnsiConsole.Confirm(
                        $"A new version of the admin backend is available ({latestRelease.TagName}). Update now?",
                        defaultValue: true);
                    if (update)
                        doDownload = true;
                    else
                        return true;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
        else if (Directory.Exists(adminDir) && !File.Exists(composerPath))
        {
            AnsiConsole.MarkupLine("[red]Admin directory exists but composer.json is missing. Downloading fresh copy.[/]");
            try
            {
                Directory.Delete(adminDir, recursive: true);
            }
            catch (Exception ex)
            {
                _debugOutputHelper.WriteErrorOutput("Failed to remove incomplete admin dir", ex);
                AnsiConsole.MarkupLine("[red]Failed to remove incomplete admin directory.[/]");
                return false;
            }
            doDownload = true;
        }
        else if (!hasExistingBackend)
        {
            doDownload = true;
        }

        if (!doDownload)
            return true;

        if (doDownload && Directory.Exists(adminDir))
        {
            try
            {
                Directory.Delete(adminDir, recursive: true);
            }
            catch (Exception ex)
            {
                _debugOutputHelper.WriteErrorOutput("Failed to remove existing admin dir for update", ex);
                AnsiConsole.MarkupLine("[red]Failed to remove existing admin directory for update.[/]");
                return false;
            }
        }

        var (release, asset) = await GetLatestReleaseAndArchiveAssetAsync();
        if (release == null || asset == null)
        {
            AnsiConsole.MarkupLine("[red]No webdev-admin release or tar.gz asset found on GitHub.[/]");
            return false;
        }

        AnsiConsole.MarkupLine("[grey]Downloading webdev-admin {0}...[/]", release.TagName);

        string archivePath = null;
        string tempExtractPath = null;
        try
        {
            archivePath = Path.Combine(Path.GetTempPath(), $"webdev-admin-{release.Id}.tar.gz");
            var response = await HttpClient.GetAsync(asset.BrowserDownloadUrl);
            response.EnsureSuccessStatusCode();
            await using (var stream = await response.Content.ReadAsStreamAsync())
            await using (var fileStream = File.Create(archivePath))
                await stream.CopyToAsync(fileStream);

            tempExtractPath = Path.Combine(Path.GetTempPath(), $"webdev-admin-extract-{release.Id}");
            Directory.CreateDirectory(tempExtractPath);
            ExtractTarGz(archivePath, tempExtractPath);

            Directory.CreateDirectory(adminDir);

            var topLevel = Directory.GetFileSystemEntries(tempExtractPath);
            if (topLevel.Length == 1)
            {
                var single = topLevel[0];
                if (Directory.Exists(single))
                {
                    CopyDirectory(single, adminDir);
                }
                else
                {
                    File.Copy(single, Path.Combine(adminDir, Path.GetFileName(single)), overwrite: true);
                }
            }
            else
            {
                foreach (var entry in topLevel)
                {
                    var name = Path.GetFileName(entry);
                    var dest = Path.Combine(adminDir, name);
                    if (Directory.Exists(entry))
                        CopyDirectory(entry, dest);
                    else
                        File.Copy(entry, dest, overwrite: true);
                }
            }

            // Create a .env.local file with the values for:
            // PROJECTS_BASE_PATH="/home/user/projects"
            // Ask the user for the projects base path
            var projectsBasePath = AnsiConsole.Ask<string>("Enter the projects base path (for example: /home/user/projects):");

            File.WriteAllText(Path.Combine(adminDir, ".env.local"), $"PROJECTS_BASE_PATH={projectsBasePath}");

            AnsiConsole.MarkupLine("[green]Admin backend ready.[/]");
            return true;
        }
        catch (Exception ex)
        {
            _debugOutputHelper.WriteErrorOutput("Admin backend download or extract failed", ex);
            AnsiConsole.MarkupLine("[red]Failed to download or extract admin backend: {0}[/]", ex.Message);
            return false;
        }
        finally
        {
            if (archivePath != null && File.Exists(archivePath))
                try { File.Delete(archivePath); } catch { /* ignore */ }
            if (tempExtractPath != null && Directory.Exists(tempExtractPath))
                try { Directory.Delete(tempExtractPath, recursive: true); } catch { /* ignore */ }
        }
    }

    private static void ExtractTarGz(string archivePath, string destDir)
    {
        using var gzStream = new GZipStream(File.OpenRead(archivePath), CompressionMode.Decompress);
        using var tarReader = new TarReader(gzStream);
        TarEntry entry;
        while ((entry = tarReader.GetNextEntry()) != null)
        {
            var destPath = Path.Combine(destDir, entry.Name);
            if (entry.EntryType == TarEntryType.Directory)
            {
                Directory.CreateDirectory(destPath);
            }
            else
            {
                var parentDir = Path.GetDirectoryName(destPath);
                if (!string.IsNullOrEmpty(parentDir))
                    Directory.CreateDirectory(parentDir);
                entry.ExtractToFile(destPath, overwrite: true);
            }
        }
    }

    private static async Task<(Release Release, ReleaseAsset ArchiveAsset)> GetLatestReleaseAndArchiveAssetAsync()
    {
        try
        {
            var client = new GitHubClient(new ProductHeaderValue("webdev-tool"));
            var releases = await client.Repository.Release.GetAll(WebdevAdminRepoOwner, WebdevAdminRepoName);
            var latest = releases.FirstOrDefault(r => !r.Draft);
            if (latest == null) return (null, null);

            var archiveAsset = latest.Assets.FirstOrDefault(a =>
                a.Name.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase));
            return (latest, archiveAsset);
        }
        catch (Exception)
        {
            return (null, null);
        }
    }

    private static SemVersion GetLocalVersion(string composerPath)
    {
        try
        {
            var json = JObject.Parse(File.ReadAllText(composerPath));
            var versionToken = json["version"];
            if (versionToken == null) return null;
            var v = versionToken.Value<string>();
            return string.IsNullOrEmpty(v) ? null : SemVersion.Parse(v, SemVersionStyles.Strict);
        }
        catch
        {
            return null;
        }
    }

    private static SemVersion ParseTagToSemVer(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return null;
        var v = tag.TrimStart('v');
        return SemVersion.TryParse(v, SemVersionStyles.Strict, out var sem) ? sem : null;
    }

    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);
        foreach (var entry in Directory.GetFileSystemEntries(sourceDir))
        {
            var name = Path.GetFileName(entry);
            var dest = Path.Combine(targetDir, name);
            if (Directory.Exists(entry))
                CopyDirectory(entry, dest);
            else
                File.Copy(entry, dest, overwrite: true);
        }
    }
}
