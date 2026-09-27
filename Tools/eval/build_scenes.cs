// RunnerPal scenes (RUNNERPAL_TASK.md "Sahne Kurulumu" steps 3-5): Game and MainMenu, built from code so they can
// be rebuilt at any time. Run build_assets.cs first. Safe to rerun (both scenes are recreated).
const string root = "Assets/RunnerPal/";
if (!UnityEditor.AssetDatabase.IsValidFolder(root + "Scenes")) UnityEditor.AssetDatabase.CreateFolder("Assets/RunnerPal", "Scenes");
if (!UnityEditor.AssetDatabase.IsValidFolder(root + "Prefabs/UI")) UnityEditor.AssetDatabase.CreateFolder("Assets/RunnerPal/Prefabs", "UI");

System.Action<UnityEngine.Object, string, UnityEngine.Object> setRef = (target, field, value) =>
{
    var so = new UnityEditor.SerializedObject(target);
    var p = so.FindProperty(field);
    if (p == null) throw new System.Exception("Missing field " + field + " on " + target.GetType().Name);
    p.objectReferenceValue = value;
    so.ApplyModifiedPropertiesWithoutUndo();
};
System.Func<string, UnityEngine.GameObject> prefab = n => UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(root + "Prefabs/" + n + ".prefab");
System.Func<string, UnityEngine.Sprite> sprite = n => UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(root + "Sprites/" + n + ".png");

var characters = new System.Collections.Generic.List<CharacterData>();
foreach (var id in new[] { "default", "kemal", "burak", "hasan" })
    characters.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterData>(root + "Characters/Character_" + id + ".asset"));
var levels = new LevelData[40];
for (int i = 0; i < 40; i++) levels[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(root + "Levels/Level_" + (i + 1).ToString("00") + ".asset");
if (levels[0] == null || characters[0] == null || prefab("Gold") == null) throw new System.Exception("run build_assets.cs first");

ItemType[] allItems = { ItemType.Gomlek, ItemType.Pantolon, ItemType.Ceket, ItemType.Ayakkabi, ItemType.Saat, ItemType.Telefon, ItemType.Laptop };
var itemLabels = new System.Collections.Generic.Dictionary<ItemType, string>
{
    { ItemType.Gomlek, "Gömlek" }, { ItemType.Pantolon, "Pantolon" }, { ItemType.Ceket, "Ceket" },
    { ItemType.Ayakkabi, "Ayakkabı" }, { ItemType.Saat, "Saat" }, { ItemType.Telefon, "Telefon" }, { ItemType.Laptop, "Laptop" },
};

// ---------- UI helpers ----------
var center = new UnityEngine.Vector2(0.5f, 0.5f);
var textColor = new UnityEngine.Color(1f, 1f, 1f, 1f);
var cardColor = new UnityEngine.Color(0.1f, 0.13f, 0.22f, 0.96f);
var green = new UnityEngine.Color(0.2f, 0.72f, 0.38f, 1f);
var blue = new UnityEngine.Color(0.2f, 0.45f, 0.85f, 1f);
var grey = new UnityEngine.Color(0.3f, 0.34f, 0.45f, 1f);
System.Func<string, UnityEngine.Transform, UnityEngine.Vector2, UnityEngine.Vector2, UnityEngine.Vector2, UnityEngine.RectTransform> rect = (n, parent, anchor, pos, size) =>
{
    var go = new UnityEngine.GameObject(n, typeof(UnityEngine.RectTransform));
    if (parent != null) go.transform.SetParent(parent, false);
    var rt = (UnityEngine.RectTransform)go.transform;
    rt.anchorMin = rt.anchorMax = anchor;
    rt.pivot = center;
    rt.anchoredPosition = pos;
    rt.sizeDelta = size;
    return rt;
};
System.Func<string, UnityEngine.Transform, UnityEngine.RectTransform> stretch = (n, parent) =>
{
    var rt = rect(n, parent, center, UnityEngine.Vector2.zero, UnityEngine.Vector2.zero);
    rt.anchorMin = UnityEngine.Vector2.zero;
    rt.anchorMax = UnityEngine.Vector2.one;
    return rt;
};
System.Func<UnityEngine.RectTransform, string, float, UnityEngine.Color, TMPro.TextMeshProUGUI> text = (rt, value, size, color) =>
{
    var t = rt.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
    t.text = value;
    t.fontSize = size;
    t.color = color;
    t.alignment = TMPro.TextAlignmentOptions.Center;
    t.raycastTarget = false;
    return t;
};
System.Func<UnityEngine.RectTransform, UnityEngine.Color, UnityEngine.UI.Image> panel = (rt, color) =>
{
    var img = rt.gameObject.AddComponent<UnityEngine.UI.Image>();
    img.sprite = sprite("RoundTile");
    img.type = UnityEngine.UI.Image.Type.Sliced;
    img.color = color;
    return img;
};
System.Func<string, UnityEngine.Transform, UnityEngine.Vector2, UnityEngine.Vector2, string, UnityEngine.Color, UnityEngine.UI.Button> button = (n, parent, pos, size, caption, color) =>
{
    var rt = rect(n, parent, center, pos, size);
    var img = panel(rt, color);
    var b = rt.gameObject.AddComponent<UnityEngine.UI.Button>();
    b.targetGraphic = img;
    var label = text(stretch("Label", rt), caption, 54f, textColor);
    label.fontStyle = TMPro.FontStyles.Bold;
    return b;
};
System.Func<UnityEngine.Transform, string, UnityEngine.RectTransform> dimPanel = (parent, n) =>
{
    var rt = stretch(n, parent);
    rt.gameObject.AddComponent<UnityEngine.UI.Image>().color = new UnityEngine.Color(0.02f, 0.03f, 0.06f, 0.78f);
    return rt;
};
// Round tiles need a 9-slice border so corners stay round when stretched.
{
    var tilePath = root + "Sprites/RoundTile.png";
    var imp = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(tilePath);
    if (imp.spriteBorder == UnityEngine.Vector4.zero) { imp.spriteBorder = new UnityEngine.Vector4(40, 40, 40, 40); imp.SaveAndReimport(); }
}
System.Func<UnityEngine.GameObject> makeCanvas = () =>
{
    var canvasGo = new UnityEngine.GameObject("Canvas");
    var canvas = canvasGo.AddComponent<UnityEngine.Canvas>();
    canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceOverlay;
    var scaler = canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
    scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
    scaler.referenceResolution = new UnityEngine.Vector2(1080f, 1920f);
    scaler.matchWidthOrHeight = 0f;
    canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
    var es = new UnityEngine.GameObject("EventSystem");
    es.AddComponent<UnityEngine.EventSystems.EventSystem>();
    es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
    return canvasGo;
};

// ---------- Required-item card (GameUI.requiredIconPrefab): ItemCard = background, 3D icon, name, check ----------
var iconCard = new UnityEngine.GameObject("RequiredIcon", typeof(UnityEngine.RectTransform));
((UnityEngine.RectTransform)iconCard.transform).sizeDelta = new UnityEngine.Vector2(150f, 185f);
var cardBg = panel(rect("Background", iconCard.transform, center, new UnityEngine.Vector2(0f, 0f), new UnityEngine.Vector2(150f, 185f)), new UnityEngine.Color(0.12f, 0.14f, 0.22f, 0.92f));
cardBg.raycastTarget = false;
var iconImg = rect("Icon", iconCard.transform, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -68f), new UnityEngine.Vector2(124f, 124f)).gameObject.AddComponent<UnityEngine.UI.Image>();
iconImg.preserveAspect = true;
iconImg.raycastTarget = false;
var iconLabel = text(rect("Label", iconCard.transform, new UnityEngine.Vector2(0.5f, 0f), new UnityEngine.Vector2(0f, 26f), new UnityEngine.Vector2(146f, 44f)), "Gömlek", 30f, textColor);
iconLabel.fontStyle = TMPro.FontStyles.Bold;
iconLabel.enableAutoSizing = true;
iconLabel.fontSizeMin = 20f;
iconLabel.fontSizeMax = 30f;
var checkImg = rect("Check", iconCard.transform, new UnityEngine.Vector2(1f, 1f), new UnityEngine.Vector2(-14f, -14f), new UnityEngine.Vector2(60f, 60f)).gameObject.AddComponent<UnityEngine.UI.Image>();
checkImg.sprite = sprite("Check");
checkImg.raycastTarget = false;
checkImg.enabled = false;
var itemCard = iconCard.AddComponent<ItemCard>();
itemCard.background = cardBg;
itemCard.icon = iconImg;
itemCard.check = checkImg;
itemCard.label = iconLabel;
var iconPrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(iconCard, root + "Prefabs/UI/RequiredIcon.prefab");
UnityEngine.Object.DestroyImmediate(iconCard);

