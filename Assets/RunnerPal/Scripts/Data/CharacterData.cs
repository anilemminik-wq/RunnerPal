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

    [Header("Özellikler (her karakter farklı oynanır)")]
    [Tooltip("Bölüm başındaki can sayısı")]
    [Range(1, 5)] public int lives = 3;
    [Tooltip("Koşu hızı çarpanı (1 = normal)")]
    public float speedMultiplier = 1f;
    [Tooltip("Zıplama yüksekliği çarpanı (1 = normal)")]
    public float jumpMultiplier = 1f;
    [Tooltip("Mağazada görünen kısa tanım")]
    public string description = "Her işi dengeli yapar.";
}
