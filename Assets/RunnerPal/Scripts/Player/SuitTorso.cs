using UnityEngine;

// Takım elbise gövdesi tek bir mesh: ceket, gömlek, kravat ve eller ayrı malzeme yuvaları.
// Gömlek ve ceket ayrı toplandığı için yuvaları duruma göre boyar:
//   sadece gömlek -> ceket bölgesi de beyaz (uzun kollu gömlek gibi)
//   sadece ceket  -> gömlek ve kravat bölgesi ten rengi (çıplak göğse ceket)
//   ikisi de      -> gerçek takım
[RequireComponent(typeof(SkinnedMeshRenderer))]
public class SuitTorso : MonoBehaviour
{
    public PlayerOutfit outfit;
    public int jacketSlot, shirtSlot, tieSlot;
    public Material jacket, shirt, tie, skin;

    SkinnedMeshRenderer smr;

    void Awake()
    {
        smr = GetComponent<SkinnedMeshRenderer>();
        if (outfit) outfit.Changed += Refresh;
    }

    void OnDestroy()
    {
        if (outfit) outfit.Changed -= Refresh;
    }

    void Refresh()
    {
        bool hasShirt = outfit.IsWearing(ItemType.Gomlek);
        bool hasJacket = outfit.IsWearing(ItemType.Ceket);
        var mats = smr.sharedMaterials;
        mats[jacketSlot] = hasJacket ? jacket : shirt;
        mats[shirtSlot] = hasShirt ? shirt : skin;
        mats[tieSlot] = hasShirt ? tie : skin;
        smr.sharedMaterials = mats;
    }
}