// ---------- Shop card (ShopManager.shopItemPrefab): ShopItemView on the root ----------
var shopCard = new UnityEngine.GameObject("ShopCard", typeof(UnityEngine.RectTransform));
var shopCardRt = (UnityEngine.RectTransform)shopCard.transform;
shopCardRt.sizeDelta = new UnityEngine.Vector2(440f, 760f);
var shopCardImg = panel(shopCardRt, cardColor);
var shopBtn = shopCard.AddComponent<UnityEngine.UI.Button>();
shopBtn.targetGraphic = shopCardImg;
var shopIcon = rect("Icon", shopCard.transform, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -210f), new UnityEngine.Vector2(380f, 380f)).gameObject.AddComponent<UnityEngine.UI.Image>();
shopIcon.raycastTarget = false;
var shopName = text(rect("Name", shopCard.transform, new UnityEngine.Vector2(0.5f, 0f), new UnityEngine.Vector2(0f, 300f), new UnityEngine.Vector2(410f, 70f)), "Mehmet Bey", 46f, new UnityEngine.Color(1f, 0.85f, 0.3f, 1f));
shopName.fontStyle = TMPro.FontStyles.Bold;
var shopStats = text(rect("Stats", shopCard.transform, new UnityEngine.Vector2(0.5f, 0f), new UnityEngine.Vector2(0f, 190f), new UnityEngine.Vector2(410f, 130f)), "3 can", 32f, textColor);
shopStats.enableAutoSizing = true;
shopStats.fontSizeMin = 22f;
shopStats.fontSizeMax = 32f;
var priceBar = panel(rect("PriceBar", shopCard.transform, new UnityEngine.Vector2(0.5f, 0f), new UnityEngine.Vector2(0f, 70f), new UnityEngine.Vector2(380f, 90f)), green);
priceBar.raycastTarget = false;
var shopPrice = text(rect("Price", priceBar.transform, center, UnityEngine.Vector2.zero, new UnityEngine.Vector2(370f, 80f)), "Seç", 42f, textColor);
shopPrice.fontStyle = TMPro.FontStyles.Bold;
var view = shopCard.AddComponent<ShopItemView>();
view.icon = shopIcon;
view.nameText = shopName;
view.priceText = shopPrice;
view.statsText = shopStats;
view.button = shopBtn;
var shopCardPrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(shopCard, root + "Prefabs/UI/ShopCard.prefab");
UnityEngine.Object.DestroyImmediate(shopCard);

