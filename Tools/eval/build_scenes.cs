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

// ---------- Required-item icon card (GameUI.requiredIconPrefab): Icon (first Image) + Label ----------
var iconCard = new UnityEngine.GameObject("RequiredIcon", typeof(UnityEngine.RectTransform));
((UnityEngine.RectTransform)iconCard.transform).sizeDelta = new UnityEngine.Vector2(150f, 180f);
var iconImg = rect("Icon", iconCard.transform, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -60f), new UnityEngine.Vector2(110f, 110f)).gameObject.AddComponent<UnityEngine.UI.Image>();
iconImg.raycastTarget = false;
var iconLabel = text(rect("Label", iconCard.transform, new UnityEngine.Vector2(0.5f, 0f), new UnityEngine.Vector2(0f, 26f), new UnityEngine.Vector2(160f, 50f)), "Gömlek", 30f, textColor);
iconLabel.enableAutoSizing = true;
iconLabel.fontSizeMin = 20f;
iconLabel.fontSizeMax = 30f;
var iconPrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(iconCard, root + "Prefabs/UI/RequiredIcon.prefab");
UnityEngine.Object.DestroyImmediate(iconCard);

// ---------- Shop card (ShopManager.shopItemPrefab): ShopItemView on the root ----------
var shopCard = new UnityEngine.GameObject("ShopCard", typeof(UnityEngine.RectTransform));
var shopCardRt = (UnityEngine.RectTransform)shopCard.transform;
shopCardRt.sizeDelta = new UnityEngine.Vector2(420f, 520f);
var shopCardImg = panel(shopCardRt, cardColor);
var shopBtn = shopCard.AddComponent<UnityEngine.UI.Button>();
shopBtn.targetGraphic = shopCardImg;
var shopIcon = rect("Icon", shopCard.transform, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -170f), new UnityEngine.Vector2(260f, 260f)).gameObject.AddComponent<UnityEngine.UI.Image>();
shopIcon.raycastTarget = false;
var shopName = text(rect("Name", shopCard.transform, new UnityEngine.Vector2(0.5f, 0f), new UnityEngine.Vector2(0f, 160f), new UnityEngine.Vector2(380f, 70f)), "Mehmet Bey", 44f, textColor);
shopName.fontStyle = TMPro.FontStyles.Bold;
var shopPrice = text(rect("Price", shopCard.transform, new UnityEngine.Vector2(0.5f, 0f), new UnityEngine.Vector2(0f, 70f), new UnityEngine.Vector2(380f, 70f)), "Seç", 40f, new UnityEngine.Color(1f, 0.85f, 0.3f, 1f));
var view = shopCard.AddComponent<ShopItemView>();
view.icon = shopIcon;
view.nameText = shopName;
view.priceText = shopPrice;
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
spawner.obstaclePrefabs = new[] { prefab("Obstacle_Low"), prefab("Obstacle_High") };
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

