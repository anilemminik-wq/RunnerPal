using UnityEngine;

// Yolda toplanabilen her şey: altın, kıyafet/eşya, güçlendirici, yavaşlama tuzağı.
// Objeye trigger collider ekle ve Player objesinin tag'ini "Player" yap.
public class Pickup : MonoBehaviour
{
    // SpeedBoost = enerji içeceği (sağdaki butona eklenir), Taxi = nadir taksi.
    public enum Kind { Gold, Item, SpeedBoost, Shield, SlowTrap, Taxi }

    // Ses ve efektler için (ör. altın sesi).
    public static event System.Action<Pickup> Collected;

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
    [Tooltip("Dönen görsel (boşsa objenin kendisi döner; yazı gibi sabit kalacak parçalar dışarıda kalır)")]
    public Transform spinTarget;

    [Header("Taksi mıknatısı")]
    public float magnetRange = 16f;
    public float magnetSpeed = 40f;

    void Update()
    {
        if (kind != Kind.SlowTrap) (spinTarget ? spinTarget : transform).Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

        // Taksideyken altınlar oyuncuya uçar.
        if (kind != Kind.Gold) return;
        var gm = GameManager.Instance;
        var player = gm ? gm.player : null;
        if (!player || !player.InTaxi) return;
        Vector3 target = player.transform.position + Vector3.up * 0.3f;
        Vector3 d = target - transform.position;
        if (d.z < -2f || d.sqrMagnitude > magnetRange * magnetRange) return;
        transform.position = Vector3.MoveTowards(transform.position, target, (magnetSpeed + player.CurrentSpeed) * Time.deltaTime);
        if (d.sqrMagnitude < 0.6f) Collect(player);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        Collect(other.GetComponentInParent<PlayerController>());
    }

    void Collect(PlayerController player)
    {
        if (!gameObject.activeSelf) return;
        var gm = GameManager.Instance;

        switch (kind)
        {
            case Kind.Gold:       gm.AddGold(goldAmount); break;
            case Kind.Item:       gm.CollectItem(item); break;
            case Kind.SpeedBoost: player.AddEnergyDrink(); break;
            case Kind.Shield:     player.ApplyShield(duration); break;
            case Kind.SlowTrap:   if (!player.InTaxi) player.ApplySpeedModifier(speedMultiplier, duration); break;
            case Kind.Taxi:       player.EnterTaxi(duration); break;
        }

        Collected?.Invoke(this);
        if (collectEffect) Instantiate(collectEffect, transform.position, Quaternion.identity);
        gameObject.SetActive(false);
    }
}
