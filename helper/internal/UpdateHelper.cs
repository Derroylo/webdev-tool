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

namespace WebDev.Tool.Helper.Internal
{
    internal class UpdateHelper
    {  
        public static string CurrentVersion {
            get {
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

                // Include PreRelease Version info if it exists
                if (Assembly.GetExecutingAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion != currentVersion) {
                    currentVersion += Assembly.GetExecutingAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
                }

                return currentVersion;
            }
        }
    }
}