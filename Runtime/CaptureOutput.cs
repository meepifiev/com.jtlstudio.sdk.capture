#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Capture
{
    public static class CaptureOutput
    {
        public static string Folder(CaptureSettings settings)
        {
            string path = settings.OutputPath;

            if (Path.IsPathRooted(path) == false)
            {
                path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, path);
            }

            Directory.CreateDirectory(path);
            return path;
        }

        public static string Name(CapturePreset preset, Language language, string extension)
        {
            string product = Sanitize(Application.productName);
            string stamp = DateTime.Now.ToString("MMdd_HHmmss");
            return product + "_" + preset.FileName + "_" + CaptureLanguages.Code(language) + "_" + stamp + "." + extension;
        }

        public static void Reveal(string folder)
        {
            if (Directory.Exists(folder))
            {
                EditorUtility.RevealInFinder(folder);
            }
        }

        private static string Sanitize(string value)
        {
            string result = "";

            foreach (char symbol in value)
            {
                result += char.IsLetterOrDigit(symbol) && symbol < 128 ? char.ToLowerInvariant(symbol).ToString() : "";
            }

            return result.Length == 0 ? "game" : result;
        }
    }
}
#endif
