#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEditor.Media;
using UnityEngine;

namespace JTLStudio.SDK.Capture
{
    public class CaptureRuntime : MonoBehaviour
    {
        private const string NoSwitch = "The language cannot be switched: JTL SDK is not created in this scene. Create the SDK, or assign CaptureLanguages.Switch, or leave one language selected.";

        private const int StartupFrames = 3;
        private const float StartupSeconds = 10f;

        private static CaptureRuntime _instance;

        private readonly CaptureScene _scene = new CaptureScene();
        private readonly CaptureCanvases _canvases = new CaptureCanvases();
        private readonly List<MediaEncoder> _encoders = new List<MediaEncoder>();
        private readonly List<string> _videos = new List<string>();

        private float _captureDelta;
        private bool _audioStarted;

        public static bool IsRecording { get; private set; }
        public static bool IsBusy { get; private set; }
        public static string Status { get; private set; } = "";

        public static CaptureRuntime Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                if (Application.isPlaying == false)
                {
                    return null;
                }

                GameObject host = new GameObject("JTLSDK Capture");
                host.hideFlags = HideFlags.HideInHierarchy;
                DontDestroyOnLoad(host);
                _instance = host.AddComponent<CaptureRuntime>();
                return _instance;
            }
        }

        public void Run()
        {
            CaptureSettings settings = CaptureSettings.instance;
            Begin(settings.TakeScreenshots, settings.RecordVideo);
        }

        public void TakeScreenshots()
        {
            Begin(true, false);
        }

        public void StartVideo()
        {
            Begin(false, true);
        }

        public void StopVideo()
        {
            IsRecording = false;
        }

        public static void Cancel()
        {
            IsRecording = false;
        }

        public void RunWhenReady()
        {
            StartCoroutine(ReadyRoutine());
        }

        public void Toggle()
        {
            if (IsRecording)
            {
                StopVideo();
                return;
            }

            Run();
        }

        private IEnumerator ReadyRoutine()
        {
            for (int frame = 0; frame < StartupFrames; frame++)
            {
                yield return null;
            }

            float deadline = Time.realtimeSinceStartup + StartupSeconds;

            while (JTLSDK.IsCreated && JTLSDK.IsReady == false && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Run();
        }

        private void Begin(bool screenshots, bool video)
        {
            if (IsBusy || (screenshots == false && video == false))
            {
                return;
            }

            if (CaptureLanguages.Selected().Count > 1 && CaptureLanguages.CanSwitch == false)
            {
                Status = NoSwitch;
                Debug.LogWarning(NoSwitch);
                return;
            }

            if (CaptureSettings.instance.Source == CaptureSource.GameView)
            {
                UnityEditor.EditorApplication.ExecuteMenuItem("Window/General/Game");
            }

            StartCoroutine(Sequence(screenshots, video));
        }

        private IEnumerator Sequence(bool screenshots, bool video)
        {
            if (screenshots)
            {
                yield return ScreenshotRoutine();
            }

            if (video)
            {
                yield return VideoRoutine();
            }

            if (CaptureSettings.instance.ExitPlayMode)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
        }

        private void Update()
        {
            CaptureSettings settings = CaptureSettings.instance;

            if (Input.GetKeyDown(settings.CaptureKey))
            {
                Toggle();
            }
        }

        private IEnumerator ScreenshotRoutine()
        {
            CaptureSettings settings = CaptureSettings.instance;
            List<Language> languages = CaptureLanguages.Selected();
            Vector2Int size = settings.Size;

            IsBusy = true;
            Language original = JTLSDK.IsCreated ? JTLSDK.Language.Current : languages[0];
            float scale = Time.timeScale;
            Time.timeScale = 0f;
            _scene.Hide(settings);
            string folder = CaptureOutput.Folder(settings);
            bool fromGameView = settings.Source == CaptureSource.GameView;
            bool resized = fromGameView == false || GameViewResolution.Apply(size.x, size.y);

            if (fromGameView == false && settings.IncludeOverlayUi)
            {
                _canvases.Attach(CaptureFrame.Find(settings.CameraName));
            }

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            int saved = 0;

            foreach (Language language in languages)
            {
                CaptureLanguages.Apply(language);
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();

                Texture2D shot = CaptureFrame.Grab(settings, size);

                if (shot == null)
                {
                    break;
                }

                CaptureImage.WritePng(shot, Path.Combine(folder, CaptureOutput.Name(size, language, "png")));
                DestroyImmediate(shot);
                saved++;
                Status = (resized ? "Captured " : "Game View kept its size, cropping: ") + saved + " of " + languages.Count;
            }

            GameViewResolution.Restore();
            _canvases.Restore();
            CaptureLanguages.Apply(original);
            _scene.Restore();
            Time.timeScale = scale;
            IsBusy = false;

            if (saved < languages.Count)
            {
                Status = "The Game View stopped rendering: " + saved + " of " + languages.Count + " frames are saved.";
                Debug.LogWarning(Status);
            }
            else
            {
                Status = "Done: " + saved + " frames " + size.x + "x" + size.y + " in " + folder;
            }

            if (saved > 0)
            {
                CaptureOutput.Reveal(folder);
            }
        }

        private IEnumerator VideoRoutine()
        {
            CaptureSettings settings = CaptureSettings.instance;
            List<Language> languages = CaptureLanguages.Selected();
            Vector2Int size = settings.Size;

            IsBusy = true;
            IsRecording = true;
            Language original = JTLSDK.IsCreated ? JTLSDK.Language.Current : languages[0];
            _scene.Hide(settings);

            if (settings.Source == CaptureSource.GameView)
            {
                GameViewResolution.Apply(size.x, size.y);
            }
            else if (settings.IncludeOverlayUi)
            {
                _canvases.Attach(CaptureFrame.Find(settings.CameraName));
            }

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            string folder = CaptureOutput.Folder(settings);
            VideoTrackAttributes video = new VideoTrackAttributes
            {
                frameRate = new MediaRational(settings.FrameRate),
                width = (uint)size.x,
                height = (uint)size.y,
                includeAlpha = false
            };

            AudioTrackAttributes audio = new AudioTrackAttributes
            {
                sampleRate = new MediaRational(AudioSettings.outputSampleRate),
                channelCount = 2,
                language = ""
            };

            foreach (Language language in languages)
            {
                string path = Path.Combine(folder, CaptureOutput.Name(size, language, "mp4"));
                _videos.Add(path);
                _encoders.Add(settings.RecordAudio
                    ? new MediaEncoder(path, video, audio)
                    : new MediaEncoder(path, video));
            }

            _captureDelta = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / (settings.FrameRate * languages.Count);

            if (settings.RecordAudio)
            {
                AudioRenderer.Start();
                _audioStarted = true;
            }

            bool manual = settings.RecordMode == RecordMode.Manual;
            int frames = manual ? int.MaxValue : Mathf.RoundToInt(settings.VideoSeconds * settings.FrameRate);
            int written = 0;
            bool lost = false;

            try
            {
                for (int frame = 0; frame < frames && IsRecording && lost == false; frame++)
                {
                    NativeArray<float> samples = default;
                    int sampleCount = 0;

                    for (int index = 0; index < languages.Count; index++)
                    {
                        CaptureLanguages.Apply(languages[index]);
                        yield return new WaitForEndOfFrame();

                        Texture2D picture = CaptureFrame.Grab(settings, size);

                        if (picture == null)
                        {
                            lost = true;
                            break;
                        }

                        _encoders[index].AddFrame(picture);
                        DestroyImmediate(picture);

                        if (settings.RecordAudio)
                        {
                            int count = AudioRenderer.GetSampleCountForCaptureFrame();
                            NativeArray<float> part = new NativeArray<float>(count * 2, Allocator.Temp);
                            AudioRenderer.Render(part);

                            if (index == 0)
                            {
                                samples = new NativeArray<float>(part.Length * languages.Count, Allocator.Temp);
                            }

                            NativeArray<float>.Copy(part, 0, samples, sampleCount, part.Length);
                            sampleCount += part.Length;
                            part.Dispose();
                        }
                    }

                    if (settings.RecordAudio && samples.IsCreated)
                    {
                        if (lost == false)
                        {
                            NativeArray<float> mixed = samples.GetSubArray(0, sampleCount);

                            foreach (MediaEncoder encoder in _encoders)
                            {
                                encoder.AddSamples(mixed);
                            }
                        }

                        samples.Dispose();
                    }

                    if (lost)
                    {
                        break;
                    }

                    written++;
                    Status = manual
                        ? "Recording, " + written + " frames, press Stop when ready"
                        : "Recorded " + written + " of " + frames + " frames";
                }
            }
            finally
            {
                Finish(written == 0);
            }

            GameViewResolution.Restore();
            _canvases.Restore();
            CaptureLanguages.Apply(original);
            _scene.Restore();

            if (lost)
            {
                Status = written == 0
                    ? "The Game View stopped rendering before a single frame was written, nothing is saved."
                    : "Play Mode ended during the recording: " + written + " frames are saved in " + folder;
                Debug.LogWarning(Status);
            }
            else
            {
                Status = "Done: " + languages.Count + " videos " + size.x + "x" + size.y + ", " + written + " frames each, in " + folder;
            }

            if (written > 0)
            {
                CaptureOutput.Reveal(folder);
            }
        }

        private void OnDisable()
        {
            Finish(false);
        }

        private void Finish(bool discard)
        {
            if (_audioStarted)
            {
                _audioStarted = false;

                if (Application.isPlaying)
                {
                    AudioRenderer.Stop();
                }
            }

            foreach (MediaEncoder encoder in _encoders)
            {
                encoder.Dispose();
            }

            if (_encoders.Count > 0)
            {
                Time.captureDeltaTime = _captureDelta;
            }

            _encoders.Clear();

            if (discard)
            {
                foreach (string path in _videos)
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }

            _videos.Clear();
            IsRecording = false;
            IsBusy = false;
        }
    }
}
#endif
