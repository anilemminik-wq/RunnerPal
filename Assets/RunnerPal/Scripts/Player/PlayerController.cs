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

    [Header("Enerji içeceği (sağdaki buton)")]
    public int startEnergyDrinks = 1;
    public int maxEnergyDrinks = 3;
    public float energyDuration = 8f;
    public float energyMultiplier = 1.5f;

    [Header("Taksi (nadir güçlendirici)")]
    [Tooltip("Taksideyken hız = bölümün en yüksek hızı x bu değer")]
    public float taxiSpeedFactor = 1.2f;
    [Tooltip("Taksiye binince görünen model (Player altında, kapalı başlar)")]
    public GameObject taxiModel;

    [Header("Karakterler (mağazadakilerle aynı liste)")]
    public CharacterData[] characters;
    public Transform modelRoot;               // Modelin yerleşeceği boş child obje

    public PlayerOutfit Outfit { get; private set; }
    public CharacterData Character { get; private set; }
    public float DistanceTravelled { get; private set; }
    public float CurrentSpeed { get; private set; }
    public bool HasShield => shieldTimer > 0f;
    public bool InTaxi => taxiTimer > 0f;
    public float TaxiTimeLeft => taxiTimer;
    // Hız çizgileri için: 1 = normal, >1 = hızlanmış.
    public float SpeedFactor => InTaxi ? 2f : speedMul;
    public bool EnergyActive => energyTimer > 0f;
    public float EnergyTimeLeft => energyTimer;
    public int EnergyDrinks { get; private set; }

    Animator anim;
    CapsuleCollider col;
    float colHeight; Vector3 colCenter;

    int lane = 1;                 // 0 sol, 1 orta, 2 sağ
    float verticalVel;
    float groundY;
    bool running, sliding;
    float slideTimer;

    float speedMul = 1f, speedMulTimer;  // hızlanma / yavaşlama tuzağı
    float energyTimer;
    float taxiTimer;
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
        if (Character) jumpHeight *= Character.jumpMultiplier;
        EnergyDrinks = startEnergyDrinks;
        if (taxiModel) taxiModel.SetActive(false);

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
            Character = ch;
            foreach (Transform c in modelRoot) DestroyImmediate(c.gameObject);
            Instantiate(ch.modelPrefab, modelRoot);
            return;
        }
        if (characters.Length > 0) Character = characters[0];
    }

    public void BeginRun()
    {
        running = true;
        anim?.SetBool("Running", true);
        EnergyChanged?.Invoke(EnergyDrinks);
    }

    public void StopRun()
    {
        running = false;
        CurrentSpeed = 0f;
        anim?.SetBool("Running", false);
        if (InTaxi) EndTaxi();
    }

    void Update()
    {
        if (!running || Time.timeScale == 0f) return;

        HandleInput();
        UpdateTimers();

        // İleri hareket: hız bölüm ilerledikçe artar; karakterin hız çarpanı ve taksi de etkiler
        var gm = GameManager.Instance;
        float characterSpeed = Character ? Character.speedMultiplier : 1f;
        CurrentSpeed = InTaxi
            ? gm.Level.maxSpeed * taxiSpeedFactor
            : gm.Level.GetSpeed(gm.Progress01) * speedMul * characterSpeed;
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
        if (Input.GetKeyDown(KeyCode.E)) UseEnergyDrink();

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
        if (!IsGrounded || InTaxi) return;
        EndSlide();
        verticalVel = Mathf.Sqrt(2f * -gravity * jumpHeight);
        anim?.SetTrigger("Jump");
        Jumped?.Invoke();
    }

    void Slide()
    {
        if (InTaxi) return;
        if (!IsGrounded) verticalVel = gravity * 0.5f; // Havadaysa hızlıca in
        sliding = true;
        slideTimer = slideDuration;
        col.height = colHeight * 0.5f;
        col.center = new Vector3(colCenter.x, colCenter.y - colHeight * 0.25f, colCenter.z);
        anim?.SetTrigger("Slide");
        Slid?.Invoke();
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
        if (energyTimer > 0f) energyTimer -= Time.deltaTime;
        if (speedMulTimer > 0f && (speedMulTimer -= Time.deltaTime) <= 0f) speedMul = 1f;
        if (taxiTimer > 0f && (taxiTimer -= Time.deltaTime) <= 0f) EndTaxi();
    }

    public void ApplySpeedModifier(float multiplier, float duration)
    {
        speedMul = multiplier;
        speedMulTimer = duration;
        if (multiplier < 1f) energyTimer = 0f; // tuzak / çarpma enerjiyi keser
    }

    public void ApplyShield(float duration) => shieldTimer = duration;

    // Yoldan toplanan enerji içeceği sağdaki butona eklenir.
    public void AddEnergyDrink()
    {
        EnergyDrinks = Mathf.Min(EnergyDrinks + 1, maxEnergyDrinks);
        EnergyChanged?.Invoke(EnergyDrinks);
    }

    // Sağdaki buton: bir içecek harcar, 8 saniye hızlanır.
    public void UseEnergyDrink()
    {
        if (!running || Time.timeScale == 0f || EnergyDrinks <= 0 || EnergyActive || InTaxi) return;
        EnergyDrinks--;
        energyTimer = energyDuration;
        ApplySpeedModifier(energyMultiplier, energyDuration);
        EnergyChanged?.Invoke(EnergyDrinks);
        EnergyUsed?.Invoke();
    }

    // Taksi: altınlar kendiliğinden gelir, engeller can götürmez, en yüksek hızda gidilir.
    public void EnterTaxi(float duration)
    {
        EndSlide();
        verticalVel = 0f;
        var p = transform.position; p.y = groundY; transform.position = p;
        taxiTimer = Mathf.Max(taxiTimer, duration);
        if (taxiModel) taxiModel.SetActive(true);
        if (modelRoot) modelRoot.gameObject.SetActive(false);
        TaxiChanged?.Invoke(true);
    }

    void EndTaxi()
    {
        taxiTimer = 0f;
        if (taxiModel) taxiModel.SetActive(false);
        if (modelRoot)
        {
            modelRoot.gameObject.SetActive(true);
            anim?.SetBool("Running", running);
        }
        // İnince kısa dokunulmazlık, önüne çıkan engele hemen çarpmasın.
        invulnTimer = Mathf.Max(invulnTimer, 1f);
        TaxiChanged?.Invoke(false);
    }

    // Ses ve görsel geri bildirim için (kurallar değişmez).
    public event System.Action Jumped;
    public event System.Action Slid;
    public event System.Action<int> EnergyChanged;
    public event System.Action EnergyUsed;
    public event System.Action<bool> TaxiChanged;
    // Engel, can gitti mi (kalkanla / taksiyle çarpınca false).
    public event System.Action<Obstacle, bool> HitObstacle;
    public bool IsInvulnerable => invulnTimer > 0f;

    // Engel çarpınca Obstacle çağırır
    public void OnHitObstacle(Obstacle obstacle = null)
    {
        if (InTaxi) { HitObstacle?.Invoke(obstacle, false); return; }
        if (invulnTimer > 0f) return;
        if (HasShield) { HitObstacle?.Invoke(obstacle, false); return; }
        invulnTimer = hitInvulnerability;
        ApplySpeedModifier(hitSlowdown, 0.6f);
        anim?.SetTrigger("Hit");
        HitObstacle?.Invoke(obstacle, true);
        GameManager.Instance.LoseLife();
    }
}
