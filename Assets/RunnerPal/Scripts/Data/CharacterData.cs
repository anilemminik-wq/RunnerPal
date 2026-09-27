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
