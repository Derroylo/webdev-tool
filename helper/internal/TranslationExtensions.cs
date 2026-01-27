// helper/internal/TranslationExtensions.cs
using System;
using Spectre.Console;
using WebDev.Tool.Helper.Internal;

namespace WebDev.Tool.Helper.Internal
{
    public static class TranslationExtensions
    {
        public static void MarkupLineLocalized(this IAnsiConsole console, string key, params object[] args)
        {
            TranslationHelper _translationHelper = new TranslationHelper(new AppSettingsHelper(), new PathHelper());

            // If [translate]...[/] exists, translate only the content inside [translate]...[/]
            if (key.Contains("[translate]") && key.Contains("[/]", StringComparison.OrdinalIgnoreCase))
            {
                int startIdx = key.IndexOf("[translate]") + "[translate]".Length;
                int endIdx = key.IndexOf("[/]", startIdx, StringComparison.OrdinalIgnoreCase);

                if (startIdx >= "[translate]".Length && endIdx > startIdx)
                {
                    string before = key.Substring(0, key.IndexOf("[translate]"));
                    string toTranslate = key.Substring(startIdx, endIdx - startIdx);
                    string after = key.Substring(endIdx + "[/]".Length);

                    var translated = _translationHelper.GetString(toTranslate, args);
                    var fullText = before + translated + after;
                    console.MarkupLine(fullText);
                }
                else
                {
                    var text = _translationHelper.GetString(key, args);
                    console.MarkupLine(text);
                }
            }
            else
            {
                var text = _translationHelper.GetString(key, args);
                console.MarkupLine(text);
            }
        }
        
        public static string Localized(this string key, params object[] args)
        {
            TranslationHelper _translationHelper = new TranslationHelper(new AppSettingsHelper(), new PathHelper());
            
            return _translationHelper.GetString(key, args);
        }
    }
}