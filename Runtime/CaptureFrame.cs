#if UNITY_EDITOR
using UnityEngine;

namespace JTLStudio.SDK.Capture
{
    public static class CaptureFrame
    {
        public static Camera Find(string name)
        {
            if (string.IsNullOrEmpty(name) == false)
            {
                foreach (Camera camera in Camera.allCameras)
                {
                    if (camera.name == name)
                    {
                        return camera;
                    }
                }
            }

            return Camera.main != null ? Camera.main : (Camera.allCameras.Length > 0 ? Camera.allCameras[0] : null);
        }

        public static Texture2D Grab(CaptureSettings settings, Vector2Int size)
        {
            if (settings.Source == CaptureSource.Camera)
            {
                Camera camera = Find(settings.CameraName);

                if (camera != null)
                {
                    return FromCamera(camera, size);
                }
            }

            Texture2D screen = ScreenCapture.CaptureScreenshotAsTexture();
            Texture2D fitted = CaptureImage.Fit(screen, size.x, size.y);

            if (fitted != screen)
            {
                Object.DestroyImmediate(screen);
            }

            return fitted;
        }

        private static Texture2D FromCamera(Camera camera, Vector2Int size)
        {
            Canvas.ForceUpdateCanvases();
            RenderTexture target = RenderTexture.GetTemporary(size.x, size.y, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;

            camera.targetTexture = target;
            camera.Render();
            camera.targetTexture = previousTarget;

            RenderTexture.active = target;
            Texture2D result = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0f, 0f, size.x, size.y), 0, 0);
            result.Apply(false);
            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(target);
            return result;
        }
    }
}
#endif
