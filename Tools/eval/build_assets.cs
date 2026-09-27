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

// ---------- Road segments with a city around them: 4 variants per world ----------
// 20 m long, 3 lanes of 2.5 m (road 7.5 m wide), sidewalks, street lamps, a row of buildings facing the road and a
// bigger skyline row behind. Buildings: Kenney City Kit Commercial / Suburban (CC0), scaled x9 (1 unit ~ 9 m).
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
// Ground beyond the sidewalks: plaza stone in town, grass in the suburbs.
UnityEngine.Color[] landColors =
{
    new UnityEngine.Color(0.55f, 0.55f, 0.53f), new UnityEngine.Color(0.6f, 0.56f, 0.5f),
    new UnityEngine.Color(0.35f, 0.55f, 0.3f), new UnityEngine.Color(0.3f, 0.3f, 0.36f),
};
const string commercial = "Assets/RunnerPal/ThirdParty/Kenney_CityKitCommercial/";
const string suburban = "Assets/RunnerPal/ThirdParty/Kenney_CityKitSuburban/";
System.Func<string, string[], UnityEngine.GameObject[]> kit = (dir, names) =>
{
    var list = new System.Collections.Generic.List<UnityEngine.GameObject>();
    foreach (var n in names)
    {
        var g = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(dir + n + ".fbx");
        if (g != null) list.Add(g);
    }
    return list.ToArray();
};
System.Func<string, char, char, string[]> range = (prefix, from, to) =>
{
    var l = new System.Collections.Generic.List<string>();
    for (char c = from; c <= to; c++) l.Add(prefix + c);
    return l.ToArray();
};
var shops = kit(commercial, range("building-", 'a', 'n'));
var towers = kit(commercial, range("building-skyscraper-", 'a', 'e'));
var farBlocks = kit(commercial, range("low-detail-building-", 'a', 'n'));
var houses = kit(suburban, range("building-type-", 'a', 'u'));
var trees = kit(suburban, new[] { "tree-large", "tree-small" });
if (shops.Length == 0 || houses.Length == 0) throw new System.Exception("Kenney city kits missing under ThirdParty/");
// Front row and skyline per world.
UnityEngine.GameObject[][] frontSets = { shops, CombineTowers(towers, shops), houses, towers };
UnityEngine.GameObject[][] backSets = { farBlocks, CombineTowers(towers, farBlocks), trees, farBlocks };
UnityEngine.GameObject[] CombineTowers(UnityEngine.GameObject[] a, UnityEngine.GameObject[] b)
{
    var l = new System.Collections.Generic.List<UnityEngine.GameObject>(a);
    l.AddRange(b);
    return l.ToArray();
}

var sidewalkMat = mat("Sidewalk", new UnityEngine.Color(0.72f, 0.72f, 0.7f), 0.1f);
var lampMat = mat("LampPost", new UnityEngine.Color(0.2f, 0.21f, 0.24f), 0.4f);
var lampLight = glow("LampLight", new UnityEngine.Color(1f, 0.92f, 0.7f));

