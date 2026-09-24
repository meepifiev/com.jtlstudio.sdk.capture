using System.Collections.Generic;
using NUnit.Framework;

namespace JTLStudio.SDK.Capture.Tests
{
    public class CaptureTests
    {
        [Test]
        public void DefaultPresetsCoverEveryStore()
        {
            List<CapturePreset> presets = CapturePresets.Defaults();
            List<string> groups = new List<string>();

            foreach (CapturePreset preset in presets)
            {
                if (groups.Contains(preset.Group) == false)
                {
                    groups.Add(preset.Group);
                }
            }

            Assert.Contains(CapturePresets.Yandex, groups);
            Assert.Contains(CapturePresets.YouTube, groups);
            Assert.Contains(CapturePresets.GooglePlay, groups);
            Assert.Contains(CapturePresets.AppStore, groups);
            Assert.Contains(CapturePresets.RuStore, groups);
        }

        [Test]
        public void DefaultsHaveNoPromoBanners()
        {
            foreach (CapturePreset preset in CapturePresets.Defaults())
            {
                Assert.AreNotEqual(470, preset.Height, preset.Name);
                Assert.AreNotEqual(500, preset.Height, preset.Name);
            }
        }

        [Test]
        public void PresetSizeIsClamped()
        {
            CapturePreset preset = new CapturePreset("Тест", "Кадр", "test_frame", 1920, 1080, CaptureKind.Screenshot);

            preset.Width = 100000;
            preset.Height = 1;

            Assert.AreEqual(8192, preset.Width);
            Assert.AreEqual(16, preset.Height);
        }

        [Test]
        public void FileNameHasPresetSizeAndLanguage()
        {
            CapturePreset preset = new CapturePreset(CapturePresets.Yandex, "Скриншот", "yandex_landscape", 1920, 1080, CaptureKind.Screenshot);

            string name = CaptureOutput.Name(preset, Language.Russian, "png");

            StringAssert.Contains("yandexlandscape_1920x1080", name);
            StringAssert.Contains("_ru_", name);
            StringAssert.EndsWith(".png", name);
        }

        [Test]
        public void ConfiguredLanguagesAreNeverEmpty()
        {
            Assert.Greater(CaptureLanguages.Configured().Count, 0);
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
