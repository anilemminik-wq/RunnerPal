using UnityEngine;

// Basit kayıt sistemi (PlayerPrefs). İleride bulut kaydına geçilebilir.
public static class SaveSystem
{
    const string KeyGold = "rp_gold";
    const string KeyLevel = "rp_unlocked_level";
    const string KeySelectedChar = "rp_selected_char";
    const string KeyOwnedPrefix = "rp_owned_";

    public static int Gold
    {
        get => PlayerPrefs.GetInt(KeyGold, 0);
        set { PlayerPrefs.SetInt(KeyGold, Mathf.Max(0, value)); PlayerPrefs.Save(); }
    }

    public static int UnlockedLevel
    {
        get => PlayerPrefs.GetInt(KeyLevel, 1);
        set { PlayerPrefs.SetInt(KeyLevel, Mathf.Max(1, value)); PlayerPrefs.Save(); }
    }

    public static string SelectedCharacter
    {
        get => PlayerPrefs.GetString(KeySelectedChar, "default");
        set { PlayerPrefs.SetString(KeySelectedChar, value); PlayerPrefs.Save(); }
    }

    public static bool IsOwned(string charId) =>
        charId == "default" || PlayerPrefs.GetInt(KeyOwnedPrefix + charId, 0) == 1;

    public static void SetOwned(string charId)
    {
        PlayerPrefs.SetInt(KeyOwnedPrefix + charId, 1);
        PlayerPrefs.Save();
    }
}
