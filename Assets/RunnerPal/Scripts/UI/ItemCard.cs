using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Gerekli eşya kartı (bölüm başı listesi ve üstteki HUD listesi): arka plan, eşyanın resmi, isim, tik işareti.
public class ItemCard : MonoBehaviour
{
    public Image background;
    public Image icon;
    public Image check;
    public TMP_Text label;

    public Color missingColor = new Color(0.12f, 0.14f, 0.22f, 0.92f);
    public Color collectedColor = new Color(0.18f, 0.62f, 0.32f, 0.95f);

    float pop;

    // HUD'da: toplanmadıysa soluk, toplanınca yeşil + tik + küçük zıplama.
    public void SetCollected(bool collected, bool animate)
    {
        if (background) background.color = collected ? collectedColor : missingColor;
        if (icon) icon.color = collected ? Color.white : new Color(1f, 1f, 1f, 0.55f);
        if (check) check.enabled = collected;
        if (animate) pop = 1f;
    }

    void Update()
    {
        if (pop <= 0f) return;
        pop = Mathf.Max(0f, pop - Time.unscaledDeltaTime * 3f);
        transform.localScale = Vector3.one * (1f + Mathf.Sin(pop * Mathf.PI) * 0.35f);
    }
}
