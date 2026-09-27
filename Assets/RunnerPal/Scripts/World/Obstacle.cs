using UnityEngine;

// Engel. Tipine göre zıplanarak, eğilerek ya da şerit değiştirerek kaçılır.
// Kaçma şekli collider'ın yüksekliğiyle belirlenir (alçak engel = zıpla, yüksek bariyer = eğil).
public class Obstacle : MonoBehaviour
{
    // Breakable: çarpınca parçalanır. Hole: açık rögar, içine düşülür (zıpla ya da şerit değiştir).
    // Vehicle: araba, çarpınca savrulur.
    public enum Kind { Breakable, Hole, Vehicle }
    public Kind kind = Kind.Breakable;
    [Tooltip("Bu engel en erken hangi bölümde çıkar")]
    public int minLevel = 1;

    [Tooltip("Çarpılınca kaç parçaya ayrılsın")]
    public int debrisCount = 8;
    [Tooltip("Parçaların malzemesi (boşsa modelin ilk malzemesi)")]
    public Material debrisMaterial;

    bool broken;

    void OnTriggerEnter(Collider other)
    {
        if (broken || !other.CompareTag("Player")) return;
        other.GetComponentInParent<PlayerController>()?.OnHitObstacle(this);
    }

    // Çarpışmada engel parçalanır: görseli ve tetikleyicisi kapanır, yerine savrulan parçalar çıkar.
    public void Break(Vector3 hitFrom)
    {
        if (broken) return;
        broken = true;
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
        if (kind == Kind.Hole) return; // çukur yerinde kalır, düşme efektini HitFeedback yapar
        if (kind == Kind.Vehicle)
        {
            var mover = GetComponent<ObstacleMover>();
            if (mover) mover.enabled = false;
            Vector3 side = transform.position - hitFrom; side.y = 0f;
            var fly = gameObject.AddComponent<Debris>();
            fly.life = 1.4f;
            fly.velocity = side.normalized * 6f + Vector3.up * 7f + Vector3.forward * 10f;
            return;
        }
        var renderers = GetComponentsInChildren<Renderer>();
        Material mat = debrisMaterial ? debrisMaterial : renderers.Length > 0 ? renderers[0].sharedMaterial : null;
        Bounds bounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds(transform.position, Vector3.one);
        foreach (var r in renderers) { bounds.Encapsulate(r.bounds); r.enabled = false; }

        for (int i = 0; i < debrisCount; i++)
        {
            var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(piece.GetComponent<Collider>());
            piece.name = "Debris";
            if (mat) piece.GetComponent<Renderer>().sharedMaterial = mat;
            Vector3 p = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), bounds.center.z);
            piece.transform.position = p;
            piece.transform.localScale = Vector3.one * Random.Range(0.18f, 0.35f);
            piece.transform.rotation = Random.rotation;
            var d = piece.AddComponent<Debris>();
            Vector3 away = p - hitFrom; away.y = 0f;
            d.velocity = away.normalized * Random.Range(2f, 5f) + Vector3.up * Random.Range(3f, 6f) + Vector3.forward * Random.Range(4f, 9f);
        }
    }
}

// Savrulan engel parçası: basit balistik hareket, küçülüp kaybolur.
public class Debris : MonoBehaviour
{
    public Vector3 velocity;
    public float life = 1.1f;
    Vector3 spin;
    float age;
    Vector3 startScale;

    void Start()
    {
        spin = Random.insideUnitSphere * 540f;
        startScale = transform.localScale;
    }

    void Update()
    {
        age += Time.deltaTime;
        velocity += Physics.gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        if (transform.position.y < 0.1f && velocity.y < 0f) { velocity.y *= -0.35f; velocity.x *= 0.6f; velocity.z *= 0.6f; }
        transform.Rotate(spin * Time.deltaTime, Space.World);
        transform.localScale = startScale * Mathf.Clamp01(1f - (age - life * 0.6f) / (life * 0.4f));
        if (age >= life) Destroy(gameObject);
    }
}
