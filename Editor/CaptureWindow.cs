using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Capture
{
    public class CaptureWindow : EditorWindow
    {
        private const float ContentWidth = 460f;
        private const float LabelWidth = 150f;

        private string _newObject = "";
        private Vector2 _scroll;

        [MenuItem("JTL SDK/Capture", false, 11)]
        public static void Open()
        {
            CaptureWindow window = GetWindow<CaptureWindow>();
            window.titleContent = new GUIContent("Capture");
            window.minSize = new Vector2(420f, 400f);
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

            DrawSource(settings);
            DrawResolution(settings);
            DrawLanguages(settings);
            DrawSettings(settings);
            DrawHiddenObjects(settings);
            EditorGUILayout.Space();
            DrawButtons(settings);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            EditorGUIUtility.labelWidth = labelWidth;
            settings.Persist();
        }

        private void DrawSource(CaptureSettings settings)
        {
            EditorGUILayout.LabelField("Source", EditorStyles.boldLabel);
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
            EditorGUILayout.LabelField(" ", settings.IncludeOverlayUi
                ? "Overlay canvases render through this camera while capturing"
                : "Camera render only, Screen Space Overlay UI stays out", EditorStyles.miniLabel);
        }

        private void DrawResolution(CaptureSettings settings)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Resolution", EditorStyles.boldLabel);
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

            Vector2Int size = settings.Size;
            EditorGUILayout.LabelField(" ", "Capturing " + size.x + " x " + size.y, EditorStyles.miniLabel);
        }

        private void DrawLanguages(CaptureSettings settings)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Languages", EditorStyles.boldLabel);
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

        private void DrawSettings(CaptureSettings settings)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            settings.OutputPath = EditorGUILayout.TextField("Output folder", settings.OutputPath);
            settings.FrameRate = EditorGUILayout.IntSlider("Frame rate", settings.FrameRate, 10, 120);
            settings.VideoSeconds = EditorGUILayout.Slider("Video length, s", settings.VideoSeconds, 1f, 120f);
            settings.RecordAudio = EditorGUILayout.Toggle("Record audio", settings.RecordAudio);
            settings.HiddenLayers = EditorGUILayout.MaskField("Hide layers", settings.HiddenLayers, UnityEditorInternal.InternalEditorUtility.layers);
            settings.ScreenshotKey = (KeyCode)EditorGUILayout.EnumPopup("Screenshot key", settings.ScreenshotKey);
            settings.VideoKey = (KeyCode)EditorGUILayout.EnumPopup("Record key", settings.VideoKey);
        }

        private void DrawButtons(CaptureSettings settings)
        {
            if (Application.isPlaying == false)
            {
                EditorGUILayout.HelpBox("Capture runs in Play Mode: enter play, get the game to the right moment and shoot.", MessageType.Info);
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
        }

        private void DrawHiddenObjects(CaptureSettings settings)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Hide objects by name", EditorStyles.boldLabel);

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
