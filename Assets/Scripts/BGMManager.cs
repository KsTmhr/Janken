using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [Tooltip("ループさせたいBGMをセット")]
    public AudioClip bgmClip;

    private AudioSource audioSource;

    void Awake()
    {
        // シングルトンを維持（既に存在するなら自分を破棄）
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // AudioSource 設定
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = bgmClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0.7f; // 好みに応じて
    }

    void Start()
    {
        if (bgmClip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    // 再生中のBGMをフェードさせて差し替える（オプション）
    public void CrossfadeTo(AudioClip newClip, float duration)
    {
        if (newClip == null) return;
        StartCoroutine(CrossfadeCoroutine(newClip, duration));
    }

    private System.Collections.IEnumerator CrossfadeCoroutine(AudioClip newClip, float duration)
    {
        float half = duration / 2f;
        // フェードアウト
        float startVol = audioSource.volume;
        for (float t = 0; t < half; t += Time.unscaledDeltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVol, 0f, t / half);
            yield return null;
        }
        audioSource.volume = 0f;

        // クリップを差し替えて再生
        audioSource.clip = newClip;
        audioSource.Play();

        // フェードイン
        for (float t = 0; t < half; t += Time.unscaledDeltaTime)
        {
            audioSource.volume = Mathf.Lerp(0f, startVol, t / half);
            yield return null;
        }
        audioSource.volume = startVol;
    }

    // BGM のボリューム調整（0〜1）
    public void SetVolume(float v)
    {
        audioSource.volume = Mathf.Clamp01(v);
    }
}
