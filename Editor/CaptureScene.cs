using System.Collections.Generic;
using UnityEngine;

namespace JTLStudio.SDK.Capture
{
    public class CaptureScene
    {
        private readonly List<GameObject> _hidden = new List<GameObject>();
        private readonly Dictionary<Camera, int> _masks = new Dictionary<Camera, int>();

        public void Hide(CaptureSettings settings)
        {
            Restore();

            foreach (string name in settings.HiddenObjects)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    continue;
                }

                GameObject found = GameObject.Find(name.Trim());

                if (found != null && found.activeSelf)
                {
                    found.SetActive(false);
                    _hidden.Add(found);
                }
            }

            int layers = settings.HiddenLayers.value;

            if (layers == 0)
            {
                return;
            }

            foreach (Camera camera in Camera.allCameras)
            {
                _masks[camera] = camera.cullingMask;
                camera.cullingMask &= ~layers;
            }
        }

        public void Restore()
        {
            foreach (GameObject hidden in _hidden)
            {
                if (hidden != null)
                {
                    hidden.SetActive(true);
                }
            }

            _hidden.Clear();

            foreach (KeyValuePair<Camera, int> mask in _masks)
            {
                if (mask.Key != null)
                {
                    mask.Key.cullingMask = mask.Value;
                }
            }

            _masks.Clear();
        }
    }
}
