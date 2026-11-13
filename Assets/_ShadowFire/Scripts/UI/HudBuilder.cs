using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ShadowFire
{
    public static class KillFeed
    {
        public static event System.Action<string> OnEntry;
        public static void Push(string s) { OnEntry?.Invoke(s); }
    }

    /// Builds the whole mobile HUD in code: joystick L, fire L, look-drag R half, action buttons R,
    /// minimap + compass, kill feed, zone timer, hp/ammo, crosshair + hit marker.
    public class HudBuilder : MonoBehaviour
    {
        public PlayerController player; public ZoneController zone;
        WeaponSystem weapon; Health hp; Camera miniCam;
        Text ammoTxt, zoneTxt, compassTxt, feedTxt; RectTransform hpFill; Image cross; float hitT;
        readonly List<KeyValuePair<string, float>> feed = new List<KeyValuePair<string, float>>();
        static readonly string[] Dir = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

        void Start()
        {
            weapon = player.GetComponent<WeaponSystem>(); hp = player.GetComponent<Health>();
            KillFeed.OnEntry += AddFeed; WeaponSystem.OnHit += OnHit;
            Build();
        }
        void OnDestroy() { KillFeed.OnEntry -= AddFeed; WeaponSystem.OnHit -= OnHit; }
        void AddFeed(string s) { feed.Add(new KeyValuePair<string, float>(s, Time.time + 5f)); if (feed.Count > 5) feed.RemoveAt(0); }
        void OnHit(bool head) { hitT = 0.12f; cross.color = head ? Color.red : new Color(1f, .6f, .2f); }

        static Vector2 RB(float x, float y) { return new Vector2(-x, y); }

        Image Hold(Transform cv, string label, Vector2 anchor, Vector2 pos, float size, System.Action<bool> act, Color c)
        {
            var img = UiKit.Img(cv, label, anchor, pos, new Vector2(size, size), c, true);
            img.gameObject.AddComponent<HoldButton>().OnHold = act;
            UiKit.Txt(img.transform, label, 28, new Vector2(.5f, .5f), Vector2.zero, new Vector2(size, size));
            return img;
        }
        void Tap(Transform cv, string label, Vector2 pos, System.Action a)
        {
            Hold(cv, label, new Vector2(1, 0), pos, 120f, h => { if (h) a(); }, new Color(.15f, .18f, .25f, .7f));
        }

        void Build()
        {
            var cv = UiKit.MakeCanvas("HUD", 0).transform;
            var look = UiKit.Stretch(cv, "LookArea", new Vector2(.5f, 0), Vector2.one);
            look.gameObject.AddComponent<Image>().color = Color.clear; look.gameObject.AddComponent<TouchLook>();

            // left: joystick + fire
            var jbg = UiKit.Img(cv, "JoyBg", Vector2.zero, new Vector2(70, 70), new Vector2(280, 280), new Color(1, 1, 1, .18f), true);
            var knob = UiKit.Img(jbg.transform, "Knob", new Vector2(.5f, .5f), Vector2.zero, new Vector2(120, 120), new Color(1, 1, 1, .5f), true);
            knob.raycastTarget = false;
            var js = jbg.gameObject.AddComponent<VirtualJoystick>(); js.bg = jbg.rectTransform; js.knob = knob.rectTransform;
            Hold(cv, "FIRE", Vector2.zero, new Vector2(380, 190), 190f, h => GameInput.Fire = h, new Color(1f, .3f, .2f, .55f));

            // right: action grid
            Tap(cv, "JUMP", RB(60, 60), () => GameInput.Jump = true);
            Tap(cv, "CROUCH", RB(200, 60), () => GameInput.CrouchToggle = true);
            Tap(cv, "PRONE", RB(340, 60), () => GameInput.ProneToggle = true);
            Tap(cv, "RELOAD", RB(60, 200), () => GameInput.Reload = true);
            Tap(cv, "AIM", RB(200, 200), () => GameInput.AdsToggle = true);
            Tap(cv, "COVER", RB(340, 200), () => GameInput.Cover = true);
            Tap(cv, "SKILL", RB(60, 340), () => GameInput.Ability = true);
            Tap(cv, "HEAL", RB(200, 340), () => GameInput.Heal = true);

            // crosshair
            cross = UiKit.Img(cv, "Cross", new Vector2(.5f, .5f), Vector2.zero, new Vector2(10, 10), Color.white, true); cross.raycastTarget = false;

            // minimap + compass
            var rt = new RenderTexture(256, 256, 16);
            var mc = new GameObject("MinimapCam").AddComponent<Camera>();
            mc.orthographic = true; mc.orthographicSize = 70f; mc.targetTexture = rt; mc.clearFlags = CameraClearFlags.SolidColor;
            mc.backgroundColor = new Color(.05f, .1f, .15f); mc.farClipPlane = 500f; mc.depth = -10f;
            mc.transform.rotation = Quaternion.Euler(90f, 0f, 0f); miniCam = mc;
            var mm = UiKit.Rect(cv, "Minimap", new Vector2(0, 1), new Vector2(30, -30), new Vector2(260, 260));
            mm.gameObject.AddComponent<RawImage>().texture = rt;
            UiKit.Img(mm, "Me", new Vector2(.5f, .5f), Vector2.zero, new Vector2(14, 14), Color.white, true);
            compassTxt = UiKit.Txt(cv, "N", 36, new Vector2(.5f, 1), new Vector2(0, -20), new Vector2(300, 50));
            zoneTxt = UiKit.Txt(cv, "", 30, new Vector2(.5f, 1), new Vector2(0, -70), new Vector2(800, 50));
            feedTxt = UiKit.Txt(cv, "", 26, new Vector2(1, 1), new Vector2(-140, -30), new Vector2(520, 200), TextAnchor.UpperRight);
            UiKit.Btn(cv, "II", new Vector2(1, 1), new Vector2(-30, -30), new Vector2(90, 90), SettingsPanel.Toggle, true);

            // hp + ammo
            UiKit.Img(cv, "HpBg", new Vector2(.5f, 0), new Vector2(0, 40), new Vector2(400, 20), new Color(0, 0, 0, .6f));
            hpFill = UiKit.Img(cv, "HpFill", new Vector2(.5f, 0), new Vector2(0, 40), new Vector2(400, 20), new Color(.3f, .85f, .4f)).rectTransform;
            ammoTxt = UiKit.Txt(cv, "", 40, new Vector2(.5f, 0), new Vector2(0, 70), new Vector2(500, 60));
        }

        string Fmt(float s) { int t = Mathf.CeilToInt(Mathf.Max(0, s)); return (t / 60) + ":" + (t % 60).ToString("00"); }

        void Update()
        {
            if (hp == null || ammoTxt == null) return;
            hpFill.sizeDelta = new Vector2(400f * Mathf.Clamp01(hp.hp / hp.maxHp), 20f);
            hpFill.GetComponent<Image>().color = hp.Downed ? Color.red : new Color(.3f, .85f, .4f);
            if (weapon && weapon.data)
                ammoTxt.text = weapon.Reloading ? "RELOADING" : weapon.Ammo + " / " + weapon.MagSize + "   +" + weapon.Reserve;
            float yaw = ((player.yaw % 360f) + 360f) % 360f;
            compassTxt.text = Dir[Mathf.RoundToInt(yaw / 45f) % 8] + "  " + Mathf.RoundToInt(yaw) + "°";
            if (zone)
            {
                string st = zone.Finished ? "FINAL ZONE" : zone.Shrinking ? "ZONE CLOSING " + Fmt(zone.TimeLeft) : "ZONE CLOSES IN " + Fmt(zone.TimeLeft);
                bool outside = zone.IsOutside(player.transform.position);
                zoneTxt.text = st + "   " + zone.Dps + " dmg/s" + (outside ? "   ! OUTSIDE ZONE" : "");
                zoneTxt.color = outside ? Color.red : Color.white;
            }
            feed.RemoveAll(e => e.Value < Time.time);
            feedTxt.text = string.Join("\n", feed.ConvertAll(e => e.Key));
            if (hitT > 0f) { hitT -= Time.deltaTime; if (hitT <= 0f) cross.color = Color.white; }
        }
        void LateUpdate()
        {
            if (miniCam && player) miniCam.transform.position = new Vector3(player.transform.position.x, 200f, player.transform.position.z);
        }
    }
}
