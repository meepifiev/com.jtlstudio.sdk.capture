using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace JTLStudio.SDK.Capture.Tests
{
    public class CaptureTests
    {
        [Test]
        public void EveryResolutionHasALabel()
        {
            Assert.AreEqual(CaptureResolutions.Sizes.Length + 1, CaptureResolutions.Labels.Length);
            Assert.AreEqual("Custom", CaptureResolutions.Labels[CaptureResolutions.CustomIndex]);
        }

        [Test]
        public void CustomIndexTakesTheCustomSize()
        {
            Vector2Int custom = new Vector2Int(640, 360);

            Assert.AreEqual(new Vector2Int(1920, 1080), CaptureResolutions.Size(0, custom));
            Assert.AreEqual(custom, CaptureResolutions.Size(CaptureResolutions.CustomIndex, custom));
        }

        [Test]
        public void CustomSizeIsClamped()
        {
            CaptureSettings settings = CaptureSettings.instance;
            int index = settings.ResolutionIndex;
            Vector2Int size = settings.CustomSize;

            try
            {
                settings.CustomSize = new Vector2Int(100000, 1);

                Assert.AreEqual(8192, settings.CustomSize.x);
                Assert.AreEqual(16, settings.CustomSize.y);
            }
            finally
            {
                settings.CustomSize = size;
                settings.ResolutionIndex = index;
            }
        }

        [Test]
        public void FileNameHasSizeAndLanguage()
        {
            string name = CaptureOutput.Name(new Vector2Int(1080, 1920), Language.Russian, "png");

            StringAssert.Contains("1080x1920", name);
            StringAssert.Contains("_ru_", name);
            StringAssert.EndsWith(".png", name);
        }

        [Test]
        public void ResolvedFolderIsNotCreatedUntilItIsUsed()
        {
            CaptureSettings settings = CaptureSettings.instance;
            string path = settings.OutputPath;

            try
            {
                settings.OutputPath = "Captures/JTLSDKTestFolder";
                string resolved = CaptureOutput.Resolve(settings);

                Assert.IsFalse(Directory.Exists(resolved));
                Assert.AreEqual(resolved, CaptureOutput.Folder(settings));
                Assert.IsTrue(Directory.Exists(resolved));
                Directory.Delete(resolved);
            }
            finally
            {
                settings.OutputPath = path;
            }
        }

        [Test]
        public void ManualRecordingIsTheDefault()
        {
            Assert.AreEqual(RecordMode.Manual, default(RecordMode));
        }

        [Test]
        public void RecordingModeIsStored()
        {
            CaptureSettings settings = CaptureSettings.instance;
            RecordMode mode = settings.RecordMode;
            bool exit = settings.ExitPlayMode;

            try
            {
                settings.RecordMode = RecordMode.Duration;
                settings.ExitPlayMode = true;

                Assert.AreEqual(RecordMode.Duration, settings.RecordMode);
                Assert.IsTrue(settings.ExitPlayMode);
            }
            finally
            {
                settings.RecordMode = mode;
                settings.ExitPlayMode = exit;
            }
        }

        [Test]
        public void ConfiguredLanguagesAreNeverEmpty()
        {
            Assert.Greater(CaptureLanguages.Configured().Count, 0);
        }

        [Test]
        public void CustomSwitchIsEnoughToSwitchWithoutTheSdk()
        {
            Assert.AreEqual(JTLSDK.IsCreated, CaptureLanguages.CanSwitch);

            CaptureLanguages.Switch = language => { };

            Assert.IsTrue(CaptureLanguages.CanSwitch);

            CaptureLanguages.Switch = null;
        }

        [Test]
        public void CustomSwitchReplacesTheSdk()
        {
            Language captured = Language.English;
            CaptureLanguages.Switch = language => captured = language;

            CaptureLanguages.Apply(Language.Turkish);
            CaptureLanguages.Switch = null;

            Assert.AreEqual(Language.Turkish, captured);
        }
    }
}