// =====================================================================================================
// Game scene
// =====================================================================================================
var game = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects, UnityEditor.SceneManagement.NewSceneMode.Single);
UnityEngine.RenderSettings.fog = true;
UnityEngine.RenderSettings.fogMode = UnityEngine.FogMode.Linear;
UnityEngine.RenderSettings.fogStartDistance = 45f;
UnityEngine.RenderSettings.fogEndDistance = 140f;
UnityEngine.RenderSettings.skybox = levels[0].skyboxMaterial;
UnityEngine.RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;

var sun = UnityEngine.Object.FindFirstObjectByType<UnityEngine.Light>();
sun.transform.rotation = UnityEngine.Quaternion.Euler(50f, -30f, 0f);
sun.intensity = 1.2f;
sun.shadows = UnityEngine.LightShadows.Soft;

// Player: capsule collider spanning 0..2 m (transform at y = 1), model under ModelRoot with its feet at y = 0.
var playerGo = new UnityEngine.GameObject("Player");
playerGo.tag = "Player";
playerGo.transform.position = new UnityEngine.Vector3(0f, 1f, 0f);
var controller = playerGo.AddComponent<PlayerController>();
var capsule = playerGo.GetComponent<UnityEngine.CapsuleCollider>();
capsule.radius = 0.4f;
capsule.height = 2f;
capsule.center = UnityEngine.Vector3.zero;
var modelRoot = new UnityEngine.GameObject("ModelRoot").transform;
modelRoot.SetParent(playerGo.transform, false);
modelRoot.localPosition = new UnityEngine.Vector3(0f, -1f, 0f);
var defaultModel = (UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(characters[0].modelPrefab, modelRoot);
controller.characters = characters.ToArray();
controller.modelRoot = modelRoot;
// Taxi ride (Pickup Taxi): a full-size Kenney taxi around the runner, hidden until he gets in.
var taxiRide = new UnityEngine.GameObject("TaxiRide").transform;
taxiRide.SetParent(playerGo.transform, false);
{
    var taxiSrc = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/RunnerPal/ThirdParty/Kenney_CarKit/taxi.fbx");
    var car = (UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(taxiSrc, taxiRide);
    var rs = car.GetComponentsInChildren<UnityEngine.Renderer>();
    var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
    car.transform.localScale = UnityEngine.Vector3.one * (4.2f / b.size.z);
    rs = car.GetComponentsInChildren<UnityEngine.Renderer>();
    b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
    // Wheels on the road: the player transform is 1 m above the ground.
    car.transform.position -= new UnityEngine.Vector3(b.center.x - playerGo.transform.position.x, b.min.y, b.center.z - playerGo.transform.position.z - 0.4f);
}
controller.taxiModel = taxiRide.gameObject;
taxiRide.gameObject.SetActive(false);

var cam = UnityEngine.Camera.main;
cam.transform.position = new UnityEngine.Vector3(0f, 4f, -6f);
cam.farClipPlane = 220f;
cam.clearFlags = UnityEngine.CameraClearFlags.Skybox;
var follow = cam.gameObject.AddComponent<CameraFollow>();
follow.target = playerGo.transform;
// Higher and further back than the script default so the runner does not hide the lanes ahead.
follow.offset = new UnityEngine.Vector3(0f, 5.2f, -8.5f);

var spawnerGo = new UnityEngine.GameObject("TrackSpawner");
var spawner = spawnerGo.AddComponent<TrackSpawner>();
// Low / high barriers from the start; manhole (3), parked car (5), oncoming car (12) unlock later (Obstacle.minLevel).
spawner.obstaclePrefabs = new[] { prefab("Obstacle_Low"), prefab("Obstacle_High"), prefab("Obstacle_Low"), prefab("Obstacle_High"), prefab("Obstacle_Manhole"), prefab("Obstacle_ParkedCar"), prefab("Obstacle_Car") };
spawner.swayingObstaclePrefab = prefab("Obstacle_Sway");
spawner.taxiPrefab = prefab("Taxi");
if (spawner.taxiPrefab == null || spawner.swayingObstaclePrefab == null) throw new System.Exception("run build_assets.cs first (Taxi / Obstacle_Sway missing)");
spawner.goldPrefab = prefab("Gold");
spawner.speedBoostPrefab = prefab("SpeedBoost");
spawner.shieldPrefab = prefab("Shield");
spawner.slowTrapPrefab = prefab("SlowTrap");
spawner.finishPrefab = prefab("Finish");
var itemPrefabs = new TrackSpawner.ItemPrefab[allItems.Length];
for (int i = 0; i < allItems.Length; i++) itemPrefabs[i] = new TrackSpawner.ItemPrefab { item = allItems[i], prefab = prefab("Item_" + allItems[i]) };
spawner.itemPrefabs = itemPrefabs;

var gmGo = new UnityEngine.GameObject("GameManager");
var gm = gmGo.AddComponent<GameManager>();
gm.levels = levels;
gm.player = controller;
gm.spawner = spawner;

// ---------- Game UI ----------
var canvasGo = makeCanvas();
var ui = canvasGo.AddComponent<GameUI>();
var cv = canvasGo.transform;

// Briefing: job, time limit and the outfit list, then BAŞLA.
var briefing = dimPanel(cv, "BriefingPanel");
var bCard = rect("Card", briefing, center, new UnityEngine.Vector2(0f, 40f), new UnityEngine.Vector2(960f, 1000f));
panel(bCard, cardColor);
var bTitle = text(rect("Title", bCard, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -150f), new UnityEngine.Vector2(880f, 220f)), "Bölüm 1 - Ofis Çalışanı", 52f, textColor);
bTitle.fontStyle = TMPro.FontStyles.Bold;
text(rect("NeedLabel", bCard, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -310f), new UnityEngine.Vector2(880f, 60f)), "Yoldan toplaman gerekenler:", 38f, new UnityEngine.Color(0.85f, 0.9f, 1f, 0.9f));
var reqList = rect("RequiredList", bCard, center, new UnityEngine.Vector2(0f, -30f), new UnityEngine.Vector2(880f, 400f));
var reqGrid = reqList.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();
reqGrid.cellSize = new UnityEngine.Vector2(160f, 185f);
reqGrid.spacing = new UnityEngine.Vector2(28f, 20f);
reqGrid.childAlignment = UnityEngine.TextAnchor.MiddleCenter;
reqGrid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
reqGrid.constraintCount = 4;
var startButton = button("StartButton", bCard, new UnityEngine.Vector2(0f, -380f), new UnityEngine.Vector2(520f, 150f), "BAŞLA", green);

