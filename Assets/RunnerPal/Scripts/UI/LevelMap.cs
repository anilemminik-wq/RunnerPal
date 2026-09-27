using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Ana menüdeki bölüm haritası: aşağıdan yukarı kıvrılan bir yol, her iş (10 bölüm) kendi renginde bir bölge.
// Bölge başında o işin şehri, gereken eşyalar ve "terfi" bilgisi görünür. Açık bir bölüme dokununca o bölüm başlar.
public class LevelMap : MonoBehaviour
{
    [System.Serializable]
    public struct World
    {
        public string title;          // "Ofis Çalışanı"
        public string subtitle;       // "Sabah trafiği, şehir merkezi"
        public Color color;
        public Sprite cityImage;      // o dünyanın yolundan çekilmiş görüntü
    }

    public LevelData[] levels;
    public World[] worlds;
    public CharacterData[] characters;
    public GameUI.ItemIcon[] itemIcons;

    [Header("UI")]
    public ScrollRect scroll;
    public RectTransform content;
    public Sprite circle;
    public Sprite roundTile;
    public Sprite check;
    public Sprite lockIcon;
    public TMP_Text playButtonText;

    const float NodeSpacing = 230f;
    const float HeaderHeight = 760f;
    const float Width = 1080f;

    RectTransform currentNode;
    float pulse;

    void Start() => Build();

