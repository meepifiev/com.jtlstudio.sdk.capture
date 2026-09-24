using System.IO;
using UnityEngine;

namespace JTLStudio.SDK.Capture
{
    public static class CaptureImage
    {
        public static Texture2D Fit(Texture2D source, int width, int height)
        {
            if (source.width == width && source.height == height)
            {
                return source;
            }

            float sourceAspect = source.width / (float)source.height;
            float targetAspect = width / (float)height;
            int cropWidth = source.width;
            int cropHeight = source.height;

            if (sourceAspect > targetAspect)
            {
                cropWidth = Mathf.RoundToInt(source.height * targetAspect);
            }
            else
            {
                cropHeight = Mathf.RoundToInt(source.width / targetAspect);
            }

            int x = (source.width - cropWidth) / 2;
            int y = (source.height - cropHeight) / 2;
            Texture2D cropped = new Texture2D(cropWidth, cropHeight, TextureFormat.RGBA32, false);
            cropped.SetPixels(source.GetPixels(x, y, cropWidth, cropHeight));
            cropped.Apply(false);
            Texture2D result = Resize(cropped, width, height);
            Object.DestroyImmediate(cropped);
            return result;
        }

        public static Texture2D Resize(Texture2D source, int width, int height)
        {
            RenderTexture target = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            RenderTexture previous = RenderTexture.active;
            source.filterMode = FilterMode.Bilinear;
            Graphics.Blit(source, target);
            RenderTexture.active = target;
            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
            result.Apply(false);
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            return result;
        }

        public static void WritePng(Texture2D texture, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, texture.EncodeToPNG());
        }
    }
}
