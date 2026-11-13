using UnityEngine;
namespace ShadowFire
{
    public static class DamageSystem
    {
        public static float PartMult(HitPart p) => p == HitPart.Head ? 2.2f : p == HitPart.Chest ? 1.0f : 0.65f;
        static readonly float[] ArmorReduce = { 0f, 0.20f, 0.30f, 0.40f };
        static readonly float[] HelmetReduce = { 0f, 0.25f, 0.35f, 0.45f };

        /// raw = true bypasses armor/part multipliers (zone damage).
        public static float Apply(Health t, float dmg, HitPart part, Health attacker, bool raw = false)
        {
            if (t == null || t.Dead) return 0f;
            if (attacker != null && attacker != t && attacker.team == t.team) return 0f;
            float d = dmg;
            if (!raw)
            {
                d *= PartMult(part);
                if (part == HitPart.Head) d *= 1f - HelmetReduce[Mathf.Clamp(t.helmetTier, 0, 3)];
                else d *= 1f - ArmorReduce[Mathf.Clamp(t.armorTier, 0, 3)];
            }
            if (t.shield > 0f) { float a = Mathf.Min(t.shield, d); t.shield -= a; d -= a; }
            t.OnDamaged?.Invoke(d, part, attacker);

            if (t.Downed) { t.downedHp -= d; if (t.downedHp <= 0f) Kill(t, attacker); return d; }
            t.hp -= d;
            if (t.hp <= 0f)
            {
                if (MatchConfig.Squad && HasLivingMate(t)) Down(t, attacker);
                else Kill(t, attacker);
            }
            return d;
        }

        static bool HasLivingMate(Health t)
        {
            foreach (var h in Health.All) if (h != t && h.team == t.team && !h.Dead) return true;
            return false;
        }
        static void Down(Health t, Health by)
        {
            t.hp = 0f; t.Downed = true; t.downedHp = 60f; t.reviveProgress = 0f;
            t.OnDowned?.Invoke(by);
            KillFeed.Push(Name(by) + " knocked " + t.displayName);
        }
        static void Kill(Health t, Health by)
        {
            t.hp = 0f; t.Dead = true; t.Downed = false;
            t.OnDied?.Invoke(by);
            KillFeed.Push(Name(by) + " eliminated " + t.displayName);
        }
        static string Name(Health h) => h ? h.displayName : "Zone";

        /// Call each frame while a teammate channels a revive on a downed player.
        public static bool Revive(Health t, float dt, float reviveTime = 5f)
        {
            if (t == null || !t.Downed) return false;
            t.reviveProgress += dt;
            if (t.reviveProgress < reviveTime) return false;
            t.Downed = false; t.hp = 30f; t.reviveProgress = 0f; return true;
        }
    }
}
