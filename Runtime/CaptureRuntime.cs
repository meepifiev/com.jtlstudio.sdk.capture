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

        private static CaptureRuntime _instance;

        private readonly CaptureScene _scene = new CaptureScene();
        private readonly CaptureCanvases _canvases = new CaptureCanvases();

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

        public void TakeScreenshots()
        {
            if (IsBusy == false)
            {
                StartCoroutine(ScreenshotRoutine());
            }
        }

        public void StartVideo()
        {
            if (IsBusy == false)
            {
                StartCoroutine(VideoRoutine());
            }
        }

        public void StopVideo()
        {
            IsRecording = false;
        }

        private void Update()
        {
            CaptureSettings settings = CaptureSettings.instance;

            if (Input.GetKeyDown(settings.ScreenshotKey))
            {
                TakeScreenshots();
            }

            if (Input.GetKeyDown(settings.VideoKey))
            {
                if (IsRecording)
                {
                    StopVideo();
                }
                else
                {
                    StartVideo();
                }
            }
        }

        private IEnumerator ScreenshotRoutine()
        {
            CaptureSettings settings = CaptureSettings.instance;
            List<Language> languages = CaptureLanguages.Selected();
            Vector2Int size = settings.Size;

            if (languages.Count > 1 && CaptureLanguages.CanSwitch == false)
            {
                Status = NoSwitch;
                Debug.LogWarning(NoSwitch);
                yield break;
            }

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
            Status = "Done: " + saved + " frames " + size.x + "x" + size.y + " in " + folder;
            CaptureOutput.Reveal(folder);
        }

        private IEnumerator VideoRoutine()
        {
            CaptureSettings settings = CaptureSettings.instance;
            List<Language> languages = CaptureLanguages.Selected();
            Vector2Int size = settings.Size;

            if (languages.Count > 1 && CaptureLanguages.CanSwitch == false)
            {
                Status = NoSwitch;
                Debug.LogWarning(NoSwitch);
                yield break;
            }

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
            Directory.CreateDirectory(folder);
            List<MediaEncoder> encoders = new List<MediaEncoder>();
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
                encoders.Add(settings.RecordAudio
                    ? new MediaEncoder(path, video, audio)
                    : new MediaEncoder(path, video));
            }

            float capture = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / (settings.FrameRate * languages.Count);

            if (settings.RecordAudio)
            {
                AudioRenderer.Start();
            }

            bool manual = settings.RecordMode == RecordMode.Manual;
            int frames = manual ? int.MaxValue : Mathf.RoundToInt(settings.VideoSeconds * settings.FrameRate);
            int written = 0;

            for (int frame = 0; frame < frames && IsRecording; frame++)
            {
                NativeArray<float> samples = default;
                int sampleCount = 0;

                for (int index = 0; index < languages.Count; index++)
                {
                    CaptureLanguages.Apply(languages[index]);
                    yield return new WaitForEndOfFrame();

                    Texture2D picture = CaptureFrame.Grab(settings, size);
                    encoders[index].AddFrame(picture);
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
                    NativeArray<float> mixed = samples.GetSubArray(0, sampleCount);

                    foreach (MediaEncoder encoder in encoders)
                    {
                        encoder.AddSamples(mixed);
                    }

                    samples.Dispose();
                }

                written++;
                Status = manual
                    ? "Recording, " + written + " frames, press Stop when ready"
                    : "Recorded " + written + " of " + frames + " frames";
            }

            if (settings.RecordAudio)
            {
                AudioRenderer.Stop();
            }

            foreach (MediaEncoder encoder in encoders)
            {
                encoder.Dispose();
            }

            Time.captureDeltaTime = capture;
            GameViewResolution.Restore();
            _canvases.Restore();
            CaptureLanguages.Apply(original);
            _scene.Restore();
            IsRecording = false;
            IsBusy = false;
            Status = "Done: " + languages.Count + " videos " + size.x + "x" + size.y + ", " + written + " frames each, in " + folder;
            CaptureOutput.Reveal(folder);

            if (settings.ExitPlayMode)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
        }
    }
}
#endif
