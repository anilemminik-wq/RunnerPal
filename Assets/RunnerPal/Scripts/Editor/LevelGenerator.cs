#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Menü: RunnerPal > 40 Bölüm Oluştur
// Assets/RunnerPal/Levels klasörüne Level_01 ... Level_40 asset'lerini üretir.
// Sonra istediğin bölümü tek tek elle ince ayar yapabilirsin.
public static class LevelGenerator
{
    const string Folder = "Assets/RunnerPal/Levels";

    static readonly string[] Jobs =
    {
        "Ofis Çalışanı",     // 1-10
        "Banka Memuru",      // 11-20
        "Satış Temsilcisi",  // 21-30
        "Yazılımcı"          // 31-40
    };

    [MenuItem("RunnerPal/40 Bölüm Oluştur")]
    public static void Generate()
    {
        if (!AssetDatabase.IsValidFolder("Assets/RunnerPal")) AssetDatabase.CreateFolder("Assets", "RunnerPal");
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/RunnerPal", "Levels");

        for (int n = 1; n <= 40; n++)
        {
            int world = (n - 1) / 10;       // 0..3
            int inWorld = (n - 1) % 10;     // 0..9

            var level = ScriptableObject.CreateInstance<LevelData>();
            level.levelNumber = n;
            level.jobName = Jobs[world];

            // Gereksinimler: her 10 bölümde bir eşya eklenir
            level.requiredItems = new List<ItemType>
                { ItemType.Gomlek, ItemType.Pantolon, ItemType.Ceket, ItemType.Ayakkabi };
            if (world >= 1) level.requiredItems.Add(ItemType.Saat);
            if (world >= 2) level.requiredItems.Add(ItemType.Telefon);
            if (world >= 3) level.requiredItems.Add(ItemType.Laptop);

            // Zorluk: her bölüm biraz daha hızlı ve uzun
            level.startSpeed = 10f + (n - 1) * 0.2f;
            level.maxSpeed = level.startSpeed + 8f + world * 1.5f;
            level.levelLength = 500f + inWorld * 30f + world * 80f;

            // Süre: ortalama hızla gereken sürenin %35 fazlası (tuzaklar/çarpışmalar için pay)
            float avgSpeed = (level.startSpeed + level.maxSpeed) * 0.5f;
            level.timeLimit = Mathf.Round(level.levelLength / avgSpeed * 1.35f);

            level.obstacleChance = Mathf.Min(0.35f + n * 0.01f, 0.75f);
            level.slowTrapChance = Mathf.Min(0.05f + n * 0.004f, 0.2f);
            level.goldChance = 0.6f;
            level.powerUpChance = 0.08f;

            string path = $"{Folder}/Level_{n:00}.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(level, path);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("RunnerPal: 40 bölüm oluşturuldu -> " + Folder +
                  "\nHer dünyanın yol prefablarını ve skybox'ını bölümlere atamayı unutma.");
    }
}
#endif
