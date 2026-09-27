# RunnerPal — session handoff

> **Türkçe özet:** Proje oynanabilir: gerçek karakterler ve animasyonlar (zıplama dahil), gerçek eşya / engel / kapı
> modelleri, şehir, sesler ve müzik. 40 bölüm, 4 dünya,
> kurallar görev dosyasındaki gibi çalışıyor ve test edildi. Sıradaki adımlar aşağıda "Next steps" bölümünde.
> Yeni oturumda Claude'a "HANDOFF.md'yi oku ve devam edelim" demen yeterli.

Last updated: 2026-09-27. Read this first, then `CLAUDE.md`, then `RUNNERPAL_TASK.md` for the full spec.


## Cartoon art pass (2026-09-27)

- Every box/cylinder the game builds itself (props, obstacles, road furniture, debris) now uses
  `ProceduralMesh.RoundedBox` / `RoundedCylinder` (`Scripts/Core/ProceduralMesh.cs`, runtime-safe, not
  editor-only) instead of Unity's sharp default primitives, so the whole game reads as one soft, "polished"
  style. A shared global post-process volume (`Settings/ToonPostProcess.asset`: Neutral tonemapping, light
  bloom, +saturation/contrast, gentle vignette) ties the look together in the Game scene.
- **New obstacle: stray dog** (`Obstacle.Kind.Critter`, `Prefabs/Obstacle_Dog`, procedural rounded-box model,
  level 6+): trots between the two blocked lanes (`ObstacleMover.Sway`, same safe pattern as `Obstacle_Sway`)
  so it always leaves the guaranteed-clear lane open; on hit it flinches and runs off (`CritterFlee`) instead
  of breaking apart — costs a life like any other obstacle, but never gibbed (kept cute).
  `TrackSpawner.swayingObstaclePrefab` -> `swayingObstaclePrefabs[]` (picks one at random) to fit it in
  alongside the swaying barrier.
- City buildings (Kenney kits) were left as-is — re-texturing/rebuilding those was out of scope for this pass.

**Follow-up the same day:** user said the first pass still looked "bad / nothing matches" and asked for a full
redo, referencing Subway Surfers. Since hand-painted textures are out of reach, the fix was palette + shading,
not geometry: `mat()` / `Lit()` now force `_SpecularHighlights` and `_EnvironmentReflections` off everywhere
(kills the shiny-plastic look), and Kenney's city buildings — previously stuck at their default grey/beige
`colormap` material — get tinted per-instance from a per-world candy palette (`buildingPalettes` in
`build_assets.cs`; `tint()` duplicates the shared material per (material, color) pair, trees keep a white/no-op
tint so foliage stays natural). Road/curb/sidewalk/lamp colors and the global post-process (saturation, contrast,
bloom) were all pushed further. This made a real difference — screenshots after vs. before are night and day.
**Gotcha:** the very first character portrait rendered in a session can still come out blank even with the
double-render in `portrait()`; re-running `build_assets.cs` a second time fixes it (shaders fully warmed up).
## Where things stand

- Project created with the Unity CLI from the 3D URP template; the spec's scripts are in
  `Assets/RunnerPal/Scripts` (as delivered, plus the fixes below). Compiles clean.
- `RunnerPal > 40 Bölüm Oluştur` generated `Assets/RunnerPal/Levels/Level_01..40`. `build_assets.cs` gives each
  world its road prefab (`Road_Office/Bank/Sales/Dev`), procedural skybox and fog color.
