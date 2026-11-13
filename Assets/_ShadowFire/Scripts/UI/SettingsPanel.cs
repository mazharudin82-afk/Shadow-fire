using UnityEngine;
using UnityEngine.UI;
namespace ShadowFire
{
    /// Settings page wired directly to SensitivityModel.S (live), saved on close.
    public static class SettingsPanel
    {
        static GameObject root; static float y;
        static readonly int[] Fps = { 30, 60, 90, 120 };

        public static void Toggle() { if (root) Close(); else Open(); }
        public static void Close() { SensitivityModel.Save(); if (root) Object.Destroy(root); root = null; }

        static void Row(Transform p, string label, float min, float max, float val, System.Action<float> set)
        {
            UiKit.Txt(p, label, 32, new Vector2(.5f, 1), new Vector2(-520, y), new Vector2(360, 50), TextAnchor.MiddleLeft);
            var vt = UiKit.Txt(p, val.ToString("0.00"), 32, new Vector2(.5f, 1), new Vector2(480, y), new Vector2(160, 50));
            UiKit.Sld(p, new Vector2(.5f, 1), new Vector2(0, y - 5), new Vector2(560, 40), min, max, val, v => { set(v); vt.text = v.ToString("0.00"); });
            y -= 75f;
        }
        static Text CycleBtn(Transform p, string txt, System.Action onClick)
        {
            var b = UiKit.Btn(p, txt, new Vector2(.5f, 1), new Vector2(0, y), new Vector2(620, 64), onClick);
            y -= 80f; return b.GetComponentInChildren<Text>();
        }

        public static void Open()
        {
            var S = SensitivityModel.S;
            var c = UiKit.MakeCanvas("Settings", 100); root = c.gameObject;
            var bg = UiKit.Stretch(c.transform, "Bg", Vector2.zero, Vector2.one);
            bg.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, .9f);
            UiKit.Txt(bg, "SETTINGS", 54, new Vector2(.5f, 1), new Vector2(0, -20), new Vector2(600, 80));
            y = -120f;
            Row(bg, "Hip-fire sens", .1f, 3f, S.hip, v => S.hip = v);
            Row(bg, "ADS sens", .1f, 3f, S.ads, v => S.ads = v);
            Row(bg, "Scoped sens", .1f, 3f, S.scoped, v => S.scoped = v);
            Row(bg, "Gyro sens", .1f, 3f, S.gyro, v => S.gyro = v);
            Row(bg, "Curve exponent", 1f, 2f, S.exponent, v => S.exponent = v);

            Text a = null; a = CycleBtn(bg, "Aim assist: " + S.assist, () => { S.assist = (AimAssistLevel)(((int)S.assist + 1) % 4); a.text = "Aim assist: " + S.assist; });
            Text f = null; f = CycleBtn(bg, "Auto-fire: " + (S.autoFire ? "ON" : "OFF"), () => { S.autoFire = !S.autoFire; f.text = "Auto-fire: " + (S.autoFire ? "ON" : "OFF"); });
            Text g = null; g = CycleBtn(bg, "Gyro: " + (S.gyroEnabled ? "ON" : "OFF"), () => { S.gyroEnabled = !S.gyroEnabled; g.text = "Gyro: " + (S.gyroEnabled ? "ON" : "OFF"); if (S.gyroEnabled && SystemInfo.supportsGyroscope) Input.gyro.enabled = true; });
            Text q = null; q = CycleBtn(bg, "Graphics: " + QualitySettings.names[QualitySettings.GetQualityLevel()], () =>
            {
                QualitySettings.SetQualityLevel((QualitySettings.GetQualityLevel() + 1) % QualitySettings.names.Length, true);
                q.text = "Graphics: " + QualitySettings.names[QualitySettings.GetQualityLevel()];
            });
            Text fp = null; fp = CycleBtn(bg, "FPS cap: " + Application.targetFrameRate, () =>
            {
                int i = System.Array.IndexOf(Fps, Application.targetFrameRate); Application.targetFrameRate = Fps[(i + 1) % Fps.Length];
                fp.text = "FPS cap: " + Application.targetFrameRate;
            });
            UiKit.Btn(bg, "RESET", new Vector2(.5f, 0), new Vector2(-190, 40), new Vector2(340, 80), () => { SensitivityModel.Reset(); Close(); Open(); });
            UiKit.Btn(bg, "SAVE & CLOSE", new Vector2(.5f, 0), new Vector2(190, 40), new Vector2(340, 80), Close);
        }
    }
}
