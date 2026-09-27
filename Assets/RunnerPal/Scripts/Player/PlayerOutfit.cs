using System;
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
    }

    public ItemVisual[] visuals;
    public ParticleSystem wearEffect; // Giyinirken çıkan küçük "puf" efekti (opsiyonel)

    void Awake()
    {
        foreach (var v in visuals)
            if (v.visual) v.visual.SetActive(false);
    }

    public void Wear(ItemType item)
    {
        foreach (var v in visuals)
            if (v.item == item && v.visual) v.visual.SetActive(true);
        if (wearEffect) wearEffect.Play();
    }
}
