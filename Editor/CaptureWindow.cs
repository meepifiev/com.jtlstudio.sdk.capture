using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Capture
{
    public class CaptureWindow : EditorWindow
    {
        private string _newObject = "";

        [MenuItem("JTL SDK/Capture", false, 11)]
        public static void Open()
        {
            CaptureWindow window = GetWindow<CaptureWindow>();
            window.titleContent = new GUIContent("Capture");
            window.minSize = new Vector2(420f, 380f);
            window.Show();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnGUI()
        {
            CaptureSettings settings = CaptureSettings.instance;
            List<Language> languages = CaptureLanguages.Selected();

            EditorGUILayout.LabelField("Resolution", EditorStyles.boldLabel);
            settings.ResolutionIndex = EditorGUILayout.Popup("Frame size", settings.ResolutionIndex, CaptureResolutions.Labels);

            if (settings.ResolutionIndex == CaptureResolutions.CustomIndex)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Custom size");
                int width = EditorGUILayout.IntField(settings.CustomSize.x, GUILayout.Width(70f));
                EditorGUILayout.LabelField("x", GUILayout.Width(12f));
                int height = EditorGUILayout.IntField(settings.CustomSize.y, GUILayout.Width(70f));
                EditorGUILayout.EndHorizontal();
                settings.CustomSize = new Vector2Int(width, height);
            }

            Vector2Int size = settings.Size;
            EditorGUILayout.LabelField(" ", "Capturing " + size.x + " x " + size.y, EditorStyles.miniLabel);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Languages", EditorStyles.boldLabel);
            settings.EveryLanguageOfConfiguration = EditorGUILayout.ToggleLeft("Every language of the active configuration", settings.EveryLanguageOfConfiguration);

            if (settings.EveryLanguageOfConfiguration == false)
            {
                DrawLanguagePicker(settings);
            }

            EditorGUILayout.LabelField(" ", string.Join(", ", languages), EditorStyles.miniLabel);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            settings.OutputPath = EditorGUILayout.TextField("Output folder", settings.OutputPath);
            settings.FrameRate = EditorGUILayout.IntSlider("Frame rate", settings.FrameRate, 10, 120);
            settings.VideoSeconds = EditorGUILayout.Slider("Video length, s", settings.VideoSeconds, 1f, 120f);
            settings.RecordAudio = EditorGUILayout.Toggle("Record audio", settings.RecordAudio);
            settings.HiddenLayers = EditorGUILayout.MaskField("Hide layers", settings.HiddenLayers, UnityEditorInternal.InternalEditorUtility.layers);
            settings.ScreenshotKey = (KeyCode)EditorGUILayout.EnumPopup("Screenshot key", settings.ScreenshotKey);
            settings.VideoKey = (KeyCode)EditorGUILayout.EnumPopup("Record key", settings.VideoKey);

            DrawHiddenObjects(settings);
            EditorGUILayout.Space();

            if (Application.isPlaying == false)
            {
                EditorGUILayout.HelpBox("Capture runs in Play Mode: enter play, get the game to the right moment and shoot.", MessageType.Info);
                settings.Persist();
                return;
            }

            using (new EditorGUI.DisabledScope(CaptureRuntime.IsBusy))
            {
                if (GUILayout.Button("Capture frame in every language", GUILayout.Height(28f)))
                {
                    CaptureRuntime.Instance.TakeScreenshots();
                }
            }

            if (CaptureRuntime.IsRecording)
            {
                if (GUILayout.Button("Stop recording", GUILayout.Height(28f)))
                {
                    CaptureRuntime.Instance.StopVideo();
                }
            }
            else
            {
                using (new EditorGUI.DisabledScope(CaptureRuntime.IsBusy))
                {
                    if (GUILayout.Button("Record video in every language", GUILayout.Height(28f)))
                    {
                        CaptureRuntime.Instance.StartVideo();
                    }
                }
            }

            if (string.IsNullOrEmpty(CaptureRuntime.Status) == false)
            {
                EditorGUILayout.HelpBox(CaptureRuntime.Status, MessageType.None);
            }

            settings.Persist();
        }

        private void DrawLanguagePicker(CaptureSettings settings)
        {
            foreach (Language language in CaptureLanguages.Configured())
            {
                bool selected = settings.Languages.Contains(language);
                bool edited = EditorGUILayout.ToggleLeft("   " + language, selected);

                if (edited != selected)
                {
                    if (edited)
                    {
                        settings.Languages.Add(language);
                    }
                    else
                    {
                        settings.Languages.Remove(language);
                    }
                }
            }
        }

        private void DrawHiddenObjects(CaptureSettings settings)
        {
            EditorGUILayout.LabelField("Hide objects by name", EditorStyles.miniBoldLabel);

            for (int index = 0; index < settings.HiddenObjects.Count; index++)
            {
                EditorGUILayout.BeginHorizontal();
                settings.HiddenObjects[index] = EditorGUILayout.TextField(settings.HiddenObjects[index]);

                if (GUILayout.Button("Remove", GUILayout.Width(70f)))
                {
                    settings.HiddenObjects.RemoveAt(index);
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            _newObject = EditorGUILayout.TextField(_newObject);

            if (GUILayout.Button("Add", GUILayout.Width(70f)) && string.IsNullOrWhiteSpace(_newObject) == false)
            {
                settings.HiddenObjects.Add(_newObject.Trim());
                _newObject = "";
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
