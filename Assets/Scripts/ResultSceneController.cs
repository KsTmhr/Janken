using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class ResultSceneController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text resultText;        // "You Win" 等を表示
    public Image imageDisplay;        // 画像表示用（同じ Image を再利用）

    [Header("Sprites")]
    public Sprite firstSprite;        // 最初に表示する画像
    public Sprite secondSprite;       // そのあと表示する画像

    [Header("Timing")]
    public float firstImageDuration = 2f;   // 一枚目を表示する時間（秒）
    public float betweenDelay = 0.5f;       // 一枚目消して二枚目までの小さな間
    public float screamDelay = 1f;
    public float afterSecondDelay = 0f;   // 二枚目表示後にそのまま置いておく時間（必要なら次の遷移用）

    public AudioClip scream; // インスペクタで設定
    public AudioClip cut;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 1f;
    }

    void Start()
    {
        // 1. シーン名に応じてテキストを決定
        string sceneName = SceneManager.GetActiveScene().name;
        if (resultText != null)
        {
            switch (sceneName)
            {
                case "Win":
                    resultText.text = "You Win";
                    break;
                case "Lose":
                    resultText.text = "You Lose";
                    break;
                case "Draw":
                    resultText.text = "Draw";
                    break;
                default:
                    resultText.text = sceneName; // 想定外ならシーン名そのまま
                    break;
            }
        }

        // 2. 画像シーケンス開始
        if (imageDisplay != null)
        {
            StartCoroutine(PlayImageSequence());
        }
    }

    private IEnumerator PlayImageSequence()
    {
        // 一枚目表示
        if (firstSprite != null)
        {
            imageDisplay.sprite = firstSprite;
            imageDisplay.enabled = true;
        }

        yield return new WaitForSeconds(firstImageDuration);

        // 一枚目を消す（透明にする or 非表示）
        imageDisplay.enabled = false;

        yield return new WaitForSeconds(betweenDelay);

        // 二枚目表示
        if (secondSprite != null)
        {
            imageDisplay.sprite = secondSprite;
            imageDisplay.enabled = true;
            audioSource.clip = cut;
            audioSource.Play();
        }

        yield return new WaitForSeconds(screamDelay);
        audioSource.clip = scream;
        audioSource.Play();

        yield return new WaitForSeconds(afterSecondDelay);

        SceneManager.LoadScene("GameScene");
    }
}
