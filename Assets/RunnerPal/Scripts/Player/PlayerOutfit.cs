using System;
using System.Collections.Generic;
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
        [Tooltip("Bu eşya giyilince gizlenen başlangıç parçaları (ör. pantolon gelince çıplak bacaklar)")]
        public GameObject[] replaces;
    }

    public ItemVisual[] visuals;
    public ParticleSystem wearEffect; // Giyinirken çıkan küçük "puf" efekti (opsiyonel)

    readonly HashSet<ItemType> worn = new HashSet<ItemType>();

    // Bir eşya giyildiğinde (ör. gömlek/ceket aynı gövde parçasını paylaşır, görünümünü buna göre ayarlar).
    public event Action Changed;

    public bool IsWearing(ItemType item) => worn.Contains(item);

    void Awake()
    {
        foreach (var v in visuals)
            if (v.visual) v.visual.SetActive(false);
    }

    public void Wear(ItemType item)
    {
        worn.Add(item);
        foreach (var v in visuals)
        {
            if (v.item != item) continue;
            if (v.visual) v.visual.SetActive(true);
            if (v.replaces != null)
                foreach (var part in v.replaces)
                    if (part) part.SetActive(false);
        }
        if (wearEffect) wearEffect.Play();
        Changed?.Invoke();
    }
}
