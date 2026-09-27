using UnityEngine;

// Bölüm sonu (iş yerinin kapısı). TrackSpawner bunu levelLength mesafesine koyar.
public class FinishLine : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        GameManager.Instance.ReachFinish();
    }
}
