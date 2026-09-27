# RunnerPal – Claude Code Görev Dosyası

Bu dosyayı Unity projesinin kök klasörüne koy ve Claude Code'a şunu yaz:

> `RUNNERPAL_TASK.md` dosyasını oku. İçindeki tüm scriptleri belirtilen yollara oluştur, derleme hatalarını düzelt, sonra "Sahne Kurulumu" bölümündeki adımları Unity bağlantını kullanarak uygula. Modeller yerine şimdilik küp/kapsül kullan.

---

## Claude Code için talimatlar

Sen bir Unity (Universal 3D, Android hedefli) projesinde çalışıyorsun. Oyunun adı **RunnerPal**.

### Oyun özeti
İşe geç kalan bir adam don ve atletle evden çıkıyor. 3 şeritli, arkadan kameralı (Subway Surfers tarzı) bir yolda koşuyor. Altın topluyor, engellerden kaçıyor ve işe giyeceği kıyafetleri yoldan topluyor.

### Kurallar (kodda birebir uygulanmalı, değiştirme)
1. Kontroller: sağa/sola kaydır = şerit değiştir, yukarı = zıpla, aşağı = eğil. Editörde ok tuşları / WASD.
2. Hız bölüm ilerledikçe artar. Her bölüm bir öncekinden daha hızlı başlar.
3. 3 can. Engele çarpınca 1 can gider, kısa dokunulmazlık olur. Can biterse bölüm başarısız.
4. Güçlendiriciler: **Hızlanma** (süreli hız artışı), **Kalkan** (süre boyunca çarpışmada can gitmez).
5. **Yavaşlama bir tuzaktır** (ör. dökülmüş kahve): süreli hız düşüşü.
6. Her bölümün süre sınırı var. Süre dolmadan bitişe varılmazsa bölüm başarısız.
7. Bölüm başında toplanması gereken kıyafetler ekranda gösterilir.
8. Bölüm sonu kontrolü:
   - 0 eksik → bölüm geçildi
   - 1 eksik → o bölümde toplanan altının %50'si ödenir ve geçilir. Altın 0 veya 1 ise başarısız.
   - 2 veya daha fazla eksik → game over, bölüm baştan
9. Başarısız bölümde toplanan altın kasaya eklenmez.
10. Her 10 bölümde iş, arka plan, yol ve gereksinimler değişir:
    - 1–10: Gömlek, Pantolon, Ceket, Ayakkabı (Ofis Çalışanı)
    - 11–20: + Saat (Banka Memuru)
    - 21–30: + Telefon (Satış Temsilcisi)
    - 31–40: + Laptop (Yazılımcı)
11. Ana menüde mağaza: toplanan altınla karakter satın alınır ve seçilir.

### Mimari
- Bölümler `LevelData` ScriptableObject ile tanımlanır. `RunnerPal > 40 Bölüm Oluştur` menüsü 40 bölümü üretir.
- `GameManager` kuralları yönetir, UI olaylarla (event) bağlanır.
- `TrackSpawner` yolu baştan sona üretir. Gerekli kıyafetlerin hepsi yola garanti ve engelle kapanmayan şeride konur.
- Eski Input sistemi kullanılıyor: Project Settings > Player > Active Input Handling = **Both** yap.
- UI için TextMeshPro kullanılıyor.

---

## Sahne Kurulumu (Unity bağlantısıyla yap)

1. `RunnerPal > 40 Bölüm Oluştur` menüsünü çalıştır.
2. Placeholder prefablar oluştur (`Assets/RunnerPal/Prefabs`):
   - Altın: sarı küçük silindir, trigger collider, `Pickup` Kind=Gold
   - Her kıyafet/eşya için farklı renkte küp, trigger collider, `Pickup` Kind=Item ve ilgili ItemType
   - Hızlanma (yeşil), Kalkan (mavi), Yavaşlama tuzağı (kahverengi, yassı): `Pickup`
   - 2 engel: alçak (zıplanacak) ve yüksek bariyer (altından eğilinecek), trigger collider + `Obstacle`
   - Bitiş: geniş trigger kutu + `FinishLine`
   - Yol parçası: 20 m uzunluğunda, 7.5 m genişliğinde zemin (3 şerit, 2.5 m), şerit çizgileriyle
