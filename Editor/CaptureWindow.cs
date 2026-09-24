using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Capture
{
    public class CaptureWindow : EditorWindow
    {
        private Vector2 _scroll;
        private string _newObject = "";

        [MenuItem("JTL SDK/Capture", false, 11)]
        public static void Open()
        {
            CaptureWindow window = GetWindow<CaptureWindow>();
            window.titleContent = new GUIContent("Capture");
            window.minSize = new Vector2(460f, 420f);
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

            EditorGUILayout.LabelField("Языки съёмки", EditorStyles.boldLabel);
            settings.EveryLanguageOfConfiguration = EditorGUILayout.ToggleLeft("Все языки активной конфигурации", settings.EveryLanguageOfConfiguration);

            if (settings.EveryLanguageOfConfiguration == false)
            {
                DrawLanguagePicker(settings);
            }

            EditorGUILayout.LabelField("Снимаем на: " + string.Join(", ", languages), EditorStyles.miniLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Пресеты", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(140f));
            string group = "";

            foreach (CapturePreset preset in settings.Presets)
            {
                if (preset.Group != group)
                {
                    group = preset.Group;
                    EditorGUILayout.LabelField(group, EditorStyles.miniBoldLabel);
                }

                EditorGUILayout.BeginHorizontal();
                preset.Enabled = EditorGUILayout.Toggle(preset.Enabled, GUILayout.Width(18f));
                EditorGUILayout.LabelField(preset.Name + (preset.Kind == CaptureKind.Video ? "  (видео)" : ""), GUILayout.Width(210f));
                preset.Width = EditorGUILayout.IntField(preset.Width, GUILayout.Width(60f));
                EditorGUILayout.LabelField("x", GUILayout.Width(10f));
                preset.Height = EditorGUILayout.IntField(preset.Height, GUILayout.Width(60f));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Вернуть пресеты по умолчанию"))
            {
                settings.ResetPresets();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Настройки", EditorStyles.boldLabel);
            settings.OutputPath = EditorGUILayout.TextField("Папка", settings.OutputPath);
            settings.FrameRate = EditorGUILayout.IntSlider("Кадров в секунду", settings.FrameRate, 10, 120);
            settings.VideoSeconds = EditorGUILayout.Slider("Длина ролика, с", settings.VideoSeconds, 1f, 120f);
            settings.RecordAudio = EditorGUILayout.Toggle("Писать звук", settings.RecordAudio);
            settings.HiddenLayers = EditorGUILayout.MaskField("Прятать слои", settings.HiddenLayers, UnityEditorInternal.InternalEditorUtility.layers);
            settings.ScreenshotKey = (KeyCode)EditorGUILayout.EnumPopup("Клавиша скриншота", settings.ScreenshotKey);
            settings.VideoKey = (KeyCode)EditorGUILayout.EnumPopup("Клавиша записи", settings.VideoKey);

            DrawHiddenObjects(settings);

            EditorGUILayout.Space();

            if (Application.isPlaying == false)
            {
                EditorGUILayout.HelpBox("Съёмка идёт в Play Mode: запустите игру, доведите её до нужного места и снимайте.", MessageType.Info);
                settings.Persist();
                return;
            }

            using (new EditorGUI.DisabledScope(CaptureRuntime.IsBusy))
            {
                if (GUILayout.Button("Снять скриншоты на всех языках", GUILayout.Height(28f)))
                {
                    CaptureRuntime.Instance.TakeScreenshots();
                }
            }

            if (CaptureRuntime.IsRecording)
            {
                if (GUILayout.Button("Остановить запись", GUILayout.Height(28f)))
                {
                    CaptureRuntime.Instance.StopVideo();
                }
            }
            else
            {
                using (new EditorGUI.DisabledScope(CaptureRuntime.IsBusy))
                {
                    if (GUILayout.Button("Записать видео на всех языках", GUILayout.Height(28f)))
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
            EditorGUILayout.LabelField("Прятать объекты по имени", EditorStyles.miniBoldLabel);

            for (int index = 0; index < settings.HiddenObjects.Count; index++)
            {
                EditorGUILayout.BeginHorizontal();
                settings.HiddenObjects[index] = EditorGUILayout.TextField(settings.HiddenObjects[index]);

                if (GUILayout.Button("Убрать", GUILayout.Width(70f)))
                {
                    settings.HiddenObjects.RemoveAt(index);
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            _newObject = EditorGUILayout.TextField(_newObject);

            if (GUILayout.Button("Добавить", GUILayout.Width(70f)) && string.IsNullOrWhiteSpace(_newObject) == false)
            {
                settings.HiddenObjects.Add(_newObject.Trim());
                _newObject = "";
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
