// RunnerPal placeholder assets (RUNNERPAL_TASK.md "Sahne Kurulumu" step 2): materials, UI icon sprites, pickup /
// obstacle / finish / road prefabs, character models + CharacterData, and the per-world theme on the 40 levels.
// Everything is simple primitives until real models arrive. Safe to rerun (overwrites, keeps asset GUIDs where
// Unity can). Run with: unity command eval_file --file "C:\oyunyapimi\RunnerPall\Tools\eval\build_assets.cs"
const string root = "Assets/RunnerPal/";
foreach (var dir in new[] { "Materials", "Prefabs", "Sprites", "Characters", "Models" })
    if (!UnityEditor.AssetDatabase.IsValidFolder(root + dir)) UnityEditor.AssetDatabase.CreateFolder("Assets/RunnerPal", dir);

// ---------- Materials ----------
var lit = UnityEngine.Shader.Find("Universal Render Pipeline/Lit");
System.Func<string, UnityEngine.Color, float, UnityEngine.Material> mat = (name, color, smooth) =>
{
    string path = root + "Materials/" + name + ".mat";
    var m = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(path);
    if (m == null) { m = new UnityEngine.Material(lit); UnityEditor.AssetDatabase.CreateAsset(m, path); }
    m.shader = lit;
    m.SetColor("_BaseColor", color);
    m.SetFloat("_Smoothness", smooth);
    UnityEditor.EditorUtility.SetDirty(m);
    return m;
};
System.Func<string, UnityEngine.Color, UnityEngine.Material> glow = (name, color) =>
{
    var m = mat(name, color, 0.6f);
    m.EnableKeyword("_EMISSION");
    m.SetColor("_EmissionColor", color * 0.8f);
    m.globalIlluminationFlags = UnityEngine.MaterialGlobalIlluminationFlags.None;
    return m;
};

var itemColors = new System.Collections.Generic.Dictionary<ItemType, UnityEngine.Color>
{
    { ItemType.Gomlek, new UnityEngine.Color(0.95f, 0.95f, 0.97f) },
    { ItemType.Pantolon, new UnityEngine.Color(0.14f, 0.2f, 0.42f) },
    { ItemType.Ceket, new UnityEngine.Color(0.32f, 0.33f, 0.37f) },
    { ItemType.Ayakkabi, new UnityEngine.Color(0.36f, 0.2f, 0.1f) },
    { ItemType.Saat, new UnityEngine.Color(0.95f, 0.75f, 0.2f) },
    { ItemType.Telefon, new UnityEngine.Color(0.1f, 0.1f, 0.12f) },
    { ItemType.Laptop, new UnityEngine.Color(0.72f, 0.75f, 0.8f) },
};
var itemLabels = new System.Collections.Generic.Dictionary<ItemType, string>
{
    { ItemType.Gomlek, "Gömlek" }, { ItemType.Pantolon, "Pantolon" }, { ItemType.Ceket, "Ceket" },
    { ItemType.Ayakkabi, "Ayakkabı" }, { ItemType.Saat, "Saat" }, { ItemType.Telefon, "Telefon" }, { ItemType.Laptop, "Laptop" },
};
var itemMats = new System.Collections.Generic.Dictionary<ItemType, UnityEngine.Material>();
foreach (var kv in itemColors) itemMats[kv.Key] = mat("Item_" + kv.Key, kv.Value, 0.35f);
var goldMat = glow("Gold", new UnityEngine.Color(1f, 0.78f, 0.15f));
var boostMat = glow("SpeedBoost", new UnityEngine.Color(0.25f, 0.95f, 0.35f));
var shieldMat = glow("Shield", new UnityEngine.Color(0.3f, 0.6f, 1f));
var coffeeMat = mat("Coffee", new UnityEngine.Color(0.3f, 0.17f, 0.08f), 0.9f);
var obstacleMat = mat("Obstacle", new UnityEngine.Color(0.9f, 0.32f, 0.2f), 0.3f);
var barrierMat = mat("Barrier", new UnityEngine.Color(0.95f, 0.8f, 0.15f), 0.3f);
var postMat = mat("Post", new UnityEngine.Color(0.25f, 0.25f, 0.28f), 0.2f);
var lineMat = mat("LaneLine", new UnityEngine.Color(0.95f, 0.95f, 0.9f), 0.1f);
var curbMat = mat("Curb", new UnityEngine.Color(0.6f, 0.6f, 0.62f), 0.1f);
var doorMat = mat("Door", new UnityEngine.Color(0.2f, 0.55f, 0.35f), 0.4f);
var skinMat = mat("Skin", new UnityEngine.Color(0.93f, 0.74f, 0.6f), 0.2f);