3. `Game` sahnesi:
   - Player: kapsül, tag `Player`, `CapsuleCollider` (trigger değil), `Rigidbody`, `PlayerController`. İçinde boş `ModelRoot` child'ı. Modele `PlayerOutfit` ekle ve her ItemType için gizli bir görsel parça (renkli küp) bağla.
   - Main Camera: `CameraFollow`, target = Player
   - GameManager objesi: 40 bölümü sırayla bağla, Player ve TrackSpawner'ı bağla
   - TrackSpawner objesi: prefabları bağla. Tüm bölümlerin `roadSegmentPrefabs` alanına yol parçasını ata.
   - Canvas + `GameUI`: Briefing paneli (başlık, gerekli eşya listesi, Başla butonu), HUD (altın, süre, 3 kalp, eşya checklist'i), Win paneli (Sonraki Bölüm → `GameManager.NextLevel`), Fail paneli (Tekrar → `GameManager.RestartLevel`)
4. `MainMenu` sahnesi: Canvas + `ShopManager`, `ShopItemView` içeren kart prefabı, Oyna butonu → `ShopManager.Play`. İlk karakterin id'si `default`, fiyatı 0.
5. Build Settings: `MainMenu` (0), `Game` (1).
6. Play modunda test et: 1. bölümü kıyafetleri toplayarak, bir kıyafeti kaçırarak ve iki kıyafeti kaçırarak dene. Üç durumun da kurallara uyduğunu doğrula.

---

## Scriptler

Aşağıdaki her dosyayı başlıktaki yola oluştur.

### `Assets/RunnerPal/Scripts/Data/ItemType.cs`

```csharp
// RunnerPal - Toplanabilir kıyafet/eşya türleri
// Yeni eşya eklemek için buraya satır ekle (ör. Kravat, Canta).
public enum ItemType
{
    Gomlek,
    Pantolon,
    Ceket,
    Ayakkabi,
    Saat,      // 11. bölümden itibaren
    Telefon,   // 21. bölümden itibaren
    Laptop     // 31. bölümden itibaren
}

```

### `Assets/RunnerPal/Scripts/Data/LevelData.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;

// Her bölüm bir LevelData asset'i ile tanımlanır.
// Project penceresi > sağ tık > Create > RunnerPal > Level Data
[CreateAssetMenu(fileName = "Level_01", menuName = "RunnerPal/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Genel")]
    public int levelNumber = 1;
    [Tooltip("Bölüm başında gösterilecek iş adı, ör. 'Ofis Çalışanı'")]
    public string jobName = "Ofis Çalışanı";

    [Header("Gereksinimler")]
    [Tooltip("Bölüm sonuna kadar toplanması gereken eşyalar")]
    public List<ItemType> requiredItems = new List<ItemType>
    {
        ItemType.Gomlek, ItemType.Pantolon, ItemType.Ceket, ItemType.Ayakkabi
    };

    [Header("Yol ve Süre")]
    [Tooltip("Bölümün toplam uzunluğu (metre)")]
    public float levelLength = 600f;
    [Tooltip("Bu süre içinde bitiş çizgisine varılmazsa game over (saniye)")]
    public float timeLimit = 60f;

    [Header("Hız")]
    public float startSpeed = 10f;
    public float maxSpeed = 20f;
    [Tooltip("0 = bölüm başı, 1 = bölüm sonu. Hızın bölüm boyunca nasıl artacağı")]
    public AnimationCurve speedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Yerleştirme yoğunluğu (her segmentte)")]
    [Range(0f, 1f)] public float obstacleChance = 0.5f;
    [Range(0f, 1f)] public float goldChance = 0.6f;
    [Range(0f, 1f)] public float powerUpChance = 0.08f;
    [Range(0f, 1f)] public float slowTrapChance = 0.1f;

    [Header("Tema (her 10 bölümde bir değişir)")]
    public GameObject[] roadSegmentPrefabs;   // Bu temaya ait yol parçaları
    public Material skyboxMaterial;
    public Color fogColor = Color.gray;

    public float GetSpeed(float progress01)
    {
        return Mathf.Lerp(startSpeed, maxSpeed, speedCurve.Evaluate(Mathf.Clamp01(progress01)));
    }
}

