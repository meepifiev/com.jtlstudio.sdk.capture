using UnityEditor;

namespace JTLStudio.SDK.Capture
{
    [InitializeOnLoad]
    public static class CapturePending
    {
        private const string Key = "JTLSDK.Capture.Pending";

        static CapturePending()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        public static void Request()
        {
            SessionState.SetBool(Key, true);
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                SessionState.SetBool(Key, false);
                return;
            }

            if (change != PlayModeStateChange.EnteredPlayMode || SessionState.GetBool(Key, false) == false)
            {
                return;
            }

            SessionState.SetBool(Key, false);
            CaptureRuntime.Instance.RunWhenReady();
        }
    }
}
