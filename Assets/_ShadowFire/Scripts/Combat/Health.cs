using System;
using System.Collections.Generic;
using UnityEngine;
namespace ShadowFire
{
    public enum HitPart { Head, Chest, Limb }

    public class Health : MonoBehaviour
    {
        public static readonly List<Health> All = new List<Health>();
        public string displayName = "Player";
        public int team = 1;
        public float maxHp = 100f, hp = 100f, shield = 0f, downedHp = 60f;
        public int armorTier, helmetTier;
        public bool Downed, Dead;
        public float reviveProgress;
        public Transform head, torsoTop, torsoBottom;   // aim-assist anchors
        public Action<float, HitPart, Health> OnDamaged;
        public Action<Health> OnDowned, OnDied;

        void OnEnable() { All.Add(this); }
        void OnDisable() { All.Remove(this); }
    }
}
