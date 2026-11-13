using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
namespace ShadowFire
{
    /// Tiny code-built uGUI toolkit (no prefabs, no TMP dependency).
    public static class UiKit
    {
        static Font font; static Sprite circle;
        public static Font Font => font ? font : (font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));

        public static Sprite Circle
        {
            get
            {
                if (circle) return circle;
                const int n = 64;
                var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
                for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(n / 2f - 0.5f, n / 2f - 0.5f)) / (n / 2f);
                    t.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01((1f - d) * 6f)));
                }
                t.Apply();
                return circle = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(.5f, .5f), 100f);
            }
        }

        public static Canvas MakeCanvas(string name, int order)
        {
            var go = new GameObject(name);
            var c = go.AddComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay; c.sortingOrder = order;
            var sc = go.AddComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1920, 1080); sc.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                var e = es.AddComponent<EventSystem>(); e.pixelDragThreshold = 1;
                es.AddComponent<StandaloneInputModule>();
            }
            return c;
        }

        /// pivot == anchor, so pos is the offset of that anchor point from the parent's matching point
        public static RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = anchor; rt.pivot = anchor; rt.anchoredPosition = pos; rt.sizeDelta = size;
            return rt;
        }
        public static RectTransform Stretch(Transform parent, string name, Vector2 aMin, Vector2 aMax)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }
        public static Image Img(Transform p, string name, Vector2 anchor, Vector2 pos, Vector2 size, Color c, bool round = false)
        {
            var i = Rect(p, name, anchor, pos, size).gameObject.AddComponent<Image>();
            i.color = c; if (round) i.sprite = Circle; return i;
        }
        public static Text Txt(Transform p, string s, int fs, Vector2 anchor, Vector2 pos, Vector2 size, TextAnchor ta = TextAnchor.MiddleCenter, Color? col = null)
        {
            var t = Rect(p, "Text", anchor, pos, size).gameObject.AddComponent<Text>();
            t.font = Font; t.text = s; t.fontSize = fs; t.alignment = ta; t.color = col ?? Color.white; t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Overflow; t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }
        public static Button Btn(Transform p, string label, Vector2 anchor, Vector2 pos, Vector2 size, UnityAction onClick, bool round = false)
        {
            var img = Img(p, "Btn_" + label, anchor, pos, size, new Color(.15f, .18f, .25f, .9f), round);
            var b = img.gameObject.AddComponent<Button>(); b.targetGraphic = img;
            if (onClick != null) b.onClick.AddListener(onClick);
            var t = Txt(img.transform, label, 34, new Vector2(.5f, .5f), Vector2.zero, size);
            return b;
        }
        public static Slider Sld(Transform p, Vector2 anchor, Vector2 pos, Vector2 size, float min, float max, float val, UnityAction<float> on)
        {
            var root = Rect(p, "Slider", anchor, pos, size);
            root.gameObject.AddComponent<Image>().color = new Color(1, 1, 1, .15f);
            var fa = Stretch(root, "Fill Area", new Vector2(0, .25f), new Vector2(1, .75f)); fa.offsetMin = new Vector2(5, 0); fa.offsetMax = new Vector2(-15, 0);
            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image)); var fr = fill.GetComponent<RectTransform>();
            fr.SetParent(fa, false); fr.anchorMin = Vector2.zero; fr.anchorMax = new Vector2(0, 1); fr.sizeDelta = new Vector2(10, 0);
            fill.GetComponent<Image>().color = new Color(1f, .45f, .2f);
            var ha = Stretch(root, "Handle Slide Area", Vector2.zero, Vector2.one); ha.offsetMin = new Vector2(10, 0); ha.offsetMax = new Vector2(-10, 0);
            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image)); var hr = handle.GetComponent<RectTransform>();
            hr.SetParent(ha, false); hr.sizeDelta = new Vector2(28, 0); hr.anchorMin = Vector2.zero; hr.anchorMax = new Vector2(0, 1);
            var s = root.gameObject.AddComponent<Slider>();
            s.fillRect = fr; s.handleRect = hr; s.targetGraphic = handle.GetComponent<Image>(); s.direction = Slider.Direction.LeftToRight;
            s.minValue = min; s.maxValue = max; s.value = val;
            if (on != null) s.onValueChanged.AddListener(on);
            return s;
        }
    }
}
