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

    [Header("Duraklatma")]
    public Button pauseButton;
    public GameObject pausePanel;
    public Button resumeButton;
    public Button pauseMenuButton;

    [Header("Enerji içeceği butonu (sağda)")]
    public Button energyButton;
    public TMP_Text energyCountText;
    [Tooltip("Etkinken kalan süreyi gösteren dairesel dolgu (Image Filled)")]
    public Image energyTimerFill;
    public CanvasGroup energyGroup;

    [Header("Taksi")]
    public GameObject taxiBanner;
    public TMP_Text taxiText;

    [Header("Sonuç")]
    public GameObject winPanel;
    public TMP_Text winText;
    public GameObject failPanel;
    public TMP_Text failText;

    [Header("Eşya ikonları")]
    public ItemIcon[] itemIcons;
    [System.Serializable] public struct ItemIcon { public ItemType item; public Sprite sprite; public string label; }

    readonly Dictionary<ItemType, ItemCard> checklist = new Dictionary<ItemType, ItemCard>();
    GameManager gm;
    PlayerController player;

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

        player = gm.player;
        if (pauseButton) pauseButton.onClick.AddListener(gm.Pause);
        if (resumeButton) resumeButton.onClick.AddListener(gm.Resume);
        if (pauseMenuButton) pauseMenuButton.onClick.AddListener(gm.GoToMenu);
        gm.OnPauseChanged += p => { if (pausePanel) pausePanel.SetActive(p); };
        if (pausePanel) pausePanel.SetActive(false);
        if (energyButton) energyButton.onClick.AddListener(player.UseEnergyDrink);
        player.EnergyChanged += n => { if (energyCountText) energyCountText.text = n.ToString(); };
        if (taxiBanner) taxiBanner.SetActive(false);
    }

    void Update()
    {
        if (gm.CurrentState == GameManager.State.Running)
        {
            timerText.text = Mathf.CeilToInt(gm.TimeLeft).ToString();
            timerText.color = gm.TimeLeft < 10f ? Color.red : Color.white;
        }

        // Enerji butonu: içecek varsa ve şu an etkin değilse basılabilir; etkinken kalan süre dairede görünür.
        if (energyButton)
        {
            bool ready = player.EnergyDrinks > 0 && !player.EnergyActive && !player.InTaxi;
            energyButton.interactable = ready;
            if (energyGroup) energyGroup.alpha = ready || player.EnergyActive ? 1f : 0.45f;
            if (energyTimerFill)
            {
                energyTimerFill.enabled = player.EnergyActive;
                energyTimerFill.fillAmount = player.EnergyTimeLeft / player.energyDuration;
            }
        }
        if (taxiBanner)
        {
            bool taxi = player.InTaxi && gm.CurrentState == GameManager.State.Running;
            if (taxiBanner.activeSelf != taxi) taxiBanner.SetActive(taxi);
            if (taxi && taxiText) taxiText.text = $"TAKSİ! {Mathf.CeilToInt(player.TaxiTimeLeft)}";
        }
    }

    void ShowBriefing(LevelData level)
    {
        hudPanel.SetActive(false); winPanel.SetActive(false); failPanel.SetActive(false);
        briefingPanel.SetActive(true);
        briefingTitle.text = $"Bölüm {level.levelNumber} - {level.jobName}\nİşe geç kalma! {level.timeLimit:0} saniyen var.";
        if (player && player.Character)
            briefingTitle.text += $"\n<size=75%><color=#FFD86B>{player.Character.displayName}</color> - {player.Character.lives} can</size>";

        Fill(requiredListParent, level.requiredItems, null);
        Fill(checklistParent, level.requiredItems, checklist);
    }

    void Fill(Transform parent, List<ItemType> items, Dictionary<ItemType, ItemCard> map)
    {
        foreach (Transform c in parent) Destroy(c.gameObject);
        map?.Clear();
        foreach (var item in items)
        {
            var go = Instantiate(requiredIconPrefab, parent);
            var info = GetIcon(item);
            var card = go.GetComponent<ItemCard>();
            if (card)
            {
                card.icon.sprite = info.sprite;
                if (card.label) card.label.text = info.label;
                // Bölüm başında hepsi net görünür; HUD'da toplanana kadar soluk.
                card.SetCollected(false, false);
                if (map == null && card.icon) card.icon.color = Color.white;
                if (map != null) map[item] = card;
                continue;
            }
            var img = go.GetComponentInChildren<Image>();
            img.sprite = info.sprite;
            var label = go.GetComponentInChildren<TMP_Text>();
            if (label) label.text = info.label;
        }
    }

    void MarkCollected(ItemType item)
    {
        if (checklist.TryGetValue(item, out var card)) card.SetCollected(true, true);
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
