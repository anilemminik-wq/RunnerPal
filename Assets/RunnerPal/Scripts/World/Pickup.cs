using UnityEngine;

// Yolda toplanabilen her şey: altın, kıyafet/eşya, güçlendirici, yavaşlama tuzağı.
// Objeye trigger collider ekle ve Player objesinin tag'ini "Player" yap.
public class Pickup : MonoBehaviour
{
    public enum Kind { Gold, Item, SpeedBoost, Shield, SlowTrap }

    public Kind kind = Kind.Gold;

    [Header("Altın")]
    public int goldAmount = 1;

    [Header("Kıyafet / Eşya")]
    public ItemType item;

    [Header("Güçlendirici / Tuzak")]
    public float duration = 4f;
    [Tooltip("SpeedBoost için >1 (ör. 1.5), SlowTrap için <1 (ör. 0.5)")]
    public float speedMultiplier = 1.5f;

    [Header("Görsel")]
    public float spinSpeed = 120f;
    public GameObject collectEffect;

    void Update()
    {
        if (kind != Kind.SlowTrap) transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        var player = other.GetComponentInParent<PlayerController>();
        var gm = GameManager.Instance;

        switch (kind)
        {
            case Kind.Gold:       gm.AddGold(goldAmount); break;
            case Kind.Item:       gm.CollectItem(item); break;
            case Kind.SpeedBoost: player.ApplySpeedModifier(speedMultiplier, duration); break;
            case Kind.Shield:     player.ApplyShield(duration); break;
            case Kind.SlowTrap:   player.ApplySpeedModifier(speedMultiplier, duration); break;
        }

        if (collectEffect) Instantiate(collectEffect, transform.position, Quaternion.identity);
        gameObject.SetActive(false);
    }
}
