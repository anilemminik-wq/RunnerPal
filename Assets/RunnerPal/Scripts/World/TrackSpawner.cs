using System.Collections.Generic;
using UnityEngine;

// Bölümün yolunu LevelData'ya göre baştan sona üretir.
// Gerekli kıyafetlerin HEPSİNİ yola garanti olarak yerleştirir; engellerin arkasına saklamaz.
public class TrackSpawner : MonoBehaviour
{
    [Header("Yerleşim")]
    public float segmentLength = 20f;      // Yol parçası uzunluğu
    public float safeStartDistance = 30f;  // Başlangıçta boş alan
    public float laneWidth = 2.5f;

    [Header("Prefablar")]
    public GameObject[] obstaclePrefabs;   // Obstacle.cs içeren (alçak, yüksek, tam blok...)
    public GameObject goldPrefab;          // Pickup (Gold)
    public GameObject speedBoostPrefab;    // Pickup (SpeedBoost)
    public GameObject shieldPrefab;        // Pickup (Shield)
    public GameObject slowTrapPrefab;      // Pickup (SlowTrap)
    public GameObject finishPrefab;        // FinishLine.cs (iş yeri kapısı)

    [System.Serializable]
    public struct ItemPrefab { public ItemType item; public GameObject prefab; }
    public ItemPrefab[] itemPrefabs;       // Her eşya için Pickup (Item) prefabı

    readonly List<GameObject> spawned = new List<GameObject>();

    public void Build(LevelData level)
    {
        Clear();

        // 1) Yol parçaları
        int segCount = Mathf.CeilToInt(level.levelLength / segmentLength) + 2;
        for (int i = 0; i < segCount; i++)
        {
            if (level.roadSegmentPrefabs == null || level.roadSegmentPrefabs.Length == 0) break;
            var prefab = level.roadSegmentPrefabs[Random.Range(0, level.roadSegmentPrefabs.Length)];
            Spawn(prefab, new Vector3(0, 0, i * segmentLength));
        }

        // 2) Gerekli eşyaların yerleri: yolu eşit dilimlere böl, her dilime bir eşya
        var itemSlots = new Dictionary<int, ItemType>(); // segment index -> eşya
        var items = new List<ItemType>(level.requiredItems);
        Shuffle(items);
        float usable = level.levelLength - safeStartDistance - segmentLength * 2;
        for (int i = 0; i < items.Count; i++)
        {
            float z = safeStartDistance + usable * (i + 0.5f) / items.Count;
            itemSlots[Mathf.FloorToInt(z / segmentLength)] = items[i];
        }

        // 3) Her segmente içerik
        int firstSeg = Mathf.CeilToInt(safeStartDistance / segmentLength);
        int lastSeg = Mathf.FloorToInt((level.levelLength - segmentLength) / segmentLength);
        for (int s = firstSeg; s <= lastSeg; s++)
        {
            float z = s * segmentLength + segmentLength * 0.5f;
            int freeLane = Random.Range(0, 3); // Bu şerit asla engelle kapanmaz

            // Kıyafet: serbest şeride koy
            if (itemSlots.TryGetValue(s, out var item))
                SpawnItem(item, LanePos(freeLane, z));

            // Engeller: serbest şerit dışındaki şeritlere
            for (int lane = 0; lane < 3; lane++)
            {
                if (lane == freeLane) continue;
                if (Random.value < level.obstacleChance && obstaclePrefabs.Length > 0)
                    Spawn(obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)], LanePos(lane, z));
            }

            // Altın dizisi (kıyafetin önünde, serbest şeritte)
            if (Random.value < level.goldChance && goldPrefab)
                for (int g = 0; g < 5; g++)
                    Spawn(goldPrefab, LanePos(freeLane, z - segmentLength * 0.45f + g * 1.5f) + Vector3.up * 0.8f);

            // Güçlendirici / tuzak (segmentin başına, rastgele şeritte)
            float r = Random.value;
            Vector3 extraPos = LanePos(Random.Range(0, 3), s * segmentLength + 2f) + Vector3.up * 0.8f;
            if (r < level.powerUpChance)
                Spawn(Random.value < 0.5f ? speedBoostPrefab : shieldPrefab, extraPos);
            else if (r < level.powerUpChance + level.slowTrapChance)
                Spawn(slowTrapPrefab, LanePos(Random.Range(0, 3), s * segmentLength + 2f));
        }

        // 4) Bitiş
        if (finishPrefab) Spawn(finishPrefab, new Vector3(0, 0, level.levelLength));
    }

    void SpawnItem(ItemType item, Vector3 pos)
    {
        foreach (var ip in itemPrefabs)
            if (ip.item == item) { Spawn(ip.prefab, pos + Vector3.up * 1f); return; }
        Debug.LogWarning($"TrackSpawner: {item} için prefab atanmamış!");
    }

    Vector3 LanePos(int lane, float z) => new Vector3((lane - 1) * laneWidth, 0f, z);

    void Spawn(GameObject prefab, Vector3 pos)
    {
        if (!prefab) return;
        spawned.Add(Instantiate(prefab, pos, prefab.transform.rotation, transform));
    }

    void Clear()
    {
        foreach (var go in spawned) if (go) Destroy(go);
        spawned.Clear();
    }

    static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
