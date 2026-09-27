using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Bölümün kurallarını yöneten merkez: can, altın, süre, toplanan eşyalar, bölüm sonu kontrolü.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum State { Briefing, Running, Won, Failed }

    [Header("Bölümler (sırayla: 1, 2, 3 ...)")]
    public LevelData[] levels;

    [Header("Kurallar")]
    public int maxLives = 3;

    [Header("Referanslar")]
    public PlayerController player;
    public TrackSpawner spawner;

    // --- Durum ---
    public State CurrentState { get; private set; } = State.Briefing;
    public LevelData Level { get; private set; }
    public int Lives { get; private set; }
    public int LevelGold { get; private set; }       // Bu bölümde toplanan altın
    public float TimeLeft { get; private set; }
    public float Progress01 => Level ? Mathf.Clamp01(player.DistanceTravelled / Level.levelLength) : 0f;
    readonly HashSet<ItemType> collected = new HashSet<ItemType>();

    // --- UI için olaylar ---
    public event Action<LevelData> OnBriefing;                 // Bölüm başı: gerekli eşyaları göster
    public event Action OnLevelStarted;
    public event Action<int> OnLivesChanged;
    public event Action<int> OnGoldChanged;
    public event Action<ItemType> OnItemCollected;
    public event Action<string> OnLevelFailed;                 // sebep metni
    public event Action<int, ItemType?> OnLevelWon;           // kazanılan altın, satın alınan eksik (varsa)

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        int index = Mathf.Clamp(SaveSystem.UnlockedLevel - 1, 0, levels.Length - 1);
        LoadLevel(index);
    }

    public void LoadLevel(int index)
    {
        Level = levels[index];
        Lives = maxLives;
        LevelGold = 0;
        TimeLeft = Level.timeLimit;
        collected.Clear();
        CurrentState = State.Briefing;

        ApplyTheme();
        spawner.Build(Level);
        OnBriefing?.Invoke(Level);   // UI bu sırada gereksinim listesini gösterir, sonra StartRun() çağırır
    }

    // Briefing ekranındaki "Başla" butonuna bağla
    public void StartRun()
    {
        if (CurrentState != State.Briefing) return;
        CurrentState = State.Running;
        player.BeginRun();
        OnLevelStarted?.Invoke();
        OnLivesChanged?.Invoke(Lives);
        OnGoldChanged?.Invoke(LevelGold);
    }

    void Update()
    {
        if (CurrentState != State.Running) return;

        TimeLeft -= Time.deltaTime;
        if (TimeLeft <= 0f)
        {
            TimeLeft = 0f;
            Fail("Süre doldu! İşe geç kaldın.");
        }
    }

    // ---------- Oyun içi olaylar ----------

    public void AddGold(int amount)
    {
        if (CurrentState != State.Running) return;
        LevelGold += amount;
        OnGoldChanged?.Invoke(LevelGold);
    }

    public void CollectItem(ItemType item)
    {
        if (CurrentState != State.Running) return;
        if (collected.Add(item))
        {
            player.Outfit?.Wear(item);
            OnItemCollected?.Invoke(item);
        }
    }

    public bool HasItem(ItemType item) => collected.Contains(item);

    public void LoseLife()
    {
        if (CurrentState != State.Running) return;
        Lives--;
        OnLivesChanged?.Invoke(Lives);
        if (Lives <= 0) Fail("Canların bitti!");
    }

    // ---------- Bölüm sonu ----------

    public void ReachFinish()
    {
        if (CurrentState != State.Running) return;
        player.StopRun();

        var missing = new List<ItemType>();
        foreach (var item in Level.requiredItems)
            if (!collected.Contains(item)) missing.Add(item);

        // Kural: 2+ eksik = game over
        if (missing.Count >= 2)
        {
            Fail($"{missing.Count} parça eksik! Bu kılıkla işe giremezsin.");
            return;
        }

        ItemType? bought = null;

        // Kural: 1 eksik = altının %50'si ile satın al (0 veya 1 altın varsa başarısız)
        if (missing.Count == 1)
        {
            if (LevelGold <= 1)
            {
                Fail($"{ItemNames.Get(missing[0])} eksik ve eksiği alacak altının yok!");
                return;
            }
            int cost = Mathf.CeilToInt(LevelGold * 0.5f);
            LevelGold -= cost;
            bought = missing[0];
            player.Outfit?.Wear(missing[0]);
        }

        Win(bought);
    }

    void Win(ItemType? boughtItem)
    {
        CurrentState = State.Won;
        SaveSystem.Gold += LevelGold;

        int currentIndex = Array.IndexOf(levels, Level);
        if (currentIndex + 1 < levels.Length)
            SaveSystem.UnlockedLevel = Mathf.Max(SaveSystem.UnlockedLevel, currentIndex + 2);

        OnLevelWon?.Invoke(LevelGold, boughtItem);
    }

    void Fail(string reason)
    {
        if (CurrentState == State.Failed) return;
        CurrentState = State.Failed;
        player.StopRun();
        // Başarısız bölümde toplanan altın kasaya eklenmez.
        OnLevelFailed?.Invoke(reason);
    }

    // ---------- UI butonları ----------

    public void RestartLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    public void NextLevel() => RestartLevel(); // UnlockedLevel zaten ilerledi, sahne yeniden yüklenince sıradaki başlar

    public void GoToMenu() => SceneManager.LoadScene("MainMenu");

    // ---------- Tema ----------

    void ApplyTheme()
    {
        if (Level.skyboxMaterial) RenderSettings.skybox = Level.skyboxMaterial;
        RenderSettings.fogColor = Level.fogColor;
    }
}
