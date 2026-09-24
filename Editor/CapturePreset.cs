using System;
using UnityEngine;

namespace JTLStudio.SDK.Capture
{
    public enum CaptureKind
    {
        Screenshot = 0,
        Video = 1
    }

    [Serializable]
    public class CapturePreset
    {
        [SerializeField] private string _group = "";
        [SerializeField] private string _name = "";
        [SerializeField] private int _width = 1920;
        [SerializeField] private int _height = 1080;
        [SerializeField] private CaptureKind _kind = CaptureKind.Screenshot;
        [SerializeField] private bool _enabled = true;

        public CapturePreset(string group, string name, int width, int height, CaptureKind kind)
        {
            _group = group;
            _name = name;
            _width = width;
            _height = height;
            _kind = kind;
        }

        public string Group
        {
            get => _group;
            set => _group = value;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public int Width
        {
            get => _width;
            set => _width = Mathf.Clamp(value, 16, 8192);
        }

        public int Height
        {
            get => _height;
            set => _height = Mathf.Clamp(value, 16, 8192);
        }

        public CaptureKind Kind
        {
            get => _kind;
            set => _kind = value;
        }

        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        public string Title => _group + " · " + _name + " · " + _width + "x" + _height;

        public string FileName => Sanitize(_group) + "_" + Sanitize(_name) + "_" + _width + "x" + _height;

        private string Sanitize(string value)
        {
            string result = "";

            foreach (char symbol in value.ToLowerInvariant())
            {
                result += char.IsLetterOrDigit(symbol) && symbol < 128 ? symbol.ToString() : "";
            }

            return result.Length == 0 ? "preset" : result;
        }
    }
}
