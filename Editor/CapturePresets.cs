using System.Collections.Generic;

namespace JTLStudio.SDK.Capture
{
    public static class CapturePresets
    {
        public const string Yandex = "Яндекс Игры";
        public const string YouTube = "YouTube Playables";
        public const string GooglePlay = "Google Play";
        public const string AppStore = "App Store";
        public const string RuStore = "RuStore";

        public static List<CapturePreset> Defaults()
        {
            return new List<CapturePreset>
            {
                new CapturePreset(Yandex, "Обложка", 800, 470, CaptureKind.Screenshot),
                new CapturePreset(Yandex, "Скриншот горизонтальный", 1920, 1080, CaptureKind.Screenshot),
                new CapturePreset(Yandex, "Скриншот вертикальный", 1080, 1920, CaptureKind.Screenshot),
                new CapturePreset(Yandex, "Ролик", 1920, 1080, CaptureKind.Video),
                new CapturePreset(YouTube, "Скриншот", 1920, 1080, CaptureKind.Screenshot),
                new CapturePreset(GooglePlay, "Баннер магазина", 1024, 500, CaptureKind.Screenshot),
                new CapturePreset(GooglePlay, "Телефон", 1080, 1920, CaptureKind.Screenshot),
                new CapturePreset(GooglePlay, "Планшет", 1600, 2560, CaptureKind.Screenshot),
                new CapturePreset(GooglePlay, "Ролик", 1920, 1080, CaptureKind.Video),
                new CapturePreset(AppStore, "iPhone 6.7", 1290, 2796, CaptureKind.Screenshot),
                new CapturePreset(AppStore, "iPhone 5.5", 1242, 2208, CaptureKind.Screenshot),
                new CapturePreset(AppStore, "iPad 12.9", 2048, 2732, CaptureKind.Screenshot),
                new CapturePreset(RuStore, "Телефон", 1080, 1920, CaptureKind.Screenshot),
                new CapturePreset(RuStore, "Ролик", 1920, 1080, CaptureKind.Video)
            };
        }
    }
}
