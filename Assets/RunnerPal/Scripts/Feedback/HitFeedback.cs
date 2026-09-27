using UnityEngine;
using UnityEngine.UI;

// Can gidince bunun anlaşılır olması için: engel parçalanır, kamera sarsılır, ekran kısa kırmızı yanar,
// kaybedilen kalp büyüyüp kaybolur, dokunulmazlık boyunca karakter yanıp söner. Canlar bitince karakter düşer.
public class HitFeedback : MonoBehaviour
{
    public PlayerController player;
    public CameraFollow cameraFollow;
    [Tooltip("Tam ekran kırmızı Image (alpha 0 başlar)")]
    public Image flash;
    [Tooltip("HUD kalpleri (soldan sağa)")]
    public Image[] hearts;

    public float flashAlpha = 0.45f;
    public float flashTime = 0.35f;
    public float blinkRate = 12f;

    float flashLeft;
    Renderer[] modelRenderers;
    GameManager gm;
    int lastLives = -1;

    void Start()
    {
        gm = FindFirstObjectByType<GameManager>();
        player.HitObstacle += OnHit;
        gm.OnLivesChanged += OnLives;
        gm.OnLevelFailed += OnFailed;
        if (flash) flash.color = new Color(flash.color.r, flash.color.g, flash.color.b, 0f);
    }

    void OnDestroy()
    {
        if (player) player.HitObstacle -= OnHit;
        if (gm) { gm.OnLivesChanged -= OnLives; gm.OnLevelFailed -= OnFailed; }
    }

    void OnHit(Obstacle obstacle, bool damaged)
    {
        if (obstacle) obstacle.Break(player.transform.position);
        if (!damaged) { cameraFollow?.Shake(0.12f, 0.2f); return; }
        if (obstacle && obstacle.kind == Obstacle.Kind.Hole) StartCoroutine(FallIntoHole());
        cameraFollow?.Shake(0.35f, 0.4f);
        flashLeft = flashTime;
    }

    // Açık rögar: adam deliğe düşer, sonra çıkıp koşmaya devam eder.
    System.Collections.IEnumerator FallIntoHole()
    {
        var model = player.modelRoot;
        if (!model) yield break;
        float baseY = model.localPosition.y;
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.18f) { SetY(model, Mathf.Lerp(baseY, baseY - 1.3f, t * t)); yield return null; }
        SetY(model, baseY - 1.3f);
        yield return new WaitForSeconds(0.3f);
        for (float t = 0f; t < 1f; t += Time.deltaTime / 0.3f) { SetY(model, Mathf.Lerp(baseY - 1.3f, baseY, 1f - (1f - t) * (1f - t))); yield return null; }
        SetY(model, baseY);
    }

    static void SetY(Transform t, float y) { var p = t.localPosition; p.y = y; t.localPosition = p; }

    void OnLives(int lives)
    {
        // Kaybedilen kalbin kopyası büyüyüp solarak kaybolur.
        if (lastLives > lives && hearts != null && lives >= 0 && lives < hearts.Length && hearts[lives])
        {
            var ghost = Instantiate(hearts[lives], hearts[lives].transform.parent);
            ghost.enabled = true;
            ghost.gameObject.AddComponent<HeartPop>();
        }
        lastLives = lives;
    }

    void OnFailed(string reason)
    {
        // Canlar bittiyse adam yere yığılır.
        // Straight to the state: the last hit sets "Hit" and stops the run in the same frame, which would
        // otherwise win over a "Fall" trigger.
        if (gm.Lives > 0) return;
        var anim = player.GetComponentInChildren<Animator>();
        if (!anim) return;
        anim.ResetTrigger("Hit");
        anim.CrossFadeInFixedTime("Fall", 0.1f);
    }

    void Update()
    {
        if (flash && flashLeft > 0f)
        {
            flashLeft -= Time.deltaTime;
            var c = flash.color;
            c.a = flashAlpha * Mathf.Clamp01(flashLeft / flashTime);
            flash.color = c;
        }

        // Dokunulmazlık süresince yanıp sönme (model mağazadan sonra değişebildiği için tembel alınır).
        if (modelRenderers == null || modelRenderers.Length == 0) modelRenderers = player.GetComponentsInChildren<Renderer>(true);
        // Only while running: the invulnerability timer stops when the run ends, and the runner must stay visible.
        bool running = gm && gm.CurrentState == GameManager.State.Running;
        bool visible = !running || !player.IsInvulnerable || Mathf.Repeat(Time.time * blinkRate, 1f) < 0.55f;
        foreach (var r in modelRenderers) if (r && r.enabled != visible) r.enabled = visible;
    }
}

// Kaybedilen kalp animasyonu.
public class HeartPop : MonoBehaviour
{
    float t;
    Image img;

    void Start() => img = GetComponent<Image>();

    void Update()
    {
        t += Time.deltaTime / 0.5f;
        transform.localScale = Vector3.one * (1f + t * 1.2f);
        if (img) { var c = img.color; c.a = 1f - t; img.color = c; }
        if (t >= 1f) Destroy(gameObject);
    }
}
