using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Mağazadaki tek bir karakter kartının referansları (shopItemPrefab'in köküne ekle).
public class ShopItemView : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text statsText;      // can / hız / zıplama + kısa tanım
    public Button button;
}
