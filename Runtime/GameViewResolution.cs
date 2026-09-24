#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace JTLStudio.SDK.Capture
{
    public static class GameViewResolution
    {
        private const string Label = "JTL SDK Capture";

        private static int _previousIndex = -1;
        private static int _customIndex = -1;

        public static string Error { get; private set; } = "";

        public static bool Apply(int width, int height)
        {
            Error = "";

            try
            {
                Assembly assembly = typeof(UnityEditor.Editor).Assembly;
                Type sizesType = assembly.GetType("UnityEditor.GameViewSizes");
                Type sizeType = assembly.GetType("UnityEditor.GameViewSize");
                Type sizeKindType = assembly.GetType("UnityEditor.GameViewSizeType");
                Type gameViewType = assembly.GetType("UnityEditor.GameView");
                Type singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);

                object sizes = singletonType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                object group = sizesType.GetProperty("currentGroup", BindingFlags.Public | BindingFlags.Instance).GetValue(sizes);
                Type groupType = group.GetType();

                Remove(groupType, group);

                object size = sizeType
                    .GetConstructor(new[] { sizeKindType, typeof(int), typeof(int), typeof(string) })
                    .Invoke(new[] { Enum.Parse(sizeKindType, "FixedResolution"), width, height, (object)Label });

                groupType.GetMethod("AddCustomSize").Invoke(group, new[] { size });
                _customIndex = (int)groupType.GetMethod("GetTotalCount").Invoke(group, null) - 1;

                EditorWindow gameView = EditorWindow.GetWindow(gameViewType, false, null, false);
                PropertyInfo selected = gameViewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (_previousIndex < 0)
                {
                    _previousIndex = (int)selected.GetValue(gameView);
                }

                MethodInfo selection = gameViewType.GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                selection.Invoke(gameView, new object[] { _customIndex, null });
                gameView.Repaint();
                return true;
            }
            catch (Exception exception)
            {
                Error = exception.Message;
                return false;
            }
        }

        public static void Restore()
        {
            try
            {
                Assembly assembly = typeof(UnityEditor.Editor).Assembly;
                Type sizesType = assembly.GetType("UnityEditor.GameViewSizes");
                Type gameViewType = assembly.GetType("UnityEditor.GameView");
                Type singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                object sizes = singletonType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                object group = sizesType.GetProperty("currentGroup", BindingFlags.Public | BindingFlags.Instance).GetValue(sizes);

                if (_previousIndex >= 0)
                {
                    EditorWindow gameView = EditorWindow.GetWindow(gameViewType, false, null, false);
                    MethodInfo selection = gameViewType.GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    selection.Invoke(gameView, new object[] { _previousIndex, null });
                    gameView.Repaint();
                    _previousIndex = -1;
                }

                Remove(group.GetType(), group);
            }
            catch (Exception exception)
            {
                Error = exception.Message;
            }
        }

        public static Vector2 Current()
        {
            return Handles.GetMainGameViewSize();
        }

        private static void Remove(Type groupType, object group)
        {
            if (_customIndex < 0)
            {
                return;
            }

            MethodInfo remove = groupType.GetMethod("RemoveCustomSize");

            if (remove != null)
            {
                remove.Invoke(group, new object[] { _customIndex });
            }

            _customIndex = -1;
        }
    }
}
#endif
