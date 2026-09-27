using UnityEngine;

// Arkadan takip eden kamera. Şerit değişiminde yumuşak kayar.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 4f, -7f);
    public float xFollow = 0.6f;     // Kamera şerit hareketini ne kadar takip etsin (0-1)
    public float smooth = 8f;

    void LateUpdate()
    {
        if (!target) return;
        Vector3 desired = new Vector3(target.position.x * xFollow, 0f, target.position.z) + offset;
        desired.x = Mathf.Lerp(transform.position.x, desired.x, smooth * Time.deltaTime);
        transform.position = desired;
        transform.LookAt(target.position + Vector3.up * 1.5f + Vector3.forward * 4f);
    }
}
