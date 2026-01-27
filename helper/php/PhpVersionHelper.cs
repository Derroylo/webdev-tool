using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WebDev.Tool.Helper.Php
{
    public partial class PhpVersionHelper(ExecCommand _execCommand)
    {
        public List<string> GetAvailablePhpVersions()
        {
            var availablePhpVersions = new List<string>();

            string pattern = @"Alternative: \/usr\/bin\/php([0-9.]+)";
            string input = _execCommand.Exec("update-alternatives --query php");

            RegexOptions options = RegexOptions.Multiline;
        
            foreach (Match m in Regex.Matches(input, pattern, options))
            {
                if (!m.Groups.ContainsKey("1") || m.Groups[1].ToString().Length < 3) {
                    continue;
                }

                availablePhpVersions.Insert(0, m.Groups[1].ToString());
            }

            return availablePhpVersions;
        }

        public string GetCurrentPhpVersion()
        {
            string output = GetCurrentPhpVersionOutput();

            Match match = PhpVersionMatchRegex().Match(output);

            if (!match.Success) {
                throw new Exception("Failed to parse the php version command output to find the active version.");
            }

            return match.Groups[1].Value;
        }

        [GeneratedRegex(@"(?:PHP) ([(0-9)].[(0-9)])")]
        private partial Regex PhpVersionMatchRegex();

        public string GetCurrentPhpVersionOutput()
        {
            return _execCommand.Exec("php -version");
        }
    }
}