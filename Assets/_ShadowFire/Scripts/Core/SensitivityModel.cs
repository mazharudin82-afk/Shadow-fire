using UnityEngine;
namespace ShadowFire
{
    public enum AimAssistLevel { Off, Low, Med, High }

    [System.Serializable]
    public class SensitivitySettings
    {
        public float hip = 1.0f, ads = 0.7f, scoped = 0.4f, gyro = 1.0f;
        public float exponent = 1.35f;           // 1 = linear; >1 = flicks get extra gain
        public AimAssistLevel assist = AimAssistLevel.Med;
        public bool autoFire = false, gyroEnabled = false;
    }

    public static class SensitivityModel
    {
        public enum Mode { Hip, Ads, Scoped }
        const string Key = "sf_sens_v1";
        public static SensitivitySettings S = Load();
        public const float DegPerInch = 90f;

        static SensitivitySettings Load()
        {
            var j = PlayerPrefs.GetString(Key, "");
            return string.IsNullOrEmpty(j) ? new SensitivitySettings() : JsonUtility.FromJson<SensitivitySettings>(j);
        }
        public static void Save() { PlayerPrefs.SetString(Key, JsonUtility.ToJson(S)); PlayerPrefs.Save(); }
        public static void Reset() { S = new SensitivitySettings(); Save(); }

        /// drag px (this frame) -> degrees. DPI-normalised, per-mode + per-gun scaled,
        /// exponent curve on drag speed: slow drags precise, flicks travel further.
        public static Vector2 ToDegrees(Vector2 dragPx, float dt, Mode m, float gunScale)
        {
            float mult = m == Mode.Hip ? S.hip : m == Mode.Ads ? S.ads : S.scoped;
            float dpi = Screen.dpi > 0 ? Screen.dpi : 320f;
            Vector2 inches = dragPx / dpi;
            float speed = dt > 0f ? inches.magnitude / dt : 0f;
            float gain = Mathf.Clamp(Mathf.Pow(Mathf.Max(speed, 0.001f) / 3f, S.exponent - 1f), 0.6f, 3f);
            return inches * DegPerInch * mult * gunScale * gain;
        }
    }
}
