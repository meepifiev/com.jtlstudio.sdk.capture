#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Capture
{
    [FilePath("ProjectSettings/JTLSDKCapture.asset", FilePathAttribute.Location.ProjectFolder)]
    public class CaptureSettings : ScriptableSingleton<CaptureSettings>
    {
        [SerializeField] private string _outputPath = "Captures";
        [SerializeField] private CaptureSource _source = CaptureSource.GameView;
        [SerializeField] private string _cameraName = "";
        [SerializeField] private bool _includeOverlayUi = true;
        [SerializeField] private int _resolutionIndex;
        [SerializeField] private Vector2Int _customSize = new Vector2Int(1920, 1080);
        [SerializeField] private bool _everyLanguageOfConfiguration = true;
        [SerializeField] private List<Language> _languages = new List<Language>();
        [SerializeField] private int _frameRate = 30;
        [SerializeField] private float _videoSeconds = 15f;
        [SerializeField] private bool _recordAudio = true;
        [SerializeField] private LayerMask _hiddenLayers;
        [SerializeField] private List<string> _hiddenObjects = new List<string>();
        [SerializeField] private KeyCode _screenshotKey = KeyCode.F9;
        [SerializeField] private KeyCode _videoKey = KeyCode.F10;

        public string OutputPath
        {
            get => string.IsNullOrWhiteSpace(_outputPath) ? "Captures" : _outputPath;
            set => _outputPath = string.IsNullOrWhiteSpace(value) ? "Captures" : value.Trim();
        }

        public CaptureSource Source
        {
            get => _source;
            set => _source = value;
        }

        public string CameraName
        {
            get => _cameraName;
            set => _cameraName = value ?? "";
        }

        public bool IncludeOverlayUi
        {
            get => _includeOverlayUi;
            set => _includeOverlayUi = value;
        }

        public int ResolutionIndex
        {
            get => Mathf.Clamp(_resolutionIndex, 0, CaptureResolutions.CustomIndex);
            set => _resolutionIndex = Mathf.Clamp(value, 0, CaptureResolutions.CustomIndex);
        }

        public Vector2Int CustomSize
        {
            get => _customSize;
            set => _customSize = new Vector2Int(Mathf.Clamp(value.x, 16, 8192), Mathf.Clamp(value.y, 16, 8192));
        }

        public Vector2Int Size => CaptureResolutions.Size(ResolutionIndex, CustomSize);

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
    }
}
#endif
