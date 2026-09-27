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
        Jumped?.Invoke();
    }

    void Slide()
    {
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
        if (speedMulTimer > 0f && (speedMulTimer -= Time.deltaTime) <= 0f) speedMul = 1f;
    }

    public void ApplySpeedModifier(float multiplier, float duration)
    {
        speedMul = multiplier;
        speedMulTimer = duration;
    }

    public void ApplyShield(float duration) => shieldTimer = duration;

    // Ses ve görsel geri bildirim için (kurallar değişmez).
    public event System.Action Jumped;
    public event System.Action Slid;
    // Engel, can gitti mi (kalkanla çarpınca false).
    public event System.Action<Obstacle, bool> HitObstacle;
    public bool IsInvulnerable => invulnTimer > 0f;

    // Engel çarpınca Obstacle çağırır
    public void OnHitObstacle(Obstacle obstacle = null)
    {
        if (invulnTimer > 0f) return;
        if (HasShield) { HitObstacle?.Invoke(obstacle, false); return; }
        invulnTimer = hitInvulnerability;
        ApplySpeedModifier(hitSlowdown, 0.6f);
        anim?.SetTrigger("Hit");
        HitObstacle?.Invoke(obstacle, true);
        GameManager.Instance.LoseLife();
    }
}
