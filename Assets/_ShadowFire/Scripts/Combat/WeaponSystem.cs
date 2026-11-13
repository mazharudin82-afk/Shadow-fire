using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ShadowFire
{
    public class WeaponSystem : MonoBehaviour
    {
        public WeaponData data;
        public Camera cam;
        public readonly Dictionary<AttachSlot, Attachment> attachments = new Dictionary<AttachSlot, Attachment>();
        public static event Action<bool> OnHit; // true = headshot

        public int Ammo { get; private set; }
        public bool Reloading { get; private set; }
        PlayerController player; Health health; AimAssist assist; InventorySystem inv;
        float nextShot, lastShot, spreadAcc; int shotIndex; Vector2 recoilAcc; bool wasTrigger;

        float Prod(Func<Attachment, float> f) { float v = 1f; foreach (var a in attachments.Values) v *= f(a); return v; }
        public int MagSize { get { int b = 0; foreach (var a in attachments.Values) b += a.magBonus; return data.mag + b; } }
        public bool Scoped { get { if (data.scoped) return true; foreach (var a in attachments.Values) if (a.scope) return true; return false; } }
        public float SensScale => data.sensScale * Prod(a => a.sensMul);
        public float AdsFov => data.adsFov * Prod(a => a.adsFovMul);
        public int Reserve => inv ? inv.Count(data.ammoId) : 999;
        bool Ads => player && player.ads;
        float CurrentSpread => Mathf.Min(data.maxSpread, (data.hipSpread * (Ads ? data.adsSpreadMul : 1f) + spreadAcc) * Prod(a => a.spreadMul));

        public void Attach(Attachment a) { attachments[a.slot] = a; }

        void Start()
        {
            player = GetComponent<PlayerController>(); health = GetComponent<Health>();
            assist = GetComponent<AimAssist>(); inv = GetComponent<InventorySystem>();
            if (!cam) cam = Camera.main;
            if (data) Ammo = MagSize;
        }

        void Update()
        {
            if (!data || (health && (health.Dead || health.Downed))) return;
            float dt = Time.deltaTime;
            if (GameInput.Reload && !Reloading && Ammo < MagSize) StartCoroutine(ReloadRoutine());

            bool trigger = GameInput.Fire || (SensitivityModel.S.autoFire && assist && assist.Target != null);
            bool edge = trigger && !wasTrigger; wasTrigger = trigger;
            if (trigger && !Reloading && Time.time >= nextShot && (data.auto || edge))
            {
                if (Ammo <= 0) { if (Reserve > 0) StartCoroutine(ReloadRoutine()); }
                else Shoot();
            }

            spreadAcc = Mathf.MoveTowards(spreadAcc, 0f, data.spreadRecover * dt);
            if (Time.time - lastShot > 0.35f) shotIndex = 0;
            if (Time.time - lastShot > 0.12f && recoilAcc.sqrMagnitude > 1e-6f)
            {
                Vector2 step = Vector2.ClampMagnitude(recoilAcc, data.recoverRate * dt);
                player.AddRecoil(-step); recoilAcc -= step;
            }
        }

        IEnumerator ReloadRoutine()
        {
            Reloading = true;
            yield return new WaitForSeconds(data.reload);
            int need = MagSize - Ammo;
            Ammo += inv ? inv.TakeAmmo(data.ammoId, need) : need;
            Reloading = false;
        }

        void Shoot()
        {
            Ammo--; lastShot = Time.time; nextShot = Time.time + 60f / data.rpm;
            Ray camRay = new Ray(cam.transform.position, cam.transform.forward);
            Ray fire = assist ? assist.Redirect(camRay) : camRay;

            Vector2 c = UnityEngine.Random.insideUnitCircle * Mathf.Tan(CurrentSpread * Mathf.Deg2Rad);   // spread cone
            Vector3 dir = (fire.direction + cam.transform.right * c.x + cam.transform.up * c.y).normalized;
            spreadAcc += data.spreadPerShot;

            var hits = Physics.RaycastAll(fire.origin, dir, data.range, ~0, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            Vector3 end = fire.origin + dir * data.range;
            foreach (var h in hits)
            {
                var hb = h.collider.GetComponent<Hitbox>();
                if (hb != null)
                {
                    if (hb.owner == health) continue;
                    float fall = Mathf.Lerp(1f, 0.6f, h.distance / data.range);
                    DamageSystem.Apply(hb.owner, data.damage * fall, hb.part, health);
                    OnHit?.Invoke(hb.part == HitPart.Head);
                    end = h.point; break;
                }
                var ds = h.collider.GetComponentInParent<Destructible>();
                if (ds != null) { ds.TakeDamage(data.damage); end = h.point; break; }
                if (h.collider.transform.root == transform) continue;
                end = h.point; break;
            }
            Tracer(cam.transform.position + cam.transform.right * 0.3f - cam.transform.up * 0.2f + cam.transform.forward, end);

            int i = Mathf.Min(shotIndex, data.recoil.Length - 1);     // recoil: pattern climb + wander
            Vector2 r = data.recoil[i];
            r.x += UnityEngine.Random.Range(-1f, 1f) * data.wander;
            r *= Prod(a => a.recoilMul) * (Ads ? 0.75f : 1f);
            shotIndex++;
            player.AddRecoil(r); recoilAcc += r;
        }

        static void Tracer(Vector3 a, Vector3 b)
        {
            Vector3 d = b - a; float len = d.magnitude; if (len < 0.5f) return;
            var t = Prim.Make(PrimitiveType.Cube, "Tracer", (a + b) * 0.5f, new Vector3(0.02f, 0.02f, len), new Color(1f, 0.9f, 0.4f), null, false);
            t.transform.rotation = Quaternion.LookRotation(d);
            UnityEngine.Object.Destroy(t, 0.04f);
        }
    }
}
