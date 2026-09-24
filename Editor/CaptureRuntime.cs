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
        private static CaptureRuntime _instance;

        private readonly CaptureScene _scene = new CaptureScene();

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
            List<CapturePreset> presets = EnabledPresets(settings, CaptureKind.Screenshot);

            if (presets.Count == 0)
            {
                Status = "Ни один пресет скриншотов не включён.";
                yield break;
            }

            IsBusy = true;
            Language original = JTLSDK.IsCreated ? JTLSDK.Language.Current : languages[0];
            float scale = Time.timeScale;
            Time.timeScale = 0f;
            _scene.Hide(settings);
            string folder = CaptureOutput.Folder(settings);
            int saved = 0;
            int total = presets.Count * languages.Count;

            foreach (CapturePreset preset in presets)
            {
                bool resized = GameViewResolution.Apply(preset.Width, preset.Height);

                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();

                foreach (Language language in languages)
                {
                    CaptureLanguages.Apply(language);
                    yield return new WaitForEndOfFrame();
                    yield return new WaitForEndOfFrame();

                    Texture2D frame = ScreenCapture.CaptureScreenshotAsTexture();
                    Texture2D shot = CaptureImage.Fit(frame, preset.Width, preset.Height);
                    string path = Path.Combine(folder, CaptureOutput.Name(preset, language, "png"));
                    CaptureImage.WritePng(shot, path);

                    if (shot != frame)
                    {
                        DestroyImmediate(shot);
                    }

                    DestroyImmediate(frame);
                    saved++;
                    Status = (resized ? "Снято " : "Game View не перестроился, кадрирую: ") + saved + " из " + total;
                }
            }

            GameViewResolution.Restore();
            CaptureLanguages.Apply(original);
            _scene.Restore();
            Time.timeScale = scale;
            IsBusy = false;
            Status = "Готово: " + saved + " скриншотов в " + folder;
            CaptureOutput.Reveal(folder);
        }

        private IEnumerator VideoRoutine()
        {
            CaptureSettings settings = CaptureSettings.instance;
            List<Language> languages = CaptureLanguages.Selected();
            List<CapturePreset> presets = EnabledPresets(settings, CaptureKind.Video);

            if (presets.Count == 0)
            {
                Status = "Ни один пресет видео не включён.";
                yield break;
            }

            CapturePreset preset = presets[0];
            IsBusy = true;
            IsRecording = true;
            Language original = JTLSDK.IsCreated ? JTLSDK.Language.Current : languages[0];
            _scene.Hide(settings);
            GameViewResolution.Apply(preset.Width, preset.Height);

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            string folder = CaptureOutput.Folder(settings);
            Directory.CreateDirectory(folder);
            List<MediaEncoder> encoders = new List<MediaEncoder>();
            VideoTrackAttributes video = new VideoTrackAttributes
            {
                frameRate = new MediaRational(settings.FrameRate),
                width = (uint)preset.Width,
                height = (uint)preset.Height,
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
                string path = Path.Combine(folder, CaptureOutput.Name(preset, language, "mp4"));
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

            int frames = Mathf.RoundToInt(settings.VideoSeconds * settings.FrameRate);
            int written = 0;

            for (int frame = 0; frame < frames && IsRecording; frame++)
            {
                NativeArray<float> samples = default;
                int sampleCount = 0;

                for (int index = 0; index < languages.Count; index++)
                {
                    CaptureLanguages.Apply(languages[index]);
                    yield return new WaitForEndOfFrame();

                    Texture2D shot = ScreenCapture.CaptureScreenshotAsTexture();
                    Texture2D picture = CaptureImage.Fit(shot, preset.Width, preset.Height);
                    encoders[index].AddFrame(picture);

                    if (picture != shot)
                    {
                        DestroyImmediate(picture);
                    }

                    DestroyImmediate(shot);

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
                Status = "Записано " + written + " из " + frames + " кадров";
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
            CaptureLanguages.Apply(original);
            _scene.Restore();
            IsRecording = false;
            IsBusy = false;
            Status = "Готово: " + languages.Count + " роликов по " + written + " кадров в " + folder;
            CaptureOutput.Reveal(folder);
        }

        private List<CapturePreset> EnabledPresets(CaptureSettings settings, CaptureKind kind)
        {
            List<CapturePreset> presets = new List<CapturePreset>();

            foreach (CapturePreset preset in settings.Presets)
            {
                if (preset.Enabled && preset.Kind == kind)
                {
                    presets.Add(preset);
                }
            }

            return presets;
        }
    }
}
