#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace JTLStudio.SDK.Capture
{
    public class CaptureCanvases
    {
        private readonly List<Canvas> _changed = new List<Canvas>();
        private readonly List<Camera> _cameras = new List<Camera>();
        private readonly List<float> _distances = new List<float>();

        public void Attach(Camera camera)
        {
            Restore();

            if (camera == null)
            {
                return;
            }

            float distance = Mathf.Clamp(camera.nearClipPlane + 0.5f, camera.nearClipPlane + 0.01f, camera.farClipPlane - 0.01f);

            foreach (Canvas canvas in Object.FindObjectsOfType<Canvas>())
            {
                if (canvas.isRootCanvas == false || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    continue;
                }

                _changed.Add(canvas);
                _cameras.Add(canvas.worldCamera);
                _distances.Add(canvas.planeDistance);
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = distance;
            }
        }

        public void Restore()
        {
            for (int index = 0; index < _changed.Count; index++)
            {
                Canvas canvas = _changed[index];

                if (canvas == null)
                {
                    continue;
                }

                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.worldCamera = _cameras[index];
                canvas.planeDistance = _distances[index];
            }

            _changed.Clear();
            _cameras.Clear();
            _distances.Clear();
        }
    }
}
#endif
