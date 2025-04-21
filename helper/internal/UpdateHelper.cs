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

                string preReleaseName = Assembly.GetExecutingAssembly()
                    ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
                
                // Include PreRelease Version info if it exists
                if (!String.IsNullOrEmpty(preReleaseName))
                {
                    currentVersion += preReleaseName.Substring(0, preReleaseName.IndexOf('+'));
                }

                return currentVersion;
            }
        }
    }
}