// HUD: dark top bar with pause (left), gold, timer (center), hearts (right); outfit checklist cards under it;
// energy-drink button on the right edge; taxi banner while riding.
var hud = stretch("HUD", cv);
var topBar = rect("TopBar", hud, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -100f), new UnityEngine.Vector2(1080f, 200f));
var topImg = topBar.gameObject.AddComponent<UnityEngine.UI.Image>();
topImg.color = new UnityEngine.Color(0.03f, 0.04f, 0.08f, 0.55f);
topImg.raycastTarget = false;
var pauseBtn = button("PauseButton", hud, UnityEngine.Vector2.zero, new UnityEngine.Vector2(110f, 110f), "II", new UnityEngine.Color(0.15f, 0.18f, 0.28f, 0.95f));
var pauseRt = (UnityEngine.RectTransform)pauseBtn.transform;
pauseRt.anchorMin = pauseRt.anchorMax = new UnityEngine.Vector2(0f, 1f);
pauseRt.anchoredPosition = new UnityEngine.Vector2(80f, -100f);
pauseBtn.GetComponentInChildren<TMPro.TextMeshProUGUI>().fontSize = 56f;
var coinIcon = rect("CoinIcon", hud, new UnityEngine.Vector2(0f, 1f), new UnityEngine.Vector2(200f, -100f), new UnityEngine.Vector2(76f, 76f)).gameObject.AddComponent<UnityEngine.UI.Image>();
coinIcon.sprite = sprite("Coin");
coinIcon.raycastTarget = false;
var goldText = text(rect("Gold", hud, new UnityEngine.Vector2(0f, 1f), new UnityEngine.Vector2(325f, -100f), new UnityEngine.Vector2(160f, 90f)), "0", 60f, textColor);
goldText.alignment = TMPro.TextAlignmentOptions.Left;
goldText.fontStyle = TMPro.FontStyles.Bold;
var timerText = text(rect("Timer", hud, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(40f, -100f), new UnityEngine.Vector2(240f, 110f)), "60", 84f, textColor);
timerText.fontStyle = TMPro.FontStyles.Bold;
var hearts = new UnityEngine.UI.Image[5];
for (int i = 0; i < 5; i++)
{
    var h = rect("Heart" + (i + 1), hud, new UnityEngine.Vector2(1f, 1f), new UnityEngine.Vector2(-60f - (4 - i) * 72f, -100f), new UnityEngine.Vector2(66f, 66f)).gameObject.AddComponent<UnityEngine.UI.Image>();
    h.sprite = sprite("Heart");
    h.raycastTarget = false;
    hearts[i] = h;
}
var checklist = rect("Checklist", hud, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -310f), new UnityEngine.Vector2(1060f, 190f));
var checkLayout = checklist.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
checkLayout.spacing = 10f;
checkLayout.childAlignment = UnityEngine.TextAnchor.MiddleCenter;
checkLayout.childControlWidth = checkLayout.childControlHeight = false;
checkLayout.childForceExpandWidth = checkLayout.childForceExpandHeight = false;
checklist.localScale = new UnityEngine.Vector3(0.85f, 0.85f, 1f);

