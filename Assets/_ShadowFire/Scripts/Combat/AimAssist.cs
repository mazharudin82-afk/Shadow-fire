using UnityEngine;
namespace ShadowFire
{
    /// Capsule + drag-threshold aim assist.
    /// 1) A capsule (torso segment, radius by assist level) is tested against the camera aim ray.
    /// 2) Inside it, bullets are pulled toward the BODY CENTER (never limbs).
    /// 3) When upward drag speed (deg/s) crosses the level's threshold, HeadBlend lerps 0->1 over ~80ms
    ///    and the aim point moves body -> head. Held for headHold seconds so it doesn't flicker.
    public class AimAssist : MonoBehaviour
    {
        public Camera cam;
        public float maxRange = 70f, snapLerpTime = 0.08f, headHold = 0.25f;
        public Health Target { get; private set; }
        public float HeadBlend { get; private set; }
        public float Pull { get; private set; }
        public Vector3 AimPoint { get; private set; }

        Health self; Vector2 dragVel; float snapUntil;

        struct Lvl
        {
            public float radius, bodyPull, headPull, snapThreshold, lerpMul;
            public Lvl(float r, float bp, float hp, float thr, float lm) { radius = r; bodyPull = bp; headPull = hp; snapThreshold = thr; lerpMul = lm; }
        }
        static Lvl Get(AimAssistLevel l)
        {
            switch (l)
            {
                case AimAssistLevel.Low:  return new Lvl(0.35f, 0.30f, 0.60f, 220f, 1.0f);
                case AimAssistLevel.Med:  return new Lvl(0.50f, 0.50f, 0.80f, 160f, 1.0f);
                case AimAssistLevel.High: return new Lvl(0.70f, 0.75f, 0.95f, 110f, 1.6f);   // wider capsule, faster snap
                default:                  return new Lvl(0f, 0f, 0f, 9999f, 1f);
            }
        }

        void Start() { self = GetComponent<Health>(); if (!cam) cam = Camera.main; }
        public void Feed(Vector2 degPerSec) { dragVel = degPerSec; }

        void LateUpdate()
        {
            var lv = Get(SensitivityModel.S.assist);
            Target = null; Pull = 0f;
            if (lv.radius <= 0f || !cam) { HeadBlend = Mathf.MoveTowards(HeadBlend, 0f, Time.deltaTime / snapLerpTime); return; }

            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            float bestD = float.MaxValue;
            foreach (var h in Health.All)
            {
                if (h == self || h.Dead || !h.torsoTop || !h.torsoBottom) continue;
                Vector3 a = h.torsoBottom.position, b = h.torsoTop.position;
                if (Vector3.Distance(ray.origin, a) > maxRange) continue;
                if (Vector3.Dot(ray.direction, a - ray.origin) <= 0f) continue;
                float d = RaySegDist(ray, a, b);
                if (d < lv.radius && d < bestD) { bestD = d; Target = h; }
            }

            bool snapNow = Target != null && dragVel.y > lv.snapThreshold;     // drag up fast = head
            if (snapNow) snapUntil = Time.time + headHold;
            bool wantHead = Target != null && Time.time < snapUntil;
            HeadBlend = Mathf.MoveTowards(HeadBlend, wantHead ? 1f : 0f, Time.deltaTime / (snapLerpTime / lv.lerpMul));

            if (Target == null) return;
            Vector3 body = (Target.torsoTop.position + Target.torsoBottom.position) * 0.5f;
            Vector3 headP = Target.head ? Target.head.position : body + Vector3.up * 0.3f;
            AimPoint = Vector3.Lerp(body, headP, HeadBlend);
            float basePull = lv.bodyPull * Mathf.Clamp01(1.25f - bestD / lv.radius);
            Pull = Mathf.Lerp(basePull, lv.headPull, HeadBlend);
        }

        public Ray Redirect(Ray r)
        {
            if (Target == null || Pull <= 0.001f) return r;
            Vector3 d = (AimPoint - r.origin).normalized;
            return new Ray(r.origin, Vector3.Slerp(r.direction, d, Pull).normalized);
        }

        /// shortest distance between a ray and segment ab
        static float RaySegDist(Ray r, Vector3 a, Vector3 b)
        {
            Vector3 u = r.direction, v = b - a, w = r.origin - a;
            float uv = Vector3.Dot(u, v), vv = Vector3.Dot(v, v), uw = Vector3.Dot(u, w), vw = Vector3.Dot(v, w);
            float den = vv - uv * uv;                       // uu = 1
            float t = den < 1e-6f ? 0.5f : 0f, s;
            if (den >= 1e-6f) { s = (uv * vw - uw * vv) / den; t = (s * uv + vw) / vv; }
            t = Mathf.Clamp01(t);
            Vector3 p = a + v * t;
            s = Mathf.Max(0f, Vector3.Dot(p - r.origin, u));
            return Vector3.Distance(r.origin + u * s, p);
        }
    }
}