    public void Build()
    {
        foreach (Transform c in content) Destroy(c.gameObject);
        int unlocked = Mathf.Clamp(SaveSystem.UnlockedLevel, 1, levels.Length);
        int perWorld = Mathf.Max(1, Mathf.CeilToInt(levels.Length / (float)worlds.Length));

        float zoneHeight = HeaderHeight + perWorld * NodeSpacing + 60f;
        float total = 140f + worlds.Length * zoneHeight + 260f;
        content.sizeDelta = new Vector2(content.sizeDelta.x, total);

        Vector2 prev = Vector2.zero;
        bool hasPrev = false;
        float y = 140f;
        for (int w = 0; w < worlds.Length; w++)
        {
            var world = worlds[w];
            int first = w * perWorld + 1, last = Mathf.Min(levels.Length, first + perWorld - 1);
            bool worldOpen = unlocked >= first;

            // Bölge zemini
            var zone = Rect("Zone" + (w + 1), content, new Vector2(0f, y + zoneHeight * 0.5f), new Vector2(Width - 40f, zoneHeight - 20f));
            var zoneImg = zone.gameObject.AddComponent<Image>();
            zoneImg.sprite = roundTile; zoneImg.type = Image.Type.Sliced;
            zoneImg.color = new Color(world.color.r * 0.35f, world.color.g * 0.35f, world.color.b * 0.35f, 0.95f);
            zoneImg.raycastTarget = false;

            // Başlık kartı: şehir görüntüsü + iş adı + gerekenler
            var header = Rect("Header", content, new Vector2(0f, y + HeaderHeight * 0.5f + 20f), new Vector2(980f, HeaderHeight - 70f));
            var hImg = header.gameObject.AddComponent<Image>();
            hImg.sprite = roundTile; hImg.type = Image.Type.Sliced;
            hImg.color = new Color(0.06f, 0.07f, 0.12f, 0.96f);
            hImg.raycastTarget = false;
            if (world.cityImage)
            {
                var city = Rect("City", header, new Vector2(0f, 90f), new Vector2(940f, 420f));
                var ci = city.gameObject.AddComponent<Image>();
                ci.sprite = world.cityImage;
                ci.preserveAspect = false;
                ci.raycastTarget = false;
                if (!worldOpen) ci.color = new Color(0.35f, 0.35f, 0.4f, 1f);
                // Üstüne iş adı bandı
                var band = Rect("Band", city, new Vector2(0f, 150f), new Vector2(940f, 120f));
                band.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
                Label(band, $"{w + 1}. İŞ: {world.title.ToUpper(new System.Globalization.CultureInfo("tr-TR"))}", 54f, world.color, FontStyles.Bold);
                var sub = Label(Rect("Sub", city, new Vector2(0f, -170f), new Vector2(900f, 70f)), $"Bölüm {first}-{last}  |  {world.subtitle}", 34f, Color.white, FontStyles.Normal);
                sub.gameObject.AddComponent<Shadow>().effectColor = new Color(0, 0, 0, 0.8f);
            }
            // Gerekenler satırı
            var needs = levels[first - 1].requiredItems;
            var prevNeeds = w > 0 ? levels[first - 2].requiredItems : new List<ItemType>();
            Label(Rect("NeedTitle", header, new Vector2(0f, -170f), new Vector2(900f, 50f)), "Giymen gerekenler:", 32f, new Color(0.85f, 0.9f, 1f, 0.9f), FontStyles.Normal);
            float iconSize = 96f, gap = 14f;
            float startX = -(needs.Count - 1) * (iconSize + gap) * 0.5f;
            for (int i = 0; i < needs.Count; i++)
            {
                var slot = Rect("Need", header, new Vector2(startX + i * (iconSize + gap), -250f), new Vector2(iconSize, iconSize));
                var sImg = slot.gameObject.AddComponent<Image>();
                sImg.sprite = roundTile; sImg.type = Image.Type.Sliced;
                bool isNew = !prevNeeds.Contains(needs[i]) && w > 0;
                sImg.color = isNew ? new Color(1f, 0.8f, 0.2f, 1f) : new Color(1f, 1f, 1f, 0.12f);
                sImg.raycastTarget = false;
                var icon = Rect("Icon", slot, Vector2.zero, new Vector2(iconSize - 12f, iconSize - 12f)).gameObject.AddComponent<Image>();
                icon.sprite = IconFor(needs[i]);
                icon.preserveAspect = true;
                icon.raycastTarget = false;
                if (isNew)
                    Label(Rect("New", slot, new Vector2(0f, 62f), new Vector2(120f, 34f)), "YENİ", 26f, new Color(1f, 0.85f, 0.3f), FontStyles.Bold);
            }
            if (w > 0)
            {
                // Bir önceki işten terfi
                var promo = Rect("Promotion", content, new Vector2(0f, y + 8f), new Vector2(560f, 76f));
                var pImg = promo.gameObject.AddComponent<Image>();
                pImg.sprite = roundTile; pImg.type = Image.Type.Sliced;
                pImg.color = worldOpen ? new Color(1f, 0.75f, 0.15f, 1f) : new Color(0.3f, 0.32f, 0.4f, 1f);
                pImg.raycastTarget = false;
                Label(promo, $"TERFİ: {world.title}", 36f, worldOpen ? new Color(0.15f, 0.1f, 0f) : new Color(0.75f, 0.78f, 0.85f), FontStyles.Bold);
            }

            // Bölüm düğümleri (zikzak yol)
            for (int n = first; n <= last; n++)
            {
                int i = n - first;
                float ny = y + HeaderHeight + i * NodeSpacing + NodeSpacing * 0.5f;
                float nx = Mathf.Sin((n - 1) * 0.85f) * 300f;
                var pos = new Vector2(nx, ny);
                if (hasPrev) Path(prev, pos, n <= unlocked);
                prev = pos; hasPrev = true;
                Node(n, pos, unlocked, world.color);
            }
            y += zoneHeight;
        }

        // Yol çizgileri düğümlerin arkasında kalsın
        foreach (Transform c in content)
            if (c.name == "Path") c.SetSiblingIndex(0);
        foreach (Transform c in content)
            if (c.name.StartsWith("Zone")) c.SetSiblingIndex(0);

        if (playButtonText) playButtonText.text = $"OYNA  -  BÖLÜM {unlocked}";
        Canvas.ForceUpdateCanvases();
        if (currentNode && scroll)
        {
            float viewH = ((RectTransform)scroll.viewport).rect.height;
            float target = currentNode.anchoredPosition.y - viewH * 0.4f;
            scroll.verticalNormalizedPosition = Mathf.Clamp01(target / Mathf.Max(1f, total - viewH));
        }
    }

