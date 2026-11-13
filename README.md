# Shadow Fire — Unity 2022.3 LTS + URP greybox

## Setup (5 min)
1. Unity Hub -> New project -> **3D (URP)**, Unity 2022.3 LTS.
2. Copy `Assets/_ShadowFire` into your project's `Assets/`.
3. Project Settings -> Player -> Other Settings -> **Active Input Handling = Both** (desktop test input uses the legacy Input Manager).
4. Menu **ShadowFire -> Build All Scenes** (creates Lobby, Island_Greybox, weapon asset, build settings).
5. Open `Lobby` or `Island_Greybox`, press Play.

Desktop test: WASD, mouse look, LMB fire, RMB ADS, C crouch, Z prone, R reload, F cover, Q skill, H heal, Esc frees cursor.
Mobile: Build to Android/iOS; joystick L, FIRE L, right half drag = look.

## Folder tree
Assets/_ShadowFire/
  Scripts/
    Core/    GameInput, SensitivityModel, Prim
    Player/  PlayerController, ThirdPersonCamera
    Combat/  AimAssist, WeaponSystem, WeaponData, Health, Hitbox, DamageSystem
    Systems/ InventorySystem(+LootPickup), ZoneController, Abilities(+FrostWall, Destructible), LobbyManager(+MatchConfig)
    UI/      UiKit, TouchWidgets, HudBuilder(+KillFeed), SettingsPanel
    World/   IslandLayout, TargetDummy
    Editor/  GreyboxBuilder
  Scenes/ Data/ Materials/   (generated)

## Aim assist tuning (Combat/AimAssist.cs, `Get()`)
radius (m) / bodyPull / headPull / snapThreshold (deg/s upward drag) / lerpMul. Head blend lerps over 80 ms.
Limbs are never targeted: aim point only ever moves between torso center and head.

## NOT included yet
50-player netcode (use Netcode for GameObjects / Fusion / Mirror, server-authoritative hits), bot AI, drivable vehicles,
airdrops, full 8x8 km terrain (IslandLayout only seeds POI positions), inventory UI, controls-remap UI, revive interaction input
(DamageSystem.Revive exists), character art/animation. Not compiled in Unity from this environment — expect minor fixes on first import.