```

### `Assets/RunnerPal/Scripts/Data/CharacterData.cs`

```csharp
using UnityEngine;

// Mağazada satılan karakterler.
// Create > RunnerPal > Character Data
[CreateAssetMenu(fileName = "Character_", menuName = "RunnerPal/Character Data")]
public class CharacterData : ScriptableObject
{
    public string id = "default";
    public string displayName = "Mehmet Bey";
    public int price = 0;               // 0 = başlangıçta açık
    public Sprite icon;
    public GameObject modelPrefab;      // Don + atlet halindeki model (PlayerOutfit içerir)
}

```

### `Assets/RunnerPal/Scripts/Core/SaveSystem.cs`

```csharp
using UnityEngine;

// Basit kayıt sistemi (PlayerPrefs). İleride bulut kaydına geçilebilir.
public static class SaveSystem
{
    const string KeyGold = "rp_gold";
    const string KeyLevel = "rp_unlocked_level";
    const string KeySelectedChar = "rp_selected_char";
    const string KeyOwnedPrefix = "rp_owned_";

    public static int Gold
    {
        get => PlayerPrefs.GetInt(KeyGold, 0);
        set { PlayerPrefs.SetInt(KeyGold, Mathf.Max(0, value)); PlayerPrefs.Save(); }
    }

    public static int UnlockedLevel
    {
        get => PlayerPrefs.GetInt(KeyLevel, 1);
        set { PlayerPrefs.SetInt(KeyLevel, Mathf.Max(1, value)); PlayerPrefs.Save(); }
    }

    public static string SelectedCharacter
    {
        get => PlayerPrefs.GetString(KeySelectedChar, "default");
        set { PlayerPrefs.SetString(KeySelectedChar, value); PlayerPrefs.Save(); }
    }

    public static bool IsOwned(string charId) =>
        charId == "default" || PlayerPrefs.GetInt(KeyOwnedPrefix + charId, 0) == 1;

    public static void SetOwned(string charId)
    {
        PlayerPrefs.SetInt(KeyOwnedPrefix + charId, 1);
        PlayerPrefs.Save();
    }
}

```

### `Assets/RunnerPal/Scripts/Core/GameManager.cs`

```csharp
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
                Fail($"{missing[0]} eksik ve eksiği alacak altının yok!");
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

```

### `Assets/RunnerPal/Scripts/Player/PlayerController.cs`

```csharp
using UnityEngine;

