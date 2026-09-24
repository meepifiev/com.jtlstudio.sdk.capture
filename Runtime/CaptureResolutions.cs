#if UNITY_EDITOR
using UnityEngine;

namespace JTLStudio.SDK.Capture
{
    public static class CaptureResolutions
    {
        public static readonly Vector2Int[] Sizes =
        {
            new Vector2Int(1920, 1080),
            new Vector2Int(1280, 720),
            new Vector2Int(1080, 1920),
            new Vector2Int(720, 1280),
            new Vector2Int(1600, 2560),
            new Vector2Int(1290, 2796),
            new Vector2Int(1242, 2208),
            new Vector2Int(2048, 2732),
            new Vector2Int(1024, 1024)
        };

        public static readonly string[] Labels =
        {
            "1920 x 1080 · 16:9",
            "1280 x 720 · 16:9",
            "1080 x 1920 · 9:16",
            "720 x 1280 · 9:16",
            "1600 x 2560 · 5:8",
            "1290 x 2796 · 9:19.5",
            "1242 x 2208 · 9:16",
            "2048 x 2732 · 3:4",
            "1024 x 1024 · 1:1",
            "Custom"
        };

        public static int CustomIndex => Labels.Length - 1;

        public static Vector2Int Size(int index, Vector2Int custom)
        {
            return index >= 0 && index < Sizes.Length ? Sizes[index] : custom;
        }
    }
}
#endif
