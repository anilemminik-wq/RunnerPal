using System.Collections.Generic;
using UnityEngine;

// Her bölüm bir LevelData asset'i ile tanımlanır.
// Project penceresi > sağ tık > Create > RunnerPal > Level Data
[CreateAssetMenu(fileName = "Level_01", menuName = "RunnerPal/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Genel")]
    public int levelNumber = 1;
    [Tooltip("Bölüm başında gösterilecek iş adı, ör. 'Ofis Çalışanı'")]
    public string jobName = "Ofis Çalışanı";

    [Header("Gereksinimler")]
    [Tooltip("Bölüm sonuna kadar toplanması gereken eşyalar")]
    public List<ItemType> requiredItems = new List<ItemType>
    {
        ItemType.Gomlek, ItemType.Pantolon, ItemType.Ceket, ItemType.Ayakkabi
    };

    [Header("Yol ve Süre")]
    [Tooltip("Bölümün toplam uzunluğu (metre)")]
    public float levelLength = 600f;
    [Tooltip("Bu süre içinde bitiş çizgisine varılmazsa game over (saniye)")]
    public float timeLimit = 60f;

    [Header("Hız")]
    public float startSpeed = 10f;
    public float maxSpeed = 20f;
    [Tooltip("0 = bölüm başı, 1 = bölüm sonu. Hızın bölüm boyunca nasıl artacağı")]
    public AnimationCurve speedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Yerleştirme yoğunluğu (her segmentte)")]
    [Range(0f, 1f)] public float obstacleChance = 0.5f;
    [Range(0f, 1f)] public float goldChance = 0.6f;
    [Range(0f, 1f)] public float powerUpChance = 0.08f;
    [Range(0f, 1f)] public float slowTrapChance = 0.1f;

    [Header("Tema (her 10 bölümde bir değişir)")]
    public GameObject[] roadSegmentPrefabs;   // Bu temaya ait yol parçaları
    public Material skyboxMaterial;
    public Color fogColor = Color.gray;

    public float GetSpeed(float progress01)
    {
        return Mathf.Lerp(startSpeed, maxSpeed, speedCurve.Evaluate(Mathf.Clamp01(progress01)));
    }
}