// 3 şeritli koşu kontrolü. Mobilde kaydırma (swipe), editörde klavye (ok tuşları / A-D-W-S).
[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Şerit")]
    public float laneWidth = 2.5f;
    public float laneChangeSpeed = 15f;

    [Header("Zıplama / Eğilme")]
    public float jumpHeight = 2.2f;
    public float gravity = -40f;
    public float slideDuration = 0.7f;

    [Header("Çarpışma sonrası")]
    public float hitInvulnerability = 1.2f;   // Can kaybından sonra kısa dokunulmazlık
    public float hitSlowdown = 0.5f;          // Çarpınca kısa süre yavaşla

    [Header("Swipe")]
    public float minSwipeDistance = 50f;      // piksel

    [Header("Karakterler (mağazadakilerle aynı liste)")]
    public CharacterData[] characters;
    public Transform modelRoot;               // Modelin yerleşeceği boş child obje

    public PlayerOutfit Outfit { get; private set; }
    public float DistanceTravelled { get; private set; }
    public float CurrentSpeed { get; private set; }
    public bool HasShield => shieldTimer > 0f;

    Animator anim;
    CapsuleCollider col;
    float colHeight; Vector3 colCenter;

    int lane = 1;                 // 0 sol, 1 orta, 2 sağ
    float verticalVel;
    float groundY;
    bool running, sliding;
    float slideTimer;

    float speedMul = 1f, speedMulTimer;  // hızlanma / yavaşlama tuzağı
    float shieldTimer;
    float invulnTimer;

    Vector2 touchStart; bool touchTracking;

    void Awake()
    {
        LoadSelectedCharacter();
        Outfit = GetComponentInChildren<PlayerOutfit>();
        anim = GetComponentInChildren<Animator>();
        col = GetComponent<CapsuleCollider>();
        colHeight = col.height; colCenter = col.center;
        groundY = transform.position.y;

        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;     // Hareket kodla, fizik sadece tetikleyiciler için
        rb.useGravity = false;
    }

    // Mağazada seçilen karakterin modelini yükler (modelRoot altındaki varsayılan model silinir)
    void LoadSelectedCharacter()
    {
        if (!modelRoot || characters == null) return;
        string id = SaveSystem.SelectedCharacter;
        foreach (var ch in characters)
        {
            if (ch.id != id || !ch.modelPrefab) continue;
            foreach (Transform c in modelRoot) DestroyImmediate(c.gameObject);
            Instantiate(ch.modelPrefab, modelRoot);
            return;
        }
    }

    public void BeginRun()
    {
        running = true;
        anim?.SetBool("Running", true);
    }

    public void StopRun()
    {
        running = false;
        CurrentSpeed = 0f;
        anim?.SetBool("Running", false);
    }

    void Update()
    {
        if (!running) return;

        HandleInput();
        UpdateTimers();

        // İleri hareket: hız bölüm ilerledikçe artar
        var gm = GameManager.Instance;
        CurrentSpeed = gm.Level.GetSpeed(gm.Progress01) * speedMul;
        float forward = CurrentSpeed * Time.deltaTime;
        DistanceTravelled += forward;

        // Şerit geçişi
        Vector3 pos = transform.position;
        float targetX = (lane - 1) * laneWidth;
        pos.x = Mathf.MoveTowards(pos.x, targetX, laneChangeSpeed * Time.deltaTime);

        // Zıplama / yerçekimi
        verticalVel += gravity * Time.deltaTime;
        pos.y += verticalVel * Time.deltaTime;
        if (pos.y <= groundY) { pos.y = groundY; verticalVel = 0f; }

        pos.z += forward;
        transform.position = pos;

        anim?.SetFloat("Speed", CurrentSpeed);
    }

    // ---------- Girdi ----------

    void HandleInput()
    {
        // Klavye (editörde test için)
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) MoveLane(-1);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) MoveLane(1);
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) Jump();
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) Slide();

        // Dokunmatik swipe
        if (Input.touchCount == 0) return;
        Touch t = Input.GetTouch(0);
        if (t.phase == TouchPhase.Began) { touchStart = t.position; touchTracking = true; }
        else if (touchTracking && (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Ended))
        {
            Vector2 d = t.position - touchStart;
            if (d.magnitude < minSwipeDistance) return;
            touchTracking = false;
            if (Mathf.Abs(d.x) > Mathf.Abs(d.y)) MoveLane(d.x > 0 ? 1 : -1);
            else if (d.y > 0) Jump();
            else Slide();
        }
    }

    void MoveLane(int dir) => lane = Mathf.Clamp(lane + dir, 0, 2);

    bool IsGrounded => transform.position.y <= groundY + 0.01f;

    void Jump()
    {
        if (!IsGrounded) return;
        EndSlide();
        verticalVel = Mathf.Sqrt(2f * -gravity * jumpHeight);
        anim?.SetTrigger("Jump");
    }

    void Slide()
    {
        if (!IsGrounded) verticalVel = gravity * 0.5f; // Havadaysa hızlıca in
        sliding = true;
        slideTimer = slideDuration;
        col.height = colHeight * 0.5f;
        col.center = new Vector3(colCenter.x, colCenter.y - colHeight * 0.25f, colCenter.z);
        anim?.SetTrigger("Slide");
    }

    void EndSlide()
    {
        if (!sliding) return;
        sliding = false;
        col.height = colHeight;
        col.center = colCenter;
    }

    // ---------- Güçlendiriciler & tuzaklar ----------

    void UpdateTimers()
    {
        if (sliding && (slideTimer -= Time.deltaTime) <= 0f) EndSlide();
        if (shieldTimer > 0f) shieldTimer -= Time.deltaTime;
        if (invulnTimer > 0f) invulnTimer -= Time.deltaTime;
        if (speedMulTimer > 0f && (speedMulTimer -= Time.deltaTime) <= 0f) speedMul = 1f;
    }

    public void ApplySpeedModifier(float multiplier, float duration)
    {
        speedMul = multiplier;
        speedMulTimer = duration;
    }

    public void ApplyShield(float duration) => shieldTimer = duration;

    // Engel çarpınca Obstacle çağırır
    public void OnHitObstacle()
    {
        if (HasShield || invulnTimer > 0f) return;
        invulnTimer = hitInvulnerability;
        ApplySpeedModifier(hitSlowdown, 0.6f);
        anim?.SetTrigger("Hit");
        GameManager.Instance.LoseLife();
    }
}

