using System.Collections;
using UnityEngine;

// Arka plan müziği. Sahneler arasında yaşar (bölüm tekrar yüklenince şarkı baştan başlamaz) ve parça değişince
// yumuşak geçiş yapar. Sahnelerdeki SceneMusic hangi parçanın çalacağını söyler.
public class MusicPlayer : MonoBehaviour
{
    public const float DefaultVolume = 0.35f;

    static MusicPlayer instance;
    AudioSource[] sources;
    int active;
    float targetVolume = DefaultVolume;
    Coroutine fade;

    static MusicPlayer Instance
    {
        get
        {
            if (instance) return instance;
            var go = new GameObject("MusicPlayer");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<MusicPlayer>();
            instance.sources = new AudioSource[2];
            for (int i = 0; i < 2; i++)
            {
                var s = go.AddComponent<AudioSource>();
                s.loop = true;
                s.playOnAwake = false;
                s.volume = 0f;
                instance.sources[i] = s;
            }
            return instance;
        }
    }

    public static void Play(AudioClip clip, float fadeTime = 1f)
    {
        if (!clip) return;
        var m = Instance;
        m.targetVolume = DefaultVolume;
        var current = m.sources[m.active];
        if (current.clip == clip && current.isPlaying) { m.FadeTo(current, DefaultVolume, 0.5f); return; }
        var other = m.sources[1 - m.active];
        other.clip = clip;
        other.volume = 0f;
        other.Play();
        m.active = 1 - m.active;
        m.Crossfade(current, other, fadeTime);
    }

    // Kazan / kaybet jingle'ı duyulsun diye müzik kısılır.
    public static void Duck(float volume = 0.08f)
    {
        if (!instance) return;
        instance.FadeTo(instance.sources[instance.active], volume, 0.3f);
    }

    void FadeTo(AudioSource s, float volume, float time)
    {
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(FadeRoutine(null, s, volume, time));
    }

    void Crossfade(AudioSource from, AudioSource to, float time)
    {
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(FadeRoutine(from, to, targetVolume, time));
    }

    IEnumerator FadeRoutine(AudioSource from, AudioSource to, float volume, float time)
    {
        float fromStart = from ? from.volume : 0f, toStart = to.volume;
        for (float t = 0f; t < time; t += Time.unscaledDeltaTime)
        {
            float k = t / time;
            if (from) from.volume = Mathf.Lerp(fromStart, 0f, k);
            to.volume = Mathf.Lerp(toStart, volume, k);
            yield return null;
        }
        if (from) { from.volume = 0f; from.Stop(); }
        to.volume = volume;
        fade = null;
    }
}