// Places one building with its front towards the road; returns its width along the road.
System.Func<UnityEngine.GameObject, UnityEngine.Transform, float, float, bool, float, float> placeBuilding = (src, parent, z, innerX, left, scale) =>
{
    var b = (UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(src, parent);
    b.transform.localScale = UnityEngine.Vector3.one * scale;
    // Kenney fronts face +Z; turn them to face the road.
    b.transform.localRotation = UnityEngine.Quaternion.Euler(0f, left ? 90f : -90f, 0f);
    var bounds = new UnityEngine.Bounds(b.transform.position, UnityEngine.Vector3.zero);
    bool first = true;
    foreach (var r in b.GetComponentsInChildren<UnityEngine.Renderer>())
    {
        if (first) { bounds = r.bounds; first = false; } else bounds.Encapsulate(r.bounds);
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // cheap on phones; the road keeps its shadows
    }
    float width = bounds.size.z, depth = bounds.size.x;
    float x = left ? -(innerX + depth * 0.5f) : innerX + depth * 0.5f;
    b.transform.localPosition = new UnityEngine.Vector3(x - (bounds.center.x - b.transform.position.x), 0f, z + width * 0.5f - (bounds.center.z - b.transform.position.z));
    return width;
};
// Fills [0, 20) along one side with buildings from `set`, leaving small gaps.
System.Action<UnityEngine.Transform, UnityEngine.GameObject[], bool, float, float, float, System.Random> fillSide = (parent, set, left, innerX, scale, gap, rng) =>
{
    float z = (float)rng.NextDouble() * 1.5f;
    for (int guard = 0; guard < 12 && z < 19f; guard++)
    {
        var src = set[rng.Next(set.Length)];
        float w = placeBuilding(src, parent, z, innerX, left, scale * (0.9f + (float)rng.NextDouble() * 0.2f));
        if (z + w > 20.5f) { UnityEngine.Object.DestroyImmediate(parent.GetChild(parent.childCount - 1).gameObject); break; }
        z += w + gap;
    }
};

var roadSets = new UnityEngine.GameObject[4][];
var skyboxes = new UnityEngine.Material[4];
for (int w = 0; w < 4; w++)
{
    var groundMat = mat("Road_" + worlds[w], groundColors[w], 0.15f);
    var landMat = mat("Land_" + worlds[w], landColors[w], 0.05f);
    roadSets[w] = new UnityEngine.GameObject[4];
    for (int v = 0; v < 4; v++)
    {
        var rng = new System.Random(1000 * w + v);
        var go = new UnityEngine.GameObject("Road_" + worlds[w] + "_" + v);
        // Covers z = 0..20 from the prefab's origin (TrackSpawner places segment i at z = i * 20).
        part(UnityEngine.PrimitiveType.Cube, "Ground", go.transform, new UnityEngine.Vector3(0f, -0.1f, 10f), new UnityEngine.Vector3(7.5f, 0.2f, 20f), groundMat, false);
        foreach (float x in new[] { -1.25f, 1.25f })
            part(UnityEngine.PrimitiveType.Cube, "Line", go.transform, new UnityEngine.Vector3(x, 0.005f, 10f), new UnityEngine.Vector3(0.08f, 0.01f, 20f), lineMat, false);
        foreach (float x in new[] { -3.95f, 3.95f })
            part(UnityEngine.PrimitiveType.Cube, "Curb", go.transform, new UnityEngine.Vector3(x, 0.1f, 10f), new UnityEngine.Vector3(0.4f, 0.4f, 20f), curbMat, false);
        foreach (float x in new[] { -5.45f, 5.45f })
            part(UnityEngine.PrimitiveType.Cube, "Sidewalk", go.transform, new UnityEngine.Vector3(x, 0.08f, 10f), new UnityEngine.Vector3(2.6f, 0.16f, 20f), sidewalkMat, false);
        part(UnityEngine.PrimitiveType.Cube, "Land", go.transform, new UnityEngine.Vector3(0f, -0.25f, 10f), new UnityEngine.Vector3(140f, 0.2f, 20f), landMat, false);
        // Street lamps on both sides, offset so they alternate.
        foreach (var (x, z) in new[] { (-4.5f, 4f), (4.5f, 14f) })
        {
            part(UnityEngine.PrimitiveType.Cylinder, "LampPole", go.transform, new UnityEngine.Vector3(x, 2.2f, z), new UnityEngine.Vector3(0.12f, 2.2f, 0.12f), lampMat, false);
            part(UnityEngine.PrimitiveType.Cube, "LampArm", go.transform, new UnityEngine.Vector3(x * 0.88f, 4.35f, z), new UnityEngine.Vector3(1.2f, 0.1f, 0.12f), lampMat, false);
            part(UnityEngine.PrimitiveType.Cube, "LampHead", go.transform, new UnityEngine.Vector3(x * 0.76f, 4.25f, z), new UnityEngine.Vector3(0.45f, 0.14f, 0.3f), lampLight, false);
        }
        var front = new UnityEngine.GameObject("Front").transform;
        front.SetParent(go.transform, false);
        var back = new UnityEngine.GameObject("Skyline").transform;
        back.SetParent(go.transform, false);
        bool suburbs = w == 2;
        foreach (bool left in new[] { true, false })
        {
            fillSide(front, frontSets[w], left, 7.2f, suburbs ? 7.5f : 9f, suburbs ? 3f : 0.6f, rng);
            fillSide(back, backSets[w], left, suburbs ? 22f : 30f, suburbs ? 12f : 14f, suburbs ? 1.5f : 2f, rng);
        }
        roadSets[w][v] = save(go, "Road_" + worlds[w] + "_" + v);
    }
    // The single road prefabs from the first build are replaced by the variants.
    UnityEditor.AssetDatabase.DeleteAsset(root + "Prefabs/Road_" + worlds[w] + ".prefab");

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

// Renders a character's face into a 256 px sprite (camera in front of the head, soft key light, colored backdrop).
System.Func<UnityEngine.GameObject, string, UnityEngine.Color, UnityEngine.Sprite> portrait = (modelPrefab, name, backdrop) =>
{
    const int Size = 256;
    const int Layer = 30;
    var active = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
    var temp = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Additive);
    UnityEngine.SceneManagement.SceneManager.SetActiveScene(temp);
    var m = (UnityEngine.GameObject)UnityEngine.Object.Instantiate(modelPrefab);
    foreach (var t in m.GetComponentsInChildren<UnityEngine.Transform>(true)) t.gameObject.layer = Layer;
    // Only the base look (outfit parts stay hidden like at the start of a run).
    foreach (var v in m.GetComponent<PlayerOutfit>().visuals) if (v.visual) v.visual.SetActive(false);
    var lightGo = new UnityEngine.GameObject("Key");
    var light = lightGo.AddComponent<UnityEngine.Light>();
    light.type = UnityEngine.LightType.Directional;
    light.intensity = 1.3f;
    light.cullingMask = 1 << Layer;
    lightGo.transform.rotation = UnityEngine.Quaternion.Euler(20f, 200f, 0f);
    var camGo = new UnityEngine.GameObject("PortraitCam");
    var cam = camGo.AddComponent<UnityEngine.Camera>();
    cam.cullingMask = 1 << Layer;
    cam.clearFlags = UnityEngine.CameraClearFlags.SolidColor;
    cam.backgroundColor = UnityEngine.Color.Lerp(backdrop, new UnityEngine.Color(0.15f, 0.18f, 0.28f), 0.55f);
    cam.fieldOfView = 26f;
    cam.transform.position = new UnityEngine.Vector3(0f, 1.62f, 1.9f);
    cam.transform.LookAt(new UnityEngine.Vector3(0f, 1.56f, 0f));
    var rt = new UnityEngine.RenderTexture(Size, Size, 24, UnityEngine.RenderTextureFormat.ARGB32);
    rt.antiAliasing = 4;
    cam.targetTexture = rt;
    UnityEngine.RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
    UnityEngine.RenderSettings.ambientLight = new UnityEngine.Color(0.55f, 0.55f, 0.6f);
    cam.Render(); // the first render after compiling can come out empty (shaders warming up)
    cam.Render();
    var prev = UnityEngine.RenderTexture.active;
    UnityEngine.RenderTexture.active = rt;
    var tex = new UnityEngine.Texture2D(Size, Size, UnityEngine.TextureFormat.RGBA32, false);
    tex.ReadPixels(new UnityEngine.Rect(0, 0, Size, Size), 0, 0);
    // Round badge: clear the corners.
    var px = tex.GetPixels32();
    for (int y = 0; y < Size; y++)
        for (int x = 0; x < Size; x++)
        {
            float dx = (x + 0.5f) / Size * 2f - 1f, dy = (y + 0.5f) / Size * 2f - 1f;
            float a = UnityEngine.Mathf.Clamp01((0.97f - UnityEngine.Mathf.Sqrt(dx * dx + dy * dy)) * Size * 0.5f);
            var c = px[y * Size + x];
            c.a = (byte)(a * 255);
            px[y * Size + x] = c;
        }
    tex.SetPixels32(px);
    tex.Apply();
    UnityEngine.RenderTexture.active = prev;
    cam.targetTexture = null;
    rt.Release();
    string path = root + "Sprites/" + name + ".png";
    System.IO.File.WriteAllBytes(path, UnityEngine.ImageConversion.EncodeToPNG(tex));
    UnityEngine.Object.DestroyImmediate(tex);
    UnityEngine.SceneManagement.SceneManager.SetActiveScene(active);
    UnityEditor.SceneManagement.EditorSceneManager.CloseScene(temp, true);
    UnityEditor.AssetDatabase.ImportAsset(path);
    var imp = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(path);
    imp.textureType = UnityEditor.TextureImporterType.Sprite;
    imp.mipmapEnabled = false;
    imp.alphaIsTransparency = true;
    imp.SaveAndReimport();
    return UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>(path);
};

// ---------- Characters: Quaternius modular men (CC0), assembled by Editor/ModularCharacterBuilder ----------
// Shop characters: the default look is free; the others cost gold. Each gets its own head and skin tone.
var runnerController = ModularCharacterBuilder.BuildController();
var characterSpecs = new[]
{
    new { id = "default", display = "Mehmet Bey", price = 0, head = "Suit_Head", skin = new UnityEngine.Color(0.93f, 0.74f, 0.6f), hair = new UnityEngine.Color(0.2f, 0.13f, 0.08f), shorts = new UnityEngine.Color(0.72f, 0.12f, 0.16f), tank = new UnityEngine.Color(0.97f, 0.97f, 0.97f), suit = new UnityEngine.Color(0.13f, 0.15f, 0.18f), tie = new UnityEngine.Color(0.25f, 0.3f, 0.38f) },
    new { id = "kemal", display = "Kemal Abi", price = 150, head = "Worker_Head", skin = new UnityEngine.Color(0.76f, 0.55f, 0.4f), hair = new UnityEngine.Color(0.08f, 0.08f, 0.08f), shorts = new UnityEngine.Color(0.15f, 0.32f, 0.75f), tank = new UnityEngine.Color(0.68f, 0.7f, 0.72f), suit = new UnityEngine.Color(0.1f, 0.14f, 0.32f), tie = new UnityEngine.Color(0.75f, 0.1f, 0.12f) },
    new { id = "burak", display = "Burak", price = 300, head = "Casual_Head", skin = new UnityEngine.Color(0.96f, 0.8f, 0.68f), hair = new UnityEngine.Color(0.85f, 0.62f, 0.25f), shorts = new UnityEngine.Color(0.2f, 0.62f, 0.3f), tank = new UnityEngine.Color(0.98f, 0.85f, 0.3f), suit = new UnityEngine.Color(0.56f, 0.58f, 0.62f), tie = new UnityEngine.Color(0.9f, 0.42f, 0.62f) },
    new { id = "hasan", display = "Hasan Usta", price = 500, head = "Farmer_Head", skin = new UnityEngine.Color(0.6f, 0.42f, 0.3f), hair = new UnityEngine.Color(0.75f, 0.75f, 0.75f), shorts = new UnityEngine.Color(0.92f, 0.46f, 0.1f), tank = new UnityEngine.Color(0.86f, 0.8f, 0.66f), suit = new UnityEngine.Color(0.36f, 0.22f, 0.12f), tie = new UnityEngine.Color(0.1f, 0.45f, 0.22f) },
};
var characterAssets = new System.Collections.Generic.List<UnityEngine.Object>();
foreach (var spec in characterSpecs)
{
    var modelPrefab = ModularCharacterBuilder.Build(spec.id, spec.head,
        new ModularCharacterBuilder.Look { skin = spec.skin, shorts = spec.shorts, tankTop = spec.tank, suit = spec.suit, tie = spec.tie }, runnerController);
    // Portrait for the shop: the real model (head + tank top), rendered in a temporary scene.
    var icon = portrait(modelPrefab, "Char_" + spec.id, spec.tank);
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
    level.roadSegmentPrefabs = roadSets[w];
    level.skyboxMaterial = skyboxes[w];
    level.fogColor = fogColors[w];
    UnityEditor.EditorUtility.SetDirty(level);
}

UnityEditor.AssetDatabase.SaveAssets();
return "assets ok: " + itemPrefabs.Count + " items, " + characterAssets.Count + " characters";