```

### `Assets/RunnerPal/Scripts/Player/PlayerOutfit.cs`

```csharp
using System;
using UnityEngine;

// Karakter modelinin üstündeki kıyafet objelerini açıp kapatır.
// Başta hepsi kapalı: adam don + atletle başlar. Topladıkça ilgili obje görünür olur.
public class PlayerOutfit : MonoBehaviour
{
    [Serializable]
    public struct ItemVisual
    {
        public ItemType item;
        public GameObject visual;   // Modelin içindeki gömlek/pantolon/... objesi
    }

    public ItemVisual[] visuals;
    public ParticleSystem wearEffect; // Giyinirken çıkan küçük "puf" efekti (opsiyonel)

    void Awake()
    {
        foreach (var v in visuals)
            if (v.visual) v.visual.SetActive(false);
    }

    public void Wear(ItemType item)
    {
        foreach (var v in visuals)
            if (v.item == item && v.visual) v.visual.SetActive(true);
        if (wearEffect) wearEffect.Play();
    }
}

```

### `Assets/RunnerPal/Scripts/Player/CameraFollow.cs`

```csharp
using UnityEngine;

// Arkadan takip eden kamera. Şerit değişiminde yumuşak kayar.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 4f, -7f);
    public float xFollow = 0.6f;     // Kamera şerit hareketini ne kadar takip etsin (0-1)
    public float smooth = 8f;

    void LateUpdate()
    {
        if (!target) return;
        Vector3 desired = new Vector3(target.position.x * xFollow, 0f, target.position.z) + offset;
        desired.x = Mathf.Lerp(transform.position.x, desired.x, smooth * Time.deltaTime);
        transform.position = desired;
        transform.LookAt(target.position + Vector3.up * 1.5f + Vector3.forward * 4f);
    }
}

```

### `Assets/RunnerPal/Scripts/World/Pickup.cs`

```csharp
using UnityEngine;

// Yolda toplanabilen her şey: altın, kıyafet/eşya, güçlendirici, yavaşlama tuzağı.
// Objeye trigger collider ekle ve Player objesinin tag'ini "Player" yap.
public class Pickup : MonoBehaviour
{
    public enum Kind { Gold, Item, SpeedBoost, Shield, SlowTrap }

    public Kind kind = Kind.Gold;

    [Header("Altın")]
    public int goldAmount = 1;

    [Header("Kıyafet / Eşya")]
    public ItemType item;

    [Header("Güçlendirici / Tuzak")]
    public float duration = 4f;
    [Tooltip("SpeedBoost için >1 (ör. 1.5), SlowTrap için <1 (ör. 0.5)")]
    public float speedMultiplier = 1.5f;

