using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace ShadowFire.EditorTools
{
    /// Menu: ShadowFire > Build All Scenes  -> Lobby + Island_Greybox (POI "Rustwater Docks") + weapon asset.
    public static class GreyboxBuilder
    {
        const string Root = "Assets/_ShadowFire";

        [MenuItem("ShadowFire/Build All Scenes")]
        public static void BuildAll()
        {
            Directory.CreateDirectory(Root + "/Scenes"); Directory.CreateDirectory(Root + "/Data");
            var wd = CreateWeapon();
            string lobby = BuildLobby(), island = BuildIsland(wd);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(lobby, true), new EditorBuildSettingsScene(island, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("ShadowFire: scenes built. Open Island_Greybox and press Play (WASD, mouse, LMB fire, RMB ADS, C/Z stance, R reload, F cover, Q skill, H heal, Esc = cursor).");
        }

        static WeaponData CreateWeapon()
        {
            string p = Root + "/Data/AR_Vanguard.asset";
            var w = AssetDatabase.LoadAssetAtPath<WeaponData>(p);
            if (w) return w;
            w = ScriptableObject.CreateInstance<WeaponData>();
            var pat = new Vector2[30];
            for (int i = 0; i < pat.Length; i++)   // vertical climb that eases off + horizontal sway that grows
                pat[i] = new Vector2(Mathf.Sin(i * 0.7f) * 0.12f * (1f + i / 30f), Mathf.Max(0.18f, 0.42f - i * 0.008f));
            w.recoil = pat;
            AssetDatabase.CreateAsset(w, p); return w;
        }

        static string BuildLobby()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = new GameObject("Camera").AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.06f, .08f, .12f);
            new GameObject("Lobby").AddComponent<LobbyManager>();
            string path = Root + "/Scenes/Lobby.unity"; EditorSceneManager.SaveScene(scene, path); return path;
        }

        static string BuildIsland(WeaponData wd)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var sun = new GameObject("Sun").AddComponent<Light>(); sun.type = LightType.Directional; sun.intensity = 1.1f;
            sun.transform.rotation = Quaternion.Euler(50, -30, 0);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat; RenderSettings.ambientLight = new Color(.55f, .6f, .65f);

            var env = new GameObject("Env").transform;
            Prim.Make(PrimitiveType.Cube, "Ground", new Vector3(0, -.5f, 0), new Vector3(500, 1, 500), new Color(.36f, .45f, .3f), env);
            Prim.Make(PrimitiveType.Cube, "Water", new Vector3(0, -.45f, -150), new Vector3(500, 1, 100), new Color(.15f, .35f, .5f), env);

            // island layout markers (8x8 km), POI 0 is built out as the greybox
            var layout = new GameObject("IslandLayout").transform;
            foreach (var poi in IslandLayout.Generate(1337))
            {
                var m = new GameObject("POI_" + poi.name + "_" + poi.tier).transform; m.SetParent(layout, false);
                m.position = new Vector3(poi.pos.x, 0, poi.pos.y);
            }

            // ---- POI: Rustwater Docks ----
            var poiRoot = new GameObject("POI_RustwaterDocks_Greybox").transform;
            Color wall = new Color(.55f, .5f, .45f), wood = new Color(.4f, .3f, .2f);
            Prim.Make(PrimitiveType.Cube, "Pier", new Vector3(0, .1f, -85), new Vector3(14, .4f, 70), wood, poiRoot);
            Room(poiRoot, new Vector3(-32, 0, 15), new Vector3(14, 4, 12), wall);
            Room(poiRoot, new Vector3(32, 0, 20), new Vector3(16, 4, 12), wall);
            Room(poiRoot, new Vector3(0, 0, 58), new Vector3(20, 5, 14), wall);
            Room(poiRoot, new Vector3(-38, 0, 62), new Vector3(10, 4, 10), wall);
            string[] cols = { "c1", "c2", "c3" };
            for (int i = 0; i < 3; i++)
                Prim.Make(PrimitiveType.Cube, "Container" + cols[i], new Vector3(-12 + i * 12, 1.3f, -12), new Vector3(6, 2.6f, 2.4f),
                          new Color(.7f - i * .2f, .3f + i * .15f, .25f + i * .2f), poiRoot);
            for (int i = 0; i < 8; i++)   // destructible crates = cover
            {
                var c = Prim.Make(PrimitiveType.Cube, "Crate", new Vector3(-10 + i * 3.2f, .6f, 2 + (i % 3) * 7), Vector3.one * 1.2f, new Color(.6f, .45f, .25f), poiRoot);
                c.AddComponent<Destructible>().hp = 120f;
            }
            Prim.Make(PrimitiveType.Cube, "VehicleSpawn_Jeep (placeholder)", new Vector3(14, .6f, -25), new Vector3(2f, 1.2f, 4f), new Color(.2f, .35f, .2f), poiRoot);
            Prim.Make(PrimitiveType.Cube, "VehicleSpawn_Boat (placeholder)", new Vector3(0, .1f, -130), new Vector3(2.4f, .8f, 6f), new Color(.8f, .8f, .85f), poiRoot);

            var loot = new GameObject("Loot").transform;
            LootPickup.Spawn(new Vector3(-3, 0, -20), "ammo_556", 90, loot); LootPickup.Spawn(new Vector3(3, 0, -20), "bandage", 3, loot);
            LootPickup.Spawn(new Vector3(0, 0, -16), "medkit", 1, loot); LootPickup.Spawn(new Vector3(-30, 0, 15), "armor_t2", 1, loot);
            LootPickup.Spawn(new Vector3(30, 0, 20), "helmet_t2", 1, loot); LootPickup.Spawn(new Vector3(0, 0, 58), "pack_t2", 1, loot);

            // ---- test targets ----
            var tg = new GameObject("Targets").transform;
            Dummy(tg, new Vector3(-6, 0, -2), true); Dummy(tg, new Vector3(6, 0, 12), true);
            Dummy(tg, new Vector3(0, 0, 30), true); Dummy(tg, new Vector3(10, 0, 45), false);

            // ---- player + camera ----
            var p = new GameObject("Player"); p.transform.position = new Vector3(0, 1.2f, -35);
            var cc = p.AddComponent<CharacterController>(); cc.height = 1.8f; cc.radius = .35f; cc.center = new Vector3(0, .9f, 0);
            var hp = p.AddComponent<Health>(); hp.displayName = "You"; hp.team = 1;
            var pivot = new GameObject("CamPivot"); pivot.transform.SetParent(p.transform, false); pivot.transform.localPosition = new Vector3(0, 1.6f, 0);
            var pc = p.AddComponent<PlayerController>(); pc.camPivot = pivot.transform;
            var inv = p.AddComponent<InventorySystem>(); inv.Add("ammo_556", 120);
            var ws = p.AddComponent<WeaponSystem>(); ws.data = wd;
            var aa = p.AddComponent<AimAssist>();
            p.AddComponent<Abilities>();
            Prim.Make(PrimitiveType.Capsule, "Visual", new Vector3(0, .9f, 0), new Vector3(.7f, .9f, .7f), new Color(.2f, .5f, .9f), p.transform, false);

            var camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>(); camGo.AddComponent<AudioListener>();
            var tpc = camGo.AddComponent<ThirdPersonCamera>(); tpc.target = pc; ws.cam = cam; aa.cam = cam;

            // ---- systems ----
            new GameObject("DesktopInput").AddComponent<DesktopInput>();
            var z = new GameObject("Zone").AddComponent<ZoneController>(); z.startRadius = 160f; z.center = Vector2.zero; z.speed = 3f;
            var hud = new GameObject("HUD").AddComponent<HudBuilder>(); hud.player = pc; hud.zone = z;

            string path = Root + "/Scenes/Island_Greybox.unity"; EditorSceneManager.SaveScene(scene, path); return path;
        }

        // 4 walls (front has a door gap) + floor + roof
        static void Room(Transform parent, Vector3 c, Vector3 s, Color col)
        {
            var r = new GameObject("Building").transform; r.SetParent(parent, false); r.position = c;
            float w = s.x, h = s.y, d = s.z, t = .4f, door = 3f, dh = 2.4f;
            Prim.Make(PrimitiveType.Cube, "Floor", new Vector3(0, .1f, 0), new Vector3(w, .2f, d), col * .8f, r);
            Prim.Make(PrimitiveType.Cube, "Back", new Vector3(0, h / 2, d / 2), new Vector3(w, h, t), col, r);
            Prim.Make(PrimitiveType.Cube, "L", new Vector3(-w / 2, h / 2, 0), new Vector3(t, h, d), col, r);
            Prim.Make(PrimitiveType.Cube, "R", new Vector3(w / 2, h / 2, 0), new Vector3(t, h, d), col, r);
            float seg = (w - door) / 2f;
            Prim.Make(PrimitiveType.Cube, "FrontL", new Vector3(-(door / 2 + seg / 2), h / 2, -d / 2), new Vector3(seg, h, t), col, r);
            Prim.Make(PrimitiveType.Cube, "FrontR", new Vector3((door / 2 + seg / 2), h / 2, -d / 2), new Vector3(seg, h, t), col, r);
            Prim.Make(PrimitiveType.Cube, "Lintel", new Vector3(0, dh + (h - dh) / 2, -d / 2), new Vector3(door, h - dh, t), col, r);
            Prim.Make(PrimitiveType.Cube, "Roof", new Vector3(0, h + .1f, 0), new Vector3(w + .4f, .2f, d + .4f), col * .7f, r);
        }

        static void Dummy(Transform parent, Vector3 pos, bool strafe)
        {
            var root = new GameObject("Dummy"); root.transform.SetParent(parent, false); root.transform.position = pos;
            var h = root.AddComponent<Health>(); h.displayName = "Bot"; h.team = 2; h.maxHp = h.hp = 100f;
            var body = new GameObject("Body"); body.transform.SetParent(root.transform, false);
            Color c = new Color(.85f, .35f, .3f);
            Part(body, h, PrimitiveType.Capsule, "Chest", new Vector3(0, 1.15f, 0), new Vector3(.5f, .38f, .3f), c, HitPart.Chest);
            Part(body, h, PrimitiveType.Sphere, "Head", new Vector3(0, 1.72f, 0), Vector3.one * .28f, c * 1.2f, HitPart.Head);
            Part(body, h, PrimitiveType.Cube, "LegL", new Vector3(-.12f, .4f, 0), new Vector3(.18f, .8f, .2f), c * .8f, HitPart.Limb);
            Part(body, h, PrimitiveType.Cube, "LegR", new Vector3(.12f, .4f, 0), new Vector3(.18f, .8f, .2f), c * .8f, HitPart.Limb);
            Part(body, h, PrimitiveType.Cube, "ArmL", new Vector3(-.34f, 1.15f, 0), new Vector3(.12f, .6f, .12f), c * .8f, HitPart.Limb);
            Part(body, h, PrimitiveType.Cube, "ArmR", new Vector3(.34f, 1.15f, 0), new Vector3(.12f, .6f, .12f), c * .8f, HitPart.Limb);
            h.torsoBottom = Anchor(root.transform, "TorsoBottom", .95f); h.torsoTop = Anchor(root.transform, "TorsoTop", 1.4f); h.head = Anchor(root.transform, "HeadPoint", 1.72f);
            var d = root.AddComponent<TargetDummy>(); d.strafe = strafe; d.body = body;
        }
        static void Part(GameObject body, Health owner, PrimitiveType t, string n, Vector3 pos, Vector3 sc, Color c, HitPart part)
        {
            var g = Prim.Make(t, n, pos, sc, c, body.transform);
            var hb = g.AddComponent<Hitbox>(); hb.owner = owner; hb.part = part;
        }
        static Transform Anchor(Transform p, string n, float y) { var t = new GameObject(n).transform; t.SetParent(p, false); t.localPosition = new Vector3(0, y, 0); return t; }
    }
}
