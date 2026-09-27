using UnityEngine;

// Hareketli engel.
// Sway: iki komşu şerit arasında sağa sola kayar (TrackSpawner sadece serbest olmayan iki komşu şeride koyar).
// Drive: araba karşıdan gelir; oyuncu yaklaşınca kendi şeridinde ve kendi yol parçası içinde ilerler.
public class ObstacleMover : MonoBehaviour
{
    public enum Mode { Sway, Drive }
    public Mode mode = Mode.Sway;

    [Header("Sway")]
    [Tooltip("Merkezden sağa/sola en fazla (m). 1.25 = iki şeridin ortası")]
    public float swayAmount = 1.25f;
    public float swaySpeed = 1.6f;

    [Header("Drive")]
    public float driveSpeed = 7f;
    [Tooltip("Oyuncu bu kadar yaklaşınca harekete geçer (m)")]
    public float triggerDistance = 38f;
    [Tooltip("Başlangıç noktasından en fazla bu kadar ilerler, sonra durur (yol parçasının dışına taşmaz)")]
    public float driveDistance = 14f;

    Vector3 start;
    float phase, driven;

    void Start()
    {
        start = transform.position;
        phase = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        if (mode == Mode.Sway)
        {
            float x = Mathf.Sin(Time.time * swaySpeed + phase) * swayAmount;
            transform.position = start + Vector3.right * x;
            return;
        }

        var gm = GameManager.Instance;
        if (!gm || !gm.player || gm.CurrentState != GameManager.State.Running) return;
        if (driven >= driveDistance) return;
        if (transform.position.z - gm.player.transform.position.z > triggerDistance) return;
        float step = Mathf.Min(driveSpeed * Time.deltaTime, driveDistance - driven);
        driven += step;
        transform.position += Vector3.back * step;
    }
}