    [Header("Görsel")]
    public float spinSpeed = 120f;
    public GameObject collectEffect;

    void Update()
    {
        if (kind != Kind.SlowTrap) transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var player = other.GetComponentInParent<PlayerController>();
        var gm = GameManager.Instance;

        switch (kind)
        {
            case Kind.Gold:       gm.AddGold(goldAmount); break;
            case Kind.Item:       gm.CollectItem(item); break;
            case Kind.SpeedBoost: player.ApplySpeedModifier(speedMultiplier, duration); break;
            case Kind.Shield:     player.ApplyShield(duration); break;
            case Kind.SlowTrap:   player.ApplySpeedModifier(speedMultiplier, duration); break;
        }

        if (collectEffect) Instantiate(collectEffect, transform.position, Quaternion.identity);
        gameObject.SetActive(false);
    }
}

```

### `Assets/RunnerPal/Scripts/World/Obstacle.cs`

```csharp
using UnityEngine;

// Engel. Tipine göre zıplanarak, eğilerek ya da şerit değiştirerek kaçılır.
// Kaçma şekli collider'ın yüksekliğiyle belirlenir (alçak engel = zıpla, yüksek bariyer = eğil).
public class Obstacle : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        other.GetComponentInParent<PlayerController>()?.OnHitObstacle();
    }
}

```

### `Assets/RunnerPal/Scripts/World/FinishLine.cs`

```csharp
using UnityEngine;

// Bölüm sonu (iş yerinin kapısı). TrackSpawner bunu levelLength mesafesine koyar.
public class FinishLine : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        GameManager.Instance.ReachFinish();
    }
}

```

### `Assets/RunnerPal/Scripts/World/TrackSpawner.cs`

```csharp
using System.Collections.Generic;
using UnityEngine;

// Bölümün yolunu LevelData'ya göre baştan sona üretir.
// Gerekli kıyafetlerin HEPSİNİ yola garanti olarak yerleştirir; engellerin arkasına saklamaz.
public class TrackSpawner : MonoBehaviour
{
    [Header("Yerleşim")]
    public float segmentLength = 20f;      // Yol parçası uzunluğu
    public float safeStartDistance = 30f;  // Başlangıçta boş alan
    public float laneWidth = 2.5f;

    [Header("Prefablar")]
    public GameObject[] obstaclePrefabs;   // Obstacle.cs içeren (alçak, yüksek, tam blok...)
    public GameObject goldPrefab;          // Pickup (Gold)
    public GameObject speedBoostPrefab;    // Pickup (SpeedBoost)
    public GameObject shieldPrefab;        // Pickup (Shield)
    public GameObject slowTrapPrefab;      // Pickup (SlowTrap)
    public GameObject finishPrefab;        // FinishLine.cs (iş yeri kapısı)

    [System.Serializable]
    public struct ItemPrefab { public ItemType item; public GameObject prefab; }
    public ItemPrefab[] itemPrefabs;       // Her eşya için Pickup (Item) prefabı

    readonly List<GameObject> spawned = new List<GameObject>();

