using UnityEngine;

// Arkadan takip eden kamera. Şerit değişiminde yumuşak kayar.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 4f, -7f);
    public float xFollow = 0.6f;     // Kamera şerit hareketini ne kadar takip etsin (0-1)
    public float smooth = 8f;

    float shakeTime, shakeDuration, shakeAmount;
    float smoothX;

    // Çarpışma gibi anlarda kısa sarsıntı.
    public void Shake(float amount, float duration)
    {
        shakeAmount = amount;
        shakeDuration = shakeTime = duration;
    }

    void Start() => smoothX = transform.position.x;

    void LateUpdate()
    {
        if (!target) return;
        Vector3 desired = new Vector3(target.position.x * xFollow, 0f, target.position.z) + offset;
        // Takip edilen x ayrı tutulur ki sarsıntı bir sonraki kareye taşınmasın.
        smoothX = Mathf.Lerp(smoothX, desired.x, smooth * Time.deltaTime);
        desired.x = smoothX;
        transform.position = desired;
        transform.LookAt(target.position + Vector3.up * 1.5f + Vector3.forward * 4f);

        if (shakeTime > 0f)
        {
            shakeTime -= Time.deltaTime;
            float k = shakeAmount * Mathf.Clamp01(shakeTime / shakeDuration);
            transform.position += (Vector3)(Random.insideUnitCircle * k);
        }
    }
}
