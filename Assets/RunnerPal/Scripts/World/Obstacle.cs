using UnityEngine;

// Engel. Tipine göre zıplanarak, eğilerek ya da şerit değiştirerek kaçılır.
// Kaçma şekli collider'ın yüksekliğiyle belirlenir (alçak engel = zıpla, yüksek bariyer = eğil).
public class Obstacle : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        other.GetComponentInParent<PlayerController>()?.OnHitObstacle();
    }
}
