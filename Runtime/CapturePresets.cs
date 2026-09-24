#if UNITY_EDITOR
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
                new CapturePreset(Yandex, "Скриншот горизонтальный", "yandex_landscape", 1920, 1080, CaptureKind.Screenshot),
                new CapturePreset(Yandex, "Скриншот вертикальный", "yandex_portrait", 1080, 1920, CaptureKind.Screenshot),
                new CapturePreset(Yandex, "Ролик", "yandex_video", 1920, 1080, CaptureKind.Video),
                new CapturePreset(YouTube, "Скриншот", "youtube_landscape", 1920, 1080, CaptureKind.Screenshot),
                new CapturePreset(GooglePlay, "Телефон", "googleplay_phone", 1080, 1920, CaptureKind.Screenshot),
                new CapturePreset(GooglePlay, "Планшет", "googleplay_tablet", 1600, 2560, CaptureKind.Screenshot),
                new CapturePreset(GooglePlay, "Ролик", "googleplay_video", 1920, 1080, CaptureKind.Video),
                new CapturePreset(AppStore, "iPhone 6.7", "appstore_iphone67", 1290, 2796, CaptureKind.Screenshot),
                new CapturePreset(AppStore, "iPhone 5.5", "appstore_iphone55", 1242, 2208, CaptureKind.Screenshot),
                new CapturePreset(AppStore, "iPad 12.9", "appstore_ipad129", 2048, 2732, CaptureKind.Screenshot),
                new CapturePreset(RuStore, "Телефон", "rustore_phone", 1080, 1920, CaptureKind.Screenshot),
                new CapturePreset(RuStore, "Ролик", "rustore_video", 1920, 1080, CaptureKind.Video)
            };
        }
    }
}
#endif
