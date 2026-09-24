using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Capture
{
    public class CaptureWindow : EditorWindow
    {
        private const float ContentWidth = 470f;
        private const float LabelWidth = 145f;
        private const float ButtonWidth = 74f;

        private string _newObject = "";
        private Vector2 _scroll;

        [MenuItem("JTL SDK/Capture", false, 11)]
        public static void Open()
        {
            CaptureWindow window = GetWindow<CaptureWindow>();
            window.titleContent = new GUIContent("Capture");
            window.minSize = new Vector2(430f, 420f);
            window.Show();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnGUI()
        {
            CaptureSettings settings = CaptureSettings.instance;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = LabelWidth;
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(ContentWidth));

            Section("Capture", () => DrawSource(settings));
            Section("Format", () => DrawFormat(settings));
            Section("Languages", () => DrawLanguages(settings));
            Section("Output", () => DrawOutput(settings));
            Section("Scene", () => DrawScene(settings));
            EditorGUILayout.Space();
            DrawButtons(settings);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            EditorGUIUtility.labelWidth = labelWidth;
            settings.Persist();
        }

        private void Section(string title, System.Action body)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            body();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2f);
        }

        private void DrawSource(CaptureSettings settings)
        {
            settings.Source = (CaptureSource)EditorGUILayout.EnumPopup("Capture from", settings.Source);

            if (settings.Source == CaptureSource.GameView)
            {
                EditorGUILayout.LabelField(" ", "Game View with UI, the view is resized for the shot", EditorStyles.miniLabel);
                return;
            }

            List<string> cameras = CameraNames();

            if (cameras.Count == 0)
            {
                settings.CameraName = EditorGUILayout.TextField("Camera", settings.CameraName);
                EditorGUILayout.LabelField(" ", "Enter play mode to pick a camera from the scene", EditorStyles.miniLabel);
                return;
            }

            int index = Mathf.Max(0, cameras.IndexOf(settings.CameraName));
            index = EditorGUILayout.Popup("Camera", index, cameras.ToArray());
            settings.CameraName = cameras[index];
            settings.IncludeOverlayUi = EditorGUILayout.Toggle("Include overlay UI", settings.IncludeOverlayUi);
        }

        private void DrawFormat(CaptureSettings settings)
        {
            settings.ResolutionIndex = EditorGUILayout.Popup("Frame size", settings.ResolutionIndex, CaptureResolutions.Labels);

            if (settings.ResolutionIndex == CaptureResolutions.CustomIndex)
            {
                Rect line = EditorGUILayout.GetControlRect();
                Rect field = EditorGUI.PrefixLabel(line, new GUIContent("Custom size"));
                float half = (field.width - 14f) * 0.5f;
                int width = EditorGUI.IntField(new Rect(field.x, field.y, half, field.height), settings.CustomSize.x);
                EditorGUI.LabelField(new Rect(field.x + half, field.y, 14f, field.height), "x");
                int height = EditorGUI.IntField(new Rect(field.x + half + 14f, field.y, half, field.height), settings.CustomSize.y);
                settings.CustomSize = new Vector2Int(width, height);
            }

            settings.FrameRate = EditorGUILayout.IntSlider("Frame rate", settings.FrameRate, 10, 120);
            settings.RecordMode = (RecordMode)EditorGUILayout.EnumPopup("Recording mode", settings.RecordMode);

            if (settings.RecordMode == RecordMode.Duration)
            {
                settings.VideoSeconds = EditorGUILayout.Slider("Video length, s", settings.VideoSeconds, 1f, 120f);
            }
            else
            {
                EditorGUILayout.LabelField(" ", "Recording runs until you press Stop", EditorStyles.miniLabel);
            }

            settings.RecordAudio = EditorGUILayout.Toggle("Record audio", settings.RecordAudio);
            settings.ExitPlayMode = EditorGUILayout.Toggle("Exit Play Mode after", settings.ExitPlayMode);
        }

        private void DrawLanguages(CaptureSettings settings)
        {
            settings.EveryLanguageOfConfiguration = EditorGUILayout.ToggleLeft("Every language of the active configuration", settings.EveryLanguageOfConfiguration);

            if (settings.EveryLanguageOfConfiguration == false)
            {
                foreach (Language language in CaptureLanguages.Configured())
                {
                    bool selected = settings.Languages.Contains(language);
                    bool edited = EditorGUILayout.ToggleLeft("   " + language, selected);

                    if (edited == selected)
                    {
                        continue;
                    }

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

            EditorGUILayout.LabelField(" ", string.Join(", ", CaptureLanguages.Selected()), EditorStyles.miniLabel);
        }

        private void DrawOutput(CaptureSettings settings)
        {
            EditorGUILayout.BeginHorizontal();
            settings.OutputPath = EditorGUILayout.TextField("Folder", settings.OutputPath);

            if (GUILayout.Button("Browse", GUILayout.Width(ButtonWidth)))
            {
                string picked = EditorUtility.OpenFolderPanel("Capture output", CaptureOutput.Resolve(settings), "");

                if (string.IsNullOrEmpty(picked) == false)
                {
                    settings.OutputPath = Relative(picked);
                    GUIUtility.ExitGUI();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(" ", CaptureOutput.Resolve(settings), EditorStyles.miniLabel);

            if (GUILayout.Button("Open", GUILayout.Width(ButtonWidth)))
            {
                CaptureOutput.Reveal(CaptureOutput.Folder(settings));
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawScene(CaptureSettings settings)
        {
            settings.HiddenLayers = EditorGUILayout.MaskField("Hide layers", settings.HiddenLayers, UnityEditorInternal.InternalEditorUtility.layers);
            settings.ScreenshotKey = (KeyCode)EditorGUILayout.EnumPopup("Screenshot key", settings.ScreenshotKey);
            settings.VideoKey = (KeyCode)EditorGUILayout.EnumPopup("Record key", settings.VideoKey);
            EditorGUILayout.LabelField("Hide objects by name", EditorStyles.miniBoldLabel);

            for (int index = 0; index < settings.HiddenObjects.Count; index++)
            {
                EditorGUILayout.BeginHorizontal();
                settings.HiddenObjects[index] = EditorGUILayout.TextField(settings.HiddenObjects[index]);

                if (GUILayout.Button("Remove", GUILayout.Width(ButtonWidth)))
                {
                    settings.HiddenObjects.RemoveAt(index);
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            _newObject = EditorGUILayout.TextField(_newObject);

            if (GUILayout.Button("Add", GUILayout.Width(ButtonWidth)) && string.IsNullOrWhiteSpace(_newObject) == false)
            {
                settings.HiddenObjects.Add(_newObject.Trim());
                _newObject = "";
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawButtons(CaptureSettings settings)
        {
            Vector2Int size = settings.Size;
            EditorGUILayout.LabelField("Capturing " + size.x + " x " + size.y + " in " + CaptureLanguages.Selected().Count + " languages", EditorStyles.miniLabel);

            if (Application.isPlaying == false)
            {
                EditorGUILayout.HelpBox("Capture runs in Play Mode: enter play, get the game to the right moment and shoot.", MessageType.Info);
                return;
            }

            if (CaptureLanguages.Selected().Count > 1 && CaptureLanguages.CanSwitch == false)
            {
                EditorGUILayout.HelpBox("JTL SDK is not created in this scene, so the language cannot be switched and every file would repeat one language. Create the SDK, assign CaptureLanguages.Switch or leave one language selected.", MessageType.Warning);
                return;
            }

            using (new EditorGUI.DisabledScope(CaptureRuntime.IsBusy))
            {
                if (GUILayout.Button("Capture frame in every language", GUILayout.Height(30f)))
                {
                    CaptureRuntime.Instance.TakeScreenshots();
                }
            }

            if (CaptureRuntime.IsRecording)
            {
                GUI.backgroundColor = new Color(1f, 0.45f, 0.4f);

                if (GUILayout.Button("Stop recording", GUILayout.Height(30f)))
                {
                    CaptureRuntime.Instance.StopVideo();
                }

                GUI.backgroundColor = Color.white;
            }
            else
            {
                using (new EditorGUI.DisabledScope(CaptureRuntime.IsBusy))
                {
                    if (GUILayout.Button("Record video in every language", GUILayout.Height(30f)))
                    {
                        CaptureRuntime.Instance.StartVideo();
                    }
                }
            }

            if (string.IsNullOrEmpty(CaptureRuntime.Status) == false)
            {
                EditorGUILayout.HelpBox(CaptureRuntime.Status, MessageType.None);
            }
        }

        private string Relative(string path)
        {
            string root = Directory.GetParent(Application.dataPath).FullName;
            return path.StartsWith(root) ? path.Substring(root.Length).TrimStart('/', '\\') : path;
        }

        private List<string> CameraNames()
        {
            List<string> names = new List<string>();

            foreach (Camera camera in Camera.allCameras)
            {
                if (names.Contains(camera.name) == false)
                {
                    names.Add(camera.name);
                }
            }

            return names;
        }
    }
}
