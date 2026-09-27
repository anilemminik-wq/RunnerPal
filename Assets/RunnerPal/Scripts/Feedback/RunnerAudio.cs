using UnityEngine;

// Oyun seslerini olaylara bağlar: altın, eşya, güçlendirici, tuzak, zıplama, eğilme, çarpma ("ah!" + kırılma),
// kazanma ve kaybetme. Kuralları değiştirmez; sadece dinler.
public class RunnerAudio : MonoBehaviour
{
    public PlayerController player;

    [Header("Klipler")]
    public AudioClip[] coin;
    public AudioClip item, powerUp, shield, slowTrap, jump, slide, win, fail;
    public AudioClip[] breakObstacle;
    public AudioClip thud;
    [Tooltip("Enerji içeceği içilince / taksiye binince")]
    public AudioClip energy, taxi;
    [Tooltip("Can gidince rastgele biri çalar")]
    public AudioClip[] ouch;

    [Range(0f, 1f)] public float volume = 0.9f;

    AudioSource[] pool;
    int next;
    GameManager gm;

    void Awake()
    {
        pool = new AudioSource[6];
        for (int i = 0; i < pool.Length; i++)
        {
            pool[i] = gameObject.AddComponent<AudioSource>();
            pool[i].playOnAwake = false;
            pool[i].spatialBlend = 0f;
        }
    }

    void OnEnable()
    {
        gm = FindFirstObjectByType<GameManager>();
        Pickup.Collected += OnPickup;
        if (player)
        {
            player.Jumped += OnJumped;
            player.Slid += OnSlid;
            player.HitObstacle += OnHit;
            player.EnergyUsed += OnEnergy;
            player.TaxiChanged += OnTaxi;
        }
        if (gm)
        {
            gm.OnItemCollected += OnItem;
            gm.OnLevelWon += OnWon;
            gm.OnLevelFailed += OnFailed;
        }
    }

    void OnDisable()
    {
        Pickup.Collected -= OnPickup;
        if (player)
        {
            player.Jumped -= OnJumped;
            player.Slid -= OnSlid;
            player.HitObstacle -= OnHit;
            player.EnergyUsed -= OnEnergy;
            player.TaxiChanged -= OnTaxi;
        }
        if (gm)
        {
            gm.OnItemCollected -= OnItem;
            gm.OnLevelWon -= OnWon;
            gm.OnLevelFailed -= OnFailed;
        }
    }

    public void Play(AudioClip clip, float vol = 1f, float pitch = 1f)
    {
        if (!clip) return;
        var src = pool[next];
        next = (next + 1) % pool.Length;
        src.pitch = pitch;
        src.PlayOneShot(clip, vol * volume);
    }

    void OnEnergy() => Play(energy ? energy : powerUp, 1f, 1.15f);
    void OnTaxi(bool entered) { if (entered) Play(taxi ? taxi : powerUp, 1f, 0.8f); }

    static AudioClip Pick(AudioClip[] clips) => clips != null && clips.Length > 0 ? clips[Random.Range(0, clips.Length)] : null;

    void OnPickup(Pickup p)
    {
        switch (p.kind)
        {
            // Art arda altınlarda perdeyi hafif oynat, tekdüze olmasın.
            case Pickup.Kind.Gold: Play(Pick(coin), 0.55f, Random.Range(0.95f, 1.12f)); break;
            case Pickup.Kind.SpeedBoost: Play(powerUp); break;
            case Pickup.Kind.Shield: Play(shield); break;
            case Pickup.Kind.SlowTrap: Play(slowTrap); break;
        }
    }

    void OnItem(ItemType _) => Play(item, 0.9f);
    void OnJumped() => Play(jump, 0.6f, 1.1f);
    void OnSlid() => Play(slide, 0.8f);
    void OnWon(int gold, ItemType? bought) => Play(win);
    void OnFailed(string reason) => Play(fail);

    void OnHit(Obstacle obstacle, bool damaged)
    {
        Play(Pick(breakObstacle), 1f, Random.Range(0.9f, 1.05f));
        if (damaged)
        {
            Play(thud, 0.8f);
            Play(Pick(ouch), 1f, Random.Range(0.95f, 1.08f));
        }
        else Play(shield, 0.6f, 1.3f);
    }
}