// Energy drink button (right edge, thumb height): icon, count badge, ring showing the time left while active.
var energyRt = rect("EnergyButton", hud, new UnityEngine.Vector2(1f, 0.5f), new UnityEngine.Vector2(-110f, -330f), new UnityEngine.Vector2(180f, 180f));
var energyBg = energyRt.gameObject.AddComponent<UnityEngine.UI.Image>();
energyBg.sprite = sprite("Circle");
energyBg.color = new UnityEngine.Color(0.12f, 0.45f, 0.2f, 0.95f);
var energyBtn = energyRt.gameObject.AddComponent<UnityEngine.UI.Button>();
energyBtn.targetGraphic = energyBg;
var energyColors = energyBtn.colors;
energyColors.disabledColor = new UnityEngine.Color(0.45f, 0.45f, 0.5f, 1f);
energyBtn.colors = energyColors;
var energyGroup = energyRt.gameObject.AddComponent<UnityEngine.CanvasGroup>();
var energyFill = rect("TimeRing", energyRt, center, UnityEngine.Vector2.zero, new UnityEngine.Vector2(204f, 204f)).gameObject.AddComponent<UnityEngine.UI.Image>();
energyFill.sprite = sprite("Circle");
energyFill.type = UnityEngine.UI.Image.Type.Filled;
energyFill.fillMethod = UnityEngine.UI.Image.FillMethod.Radial360;
energyFill.fillOrigin = 2;
energyFill.color = new UnityEngine.Color(0.55f, 1f, 0.45f, 0.6f);
energyFill.raycastTarget = false;
energyFill.enabled = false;
var energyIcon = rect("Icon", energyRt, center, new UnityEngine.Vector2(0f, 8f), new UnityEngine.Vector2(140f, 140f)).gameObject.AddComponent<UnityEngine.UI.Image>();
energyIcon.sprite = sprite("Icon_Energy");
energyIcon.preserveAspect = true;
energyIcon.raycastTarget = false;
var energyLabel = text(rect("Label", energyRt, new UnityEngine.Vector2(0.5f, 0f), new UnityEngine.Vector2(0f, -22f), new UnityEngine.Vector2(200f, 44f)), "ENERJİ", 32f, textColor);
energyLabel.fontStyle = TMPro.FontStyles.Bold;
var badge = rect("Count", energyRt, new UnityEngine.Vector2(1f, 1f), new UnityEngine.Vector2(-18f, -18f), new UnityEngine.Vector2(64f, 64f));
var badgeImg = badge.gameObject.AddComponent<UnityEngine.UI.Image>();
badgeImg.sprite = sprite("Circle");
badgeImg.color = new UnityEngine.Color(0.95f, 0.25f, 0.2f, 1f);
badgeImg.raycastTarget = false;
var energyCount = text(stretch("Text", badge), "1", 40f, textColor);
energyCount.fontStyle = TMPro.FontStyles.Bold;

// Taxi banner under the checklist while riding.
var taxiBanner = rect("TaxiBanner", hud, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -490f), new UnityEngine.Vector2(560f, 120f));
panel(taxiBanner, new UnityEngine.Color(1f, 0.78f, 0.1f, 0.95f)).raycastTarget = false;
var taxiIcon = rect("Icon", taxiBanner, new UnityEngine.Vector2(0f, 0.5f), new UnityEngine.Vector2(85f, 0f), new UnityEngine.Vector2(130f, 110f)).gameObject.AddComponent<UnityEngine.UI.Image>();
taxiIcon.sprite = sprite("Icon_Taxi");
taxiIcon.preserveAspect = true;
taxiIcon.raycastTarget = false;
var taxiText = text(rect("Text", taxiBanner, center, new UnityEngine.Vector2(60f, 0f), new UnityEngine.Vector2(400f, 100f)), "TAKSİ! 6", 60f, new UnityEngine.Color(0.15f, 0.1f, 0f, 1f));
taxiText.fontStyle = TMPro.FontStyles.Bold;

// Speed lines: full screen, under the HUD so its buttons stay on top.
var lines = stretch("SpeedLines", cv);
lines.SetSiblingIndex(hud.GetSiblingIndex());
var speedLines = lines.gameObject.AddComponent<SpeedLines>();
speedLines.player = controller;
speedLines.cam = cam;

// Pause panel: DEVAM / YENİDEN BAŞLA / MENÜ.
var pausePanel = dimPanel(cv, "PausePanel");
var pCard = rect("Card", pausePanel, center, UnityEngine.Vector2.zero, new UnityEngine.Vector2(820f, 700f));
panel(pCard, cardColor);
text(rect("Title", pCard, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -120f), new UnityEngine.Vector2(760f, 120f)), "DURAKLATILDI", 70f, textColor).fontStyle = TMPro.FontStyles.Bold;
var resumeBtn = button("ResumeButton", pCard, new UnityEngine.Vector2(0f, 10f), new UnityEngine.Vector2(600f, 140f), "DEVAM", green);
var pauseRestart = button("RestartButton", pCard, new UnityEngine.Vector2(0f, -150f), new UnityEngine.Vector2(600f, 110f), "YENİDEN BAŞLA", blue);
var pauseMenu = button("MenuButton", pCard, new UnityEngine.Vector2(0f, -280f), new UnityEngine.Vector2(600f, 110f), "MENÜ", grey);
UnityEditor.Events.UnityEventTools.AddPersistentListener(pauseRestart.onClick, gm.RestartLevel);

