using UnityEngine;

// Çocuklarından rastgele birini açık bırakır (ör. aynı engel prefabında farklı araba modelleri).
public class RandomVariant : MonoBehaviour
{
    public GameObject[] variants;

    void Awake()
    {
        if (variants == null || variants.Length == 0) return;
        int pick = Random.Range(0, variants.Length);
        for (int i = 0; i < variants.Length; i++) if (variants[i]) variants[i].SetActive(i == pick);
    }
}
