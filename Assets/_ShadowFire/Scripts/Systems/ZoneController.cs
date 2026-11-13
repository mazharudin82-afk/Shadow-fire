using System;
using UnityEngine;
namespace ShadowFire
{
    [Serializable] public struct ZonePhase { public float wait, shrink, endRadius, dps; }

    public class ZoneController : MonoBehaviour
    {
        public float startRadius = 5000f;
        public Vector2 center;
        public float speed = 1f;                     // >1 = faster test matches
        public ZonePhase[] phases;
        public Vector2 Center { get; private set; }
        public float Radius { get; private set; }
        public Vector2 NextCenter { get; private set; }
        public float NextRadius { get; private set; }
        public float TimeLeft { get; private set; }
        public bool Shrinking { get; private set; }
        public float Dps { get; private set; }
        public int PhaseIndex { get; private set; }
        public bool Finished { get; private set; }

        Vector2 c0; float r0, dmgTimer; LineRenderer ring, nextRing;

        /// ~12 min total at speed 1 for an 8 km island (radii scaled by s)
        public static ZonePhase[] DefaultPhases(float s)
        {
            float[,] t = { { 90, 80, 2800, 1 }, { 75, 70, 1600, 2 }, { 60, 60, 900, 4 }, { 50, 50, 450, 6 }, { 40, 40, 200, 8 }, { 30, 30, 80, 10 }, { 25, 25, 15, 12 } };
            var p = new ZonePhase[7];
            for (int i = 0; i < 7; i++) p[i] = new ZonePhase { wait = t[i, 0], shrink = t[i, 1], endRadius = t[i, 2] * s, dps = t[i, 3] };
            return p;
        }

        void Start()
        {
            if (phases == null || phases.Length == 0) phases = DefaultPhases(startRadius / 5000f);
            Radius = startRadius; Center = center;
            ring = MakeRing(new Color(0.2f, 0.6f, 1f, 1f)); nextRing = MakeRing(Color.white);
            BeginPhase(0);
        }

        void BeginPhase(int i)
        {
            PhaseIndex = i; Shrinking = false; TimeLeft = phases[i].wait; Dps = phases[i].dps;
            r0 = Radius; c0 = Center;
            NextRadius = phases[i].endRadius;
            NextCenter = c0 + UnityEngine.Random.insideUnitCircle * Mathf.Max(0f, r0 - NextRadius);
        }

        void Update()
        {
            if (Finished) { Tick(); Draw(); return; }
            TimeLeft -= Time.deltaTime * speed;
            var ph = phases[PhaseIndex];
            if (!Shrinking)
            {
                if (TimeLeft <= 0f) { Shrinking = true; TimeLeft = ph.shrink; }
            }
            else
            {
                float k = 1f - Mathf.Clamp01(TimeLeft / ph.shrink);
                Radius = Mathf.Lerp(r0, NextRadius, k); Center = Vector2.Lerp(c0, NextCenter, k);
                if (TimeLeft <= 0f) { if (PhaseIndex + 1 < phases.Length) BeginPhase(PhaseIndex + 1); else Finished = true; }
            }
            Tick(); Draw();
        }

        public bool IsOutside(Vector3 p) => Vector2.Distance(new Vector2(p.x, p.z), Center) > Radius;

        void Tick()
        {
            dmgTimer += Time.deltaTime;
            if (dmgTimer < 1f) return;
            dmgTimer -= 1f;
            for (int i = 0; i < Health.All.Count; i++)
            {
                var h = Health.All[i];
                if (!h.Dead && IsOutside(h.transform.position)) DamageSystem.Apply(h, Dps, HitPart.Chest, null, true);
            }
        }

        LineRenderer MakeRing(Color c)
        {
            var go = new GameObject("ZoneRing"); go.transform.SetParent(transform, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.loop = true; lr.useWorldSpace = true; lr.positionCount = 96;
            lr.material = new Material(Shader.Find("Sprites/Default")); lr.startColor = lr.endColor = c;
            return lr;
        }
        void DrawRing(LineRenderer lr, Vector2 c, float r)
        {
            for (int i = 0; i < 96; i++)
            {
                float a = i / 96f * Mathf.PI * 2f;
                lr.SetPosition(i, new Vector3(c.x + Mathf.Cos(a) * r, 1.5f, c.y + Mathf.Sin(a) * r));
            }
            lr.widthMultiplier = Mathf.Max(1.2f, r * 0.006f);
        }
        void Draw() { DrawRing(ring, Center, Radius); DrawRing(nextRing, NextCenter, NextRadius); }
    }
}
