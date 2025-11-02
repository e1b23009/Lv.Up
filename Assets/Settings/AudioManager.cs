using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // ランタイム生成される常設AudioSource
    private AudioSource bgmSource;
    private AudioSource seSource;

    // 既定音量
    [Range(0f, 1f)] public float defaultBgmVolume = 0.5f;
    [Range(0f, 1f)] public float defaultSeVolume = 0.8f;

    void Awake()
    {
        // シングルトン確立
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 子にBGM/SE用オブジェクトを用意（Inspector設定が無くても必ず作る）
        bgmSource = Create2DSource("BGM_Source", loop: true);
        seSource = Create2DSource("SE_Source", loop: false);

        bgmSource.volume = defaultBgmVolume;
        seSource.volume = defaultSeVolume;
    }

    private AudioSource Create2DSource(string name, bool loop)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = loop;
        src.spatialBlend = 0f;   // 2D
        return src;
    }

    // ===== BGM API =====
    public void PlayBGM(AudioClip clip, float? volume = null, float fadeSec = 0f, bool loop = true)
    {
        if (clip == null) return;
        bgmSource.loop = loop;

        if (fadeSec <= 0f || !bgmSource.isPlaying)
        {
            bgmSource.clip = clip;
            bgmSource.volume = Mathf.Clamp01(volume ?? defaultBgmVolume);
            bgmSource.Play();
            return;
        }

        // フェードで切替
        StartCoroutine(CoSwitchBgmWithFade(clip, Mathf.Clamp01(volume ?? defaultBgmVolume), fadeSec));
    }

    public void StopBGM(float fadeSec = 0f)
    {
        if (!bgmSource.isPlaying) return;
        if (fadeSec <= 0f) { bgmSource.Stop(); return; }
        StartCoroutine(CoFade(bgmSource, bgmSource.volume, 0f, fadeSec, stopAtEnd: true));
    }

    public void SetBgmVolume(float v) { bgmSource.volume = Mathf.Clamp01(v); }

    // ===== SE API =====
    public void PlaySE(AudioClip clip, float? volume = null)
    {
        if (clip == null) return;
        seSource.PlayOneShot(clip, Mathf.Clamp01(volume ?? defaultSeVolume));
    }

    public void SetSeVolume(float v) { seSource.volume = Mathf.Clamp01(v); }

    // ===== 内部コルーチン =====
    private IEnumerator CoSwitchBgmWithFade(AudioClip next, float nextVol, float sec)
    {
        // 現在のBGMをフェードアウト
        yield return CoFade(bgmSource, bgmSource.volume, 0f, sec, stopAtEnd: false);
        bgmSource.Stop();
        // 切り替え
        bgmSource.clip = next;
        bgmSource.volume = 0f;
        bgmSource.Play();
        // フェードイン
        yield return CoFade(bgmSource, 0f, nextVol, sec, stopAtEnd: false);
    }

    private IEnumerator CoFade(AudioSource src, float from, float to, float sec, bool stopAtEnd)
    {
        if (sec <= 0f) { src.volume = to; if (stopAtEnd && to <= 0f) src.Stop(); yield break; }
        float t = 0f;
        src.volume = from;
        while (t < sec)
        {
            t += Time.unscaledDeltaTime;        // ポーズ中も進めたい場合はこちら
            src.volume = Mathf.Lerp(from, to, t / sec);
            yield return null;
        }
        src.volume = to;
        if (stopAtEnd && to <= 0f) src.Stop();
    }
}