// Win / Fail panels
System.Func<string, string, UnityEngine.Color, UnityEngine.RectTransform> resultPanel = (n, title, titleColor) =>
{
    var p = dimPanel(cv, n);
    var card = rect("Card", p, center, UnityEngine.Vector2.zero, new UnityEngine.Vector2(900f, 820f));
    panel(card, cardColor);
    var t = text(rect("Title", card, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -110f), new UnityEngine.Vector2(820f, 120f)), title, 72f, titleColor);
    t.fontStyle = TMPro.FontStyles.Bold;
    return card;
};
var winCard = resultPanel("WinPanel", "İŞE YETİŞTİN!", new UnityEngine.Color(0.45f, 1f, 0.55f, 1f));
var winText = text(rect("Text", winCard, center, new UnityEngine.Vector2(0f, 60f), new UnityEngine.Vector2(820f, 240f)), "", 42f, textColor);
var nextBtn = button("NextButton", winCard, new UnityEngine.Vector2(0f, -170f), new UnityEngine.Vector2(620f, 130f), "SONRAKİ BÖLÜM", green);
var winMenu = button("MenuButton", winCard, new UnityEngine.Vector2(0f, -320f), new UnityEngine.Vector2(620f, 110f), "MENÜ", grey);
var failCard = resultPanel("FailPanel", "BAŞARISIZ", new UnityEngine.Color(1f, 0.45f, 0.4f, 1f));
var failText = text(rect("Text", failCard, center, new UnityEngine.Vector2(0f, 60f), new UnityEngine.Vector2(820f, 240f)), "", 44f, textColor);
var retryBtn = button("RetryButton", failCard, new UnityEngine.Vector2(0f, -170f), new UnityEngine.Vector2(620f, 130f), "TEKRAR", blue);
var failMenu = button("MenuButton", failCard, new UnityEngine.Vector2(0f, -320f), new UnityEngine.Vector2(620f, 110f), "MENÜ", grey);
UnityEditor.Events.UnityEventTools.AddPersistentListener(nextBtn.onClick, gm.NextLevel);
UnityEditor.Events.UnityEventTools.AddPersistentListener(retryBtn.onClick, gm.RestartLevel);
UnityEditor.Events.UnityEventTools.AddPersistentListener(winMenu.onClick, gm.GoToMenu);
UnityEditor.Events.UnityEventTools.AddPersistentListener(failMenu.onClick, gm.GoToMenu);

ui.briefingPanel = briefing.gameObject;
ui.briefingTitle = bTitle;
ui.requiredListParent = reqList;
ui.requiredIconPrefab = iconPrefab;
ui.startButton = startButton;
ui.hudPanel = hud.gameObject;
ui.goldText = goldText;
ui.timerText = timerText;
ui.heartIcons = hearts;
ui.checklistParent = checklist;
ui.pauseButton = pauseBtn;
ui.pausePanel = pausePanel.gameObject;
ui.resumeButton = resumeBtn;
ui.pauseMenuButton = pauseMenu;
ui.energyButton = energyBtn;
ui.energyCountText = energyCount;
ui.energyTimerFill = energyFill;
ui.energyGroup = energyGroup;
ui.taxiBanner = taxiBanner.gameObject;
ui.taxiText = taxiText;
ui.winPanel = winCard.parent.gameObject;
ui.winText = winText;
ui.failPanel = failCard.parent.gameObject;
ui.failText = failText;
var icons = new GameUI.ItemIcon[allItems.Length];
for (int i = 0; i < allItems.Length; i++) icons[i] = new GameUI.ItemIcon { item = allItems[i], sprite = sprite("Icon_" + allItems[i]), label = itemLabels[allItems[i]] };
ui.itemIcons = icons;
hud.gameObject.SetActive(false);
winCard.parent.gameObject.SetActive(false);
pausePanel.gameObject.SetActive(false);
failCard.parent.gameObject.SetActive(false);

UnityEditor.EditorUtility.SetDirty(ui);

// ---------- Hit feedback + sounds ----------
// Red flash over the whole screen (under the panels, above the HUD) when a life is lost.
var flash = stretch("HitFlash", cv);
flash.SetSiblingIndex(hud.GetSiblingIndex() + 1);
var flashImg = flash.gameObject.AddComponent<UnityEngine.UI.Image>();
flashImg.color = new UnityEngine.Color(0.9f, 0.05f, 0.05f, 0f);
flashImg.raycastTarget = false;
var feedback = gmGo.AddComponent<HitFeedback>();
feedback.player = controller;
feedback.cameraFollow = follow;
feedback.flash = flashImg;
feedback.hearts = hearts;

System.Func<string, UnityEngine.AudioClip> sfx = n => UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AudioClip>(root + "Audio/SFX/" + n);
var audioGo = new UnityEngine.GameObject("RunnerAudio");
var runnerAudio = audioGo.AddComponent<RunnerAudio>();
runnerAudio.player = controller;
runnerAudio.coin = new[] { sfx("coin.ogg"), sfx("coin2.ogg") };
runnerAudio.item = sfx("item.ogg");
runnerAudio.powerUp = sfx("powerup.ogg");
runnerAudio.shield = sfx("shield.ogg");
runnerAudio.slowTrap = sfx("slowtrap.ogg");
runnerAudio.jump = sfx("jump.ogg");
runnerAudio.slide = sfx("slide.ogg");
runnerAudio.win = sfx("win.ogg");
runnerAudio.fail = sfx("fail.ogg");
runnerAudio.breakObstacle = new[] { sfx("break1.ogg"), sfx("break2.ogg") };
runnerAudio.thud = sfx("thud.ogg");
runnerAudio.energy = sfx("powerup.ogg");
runnerAudio.taxi = sfx("win.ogg");
runnerAudio.ouch = new[] { sfx("ouch1.wav"), sfx("ouch2.wav"), sfx("ouch3.wav"), sfx("ouch4.wav"), sfx("ouch5.wav") };
if (runnerAudio.ouch[0] == null || runnerAudio.coin[0] == null) throw new System.Exception("sounds missing in Audio/SFX");
string gamePath = root + "Scenes/Game.unity";
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(game, gamePath);