// HUD: gold (top-left), timer (top-center), hearts (top-right), outfit checklist under the top bar.
var hud = stretch("HUD", cv);
var coinIcon = rect("CoinIcon", hud, new UnityEngine.Vector2(0f, 1f), new UnityEngine.Vector2(80f, -100f), new UnityEngine.Vector2(80f, 80f)).gameObject.AddComponent<UnityEngine.UI.Image>();
coinIcon.sprite = sprite("Coin");
coinIcon.raycastTarget = false;
var goldText = text(rect("Gold", hud, new UnityEngine.Vector2(0f, 1f), new UnityEngine.Vector2(210f, -100f), new UnityEngine.Vector2(160f, 90f)), "0", 60f, textColor);
goldText.alignment = TMPro.TextAlignmentOptions.Left;
goldText.fontStyle = TMPro.FontStyles.Bold;
var timerText = text(rect("Timer", hud, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -100f), new UnityEngine.Vector2(300f, 110f)), "60", 80f, textColor);
timerText.fontStyle = TMPro.FontStyles.Bold;
var hearts = new UnityEngine.UI.Image[3];
for (int i = 0; i < 3; i++)
{
    var h = rect("Heart" + (i + 1), hud, new UnityEngine.Vector2(1f, 1f), new UnityEngine.Vector2(-70f - (2 - i) * 90f, -100f), new UnityEngine.Vector2(80f, 80f)).gameObject.AddComponent<UnityEngine.UI.Image>();
    h.sprite = sprite("Heart");
    h.raycastTarget = false;
    hearts[i] = h;
}
var checklist = rect("Checklist", hud, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -230f), new UnityEngine.Vector2(1040f, 150f));
var checkLayout = checklist.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
checkLayout.spacing = 8f;
checkLayout.childAlignment = UnityEngine.TextAnchor.MiddleCenter;
checkLayout.childControlWidth = checkLayout.childControlHeight = false;
checkLayout.childForceExpandWidth = checkLayout.childForceExpandHeight = false;
checklist.localScale = new UnityEngine.Vector3(0.75f, 0.75f, 1f);

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
ui.winPanel = winCard.parent.gameObject;
ui.winText = winText;
ui.failPanel = failCard.parent.gameObject;
ui.failText = failText;
var icons = new GameUI.ItemIcon[allItems.Length];
for (int i = 0; i < allItems.Length; i++) icons[i] = new GameUI.ItemIcon { item = allItems[i], sprite = sprite("Icon_" + allItems[i]), label = itemLabels[allItems[i]] };
ui.itemIcons = icons;
hud.gameObject.SetActive(false);
winCard.parent.gameObject.SetActive(false);
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
// MainMenu scene: title, total gold, character shop, OYNA
// =====================================================================================================
var menu = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects, UnityEditor.SceneManagement.NewSceneMode.Single);
var menuCam = UnityEngine.Camera.main;
menuCam.clearFlags = UnityEngine.CameraClearFlags.SolidColor;
menuCam.backgroundColor = new UnityEngine.Color(0.06f, 0.08f, 0.14f);
var menuCanvas = makeCanvas();
var mc = menuCanvas.transform;
var bg = stretch("Background", mc);
bg.gameObject.AddComponent<UnityEngine.UI.Image>().color = new UnityEngine.Color(0.07f, 0.1f, 0.18f, 1f);
var title = text(rect("Title", mc, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -300f), new UnityEngine.Vector2(1000f, 180f)), "RunnerPal", 130f, new UnityEngine.Color(1f, 0.85f, 0.3f, 1f));
title.fontStyle = TMPro.FontStyles.Bold;
text(rect("Tagline", mc, new UnityEngine.Vector2(0.5f, 1f), new UnityEngine.Vector2(0f, -420f), new UnityEngine.Vector2(1000f, 70f)), "İşe geç kalma!", 48f, new UnityEngine.Color(0.85f, 0.9f, 1f, 0.9f));
var goldChip = rect("GoldChip", mc, new UnityEngine.Vector2(0f, 1f), new UnityEngine.Vector2(170f, -100f), new UnityEngine.Vector2(280f, 100f));
panel(goldChip, cardColor);
var mCoin = rect("Coin", goldChip, new UnityEngine.Vector2(0f, 0.5f), new UnityEngine.Vector2(55f, 0f), new UnityEngine.Vector2(70f, 70f)).gameObject.AddComponent<UnityEngine.UI.Image>();
mCoin.sprite = sprite("Coin");
mCoin.raycastTarget = false;
var totalGold = text(rect("Total", goldChip, new UnityEngine.Vector2(0f, 0.5f), new UnityEngine.Vector2(175f, 0f), new UnityEngine.Vector2(170f, 80f)), "0", 52f, textColor);
totalGold.alignment = TMPro.TextAlignmentOptions.Left;
totalGold.fontStyle = TMPro.FontStyles.Bold;

text(rect("ShopTitle", mc, center, new UnityEngine.Vector2(0f, 330f), new UnityEngine.Vector2(1000f, 80f)), "KARAKTERLER", 52f, textColor).fontStyle = TMPro.FontStyles.Bold;
var scroll = rect("ShopScroll", mc, center, new UnityEngine.Vector2(0f, -30f), new UnityEngine.Vector2(1080f, 580f));
var sr = scroll.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
sr.vertical = false;
var viewport = stretch("Viewport", scroll);
viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
viewport.gameObject.AddComponent<UnityEngine.UI.Image>().color = new UnityEngine.Color(0f, 0f, 0f, 0f);
var content = rect("Content", viewport, new UnityEngine.Vector2(0f, 0.5f), UnityEngine.Vector2.zero, new UnityEngine.Vector2(0f, 540f));
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

var playBtn = button("PlayButton", mc, new UnityEngine.Vector2(0f, -560f), new UnityEngine.Vector2(620f, 180f), "OYNA", green);
playBtn.GetComponentInChildren<TMPro.TextMeshProUGUI>().fontSize = 76f;
var shop = menuCanvas.AddComponent<ShopManager>();
shop.characters = characters.ToArray();
shop.listParent = content;
shop.shopItemPrefab = shopCardPrefab;
shop.totalGoldText = totalGold;
UnityEditor.Events.UnityEventTools.AddPersistentListener(playBtn.onClick, shop.Play);
UnityEditor.EditorUtility.SetDirty(shop);
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
