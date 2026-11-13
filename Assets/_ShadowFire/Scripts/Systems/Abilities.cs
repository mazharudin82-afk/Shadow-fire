using System.Collections;
using UnityEngine;
namespace ShadowFire
{
    public class Destructible : MonoBehaviour
    {
        public float hp = 150f;
        public void TakeDamage(float d) { hp -= d; if (hp <= 0f) Destroy(gameObject); }
    }

    /// Original deployable cover ("Frostwall"): rises in front of the player, destructible.
    public static class FrostWall
    {
        public static void Deploy(Transform player)
        {
            Vector3 pos = player.position + player.forward * 2.2f;
            var g = Prim.Make(PrimitiveType.Cube, "FrostWall", pos, new Vector3(3f, 0.1f, 0.4f), new Color(.55f, .85f, 1f));
            g.transform.rotation = Quaternion.Euler(0f, player.eulerAngles.y, 0f);
            g.AddComponent<Destructible>().hp = 160f;
            g.AddComponent<WallRise>().target = new Vector3(3f, 2.2f, 0.4f);
        }
    }
    public class WallRise : MonoBehaviour
    {
        public Vector3 target; float t;
        void Update()
        {
            t += Time.deltaTime / 0.35f;
            transform.localScale = Vector3.Lerp(new Vector3(target.x, 0.1f, target.z), target, Mathf.Clamp01(t));
            if (t >= 1f) Destroy(this);
        }
    }

    /// Original ability kit: Pulse (heal), Dash (speed), Aegis (shield). Cover on its own cooldown.
    public class Abilities : MonoBehaviour
    {
        public enum Kit { HealPulse, SpeedDash, ShieldBurst }
        public Kit kit = Kit.HealPulse;
        public float cooldown = 20f, coverCooldown = 8f;
        float next, nextCover;
        Health hp; PlayerController pc;
        public float Cooldown01 => Mathf.Clamp01((next - Time.time) / cooldown);
        void Start() { hp = GetComponent<Health>(); pc = GetComponent<PlayerController>(); }
        void Update()
        {
            if (hp && (hp.Dead || hp.Downed)) return;
            if (GameInput.Ability && Time.time >= next) { next = Time.time + cooldown; Activate(); }
            if (GameInput.Cover && Time.time >= nextCover) { nextCover = Time.time + coverCooldown; FrostWall.Deploy(transform); }
        }
        void Activate()
        {
            switch (kit)
            {
                case Kit.HealPulse:
                    foreach (var h in Health.All)
                        if (!h.Dead && !h.Downed && h.team == hp.team && Vector3.Distance(h.transform.position, transform.position) < 6f)
                            h.hp = Mathf.Min(h.maxHp, h.hp + 30f);
                    break;
                case Kit.SpeedDash: StartCoroutine(Dash()); break;
                case Kit.ShieldBurst: hp.shield = 50f; StartCoroutine(DropShield()); break;
            }
        }
        IEnumerator Dash() { pc.speedMul = 1.7f; yield return new WaitForSeconds(2.5f); pc.speedMul = 1f; }
        IEnumerator DropShield() { yield return new WaitForSeconds(6f); hp.shield = 0f; }
    }
}
