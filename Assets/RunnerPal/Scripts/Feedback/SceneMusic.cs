using System.Collections;
using UnityEngine;

// Sahnenin müziği: menüde menuClip, oyunda bölümün dünyasına ait parça (her 10 bölümde bir dünya).
public class SceneMusic : MonoBehaviour
{
    public AudioClip menuClip;
    [Tooltip("Dünya sırasıyla: Ofis, Banka, Satış, Yazılım")]
    public AudioClip[] worldClips;

    IEnumerator Start()
    {
        var gm = FindFirstObjectByType<GameManager>();
        if (!gm) { MusicPlayer.Play(menuClip); yield break; }

        // GameManager bölümü kendi Start'ında yükler.
        while (gm.Level == null) yield return null;
        int world = Mathf.Clamp((gm.Level.levelNumber - 1) / 10, 0, worldClips.Length - 1);
        if (worldClips.Length > 0) MusicPlayer.Play(worldClips[world]);
        gm.OnLevelWon += (_, _) => MusicPlayer.Duck();
        gm.OnLevelFailed += _ => MusicPlayer.Duck();
    }
}
