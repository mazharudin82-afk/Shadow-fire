using System;
using UnityEngine;
namespace ShadowFire
{
    public enum AttachSlot { Muzzle, Grip, Mag, Scope }

    [Serializable]
    public class Attachment
    {
        public string name; public AttachSlot slot;
        public float recoilMul = 1f, spreadMul = 1f, sensMul = 1f, adsFovMul = 1f;
        public int magBonus; public bool scope;
    }

    [CreateAssetMenu(menuName = "ShadowFire/Weapon")]
    public class WeaponData : ScriptableObject
    {
        public string displayName = "AR Vanguard", ammoId = "ammo_556";
        public float damage = 24f, rpm = 600f, range = 120f, reload = 2.1f;
        public int mag = 30; public bool auto = true, scoped = false;
        [Header("Spread (degrees)")]
        public float hipSpread = 2.2f, adsSpreadMul = 0.35f, spreadPerShot = 0.18f, spreadRecover = 6f, maxSpread = 5f;
        [Header("Recoil per shot index: x = yaw wander deg, y = pitch climb deg")]
        public Vector2[] recoil = { new Vector2(0, 0.4f) };
        public float wander = 0.08f, recoverRate = 3.5f;
        [Header("Feel")]
        public float sensScale = 1f, adsFov = 48f;
    }
}
