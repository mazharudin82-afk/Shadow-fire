using System.Collections.Generic;
using UnityEngine;
namespace ShadowFire
{
    public enum LootTier { Low, Mid, High }
    public struct Poi { public string name; public Vector2 pos; public float radius; public LootTier tier; public int vehicleSpawns; }

    /// Seeded layout for the 8x8 km island "Emberveil". Same seed = same island.
    public static class IslandLayout
    {
        public const float Size = 8000f;
        static readonly string[] Names = { "Rustwater Docks", "Ember Ridge", "Glasswind Station", "Hollow Pines", "Saltmarsh Quarry", "Verdant Spire" };
        static readonly LootTier[] Tiers = { LootTier.High, LootTier.High, LootTier.Mid, LootTier.Mid, LootTier.Mid, LootTier.Low };

        public static List<Poi> Generate(int seed)
        {
            var r = new System.Random(seed); var list = new List<Poi>();
            for (int n = 0; n < Names.Length; n++)
            {
                for (int tries = 0; tries < 400; tries++)
                {
                    var p = new Vector2((float)(r.NextDouble() * 6000 - 3000), (float)(r.NextDouble() * 6000 - 3000));
                    bool ok = true; foreach (var o in list) if (Vector2.Distance(o.pos, p) < 1300f) { ok = false; break; }
                    if (!ok && tries < 399) continue;
                    list.Add(new Poi { name = Names[n], pos = p, radius = 250f + (float)r.NextDouble() * 150f, tier = Tiers[n], vehicleSpawns = 1 + r.Next(0, 3) });
                    break;
                }
            }
            return list;
        }
    }
}