// ---------- Sprites (generated textures) ----------
System.Func<string, int, System.Func<float, float, UnityEngine.Color>, UnityEngine.Sprite> sprite = (name, size, paint) =>
{
    string path = root + "Sprites/" + name + ".png";
    var tex = new UnityEngine.Texture2D(size, size, UnityEngine.TextureFormat.RGBA32, false);
    var px = new UnityEngine.Color32[size * size];
    for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
            px[y * size + x] = paint((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f);
    tex.SetPixels32(px);
    tex.Apply();
    System.IO.File.WriteAllBytes(path, UnityEngine.ImageConversion.EncodeToPNG(tex));
    UnityEngine.Object.DestroyImmediate(tex);
    UnityEditor.AssetDatabase.ImportAsset(path);
    var imp = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(path);
    imp.textureType = UnityEditor.TextureImporterType.Sprite;
    imp.mipmapEnabled = false;
    imp.alphaIsTransparency = true;
    imp.SaveAndReimport();
    return UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(path);
};
System.Func<float, float> aa = d => UnityEngine.Mathf.Clamp01(0.5f - d * 64f);
System.Func<float, float, float, float, float> roundBox = (x, y, h, r) =>
{
    float qx = UnityEngine.Mathf.Abs(x) - h + r, qy = UnityEngine.Mathf.Abs(y) - h + r;
    return new UnityEngine.Vector2(UnityEngine.Mathf.Max(qx, 0f), UnityEngine.Mathf.Max(qy, 0f)).magnitude
        + UnityEngine.Mathf.Min(UnityEngine.Mathf.Max(qx, qy), 0f) - r;
};
// Item icons: the item's color in a rounded tile with a light rim (labels carry the name).
var itemSprites = new System.Collections.Generic.Dictionary<ItemType, UnityEngine.Sprite>();
foreach (var kv in itemColors)
{
    var c = kv.Value;
    itemSprites[kv.Key] = sprite("Icon_" + kv.Key, 128, (x, y) =>
    {
        float d = roundBox(x, y, 0.92f, 0.3f);
        float a = aa(d);
        bool rim = d > -0.1f;
        var col = rim ? new UnityEngine.Color(1f, 1f, 1f) : c;
        return new UnityEngine.Color(col.r, col.g, col.b, a);
    });
}
var heart = sprite("Heart", 128, (x, y) =>
{
    // Classic heart curve: (x^2 + y^2 - 1)^3 - x^2 y^3 <= 0, scaled into the tile.
    float hx = x * 1.25f, hy = y * 1.25f + 0.2f;
    float v = UnityEngine.Mathf.Pow(hx * hx + hy * hy - 1f, 3f) - hx * hx * hy * hy * hy;
    return new UnityEngine.Color(0.95f, 0.2f, 0.3f, UnityEngine.Mathf.Clamp01(-v * 30f));
});
var coin = sprite("Coin", 128, (x, y) =>
{
    float r = UnityEngine.Mathf.Sqrt(x * x + y * y);
    var col = r > 0.72f ? new UnityEngine.Color(0.85f, 0.6f, 0.08f) : new UnityEngine.Color(1f, 0.82f, 0.2f);
    return new UnityEngine.Color(col.r, col.g, col.b, aa(r - 0.92f));
});
var tile = sprite("RoundTile", 128, (x, y) => new UnityEngine.Color(1f, 1f, 1f, aa(roundBox(x, y, 0.95f, 0.35f))));

// ---------- Prefab helpers ----------
System.Func<UnityEngine.PrimitiveType, string, UnityEngine.Transform, UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Material, bool, UnityEngine.GameObject> part =
    (type, name, parent, pos, scale, material, keepCollider) =>
{
    var go = UnityEngine.GameObject.CreatePrimitive(type);
    go.name = name;
    if (parent != null) go.transform.SetParent(parent, false);
    go.transform.localPosition = pos;
    go.transform.localScale = scale;
    go.GetComponent<UnityEngine.Renderer>().sharedMaterial = material;
    if (!keepCollider) UnityEngine.Object.DestroyImmediate(go.GetComponent<UnityEngine.Collider>());
    return go;
};
System.Func<UnityEngine.GameObject, string, UnityEngine.GameObject> save = (go, name) =>
{
    var prefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(go, root + "Prefabs/" + name + ".prefab");
    UnityEngine.Object.DestroyImmediate(go);
    return prefab;
};
System.Func<string, UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.GameObject> triggerRoot = (name, center, size) =>
{
    var go = new UnityEngine.GameObject(name);
    var box = go.AddComponent<UnityEngine.BoxCollider>();
    box.isTrigger = true;
    box.center = center;
    box.size = size;
    return go;
};
System.Action<UnityEngine.GameObject, Pickup.Kind, float, float> setPickup = (go, kind, duration, mult) =>
{
    var p = go.AddComponent<Pickup>();
    p.kind = kind;
    p.duration = duration;
    p.speedMultiplier = mult;
};

// ---------- Pickups ----------
var goldGo = triggerRoot("Gold", UnityEngine.Vector3.zero, new UnityEngine.Vector3(0.9f, 0.9f, 0.9f));
var coinVisual = part(UnityEngine.PrimitiveType.Cylinder, "Coin", goldGo.transform, UnityEngine.Vector3.zero, new UnityEngine.Vector3(0.6f, 0.06f, 0.6f), goldMat, false);
coinVisual.transform.localRotation = UnityEngine.Quaternion.Euler(90f, 0f, 0f);
setPickup(goldGo, Pickup.Kind.Gold, 0f, 1f);
var goldPrefab = save(goldGo, "Gold");

var itemPrefabs = new System.Collections.Generic.Dictionary<ItemType, UnityEngine.GameObject>();
foreach (var kv in itemColors)
{
    var go = triggerRoot("Item_" + kv.Key, UnityEngine.Vector3.zero, new UnityEngine.Vector3(1.2f, 1.2f, 1.2f));
    part(UnityEngine.PrimitiveType.Cube, "Box", go.transform, UnityEngine.Vector3.zero, new UnityEngine.Vector3(0.8f, 0.8f, 0.8f), itemMats[kv.Key], false);
    // A glowing gold ring under each item so it reads as "collect me" from far away.
    part(UnityEngine.PrimitiveType.Cylinder, "Halo", go.transform, new UnityEngine.Vector3(0f, -0.6f, 0f), new UnityEngine.Vector3(1.2f, 0.02f, 1.2f), goldMat, false);
    setPickup(go, Pickup.Kind.Item, 0f, 1f);
    go.GetComponent<Pickup>().item = kv.Key;
    itemPrefabs[kv.Key] = save(go, "Item_" + kv.Key);
}

var boostGo = triggerRoot("SpeedBoost", UnityEngine.Vector3.zero, new UnityEngine.Vector3(1.1f, 1.1f, 1.1f));
var arrow = part(UnityEngine.PrimitiveType.Cube, "Arrow", boostGo.transform, UnityEngine.Vector3.zero, new UnityEngine.Vector3(0.5f, 0.5f, 0.5f), boostMat, false);
arrow.transform.localRotation = UnityEngine.Quaternion.Euler(45f, 0f, 45f);
setPickup(boostGo, Pickup.Kind.SpeedBoost, 4f, 1.5f);
var boostPrefab = save(boostGo, "SpeedBoost");

var shieldGo = triggerRoot("Shield", UnityEngine.Vector3.zero, new UnityEngine.Vector3(1.1f, 1.1f, 1.1f));
part(UnityEngine.PrimitiveType.Sphere, "Bubble", shieldGo.transform, UnityEngine.Vector3.zero, new UnityEngine.Vector3(0.75f, 0.75f, 0.75f), shieldMat, false);
setPickup(shieldGo, Pickup.Kind.Shield, 5f, 1f);
var shieldPrefab = save(shieldGo, "Shield");

// Spilled coffee: a flat puddle on the road (no spin).
var slowGo = triggerRoot("SlowTrap", new UnityEngine.Vector3(0f, 0.4f, 0f), new UnityEngine.Vector3(1.8f, 0.8f, 1.8f));
part(UnityEngine.PrimitiveType.Cylinder, "Puddle", slowGo.transform, new UnityEngine.Vector3(0f, 0.01f, 0f), new UnityEngine.Vector3(1.7f, 0.01f, 1.7f), coffeeMat, false);
part(UnityEngine.PrimitiveType.Cylinder, "Cup", slowGo.transform, new UnityEngine.Vector3(0.45f, 0.2f, 0.2f), new UnityEngine.Vector3(0.3f, 0.2f, 0.3f), lineMat, false);
setPickup(slowGo, Pickup.Kind.SlowTrap, 3f, 0.5f);
var slowPrefab = save(slowGo, "SlowTrap");

// ---------- Obstacles (heights match PlayerController: standing 0-2 m, sliding 0-1 m, jump apex +2.2 m) ----------
// Low: jump over it (0.8 m tall; a sliding player still hits it).
var lowGo = triggerRoot("Obstacle_Low", new UnityEngine.Vector3(0f, 0.4f, 0f), new UnityEngine.Vector3(2.2f, 0.8f, 0.6f));
part(UnityEngine.PrimitiveType.Cube, "Block", lowGo.transform, new UnityEngine.Vector3(0f, 0.4f, 0f), new UnityEngine.Vector3(2.2f, 0.8f, 0.6f), obstacleMat, false);
lowGo.AddComponent<Obstacle>();
var lowPrefab = save(lowGo, "Obstacle_Low");
// High: slide under it (bar from 1.3 m to 2.3 m; a sliding player's top is at 1 m).
var highGo = triggerRoot("Obstacle_High", new UnityEngine.Vector3(0f, 1.8f, 0f), new UnityEngine.Vector3(2.2f, 1f, 0.4f));
part(UnityEngine.PrimitiveType.Cube, "Bar", highGo.transform, new UnityEngine.Vector3(0f, 1.8f, 0f), new UnityEngine.Vector3(2.3f, 1f, 0.3f), barrierMat, false);
part(UnityEngine.PrimitiveType.Cube, "PostL", highGo.transform, new UnityEngine.Vector3(-1.15f, 1.15f, 0f), new UnityEngine.Vector3(0.12f, 2.3f, 0.12f), postMat, false);
part(UnityEngine.PrimitiveType.Cube, "PostR", highGo.transform, new UnityEngine.Vector3(1.15f, 1.15f, 0f), new UnityEngine.Vector3(0.12f, 2.3f, 0.12f), postMat, false);
highGo.AddComponent<Obstacle>();
var highPrefab = save(highGo, "Obstacle_High");

// ---------- Finish: the office door across all three lanes ----------
var finishGo = triggerRoot("Finish", new UnityEngine.Vector3(0f, 2f, 0f), new UnityEngine.Vector3(8f, 4f, 1f));
part(UnityEngine.PrimitiveType.Cube, "FrameL", finishGo.transform, new UnityEngine.Vector3(-3.9f, 2.2f, 0f), new UnityEngine.Vector3(0.4f, 4.4f, 0.4f), postMat, false);
part(UnityEngine.PrimitiveType.Cube, "FrameR", finishGo.transform, new UnityEngine.Vector3(3.9f, 2.2f, 0f), new UnityEngine.Vector3(0.4f, 4.4f, 0.4f), postMat, false);
part(UnityEngine.PrimitiveType.Cube, "Sign", finishGo.transform, new UnityEngine.Vector3(0f, 4.6f, 0f), new UnityEngine.Vector3(8.2f, 0.8f, 0.4f), doorMat, false);
finishGo.AddComponent<FinishLine>();
var finishPrefab = save(finishGo, "Finish");

// ---------- Road segments: one set per world (20 m long, 7.5 m wide, 3 lanes of 2.5 m) ----------
string[] worlds = { "Office", "Bank", "Sales", "Dev" };
UnityEngine.Color[] groundColors =
{
    new UnityEngine.Color(0.3f, 0.32f, 0.36f), new UnityEngine.Color(0.36f, 0.3f, 0.26f),
    new UnityEngine.Color(0.24f, 0.33f, 0.3f), new UnityEngine.Color(0.2f, 0.22f, 0.32f),
};
UnityEngine.Color[] fogColors =
{
    new UnityEngine.Color(0.72f, 0.82f, 0.92f), new UnityEngine.Color(0.9f, 0.8f, 0.66f),
    new UnityEngine.Color(0.7f, 0.88f, 0.8f), new UnityEngine.Color(0.35f, 0.38f, 0.55f),
};
var roadPrefabs = new UnityEngine.GameObject[4];
var skyboxes = new UnityEngine.Material[4];
for (int w = 0; w < 4; w++)
{
    var groundMat = mat("Road_" + worlds[w], groundColors[w], 0.15f);
    var go = new UnityEngine.GameObject("Road_" + worlds[w]);
    // Covers z = 0..20 from the prefab's origin (TrackSpawner places segment i at z = i * 20).
    part(UnityEngine.PrimitiveType.Cube, "Ground", go.transform, new UnityEngine.Vector3(0f, -0.1f, 10f), new UnityEngine.Vector3(7.5f, 0.2f, 20f), groundMat, false);
    foreach (float x in new[] { -1.25f, 1.25f })
        part(UnityEngine.PrimitiveType.Cube, "Line", go.transform, new UnityEngine.Vector3(x, 0.005f, 10f), new UnityEngine.Vector3(0.08f, 0.01f, 20f), lineMat, false);
    foreach (float x in new[] { -3.95f, 3.95f })
        part(UnityEngine.PrimitiveType.Cube, "Curb", go.transform, new UnityEngine.Vector3(x, 0.1f, 10f), new UnityEngine.Vector3(0.4f, 0.4f, 20f), curbMat, false);
    roadPrefabs[w] = save(go, "Road_" + worlds[w]);

    string skyPath = root + "Materials/Sky_" + worlds[w] + ".mat";
    var sky = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(skyPath);
    if (sky == null) { sky = new UnityEngine.Material(UnityEngine.Shader.Find("Skybox/Procedural")); UnityEditor.AssetDatabase.CreateAsset(sky, skyPath); }
    sky.SetColor("_SkyTint", fogColors[w]);
    sky.SetColor("_GroundColor", groundColors[w]);
    sky.SetFloat("_AtmosphereThickness", w == 3 ? 0.6f : 1f);
    sky.SetFloat("_Exposure", w == 3 ? 0.8f : 1.2f);
    UnityEditor.EditorUtility.SetDirty(sky);
    skyboxes[w] = sky;
}

// ---------- Character models: underwear + tank top body with hidden outfit parts (PlayerOutfit) ----------
// Pivot at the feet; the body spans 0..2 m like the player's capsule.
System.Func<string, UnityEngine.Color, UnityEngine.Color, UnityEngine.GameObject> model = (name, skin, hair) =>
{
    var skinM = mat("Skin_" + name, skin, 0.2f);
    var hairM = mat("Hair_" + name, hair, 0.3f);
    var go = new UnityEngine.GameObject("Model_" + name);
    var t = go.transform;
    var white = mat("TankTop", new UnityEngine.Color(0.97f, 0.97f, 0.97f), 0.1f);
    var shorts = mat("Underwear", new UnityEngine.Color(0.85f, 0.35f, 0.4f), 0.1f);
    // Body
    part(UnityEngine.PrimitiveType.Sphere, "Head", t, new UnityEngine.Vector3(0f, 1.72f, 0f), new UnityEngine.Vector3(0.42f, 0.46f, 0.42f), skinM, false);
    part(UnityEngine.PrimitiveType.Sphere, "Hair", t, new UnityEngine.Vector3(0f, 1.86f, -0.03f), new UnityEngine.Vector3(0.44f, 0.25f, 0.44f), hairM, false);
    part(UnityEngine.PrimitiveType.Cube, "TankTop", t, new UnityEngine.Vector3(0f, 1.2f, 0f), new UnityEngine.Vector3(0.55f, 0.6f, 0.3f), white, false);
    part(UnityEngine.PrimitiveType.Cube, "Underwear", t, new UnityEngine.Vector3(0f, 0.85f, 0f), new UnityEngine.Vector3(0.52f, 0.2f, 0.3f), shorts, false);
    foreach (float x in new[] { -0.14f, 0.14f })
        part(UnityEngine.PrimitiveType.Cube, "Leg", t, new UnityEngine.Vector3(x, 0.4f, 0f), new UnityEngine.Vector3(0.18f, 0.72f, 0.2f), skinM, false);
    foreach (float x in new[] { -0.38f, 0.38f })
        part(UnityEngine.PrimitiveType.Cube, "Arm", t, new UnityEngine.Vector3(x, 1.15f, 0f), new UnityEngine.Vector3(0.14f, 0.62f, 0.16f), skinM, false);

    // Outfit parts (hidden at start; PlayerOutfit shows them as they're collected)
    var outfit = go.AddComponent<PlayerOutfit>();
    var visuals = new System.Collections.Generic.List<PlayerOutfit.ItemVisual>();
    System.Func<ItemType, string, UnityEngine.GameObject> group = (item, groupName) =>
    {
        var g = new UnityEngine.GameObject(groupName);
        g.transform.SetParent(t, false);
        visuals.Add(new PlayerOutfit.ItemVisual { item = item, visual = g });
        return g;
    };
    var shirt = group(ItemType.Gomlek, "Outfit_Gomlek");
    part(UnityEngine.PrimitiveType.Cube, "Shirt", shirt.transform, new UnityEngine.Vector3(0f, 1.2f, 0f), new UnityEngine.Vector3(0.58f, 0.63f, 0.33f), itemMats[ItemType.Gomlek], false);
    foreach (float x in new[] { -0.38f, 0.38f })
        part(UnityEngine.PrimitiveType.Cube, "Sleeve", shirt.transform, new UnityEngine.Vector3(x, 1.28f, 0f), new UnityEngine.Vector3(0.17f, 0.45f, 0.19f), itemMats[ItemType.Gomlek], false);
    var pants = group(ItemType.Pantolon, "Outfit_Pantolon");
    part(UnityEngine.PrimitiveType.Cube, "Waist", pants.transform, new UnityEngine.Vector3(0f, 0.85f, 0f), new UnityEngine.Vector3(0.55f, 0.23f, 0.33f), itemMats[ItemType.Pantolon], false);
    foreach (float x in new[] { -0.14f, 0.14f })
        part(UnityEngine.PrimitiveType.Cube, "PantLeg", pants.transform, new UnityEngine.Vector3(x, 0.42f, 0f), new UnityEngine.Vector3(0.21f, 0.7f, 0.23f), itemMats[ItemType.Pantolon], false);
    var jacket = group(ItemType.Ceket, "Outfit_Ceket");
    part(UnityEngine.PrimitiveType.Cube, "Jacket", jacket.transform, new UnityEngine.Vector3(0f, 1.18f, -0.01f), new UnityEngine.Vector3(0.62f, 0.68f, 0.36f), itemMats[ItemType.Ceket], false);
    foreach (float x in new[] { -0.39f, 0.39f })
        part(UnityEngine.PrimitiveType.Cube, "JacketSleeve", jacket.transform, new UnityEngine.Vector3(x, 1.15f, 0f), new UnityEngine.Vector3(0.19f, 0.64f, 0.21f), itemMats[ItemType.Ceket], false);
    var shoes = group(ItemType.Ayakkabi, "Outfit_Ayakkabi");
    foreach (float x in new[] { -0.14f, 0.14f })
        part(UnityEngine.PrimitiveType.Cube, "Shoe", shoes.transform, new UnityEngine.Vector3(x, 0.06f, 0.05f), new UnityEngine.Vector3(0.22f, 0.12f, 0.34f), itemMats[ItemType.Ayakkabi], false);
    var watch = group(ItemType.Saat, "Outfit_Saat");
    part(UnityEngine.PrimitiveType.Cube, "Watch", watch.transform, new UnityEngine.Vector3(-0.38f, 0.9f, 0f), new UnityEngine.Vector3(0.18f, 0.07f, 0.2f), itemMats[ItemType.Saat], false);
    var phone = group(ItemType.Telefon, "Outfit_Telefon");
    part(UnityEngine.PrimitiveType.Cube, "Phone", phone.transform, new UnityEngine.Vector3(0.4f, 0.85f, 0.1f), new UnityEngine.Vector3(0.08f, 0.2f, 0.12f), itemMats[ItemType.Telefon], false);
    var laptop = group(ItemType.Laptop, "Outfit_Laptop");
    part(UnityEngine.PrimitiveType.Cube, "Laptop", laptop.transform, new UnityEngine.Vector3(-0.47f, 1.0f, 0f), new UnityEngine.Vector3(0.05f, 0.35f, 0.5f), itemMats[ItemType.Laptop], false);
    outfit.visuals = visuals.ToArray();
    // Model files live in Models/ (they are character looks, not level pieces).
    var prefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(go, root + "Models/Model_" + name + ".prefab");
    UnityEngine.Object.DestroyImmediate(go);
    return prefab;
};

// Shop characters: the default look is free; the others cost gold.
var characterSpecs = new[]
{
    new { id = "default", display = "Mehmet Bey", price = 0, skin = new UnityEngine.Color(0.93f, 0.74f, 0.6f), hair = new UnityEngine.Color(0.2f, 0.13f, 0.08f) },
    new { id = "kemal", display = "Kemal Abi", price = 150, skin = new UnityEngine.Color(0.76f, 0.55f, 0.4f), hair = new UnityEngine.Color(0.08f, 0.08f, 0.08f) },
    new { id = "burak", display = "Burak", price = 300, skin = new UnityEngine.Color(0.96f, 0.8f, 0.68f), hair = new UnityEngine.Color(0.85f, 0.62f, 0.25f) },
    new { id = "hasan", display = "Hasan Usta", price = 500, skin = new UnityEngine.Color(0.6f, 0.42f, 0.3f), hair = new UnityEngine.Color(0.75f, 0.75f, 0.75f) },
};
var characterAssets = new System.Collections.Generic.List<UnityEngine.Object>();
foreach (var spec in characterSpecs)
{
    var modelPrefab = model(spec.id, spec.skin, spec.hair);
    var icon = sprite("Char_" + spec.id, 128, (x, y) =>
    {
        // Simple portrait: hair cap over a skin-colored face in a round badge.
        float r = UnityEngine.Mathf.Sqrt(x * x + y * y);
        float face = UnityEngine.Mathf.Sqrt(x * x + (y + 0.05f) * (y + 0.05f));
        UnityEngine.Color col = new UnityEngine.Color(0.2f, 0.25f, 0.35f);
        if (face < 0.55f) col = y > 0.25f && face > 0.3f ? spec.hair : spec.skin;
        return new UnityEngine.Color(col.r, col.g, col.b, aa(r - 0.95f));
    });
    string path = root + "Characters/Character_" + spec.id + ".asset";
    var data = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterData>(path);
    if (data == null) { data = UnityEngine.ScriptableObject.CreateInstance<CharacterData>(); UnityEditor.AssetDatabase.CreateAsset(data, path); }
    data.id = spec.id;
    data.displayName = spec.display;
    data.price = spec.price;
    data.icon = icon;
    data.modelPrefab = modelPrefab;
    UnityEditor.EditorUtility.SetDirty(data);
    characterAssets.Add(data);
}

// ---------- Theme per world on the 40 levels ----------
for (int n = 1; n <= 40; n++)
{
    var level = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(root + "Levels/Level_" + n.ToString("00") + ".asset");
    if (level == null) continue;
    int w = (n - 1) / 10;
    level.roadSegmentPrefabs = new[] { roadPrefabs[w] };
    level.skyboxMaterial = skyboxes[w];
    level.fogColor = fogColors[w];
    UnityEditor.EditorUtility.SetDirty(level);
}

UnityEditor.AssetDatabase.SaveAssets();
return "assets ok: " + itemPrefabs.Count + " items, " + characterAssets.Count + " characters";
