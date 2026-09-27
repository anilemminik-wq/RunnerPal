using UnityEngine;
using UnityEngine.UI;

// Hızlanınca (enerji içeceği, taksi) ekranın kenarlarında merkeze doğru akan çizgiler çıkar ve kamera biraz
// genişler. Tam ekran bir RectTransform'a eklenir; çizgileri kendisi üretir.
[RequireComponent(typeof(RectTransform))]
public class SpeedLines : MonoBehaviour
{
    public PlayerController player;
    public Camera cam;
    public int lineCount = 36;
    public Color color = new Color(1f, 1f, 1f, 0.55f);
    [Tooltip("Hızlanınca kameranın görüş açısına eklenen derece")]
    public float fovKick = 8f;

    struct Line { public RectTransform rt; public Image img; public float angle, dist, speed, length; }
    Line[] lines;
    float intensity, baseFov;
    RectTransform area;

    void Start()
    {
        area = (RectTransform)transform;
        if (cam) baseFov = cam.fieldOfView;

        // Bir uçtan diğerine solan ince çizgi dokusu.
        var tex = new Texture2D(64, 4, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int x = 0; x < 64; x++)
            for (int y = 0; y < 4; y++)
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Sin(x / 63f * Mathf.PI) * (y == 0 || y == 3 ? 0.4f : 1f)));
        tex.Apply();
        var sprite = Sprite.Create(tex, new Rect(0, 0, 64, 4), new Vector2(0.5f, 0.5f));

        lines = new Line[lineCount];
        for (int i = 0; i < lineCount; i++)
        {
            var go = new GameObject("Line", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false;
            lines[i] = new Line { rt = (RectTransform)go.transform, img = img };
            Respawn(ref lines[i], true);
        }
    }

    void Respawn(ref Line l, bool anywhere)
    {
        l.angle = Random.value * Mathf.PI * 2f;
        l.dist = anywhere ? Random.Range(0.55f, 1.3f) : Random.Range(1.1f, 1.3f);
        l.speed = Random.Range(1.2f, 2.2f);
        l.length = Random.Range(180f, 420f);
        l.rt.sizeDelta = new Vector2(l.length, Random.Range(4f, 9f));
        l.rt.localRotation = Quaternion.Euler(0f, 0f, l.angle * Mathf.Rad2Deg);
    }

    void Update()
    {
        float target = player ? Mathf.Clamp01((player.SpeedFactor - 1f) / 0.5f) : 0f;
        intensity = Mathf.MoveTowards(intensity, target, Time.unscaledDeltaTime * 3f);
        if (cam) cam.fieldOfView = baseFov + fovKick * intensity;

        Vector2 half = area.rect.size * 0.5f;
        for (int i = 0; i < lines.Length; i++)
        {
            ref var l = ref lines[i];
            if (intensity <= 0.001f) { l.img.enabled = false; continue; }
            l.img.enabled = true;
            // Kenardan içeri akar; ekranın ortasına (0.55) gelmeden kaybolur ki yol ve karakter açık kalsın.
            l.dist -= l.speed * Time.deltaTime;
            if (l.dist < 0.55f) Respawn(ref l, false);
            var dir = new Vector2(Mathf.Cos(l.angle), Mathf.Sin(l.angle));
            l.rt.anchoredPosition = new Vector2(dir.x * half.x, dir.y * half.y) * l.dist * 1.15f;
            float fade = Mathf.Clamp01((l.dist - 0.55f) / 0.25f);
            var c = color; c.a *= intensity * fade;
            l.img.color = c;
        }
    }
}