    public void Build(LevelData level)
    {
        Clear();

        // 1) Yol parçaları
        int segCount = Mathf.CeilToInt(level.levelLength / segmentLength) + 2;
        for (int i = 0; i < segCount; i++)
        {
            if (level.roadSegmentPrefabs == null || level.roadSegmentPrefabs.Length == 0) break;
            var prefab = level.roadSegmentPrefabs[Random.Range(0, level.roadSegmentPrefabs.Length)];
            Spawn(prefab, new Vector3(0, 0, i * segmentLength));
        }

        // 2) Gerekli eşyaların yerleri: yolu eşit dilimlere böl, her dilime bir eşya
        var itemSlots = new Dictionary<int, ItemType>(); // segment index -> eşya
        var items = new List<ItemType>(level.requiredItems);
        Shuffle(items);
        float usable = level.levelLength - safeStartDistance - segmentLength * 2;
        for (int i = 0; i < items.Count; i++)
        {
            float z = safeStartDistance + usable * (i + 0.5f) / items.Count;
            itemSlots[Mathf.FloorToInt(z / segmentLength)] = items[i];
        }

        // 3) Her segmente içerik
        int firstSeg = Mathf.CeilToInt(safeStartDistance / segmentLength);
        int lastSeg = Mathf.FloorToInt((level.levelLength - segmentLength) / segmentLength);
        for (int s = firstSeg; s <= lastSeg; s++)
        {
            float z = s * segmentLength + segmentLength * 0.5f;
            int freeLane = Random.Range(0, 3); // Bu şerit asla engelle kapanmaz

            // Kıyafet: serbest şeride koy
            if (itemSlots.TryGetValue(s, out var item))
                SpawnItem(item, LanePos(freeLane, z));

            // Engeller: serbest şerit dışındaki şeritlere
            for (int lane = 0; lane < 3; lane++)
            {
                if (lane == freeLane) continue;
                if (Random.value < level.obstacleChance && obstaclePrefabs.Length > 0)
                    Spawn(obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)], LanePos(lane, z));
            }

            // Altın dizisi (kıyafetin önünde, serbest şeritte)
            if (Random.value < level.goldChance && goldPrefab)
                for (int g = 0; g < 5; g++)
                    Spawn(goldPrefab, LanePos(freeLane, z - segmentLength * 0.45f + g * 1.5f) + Vector3.up * 0.8f);

            // Güçlendirici / tuzak (segmentin başına, rastgele şeritte)
            float r = Random.value;
            Vector3 extraPos = LanePos(Random.Range(0, 3), s * segmentLength + 2f) + Vector3.up * 0.8f;
            if (r < level.powerUpChance)
                Spawn(Random.value < 0.5f ? speedBoostPrefab : shieldPrefab, extraPos);
            else if (r < level.powerUpChance + level.slowTrapChance)
                Spawn(slowTrapPrefab, LanePos(Random.Range(0, 3), s * segmentLength + 2f));
        }

        // 4) Bitiş
        if (finishPrefab) Spawn(finishPrefab, new Vector3(0, 0, level.levelLength));
    }

    void SpawnItem(ItemType item, Vector3 pos)
    {
        foreach (var ip in itemPrefabs)
            if (ip.item == item) { Spawn(ip.prefab, pos + Vector3.up * 1f); return; }
        Debug.LogWarning($"TrackSpawner: {item} için prefab atanmamış!");
    }

    Vector3 LanePos(int lane, float z) => new Vector3((lane - 1) * laneWidth, 0f, z);

    void Spawn(GameObject prefab, Vector3 pos)
    {
        if (!prefab) return;
        spawned.Add(Instantiate(prefab, pos, prefab.transform.rotation, transform));
    }

    void Clear()
    {
        foreach (var go in spawned) if (go) Destroy(go);
        spawned.Clear();
    }

    static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}

```

### `Assets/RunnerPal/Scripts/UI/GameUI.cs`

```csharp
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
        return new ItemIcon { item = item, label = item.ToString() };
    }
}