    void Node(int n, Vector2 pos, int unlocked, Color worldColor)
    {
        bool done = n < unlocked, current = n == unlocked;
        float size = current ? 190f : 150f;
        var rt = Rect("Level" + n, content, pos, new Vector2(size, size));
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = circle;
        img.color = current ? new Color(1f, 0.8f, 0.2f) : done ? worldColor : new Color(0.25f, 0.27f, 0.34f);
        var ring = Rect("Ring", rt, Vector2.zero, new Vector2(size + 16f, size + 16f)).gameObject.AddComponent<Image>();
        ring.sprite = circle;
        ring.color = new Color(1f, 1f, 1f, current ? 0.9f : done ? 0.55f : 0.15f);
        ring.raycastTarget = false;
        ring.transform.SetAsFirstSibling();
        // Kök görünmez dokunma alanı; üstünde beyaz halka ve renkli dolgu.
        var fill = Rect("Fill", rt, Vector2.zero, new Vector2(size, size)).gameObject.AddComponent<Image>();
        fill.sprite = circle;
        fill.color = img.color;
        fill.raycastTarget = false;
        img.color = new Color(0f, 0f, 0f, 0f); // kök sadece dokunma alanı

        Label(Rect("Num", rt, Vector2.zero, new Vector2(size, size)), n.ToString(), current ? 76f : 60f,
            n <= unlocked ? (current ? new Color(0.2f, 0.12f, 0f) : Color.white) : new Color(0.6f, 0.63f, 0.7f), FontStyles.Bold);

        if (done && check)
        {
            var c = Rect("Done", rt, new Vector2(size * 0.36f, -size * 0.36f), new Vector2(56f, 56f)).gameObject.AddComponent<Image>();
            c.sprite = check; c.raycastTarget = false;
        }
        if (n > unlocked && lockIcon)
        {
            var l = Rect("Lock", rt, new Vector2(size * 0.36f, -size * 0.36f), new Vector2(50f, 50f)).gameObject.AddComponent<Image>();
            l.sprite = lockIcon; l.raycastTarget = false;
        }

        var button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = fill;
        button.interactable = n <= unlocked;
        int level = n;
        button.onClick.AddListener(() => PlayLevel(level));

        if (current)
        {
            currentNode = rt;
            // Seçili karakter düğümün üstünde: "buradasın"
            var ch = SelectedCharacter();
            if (ch && ch.icon)
            {
                var face = Rect("Runner", rt, new Vector2(0f, size * 0.5f + 95f), new Vector2(150f, 150f)).gameObject.AddComponent<Image>();
                face.sprite = ch.icon; face.raycastTarget = false;
                var tag = Rect("You", rt, new Vector2(0f, size * 0.5f + 12f), new Vector2(200f, 44f));
                var ti = tag.gameObject.AddComponent<Image>();
                ti.sprite = roundTile; ti.type = Image.Type.Sliced; ti.color = new Color(0.1f, 0.12f, 0.2f, 0.95f); ti.raycastTarget = false;
                Label(tag, "BURADASIN", 26f, Color.white, FontStyles.Bold);
            }
        }
    }

    void Path(Vector2 a, Vector2 b, bool open)
    {
        // Kesik çizgi: küçük noktalar
        float len = Vector2.Distance(a, b);
        int dots = Mathf.Max(2, Mathf.RoundToInt(len / 34f));
        for (int i = 1; i < dots; i++)
        {
            var p = Vector2.Lerp(a, b, i / (float)dots);
            var d = Rect("Path", content, p, new Vector2(16f, 16f)).gameObject.AddComponent<Image>();
            d.sprite = circle;
            d.color = open ? new Color(1f, 0.85f, 0.35f, 0.9f) : new Color(1f, 1f, 1f, 0.22f);
            d.raycastTarget = false;
        }
    }

    void Update()
    {
        if (!currentNode) return;
        pulse += Time.unscaledDeltaTime * 3f;
        currentNode.localScale = Vector3.one * (1f + Mathf.Sin(pulse) * 0.05f);
    }

    public void PlayLevel(int n)
    {
        if (n > SaveSystem.UnlockedLevel) return;
        SaveSystem.PlayLevel = n;
        SceneManager.LoadScene("Game");
    }

    public void PlayCurrent() => PlayLevel(Mathf.Clamp(SaveSystem.UnlockedLevel, 1, levels.Length));

    CharacterData SelectedCharacter()
    {
        foreach (var c in characters) if (c && c.id == SaveSystem.SelectedCharacter) return c;
        return characters.Length > 0 ? characters[0] : null;
    }

    Sprite IconFor(ItemType item)
    {
        foreach (var i in itemIcons) if (i.item == item) return i.sprite;
        return null;
    }

    static RectTransform Rect(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        bool inContent = parent.GetComponent<LevelMapContent>() != null;
        rt.anchorMin = rt.anchorMax = inContent ? new Vector2(0.5f, 0f) : new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    static TMP_Text Label(RectTransform rt, string value, float size, Color color, FontStyles style)
    {
        // Bir objede tek Graphic olabilir: zemini olan objeye yazı ayrı bir çocukta eklenir.
        if (rt.GetComponent<Graphic>())
        {
            var child = Rect("Text", rt, Vector2.zero, Vector2.zero);
            child.anchorMin = Vector2.zero; child.anchorMax = Vector2.one;
            child.offsetMin = child.offsetMax = Vector2.zero;
            rt = child;
        }
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = value;
        t.fontSize = size;
        t.color = color;
        t.fontStyle = style;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        return t;
    }
}