// Music (Juhani Junkala, CC0): streamed so the long tracks do not sit in memory; one per world, see SceneMusic.
System.Func<string, UnityEngine.AudioClip> music = n =>
{
    string p = root + "Audio/Music/" + n + ".ogg";
    var imp = (UnityEditor.AudioImporter)UnityEditor.AssetImporter.GetAtPath(p);
    var st = imp.defaultSampleSettings;
    st.loadType = UnityEngine.AudioClipLoadType.Streaming;
    st.compressionFormat = UnityEngine.AudioCompressionFormat.Vorbis;
    st.quality = 0.5f;
    imp.defaultSampleSettings = st;
    imp.loadInBackground = true;
    imp.SaveAndReimport();
    return UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AudioClip>(p);
};
var gameMusic = new UnityEngine.GameObject("Music").AddComponent<SceneMusic>();
gameMusic.worldClips = new[] { music("world_office"), music("world_bank"), music("world_sales"), music("world_dev") };
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(game, gamePath);

// =====================================================================================================
// MainMenu scene: level map (four jobs), total gold, characters panel, OYNA
// =====================================================================================================
var menu = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects, UnityEditor.SceneManagement.NewSceneMode.Single);
var menuCam = UnityEngine.Camera.main;
menuCam.clearFlags = UnityEngine.CameraClearFlags.SolidColor;
menuCam.backgroundColor = new UnityEngine.Color(0.06f, 0.08f, 0.14f);
var menuCanvas = makeCanvas();
var mc = menuCanvas.transform;
var bg = stretch("Background", mc);
bg.gameObject.AddComponent<UnityEngine.UI.Image>().color = new UnityEngine.Color(0.07f, 0.1f, 0.18f, 1f);
// Level map (scrolls bottom to top through the four jobs), a top bar with the title and gold, and a bottom bar with
// KARAKTERLER (shop panel) and OYNA (current level).
var mapScroll = rect("MapScroll", mc, center, UnityEngine.Vector2.zero, UnityEngine.Vector2.zero);
mapScroll.anchorMin = new UnityEngine.Vector2(0f, 0f);
mapScroll.anchorMax = new UnityEngine.Vector2(1f, 1f);
mapScroll.offsetMin = new UnityEngine.Vector2(0f, 250f);
mapScroll.offsetMax = new UnityEngine.Vector2(0f, -200f);
var msr = mapScroll.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
msr.horizontal = false;
msr.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
msr.scrollSensitivity = 40f;
var mapViewport = stretch("Viewport", mapScroll);
mapViewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
mapViewport.gameObject.AddComponent<UnityEngine.UI.Image>().color = new UnityEngine.Color(0f, 0f, 0f, 0f);
var mapContent = rect("Content", mapViewport, new UnityEngine.Vector2(0.5f, 0f), UnityEngine.Vector2.zero, new UnityEngine.Vector2(1080f, 4000f));
mapContent.pivot = new UnityEngine.Vector2(0.5f, 0f);
mapContent.gameObject.AddComponent<LevelMapContent>();
msr.viewport = mapViewport;
msr.content = mapContent;

var topBarM = rect("TopBar", mc, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -100f), new UnityEngine.Vector2(1080f, 200f));
topBarM.gameObject.AddComponent<UnityEngine.UI.Image>().color = new UnityEngine.Color(0.04f, 0.05f, 0.1f, 0.97f);
var title = text(rect("Title", topBarM, new UnityEngine.Vector2(0.5f, 0.5f), new UnityEngine.Vector2(90f, 22f), new UnityEngine.Vector2(600f, 110f)), "RunnerPal", 92f, new UnityEngine.Color(1f, 0.85f, 0.3f, 1f));
title.fontStyle = TMPro.FontStyles.Bold;
text(rect("Tagline", topBarM, new UnityEngine.Vector2(0.5f, 0.5f), new UnityEngine.Vector2(90f, -56f), new UnityEngine.Vector2(600f, 50f)), "İşe geç kalma!", 36f, new UnityEngine.Color(0.85f, 0.9f, 1f, 0.9f));
var goldChip = rect("GoldChip", topBarM, new UnityEngine.Vector2(0f, 0.5f), new UnityEngine.Vector2(170f, 0f), new UnityEngine.Vector2(280f, 100f));
panel(goldChip, cardColor);
var mCoin = rect("Coin", goldChip, new UnityEngine.Vector2(0f, 0.5f), new UnityEngine.Vector2(55f, 0f), new UnityEngine.Vector2(70f, 70f)).gameObject.AddComponent<UnityEngine.UI.Image>();
mCoin.sprite = sprite("Coin");
mCoin.raycastTarget = false;
var totalGold = text(rect("Total", goldChip, new UnityEngine.Vector2(0f, 0.5f), new UnityEngine.Vector2(175f, 0f), new UnityEngine.Vector2(170f, 80f)), "0", 52f, textColor);
totalGold.alignment = TMPro.TextAlignmentOptions.Left;
totalGold.fontStyle = TMPro.FontStyles.Bold;

