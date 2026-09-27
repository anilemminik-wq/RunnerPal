using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Oyun içi arayüz: bölüm başı gereksinim ekranı, HUD, kazanma/kaybetme panelleri.
public class GameUI : MonoBehaviour
{
    [Header("Bölüm başı (Briefing)")]
    public GameObject briefingPanel;
    public TMP_Text briefingTitle;        // "Bölüm 12 - Banka Memuru"
    public Transform requiredListParent;  // Gerekli eşya ikonlarının dizileceği yer
    public GameObject requiredIconPrefab; // Image + (opsiyonel) TMP_Text içeren küçük kart
    public Button startButton;

    [Header("HUD")]
    public GameObject hudPanel;
    public TMP_Text goldText;
    public TMP_Text timerText;
    public Image[] heartIcons;            // 3 kalp
    public Transform checklistParent;     // Oyun sırasında gerekli eşyalar (toplanınca yanar)

    [Header("Sonuç")]
    public GameObject winPanel;
    public TMP_Text winText;
    public GameObject failPanel;
    public TMP_Text failText;

    [Header("Eşya ikonları")]
    public ItemIcon[] itemIcons;
    [System.Serializable] public struct ItemIcon { public ItemType item; public Sprite sprite; public string label; }

    readonly Dictionary<ItemType, Image> checklist = new Dictionary<ItemType, Image>();
    GameManager gm;

    void Awake()
    {
        gm = FindFirstObjectByType<GameManager>();
        gm.OnBriefing += ShowBriefing;
        gm.OnLevelStarted += () => { briefingPanel.SetActive(false); hudPanel.SetActive(true); };
        gm.OnLivesChanged += UpdateLives;
        gm.OnGoldChanged += g => goldText.text = g.ToString();
        gm.OnItemCollected += MarkCollected;
        gm.OnLevelWon += ShowWin;
        gm.OnLevelFailed += ShowFail;
        startButton.onClick.AddListener(gm.StartRun);
    }

    void Update()
    {
        if (gm.CurrentState == GameManager.State.Running)
        {
            timerText.text = Mathf.CeilToInt(gm.TimeLeft).ToString();
            timerText.color = gm.TimeLeft < 10f ? Color.red : Color.white;
        }
    }

    void ShowBriefing(LevelData level)
    {
        hudPanel.SetActive(false); winPanel.SetActive(false); failPanel.SetActive(false);
        briefingPanel.SetActive(true);
        briefingTitle.text = $"Bölüm {level.levelNumber} - {level.jobName}\nİşe geç kalma! {level.timeLimit:0} saniyen var.";

        Fill(requiredListParent, level.requiredItems, null);
        Fill(checklistParent, level.requiredItems, checklist);
    }

    void Fill(Transform parent, List<ItemType> items, Dictionary<ItemType, Image> map)
    {
        foreach (Transform c in parent) Destroy(c.gameObject);
        map?.Clear();
        foreach (var item in items)
        {
            var go = Instantiate(requiredIconPrefab, parent);
            var img = go.GetComponentInChildren<Image>();
            var info = GetIcon(item);
            img.sprite = info.sprite;
            var label = go.GetComponentInChildren<TMP_Text>();
            if (label) label.text = info.label;
            if (map != null)
            {
                img.color = new Color(1, 1, 1, 0.3f);   // Toplanmadı: soluk
                map[item] = img;
            }
        }
    }

    void MarkCollected(ItemType item)
    {
        if (checklist.TryGetValue(item, out var img)) img.color = Color.white;
    }

    void UpdateLives(int lives)
    {
        for (int i = 0; i < heartIcons.Length; i++) heartIcons[i].enabled = i < lives;
    }

    void ShowWin(int gold, ItemType? bought)
    {
        hudPanel.SetActive(false); winPanel.SetActive(true);
        var sb = new StringBuilder("İşe zamanında vardın!\n");
        if (bought.HasValue) sb.Append($"Eksik {GetIcon(bought.Value).label} altınının yarısıyla alındı.\n");
        sb.Append($"Kazanılan altın: {gold}");
        winText.text = sb.ToString();
    }

    void ShowFail(string reason)
    {
        hudPanel.SetActive(false); failPanel.SetActive(true);
        failText.text = reason;
    }

    ItemIcon GetIcon(ItemType item)
    {
        foreach (var i in itemIcons) if (i.item == item) return i;
        return new ItemIcon { item = item, label = ItemNames.Get(item) };
    }
}
