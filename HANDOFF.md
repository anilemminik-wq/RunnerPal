# RunnerPal — session handoff

> **Türkçe özet:** Proje kuruldu ve oynanabilir durumda (yer tutucu küp/kapsül modellerle). 40 bölüm, 4 dünya,
> kurallar görev dosyasındaki gibi çalışıyor ve test edildi. Sıradaki adımlar aşağıda "Next steps" bölümünde.
> Yeni oturumda Claude'a "HANDOFF.md'yi oku ve devam edelim" demen yeterli.

Last updated: 2026-09-27. Read this first, then `CLAUDE.md`, then `RUNNERPAL_TASK.md` for the full spec.

## Where things stand

- Project created with the Unity CLI from the 3D URP template; the spec's scripts are in
  `Assets/RunnerPal/Scripts` (as delivered, plus the fixes below). Compiles clean.
- `RunnerPal > 40 Bölüm Oluştur` generated `Assets/RunnerPal/Levels/Level_01..40`. `build_assets.cs` gives each
  world its road prefab (`Road_Office/Bank/Sales/Dev`), procedural skybox and fog color.
- Placeholder prefabs in `Assets/RunnerPal/Prefabs`: Gold, Item_<7 items> (colored cube + gold halo), SpeedBoost,
  Shield, SlowTrap (coffee puddle), Obstacle_Low (0.8 m, jump it), Obstacle_High (bar 1.3-2.3 m, slide under),
  Finish (office door), roads (20 m x 7.5 m, 3 lanes of 2.5 m). UI prefabs: RequiredIcon, ShopCard.
- Characters: 4 `CharacterData` (default Mehmet Bey 0, Kemal Abi 150, Burak 300, Hasan Usta 500) with primitive
  models in `Models/` (underwear + tank top; hidden outfit parts shown by `PlayerOutfit`).
- Scenes: `Scenes/MainMenu` (gold chip, character shop, OYNA) and `Scenes/Game` (player, camera, GameManager with
  the 40 levels, TrackSpawner, GameUI: briefing / HUD / win / fail). Build order MainMenu, Game.
- Verified in Play mode: briefing shows the job, time and outfit list; a real run moves, collects gold, loses a
  life on an obstacle; finish rules checked by calling GameManager directly —
  0 missing → won, all gold banked; 1 missing + 10 gold → won, paid 5; 1 missing + 1 gold → failed, nothing
  banked; 1 missing + 7 gold → won, paid 4 (half rounded up); 2 missing → failed, nothing banked.

## Changes to the delivered scripts

- `ItemType.cs`: added `ItemNames.Get` (Turkish display names); `GameManager` fail message and `GameUI` fallback
  labels use it (was showing "Ayakkabi" instead of "Ayakkabı").

## Next steps (ideas; ask the user)

1. Real art: character model + run/jump/slide animations (Animator params: Running, Speed, Jump, Slide, Hit),
   themed road pieces and props per world. Keep licenses CC0 / owned.
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