```

### `Assets/RunnerPal/Scripts/UI/ShopManager.cs`

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Ana menüdeki mağaza: toplanan altınla karakter satın alma ve seçme.
public class ShopManager : MonoBehaviour
{
    public CharacterData[] characters;
    public Transform listParent;
    public GameObject shopItemPrefab;   // Kökünde ShopItemView olan kart prefabı
    public TMP_Text totalGoldText;

    void Start() => Refresh();

    public void Refresh()
    {
        totalGoldText.text = SaveSystem.Gold.ToString();
        foreach (Transform c in listParent) Destroy(c.gameObject);

        foreach (var ch in characters)
        {
            var view = Instantiate(shopItemPrefab, listParent).GetComponent<ShopItemView>();

            if (view.icon && ch.icon) view.icon.sprite = ch.icon;
            view.nameText.text = ch.displayName;

            bool owned = SaveSystem.IsOwned(ch.id);
            bool selected = SaveSystem.SelectedCharacter == ch.id;
            view.priceText.text = selected ? "Seçili" : owned ? "Seç" : $"{ch.price} altın";
            view.button.interactable = !selected && (owned || SaveSystem.Gold >= ch.price);

            var captured = ch;
            view.button.onClick.AddListener(() => OnClick(captured));
        }
    }

    void OnClick(CharacterData ch)
    {
        if (!SaveSystem.IsOwned(ch.id))
        {
            if (SaveSystem.Gold < ch.price) return;
            SaveSystem.Gold -= ch.price;
            SaveSystem.SetOwned(ch.id);
        }
        SaveSystem.SelectedCharacter = ch.id;
        Refresh();
    }

    public void Play() => SceneManager.LoadScene("Game");
}

```

### `Assets/RunnerPal/Scripts/UI/ShopItemView.cs`

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Mağazadaki tek bir karakter kartının referansları (shopItemPrefab'in köküne ekle).
public class ShopItemView : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public Button button;
}

```

### `Assets/RunnerPal/Scripts/Editor/LevelGenerator.cs`

```csharp
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Menü: RunnerPal > 40 Bölüm Oluştur
// Assets/RunnerPal/Levels klasörüne Level_01 ... Level_40 asset'lerini üretir.
// Sonra istediğin bölümü tek tek elle ince ayar yapabilirsin.
public static class LevelGenerator
{
    const string Folder = "Assets/RunnerPal/Levels";

    static readonly string[] Jobs =
    {
        "Ofis Çalışanı",     // 1-10
        "Banka Memuru",      // 11-20
        "Satış Temsilcisi",  // 21-30
        "Yazılımcı"          // 31-40
    };

    [MenuItem("RunnerPal/40 Bölüm Oluştur")]
    public static void Generate()
    {
        if (!AssetDatabase.IsValidFolder("Assets/RunnerPal")) AssetDatabase.CreateFolder("Assets", "RunnerPal");
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/RunnerPal", "Levels");

        for (int n = 1; n <= 40; n++)
        {
            int world = (n - 1) / 10;       // 0..3
            int inWorld = (n - 1) % 10;     // 0..9

            var level = ScriptableObject.CreateInstance<LevelData>();
            level.levelNumber = n;
            level.jobName = Jobs[world];

            // Gereksinimler: her 10 bölümde bir eşya eklenir
            level.requiredItems = new List<ItemType>
                { ItemType.Gomlek, ItemType.Pantolon, ItemType.Ceket, ItemType.Ayakkabi };
            if (world >= 1) level.requiredItems.Add(ItemType.Saat);
            if (world >= 2) level.requiredItems.Add(ItemType.Telefon);
            if (world >= 3) level.requiredItems.Add(ItemType.Laptop);

            // Zorluk: her bölüm biraz daha hızlı ve uzun
            level.startSpeed = 10f + (n - 1) * 0.2f;
            level.maxSpeed = level.startSpeed + 8f + world * 1.5f;
            level.levelLength = 500f + inWorld * 30f + world * 80f;

            // Süre: ortalama hızla gereken sürenin %35 fazlası (tuzaklar/çarpışmalar için pay)
            float avgSpeed = (level.startSpeed + level.maxSpeed) * 0.5f;
            level.timeLimit = Mathf.Round(level.levelLength / avgSpeed * 1.35f);

            level.obstacleChance = Mathf.Min(0.35f + n * 0.01f, 0.75f);
            level.slowTrapChance = Mathf.Min(0.05f + n * 0.004f, 0.2f);
            level.goldChance = 0.6f;
            level.powerUpChance = 0.08f;

            string path = $"{Folder}/Level_{n:00}.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(level, path);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("RunnerPal: 40 bölüm oluşturuldu -> " + Folder +
                  "\nHer dünyanın yol prefablarını ve skybox'ını bölümlere atamayı unutma.");
    }
}
#endif

```

