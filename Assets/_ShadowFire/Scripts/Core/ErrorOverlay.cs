using System.Collections.Generic;
using UnityEngine;
namespace ShadowFire
{
    /// Auto-created in every scene (editor + device builds). Shows runtime errors/exceptions on screen
    /// in red so you can screenshot them. Tap the red box to clear.
    public class ErrorOverlay : MonoBehaviour
    {
        static ErrorOverlay inst;
        readonly List<string> lines = new List<string>();
        GUIStyle style;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (inst) return;
            var go = new GameObject("ErrorOverlay"); DontDestroyOnLoad(go); inst = go.AddComponent<ErrorOverlay>();
        }
        void OnEnable() { Application.logMessageReceived += OnLog; }
        void OnDisable() { Application.logMessageReceived -= OnLog; }

        void OnLog(string msg, string stack, LogType t)
        {
            if (t != LogType.Error && t != LogType.Exception && t != LogType.Assert) return;
            string first = stack != null ? stack.Split('\n')[0] : "";
            lines.Add(t + ": " + msg + (first.Length > 0 ? "\n   at " + first : ""));
            if (lines.Count > 6) lines.RemoveAt(0);
        }

        void OnGUI()
        {
            if (lines.Count == 0) return;
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.box) { alignment = TextAnchor.UpperLeft, wordWrap = true, richText = false };
                style.normal.textColor = Color.white;
                var tex = new Texture2D(1, 1); tex.SetPixel(0, 0, new Color(.6f, 0f, 0f, .85f)); tex.Apply();
                style.normal.background = tex;
            }
            style.fontSize = Mathf.Max(14, Screen.height / 45);
            var r = new Rect(10, Screen.height * 0.25f, Screen.width - 20, Screen.height * 0.5f);
            if (GUI.Button(r, "ERRORS (tap to clear)\n\n" + string.Join("\n\n", lines), style)) lines.Clear();
        }
    }
}
