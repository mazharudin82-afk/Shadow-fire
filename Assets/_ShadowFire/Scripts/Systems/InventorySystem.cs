using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ShadowFire
{
    public enum ItemType { Ammo, Heal, Armor, Helmet, Backpack }

    [Serializable]
    public class ItemDef
    {
        public string id, name; public ItemType type; public int tier, stackMax;
        public ItemDef(string id, string name, ItemType type, int tier, int stackMax) { this.id = id; this.name = name; this.type = type; this.tier = tier; this.stackMax = stackMax; }
    }
    [Serializable] public class ItemStack { public ItemDef def; public int amount; }

    public static class ItemDb
    {
        public static readonly Dictionary<string, ItemDef> Defs = new Dictionary<string, ItemDef>
        {
            { "ammo_556", new ItemDef("ammo_556", "5.56 Ammo", ItemType.Ammo, 0, 120) },
            { "bandage",  new ItemDef("bandage", "Bandage", ItemType.Heal, 0, 5) },
            { "medkit",   new ItemDef("medkit", "Medkit", ItemType.Heal, 1, 2) },
            { "armor_t1", new ItemDef("armor_t1", "Vest I", ItemType.Armor, 1, 1) },
            { "armor_t2", new ItemDef("armor_t2", "Vest II", ItemType.Armor, 2, 1) },
            { "armor_t3", new ItemDef("armor_t3", "Vest III", ItemType.Armor, 3, 1) },
            { "helmet_t1", new ItemDef("helmet_t1", "Helm I", ItemType.Helmet, 1, 1) },
            { "helmet_t2", new ItemDef("helmet_t2", "Helm II", ItemType.Helmet, 2, 1) },
            { "helmet_t3", new ItemDef("helmet_t3", "Helm III", ItemType.Helmet, 3, 1) },
            { "pack_t1", new ItemDef("pack_t1", "Pack I", ItemType.Backpack, 1, 1) },
            { "pack_t2", new ItemDef("pack_t2", "Pack II", ItemType.Backpack, 2, 1) },
            { "pack_t3", new ItemDef("pack_t3", "Pack III", ItemType.Backpack, 3, 1) },
        };
    }

    public class InventorySystem : MonoBehaviour
    {
        public int backpackTier = 0;
        static readonly int[] SlotsByTier = { 8, 12, 16, 22 };
        public List<ItemStack> slots = new List<ItemStack>();
        public int Capacity => SlotsByTier[Mathf.Clamp(backpackTier, 0, 3)];
        Health health; bool healing;
        Health H => health ? health : (health = GetComponent<Health>());

        void Update() { if (GameInput.Heal) UseBestHeal(); }

        /// returns leftover amount that did not fit
        public int Add(string id, int amount)
        {
            if (!ItemDb.Defs.TryGetValue(id, out var d)) return amount;
            switch (d.type)
            {
                case ItemType.Armor:    if (d.tier > H.armorTier)  { H.armorTier = d.tier;  return amount - 1; } return amount;
                case ItemType.Helmet:   if (d.tier > H.helmetTier) { H.helmetTier = d.tier; return amount - 1; } return amount;
                case ItemType.Backpack: if (d.tier > backpackTier) { backpackTier = d.tier; return amount - 1; } return amount;
            }
            foreach (var s in slots)
                if (s.def == d && s.amount < d.stackMax)
                {
                    int a = Mathf.Min(amount, d.stackMax - s.amount); s.amount += a; amount -= a;
                    if (amount == 0) return 0;
                }
            while (amount > 0 && slots.Count < Capacity)
            {
                int a = Mathf.Min(amount, d.stackMax);
                slots.Add(new ItemStack { def = d, amount = a }); amount -= a;
            }
            return amount;
        }

        public int Count(string id) { int n = 0; foreach (var s in slots) if (s.def.id == id) n += s.amount; return n; }

        public int Remove(string id, int want)
        {
            int taken = 0;
            for (int i = slots.Count - 1; i >= 0 && taken < want; i--)
            {
                if (slots[i].def.id != id) continue;
                int t = Mathf.Min(want - taken, slots[i].amount);
                slots[i].amount -= t; taken += t;
                if (slots[i].amount <= 0) slots.RemoveAt(i);
            }
            return taken;
        }
        public int TakeAmmo(string id, int want) => Remove(id, want);

        public bool UseBestHeal()
        {
            if (healing || H.Downed || H.Dead || H.hp >= H.maxHp) return false;
            string id = (Count("medkit") > 0 && H.hp < H.maxHp * 0.5f) ? "medkit" : Count("bandage") > 0 ? "bandage" : Count("medkit") > 0 ? "medkit" : null;
            if (id == null) return false;
            StartCoroutine(HealRoutine(id)); return true;
        }
        IEnumerator HealRoutine(string id)
        {
            healing = true;
            yield return new WaitForSeconds(id == "medkit" ? 4f : 2f);
            if (!H.Dead && Remove(id, 1) > 0)
                H.hp = id == "medkit" ? H.maxHp : Mathf.Min(H.hp + 15f, Mathf.Max(H.hp, 75f));
            healing = false;
        }
    }

    /// Trigger pickup: walk over it -> auto loot.
    public class LootPickup : MonoBehaviour
    {
        public string id = "ammo_556"; public int amount = 30;
        void OnTriggerEnter(Collider other)
        {
            var inv = other.GetComponent<InventorySystem>();
            if (!inv) return;
            amount = inv.Add(id, amount);
            if (amount <= 0) Destroy(gameObject);
        }
        public static GameObject Spawn(Vector3 pos, string id, int amount, Transform parent = null)
        {
            Color c = id.StartsWith("ammo") ? new Color(.9f, .75f, .2f) : id.StartsWith("med") || id.StartsWith("band") ? new Color(.9f, .2f, .25f) : new Color(.3f, .7f, .95f);
            var g = Prim.Make(PrimitiveType.Cube, "Loot_" + id, pos + Vector3.up * 0.3f, Vector3.one * 0.35f, c, parent, false);
            var sc = g.AddComponent<SphereCollider>(); sc.isTrigger = true; sc.radius = 2.5f;
            var lp = g.AddComponent<LootPickup>(); lp.id = id; lp.amount = amount;
            return g;
        }
    }
}
