using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace ShadowFire
{
    /// Procedural greybox primitives with pipeline-correct (URP) materials.
    public static class Prim
    {
        static readonly Dictionary<string, Material> cache = new Dictionary<string, Material>();
        static Shader Sh()
        {
            var rp = GraphicsSettings.currentRenderPipeline;
            if (rp != null && rp.defaultMaterial != null) return rp.defaultMaterial.shader;
            return Shader.Find("Standard");
        }
        static Material Make(Color c) { var m = new Material(Sh()); m.SetColor("_BaseColor", c); m.SetColor("_Color", c); return m; }

        public static Material Mat(Color c)
        {
            string key = ColorUtility.ToHtmlStringRGBA(c);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                string path = "Assets/_ShadowFire/Materials/M_" + key + ".mat";
                var ex = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (ex) return ex;
                System.IO.Directory.CreateDirectory("Assets/_ShadowFire/Materials");
                var m0 = Make(c); AssetDatabase.CreateAsset(m0, path); return m0;
            }
#endif
            if (cache.TryGetValue(key, out var m) && m) return m;
            m = Make(c); cache[key] = m; return m;
        }

        public static GameObject Make(PrimitiveType t, string name, Vector3 localPos, Vector3 scale, Color c,
                                      Transform parent = null, bool collider = true)
        {
            var g = GameObject.CreatePrimitive(t);
            g.name = name;
            if (parent) g.transform.SetParent(parent, false);
            g.transform.localPosition = localPos;
            g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = Mat(c);
            if (!collider)
            {
                var col = g.GetComponent<Collider>();
                if (Application.isPlaying) Object.Destroy(col); else Object.DestroyImmediate(col);
            }
            return g;
        }
    }
}
