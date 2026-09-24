using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Capture
{
    [FilePath("ProjectSettings/JTLSDKCapture.asset", FilePathAttribute.Location.ProjectFolder)]
    public class CaptureSettings : ScriptableSingleton<CaptureSettings>
    {
        [SerializeField] private string _outputPath = "Captures";
        [SerializeField] private List<CapturePreset> _presets = new List<CapturePreset>();
        [SerializeField] private bool _everyLanguageOfConfiguration = true;
        [SerializeField] private List<Language> _languages = new List<Language>();
        [SerializeField] private int _frameRate = 30;
        [SerializeField] private float _videoSeconds = 15f;
        [SerializeField] private bool _recordAudio = true;
        [SerializeField] private LayerMask _hiddenLayers;
        [SerializeField] private List<string> _hiddenObjects = new List<string>();
        [SerializeField] private KeyCode _screenshotKey = KeyCode.F9;
        [SerializeField] private KeyCode _videoKey = KeyCode.F10;
        [SerializeField] private bool _presetsCreated;

        public string OutputPath
        {
            get => string.IsNullOrWhiteSpace(_outputPath) ? "Captures" : _outputPath;
            set => _outputPath = string.IsNullOrWhiteSpace(value) ? "Captures" : value.Trim();
        }

        public List<CapturePreset> Presets
        {
            get
            {
                if (_presetsCreated == false)
                {
                    _presetsCreated = true;
                    _presets = CapturePresets.Defaults();
                    Persist();
                }

                return _presets;
            }
        }

        public bool EveryLanguageOfConfiguration
        {
            get => _everyLanguageOfConfiguration;
            set => _everyLanguageOfConfiguration = value;
        }

        public List<Language> Languages => _languages;

        public int FrameRate
        {
            get => _frameRate;
            set => _frameRate = Mathf.Clamp(value, 10, 120);
        }

        public float VideoSeconds
        {
            get => _videoSeconds;
            set => _videoSeconds = Mathf.Clamp(value, 1f, 600f);
        }

        public bool RecordAudio
        {
            get => _recordAudio;
            set => _recordAudio = value;
        }

        public LayerMask HiddenLayers
        {
            get => _hiddenLayers;
            set => _hiddenLayers = value;
        }

        public List<string> HiddenObjects => _hiddenObjects;

        public KeyCode ScreenshotKey
        {
            get => _screenshotKey;
            set => _screenshotKey = value;
        }

        public KeyCode VideoKey
        {
            get => _videoKey;
            set => _videoKey = value;
        }

        public void Persist()
        {
            Save(true);
        }

        public void ResetPresets()
        {
            _presets = CapturePresets.Defaults();
            _presetsCreated = true;
            Persist();
        }
    }
}
