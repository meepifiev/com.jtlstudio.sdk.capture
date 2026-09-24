#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;

namespace JTLStudio.SDK.Capture
{
    public static class CaptureLanguages
    {
        public static Action<Language> Switch { get; set; }

        public static List<Language> Configured()
        {
            List<Language> languages = new List<Language>();
            JTLSDKSettings settings = Settings();

            if (settings != null && settings.ActiveConfiguration != null)
            {
                foreach (Language language in settings.ActiveConfiguration.Languages)
                {
                    if (languages.Contains(language) == false)
                    {
                        languages.Add(language);
                    }
                }
            }

            if (settings != null && languages.Count == 0)
            {
                foreach (Language language in settings.SupportedLanguages)
                {
                    if (languages.Contains(language) == false)
                    {
                        languages.Add(language);
                    }
                }
            }

            if (languages.Count == 0)
            {
                languages.Add(settings == null ? Language.English : settings.DefaultLanguage);
            }

            return languages;
        }

        public static List<Language> Selected()
        {
            CaptureSettings settings = CaptureSettings.instance;

            if (settings.EveryLanguageOfConfiguration)
            {
                return Configured();
            }

            List<Language> configured = Configured();
            List<Language> selected = new List<Language>();

            foreach (Language language in settings.Languages)
            {
                if (configured.Contains(language) && selected.Contains(language) == false)
                {
                    selected.Add(language);
                }
            }

            return selected.Count == 0 ? configured : selected;
        }

        public static void Apply(Language language)
        {
            if (Switch != null)
            {
                Switch(language);
                return;
            }

            if (JTLSDK.IsCreated == false)
            {
                return;
            }

            foreach (Language supported in JTLSDK.Language.Supported)
            {
                if (supported == language)
                {
                    JTLSDK.Language.Set(language);
                    return;
                }
            }
        }

        public static string Code(Language language)
        {
            return new LanguageCodes().ToCode(language);
        }

        private static JTLSDKSettings Settings()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:" + nameof(JTLSDKSettings)))
            {
                JTLSDKSettings settings = AssetDatabase.LoadAssetAtPath<JTLSDKSettings>(AssetDatabase.GUIDToAssetPath(guid));

                if (settings != null)
                {
                    return settings;
                }
            }

            return null;
        }
    }
}
#endif
