using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Ana menüdeki mağaza: toplanan altınla karakter satın alma ve seçme.
public class ShopManager : MonoBehaviour
{
    public CharacterData[] characters;
    public Transform listParent;
    public GameObject shopItemPrefab;   // Kökünde ShopItemView olan kart prefabı
    public TMP_Text totalGoldText;

    // Seçim değişince (harita karakter resmini yeniler).
    public event System.Action Changed;

    void Start() => Refresh();

    public void Refresh()
    {
        totalGoldText.text = SaveSystem.Gold.ToString();
        foreach (Transform c in listParent) Destroy(c.gameObject);

        foreach (var ch in characters)
        {
            var view = Instantiate(shopItemPrefab, listParent).GetComponent<ShopItemView>();

            if (view.icon && ch.icon) view.icon.sprite = ch.icon;
            view.nameText.text = ch.displayName;
            if (view.statsText)
                view.statsText.text = $"<b>{ch.lives} can</b>   Hız %{Mathf.RoundToInt(ch.speedMultiplier * 100)}   Zıplama %{Mathf.RoundToInt(ch.jumpMultiplier * 100)}\n<size=85%><color=#C8D2EB>{ch.description}</color></size>";

            bool owned = SaveSystem.IsOwned(ch.id);
            bool selected = SaveSystem.SelectedCharacter == ch.id;
            view.priceText.text = selected ? "Seçili" : owned ? "Seç" : $"{ch.price} altın";
            view.button.interactable = !selected && (owned || SaveSystem.Gold >= ch.price);

            var captured = ch;
            view.button.onClick.AddListener(() => OnClick(captured));
        }
    }

    void OnClick(CharacterData ch)
    {
        if (!SaveSystem.IsOwned(ch.id))
        {
            if (SaveSystem.Gold < ch.price) return;
            SaveSystem.Gold -= ch.price;
            SaveSystem.SetOwned(ch.id);
        }
        SaveSystem.SelectedCharacter = ch.id;
        Refresh();
        Changed?.Invoke();
    }

    public void Play() => SceneManager.LoadScene("Game");
}