- Prefabs in `Assets/RunnerPal/Prefabs` (built by `build_assets.cs`, real models since 2026-09-27): Gold = Kenney
  coin; Item_Gomlek/Pantolon/Ceket/Ayakkabi = the Quaternius suit parts baked into static meshes
  (`Models/Clothes_*.asset`, skin sub-meshes dropped); Laptop = Kenney laptop; Saat / Telefon = built from primitives;
  SpeedBoost = soda can, Shield = blue star, SlowTrap = tipped coffee cup + puddle, Obstacle_Low = orange concrete
  barrier, Obstacle_High = fence + striped board between warning-light posts, Finish = stretched Kenney door + flags.
  Collider sizes are unchanged. Obstacle debris uses `Obstacle.debrisMaterial` (Kenney models share a texture atlas).
  Old placeholder list, for reference: Gold, Item_<7 items> (colored cube + gold halo), SpeedBoost,
  Shield, SlowTrap (coffee puddle), Obstacle_Low (0.8 m, jump it), Obstacle_High (bar 1.3-2.3 m, slide under),
  Finish (office door), roads (20 m x 7.5 m, 3 lanes of 2.5 m). UI prefabs: RequiredIcon, ShopCard.
- Characters: 4 `CharacterData` (default Mehmet Bey 0, Kemal Abi 150, Burak 300, Hasan Usta 500). Models are
  **Quaternius "Ultimate Modular Men" (CC0)** parts in `ThirdParty/Quaternius_UltimateModularMen/`, assembled by
  `Scripts/Editor/ModularCharacterBuilder.cs` onto one armature:
  - base look: Beach_Body (recolored into a white tank top) + Beach_Legs (red shorts = "don") + Beach_Feet
    (barefoot/flip-flops); heads: Suit (Mehmet), Worker (Kemal), Casual (Burak), Farmer (Hasan), own skin tone each.
  - outfit: Suit_Legs = Pantolon, Suit_Feet = Ayakkabı (each hides the Beach part under it), Suit_Body = Gömlek +
    Ceket (`SuitTorso` recolors its slots: shirt only → white long-sleeve; jacket only → jacket on bare chest;
    both → full suit). Saat / Telefon / Laptop = small props on the wrist bones.
  - Animator on the model root, **humanoid**: `Animation/Avatar_<id>.asset` built by hand in `BuildAvatar` (the
    pack's feet hang off `Root`, so `ParentFeetToLegs` moves them under the lower legs first; humanoid Hips = `Body`,
    because the pack's `Hips` bone only carries the upper body). Clips from Quaternius **Universal Animation Library**
    (`ThirdParty/Quaternius_UAL/UAL1_Standard.fbx`, imported Human): Idle_Loop, Sprint_Loop, Jump_Start → Jump_Loop →
    Jump_Land (timed to the 0.66 s airtime), Roll (slide, 0.7 s), Hit_Chest, Death01 (Fall).
  - Old (replaced): Animator `Animation/Runner.controller` (on `CharacterArmature`): Idle, Run, Slide = Roll clip sped up to
    0.7 s, Hit = HitRecieve, Jump = a held mid-stride Run frame (`Animation/Jump.anim`; the pack has no jump).
  - Download source: Google Drive folder `1USAAquX2JJWuA2m6zol0KUkFe3UkZ8zX` (from the pack page). Public Drive
    folders list without login via `https://drive.google.com/embeddedfolderview?id=<folder>`; files download via
    `https://drive.usercontent.google.com/download?id=<file>&export=download&confirm=t`. We took the "Separate
    Skeletal Meshes and Animations" parts (Suit, Beach, Casual, Casual2, Worker, Punk, Farmer) + `Animations.fbx`.
- Scenes: `Scenes/MainMenu` (gold chip, character shop, OYNA) and `Scenes/Game` (player, camera, GameManager with
  the 40 levels, TrackSpawner, GameUI: briefing / HUD / win / fail). Build order MainMenu, Game.
- Verified in Play mode: briefing shows the job, time and outfit list; a real run moves, collects gold, loses a
  life on an obstacle; finish rules checked by calling GameManager directly —
  0 missing → won, all gold banked; 1 missing + 10 gold → won, paid 5; 1 missing + 1 gold → failed, nothing
  banked; 1 missing + 7 gold → won, paid 4 (half rounded up); 2 missing → failed, nothing banked.

- **Characters look different:** each has its own head, skin, shorts, tank top, suit and tie colors
  (`ModularCharacterBuilder.Look`, specs in `build_assets.cs`). Shop icons are real portraits rendered from the
  model (`portrait` in `build_assets.cs`, renders twice because the first render after compiling can be empty).
- **Hit feedback** (`Scripts/Feedback/HitFeedback.cs`): the obstacle shatters into debris (`Obstacle.Break`,
  `Debris`), camera shake (`CameraFollow.Shake`), red screen flash (`HitFlash` image), lost heart pops
  (`HeartPop`), the runner blinks while invulnerable (only while running), stumble animation; out of lives →
  `Fall` (Death clip, set with CrossFade — a trigger lost against the same-frame Hit/stop). With a shield the
  obstacle still shatters, with a light shake and no life lost.
- **Sounds** (`Scripts/Feedback/RunnerAudio.cs`, clips in `Audio/SFX`, list in `Audio/SFX/LICENSES.txt`): coin
  (random pitch), item, speed boost, shield, coffee trap, jump, slide, break + thud + random "ah!" (ouch1-5, cut
  from a CC0 grunt recording with ffmpeg), win, fail.
- **Music** (Juhani Junkala, CC0, `Audio/Music/`, streamed Vorbis): `MusicPlayer` (persistent, crossfades, keeps
  playing across level reloads) + `SceneMusic` in each scene: menu track in MainMenu, one track per world in Game
  (world = (levelNumber - 1) / 10); ducked on win / fail so the jingles are heard.
- **City:** `build_assets.cs` makes 4 road variants per world (`Road_<World>_0..3`, TrackSpawner picks randomly):
  sidewalks, street lamps, a front row of buildings facing the road and a bigger skyline row, from Kenney City Kit
  Commercial (office / bank / dev) and Suburban (sales), scaled x9. Building shadows are off (phones).
  All third-party packs: `THIRD_PARTY.md`.

## Beyond the spec (user requests, 2026-09-27) — these override the spec where they differ

- **Characters play differently** (`CharacterData.lives / speedMultiplier / jumpMultiplier / description`, used
  by `PlayerController` and `GameManager.LoadLevel`): Mehmet Bey 3 can normal; Şişko John (id `kemal`) 4 can,
  %90 speed, %85 jump, wide body + belly; Sıska Manny (id `burak`) 1 can, %125 speed, thin and tall; Zıpzıp Hasan
  (id `hasan`) 2 can, %140 jump. Ids kept so owned characters stay owned. Body shape: `Look.bodyScale` (model
  root) + `Look.belly` (Abdomen bone girth, chest compensates) in `ModularCharacterBuilder`.
- **Energy drink button** (right edge): the SpeedBoost pickup is now "ENERJİ" and goes into
  `PlayerController.EnergyDrinks` (start 1, max 3); the button / key E spends one: x1.5 speed for 8 s. A hit or
  the coffee trap ends it.
- **Taxi** (rare: 3% per segment from level 2, max one per level, in the free lane): 6 s ride, full-size taxi
  replaces the runner, gold flies to him (`Pickup` magnet), obstacles break without costing a life, speed =
  level max x1.2. 1 s of invulnerability after getting out.
- **Speed lines + FOV kick** while boosted / in the taxi (`Feedback/SpeedLines.cs`).
- **Pause** (II top-left): DEVAM / YENİDEN BAŞLA / MENÜ; also pauses when the app goes to background.
- **New obstacles** (unlock by `Obstacle.minLevel`): open manhole (3, jump it or change lane; sliding in = fall-in
  effect), parked car (5), swaying barrier between the two neighbouring blocked lanes (8, only when the free lane is
  at the edge so it never crosses it), oncoming car (12, drives 14 m inside its own segment). Cars get knocked away.
- **Readable items:** clothes pickups are baked with the arms lowered, every pickup has a floating name tag (TMP) and
  spins without it (`Pickup.spinTarget`). UI icons (`Sprites/Icon_*`) are rendered from the real pickup models;
  the HUD checklist uses `ItemCard` (dark card → green + check when collected, pop animation).
- **Level map** (`UI/LevelMap.cs`, MainMenu): bottom-to-top winding path, one zone per job with the city picture
  (`Sprites/World_*`, rendered from that world's roads), the items that job needs ("YENİ" on the new one) and a
  "TERFİ" badge between jobs. Tap an open level to play it (`SaveSystem.PlayLevel`, not saved);
  `GameManager.NextLevel` goes to the level after the one played. KARAKTERLER opens the shop panel.
- **Time of day per job:** `LevelData.sunColor / sunIntensity / sunAngle / ambientIntensity` (office morning, bank
  golden hour, sales noon, developer night).

## Changes to the delivered scripts

- `ItemType.cs`: added `ItemNames.Get` (Turkish display names); `GameManager` fail message and `GameUI` fallback
  labels use it (was showing "Ayakkabi" instead of "Ayakkabı").
- Feedback hooks only (rules unchanged): `PlayerController` events `Jumped`, `Slid`, `HitObstacle(obstacle,
  damaged)` and `IsInvulnerable`; `OnHitObstacle(Obstacle)` passes the obstacle. `Pickup.Collected` static event.
  `PlayerOutfit` gained `replaces` / `IsWearing` / `Changed`. `CameraFollow` gained `Shake`.
- ffmpeg gotcha: put `-ss/-to` **before** `-i` when cutting clips that get fades, and don't use `loudnorm` on
  sub-second clips (both produced pure silence).

## Next steps (ideas; ask the user)

1. Art: characters, animations, pickups, obstacles, finish and city are real models now. Possible polish: clothes
   pickups are in T-pose (arms out), a job sign on the finish door, props per world. (Old note: road pieces, props per world, pickups, obstacles, finish
   door. Quaternius has CC0 city/office packs that could fit; keep licenses CC0 / owned. A real jump animation
   (e.g. Quaternius Universal Animation Library, needs a humanoid setup) would beat the held pose.
2. Feel: sounds and music, collect/hit effects, camera shake, UI polish (DirectBall got a "neon frame" look —
   RunnerPal may want its own style).
3. Device test (touch swipes, performance; object pooling if needed).
4. EditMode tests for the finish rules (they currently live in `GameManager.ReachFinish`).
5. Ads / IAP later (rewarded "1 more life" or "missing item for free" fit the design), behind an interface.

## How to rebuild

```
cd C:\oyunyapimi\RunnerPall
unity command eval_file --file "C:\oyunyapimi\RunnerPall\Tools\eval\setup_fonts.cs"
unity command eval_file --file "C:\oyunyapimi\RunnerPall\Tools\eval\build_assets.cs" --timeout 120
unity command eval_file --file "C:\oyunyapimi\RunnerPall\Tools\eval\build_scenes.cs" --timeout 120
```

## Gotchas

- Changing Active Input Handling from the CLI opened a restart dialog and froze the Editor (had to force-close).
  Edit `ProjectSettings/ProjectSettings.asset` (`activeInputHandler: 2`) with the Editor closed.
- The first `unity open` of a new project takes several minutes (package import, shader compile); `eval` times out
  until it settles.
- `PlayerOutfit` only hides parts in `Awake`; the game reloads the scene per level, so calling
  `GameManager.LoadLevel` twice in one scene (tests) keeps the earlier outfit visible.
- Unity CLI: the Editor only ticks Play mode while focused. Call `unity command editor_focus` before each check
  (`set_autotick` was not enough). Long `eval_file` runs report "timed out after 5000ms" but keep going.
- Mesh assets created in the same eval as the prefabs that use them: call `AssetDatabase.SaveAssets()` and reload
  the mesh first, otherwise some prefabs loaded the mesh as null.
- **Save data:** the user has real progress now (gold, level, owned characters in `rp_*` PlayerPrefs). Tests must
  not finish a level; check the values before and after.