var bottomBar = rect("BottomBar", mc, new UnityEngine.Vector2(0.5f, 0f), new UnityEngine.Vector2(0f, 125f), new UnityEngine.Vector2(1080f, 250f));
bottomBar.gameObject.AddComponent<UnityEngine.UI.Image>().color = new UnityEngine.Color(0.04f, 0.05f, 0.1f, 0.97f);
var charsBtn = button("CharactersButton", bottomBar, new UnityEngine.Vector2(-300f, 0f), new UnityEngine.Vector2(400f, 160f), "KARAKTERLER", blue);
charsBtn.GetComponentInChildren<TMPro.TextMeshProUGUI>().fontSize = 44f;
var playBtn = button("PlayButton", bottomBar, new UnityEngine.Vector2(215f, 0f), new UnityEngine.Vector2(590f, 170f), "OYNA", green);
var playText = playBtn.GetComponentInChildren<TMPro.TextMeshProUGUI>();
playText.fontSize = 56f;

// Characters panel (shop): big full-body cards with lives / speed / jump, horizontal scroll, KAPAT.
var shopPanel = dimPanel(mc, "CharactersPanel");
text(rect("ShopTitle", shopPanel, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -330f), new UnityEngine.Vector2(1000f, 90f)), "KARAKTERLER", 64f, textColor).fontStyle = TMPro.FontStyles.Bold;
text(rect("ShopHint", shopPanel, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -410f), new UnityEngine.Vector2(1000f, 60f)), "Her karakter farklı oynar: can, hız ve zıplama", 34f, new UnityEngine.Color(0.85f, 0.9f, 1f, 0.9f));
var scroll = rect("ShopScroll", shopPanel, center, new UnityEngine.Vector2(0f, 20f), new UnityEngine.Vector2(1080f, 820f));
var sr = scroll.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
sr.vertical = false;
var viewport = stretch("Viewport", scroll);
viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
viewport.gameObject.AddComponent<UnityEngine.UI.Image>().color = new UnityEngine.Color(0f, 0f, 0f, 0f);
var content = rect("Content", viewport, new UnityEngine.Vector2(0f, 0.5f), UnityEngine.Vector2.zero, new UnityEngine.Vector2(0f, 780f));
content.anchorMin = new UnityEngine.Vector2(0f, 0f);
content.anchorMax = new UnityEngine.Vector2(0f, 1f);
content.pivot = new UnityEngine.Vector2(0f, 0.5f);
var row = content.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
row.spacing = 30f;
row.padding = new UnityEngine.RectOffset(60, 60, 10, 10);
row.childAlignment = UnityEngine.TextAnchor.MiddleLeft;
row.childControlWidth = row.childControlHeight = false;
row.childForceExpandWidth = row.childForceExpandHeight = false;
content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
sr.viewport = viewport;
sr.content = content;
var closeShop = button("CloseButton", shopPanel, new UnityEngine.Vector2(0f, -560f), new UnityEngine.Vector2(520f, 140f), "KAPAT", grey);

var shop = menuCanvas.AddComponent<ShopManager>();
shop.characters = characters.ToArray();
shop.listParent = content;
shop.shopItemPrefab = shopCardPrefab;
shop.totalGoldText = totalGold;
UnityEditor.EditorUtility.SetDirty(shop);

var map = menuCanvas.AddComponent<LevelMap>();
map.levels = levels;
map.characters = characters.ToArray();
map.scroll = msr;
map.content = mapContent;
map.circle = sprite("Circle");
map.roundTile = sprite("RoundTile");
map.check = sprite("Check");
map.lockIcon = sprite("Lock");
map.playButtonText = playText;
var mapIcons = new GameUI.ItemIcon[allItems.Length];
for (int i = 0; i < allItems.Length; i++) mapIcons[i] = new GameUI.ItemIcon { item = allItems[i], sprite = sprite("Icon_" + allItems[i]), label = itemLabels[allItems[i]] };
map.itemIcons = mapIcons;
map.worlds = new[]
{
    new LevelMap.World { title = "Ofis Çalışanı", subtitle = "Sabah, şehir merkezi", color = new UnityEngine.Color(0.35f, 0.65f, 1f), cityImage = sprite("World_Office") },
    new LevelMap.World { title = "Banka Memuru", subtitle = "Akşamüstü, gökdelenler", color = new UnityEngine.Color(1f, 0.72f, 0.3f), cityImage = sprite("World_Bank") },
    new LevelMap.World { title = "Satış Temsilcisi", subtitle = "Öğle güneşi, banliyö", color = new UnityEngine.Color(0.4f, 0.85f, 0.45f), cityImage = sprite("World_Sales") },
    new LevelMap.World { title = "Yazılımcı", subtitle = "Gece mesaisi", color = new UnityEngine.Color(0.7f, 0.5f, 1f), cityImage = sprite("World_Dev") },
};
UnityEditor.EditorUtility.SetDirty(map);
UnityEditor.Events.UnityEventTools.AddPersistentListener(playBtn.onClick, map.PlayCurrent);
UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(charsBtn.onClick, shopPanel.gameObject.SetActive, true);
UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(closeShop.onClick, shopPanel.gameObject.SetActive, false);
UnityEditor.Events.UnityEventTools.AddPersistentListener(closeShop.onClick, map.Build);
shopPanel.gameObject.SetActive(false);
string menuPath = root + "Scenes/MainMenu.unity";
new UnityEngine.GameObject("Music").AddComponent<SceneMusic>().menuClip = music("menu");
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(menu, menuPath);

UnityEditor.EditorBuildSettings.scenes = new[]
{
    new UnityEditor.EditorBuildSettingsScene(menuPath, true),
    new UnityEditor.EditorBuildSettingsScene(gamePath, true),
};
UnityEditor.AssetDatabase.SaveAssets();
return "scenes ok";
