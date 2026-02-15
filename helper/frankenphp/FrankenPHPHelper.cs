using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Octokit;
using Spectre.Console;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Helper.FrankenPHP;

public class FrankenPHPHelper(IDebugOutputHelper _debugOutputHelper) : IFrankenPHPHelper
{
    private const string FrankenPHPRepoOwner = "dunglas";
    private const string FrankenPHPRepoName = "frankenphp";
    private static readonly HttpClient HttpClient = new();

    public async Task<string> GetOrDownloadFrankenPHPAsync()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var binDir = Path.Combine(baseDir, "bin");
        var binaryFileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "frankenphp.exe" : "frankenphp";
        var binaryPath = Path.Combine(binDir, binaryFileName);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            AnsiConsole.MarkupLine("[red]FrankenPHP is not available for Windows. The admin interface is supported on Linux and macOS only.[/]");
            return null;
        }

        if (File.Exists(binaryPath))
        {
            return binaryPath;
        }

        var assetName = GetFrankenPHPAssetName();
        if (string.IsNullOrEmpty(assetName))
        {
            AnsiConsole.MarkupLine("[red]Your platform is not supported by FrankenPHP (OS: {0}, Architecture: {1}).[/]",
                RuntimeInformation.OSDescription, RuntimeInformation.ProcessArchitecture);
            return null;
        }

        try
        {
            Directory.CreateDirectory(binDir);

            var client = new GitHubClient(new ProductHeaderValue("webdev-tool"));
            // dunglas/frankenphp redirects to php/frankenphp; use owner "dunglas" as per original repo
            var releases = await client.Repository.Release.GetAll(FrankenPHPRepoOwner, FrankenPHPRepoName);
            Release latestRelease = null;
            foreach (var release in releases)
            {
                if (release.Draft) continue;
                latestRelease = release;
                break;
            }

            if (latestRelease == null)
            {
                AnsiConsole.MarkupLine("[red]No FrankenPHP release found.[/]");
                return null;
            }

            var asset = latestRelease.Assets.FirstOrDefault(a => a.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase));
            if (asset == null)
            {
                _debugOutputHelper.WriteErrorOutput($"FrankenPHP asset '{assetName}' not found in release {latestRelease.TagName}", null);
                AnsiConsole.MarkupLine("[red]FrankenPHP binary for your platform was not found in the latest release.[/]");
                return null;
            }

            AnsiConsole.MarkupLine("[grey]Downloading FrankenPHP {0}...[/]", latestRelease.TagName);
            var response = await HttpClient.GetAsync(asset.BrowserDownloadUrl);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = File.Create(binaryPath);
            await stream.CopyToAsync(fileStream);
            fileStream.Close();

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    var chmodProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = "chmod",
                        ArgumentList = { "+x", binaryPath },
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                    chmodProcess?.WaitForExit(5000);
                }
                catch (Exception ex)
                {
                    _debugOutputHelper.WriteErrorOutput("Failed to chmod FrankenPHP binary", ex);
                }
            }

            return binaryPath;
        }
        catch (Exception ex)
        {
            _debugOutputHelper.WriteErrorOutput("FrankenPHP download failed", ex);
            AnsiConsole.MarkupLine("[red]Failed to download FrankenPHP: {0}[/]", ex.Message);
            if (File.Exists(binaryPath))
                try { File.Delete(binaryPath); } catch { /* ignore */ }
            return null;
        }
    }

    public void StartFrankenPHP(string documentRoot, int port, string pidFile)
    {
        // GetOrDownloadFrankenPHPAsync must have been called first; we need the path from caller
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var binDir = Path.Combine(baseDir, "bin");
        var binaryFileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "frankenphp.exe" : "frankenphp";
        var binaryPath = Path.Combine(binDir, binaryFileName);

        if (!File.Exists(binaryPath))
        {
            AnsiConsole.MarkupLine("[red]FrankenPHP binary not found. Run admin start again to download it.[/]");
            return;
        }

        var arguments = $"php-server -r \"{documentRoot}\" -l 127.0.0.1:{port}";
        var startInfo = new ProcessStartInfo
        {
            FileName = binaryPath,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(documentRoot) ?? baseDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        try
        {
            var process = Process.Start(startInfo);
            if (process == null)
            {
                AnsiConsole.MarkupLine("[red]Failed to start FrankenPHP process.[/]");
                return;
            }

            File.WriteAllText(pidFile, process.Id.ToString());
        }
        catch (Exception ex)
        {
            _debugOutputHelper.WriteErrorOutput("Failed to start FrankenPHP", ex);
            AnsiConsole.MarkupLine("[red]Failed to start FrankenPHP: {0}[/]", ex.Message);
        }
    }

    public bool StopFrankenPHP(string pidFile)
    {
        if (!File.Exists(pidFile))
        {
            AnsiConsole.MarkupLine("[yellow]Admin server is not running (no PID file found).[/]");
            return false;
        }

        var pidContent = File.ReadAllText(pidFile).Trim();
        if (!int.TryParse(pidContent, out var pid))
        {
            AnsiConsole.MarkupLine("[red]Invalid PID file content.[/]");
            try { File.Delete(pidFile); } catch { /* ignore */ }
            return false;
        }

        try
        {
            var process = Process.GetProcessById(pid);
            process.Kill();
            process.WaitForExit(5000);
        }
        catch (ArgumentException)
        {
            AnsiConsole.MarkupLine("[yellow]Admin server process (PID {0}) is not running.[/]", pid);
        }
        catch (Exception ex)
        {
            _debugOutputHelper.WriteErrorOutput("Failed to stop FrankenPHP process", ex);
            AnsiConsole.MarkupLine("[red]Failed to stop server: {0}[/]", ex.Message);
            try { File.Delete(pidFile); } catch { /* ignore */ }
            return false;
        }
        finally
        {
            try { File.Delete(pidFile); } catch { /* ignore */ }
        }

        return true;
    }

    private static string GetFrankenPHPAssetName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "frankenphp-linux-x86_64",
                Architecture.Arm64 => "frankenphp-linux-aarch64",
                _ => null
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "frankenphp-mac-x86_64",
                Architecture.Arm64 => "frankenphp-mac-arm64",
                _ => null
            };
        }

        return null;
    }
}
