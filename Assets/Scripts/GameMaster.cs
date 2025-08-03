using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameMaster : MonoBehaviour
{
    public PlayerController player;
    public OpponentController opponent;

    private Hand playerHand = Hand.None;
    private Hand opponentHand = Hand.None;
    private bool playerReceived = false;
    private bool opponentReceived = false;

    [Header("Result Scenes")]
    public string winSceneName = "Win";
    public string drawSceneName = "Draw";
    public string loseSceneName = "Lose";

    [Header("Timing / Durations")]
    public float beforeJankenDelay = 1.0f;          // 確定ボタン押したあと "Janken ..." 表示までの小さな間
    public float jankenDisplayDuration = 1.0f;      // "Janken ..." 表示時間
    public float beforeOpenDelay = 0.2f;            // "Janken ..." のあと開くまでの小さな間
    public float ponDisplayDuration = 0.5f;         // "Pon!!" 表示時間
    public float afterPonDelay = 0.2f;              // "Pon!!" のあと手を出す前の間
    public float resultDisplayDuration = 1.5f;      // 結果シーン遷移前の余韻

    [Header("Presentation / Animation")]
    [Tooltip("相手の手を隠している板の Transform（上にスライドして開く）")]
    public Transform opponentCoverBoard;
    public float openMoveDistance = 200f;
    public float openDuration = 0.5f;

    [Header("UI Display")]
    public TMP_Text jankenMessageText;      // "Janken ..." / "Pon!!" 用
    public TMP_Text playerHandText;
    public TMP_Text opponentHandText;
    public TMP_Text playerAliveFingerText;
    public TMP_Text opponentAliveFingerText;
        public Button confirmButton; // 確定ボタン

    public AudioClip Open;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = Open;
        audioSource.playOnAwake = false;
        audioSource.volume = 1f;
    }

    void Start()
    {
        ResetRound();
        opponent.StartDecision();
    }
    
    // 確定ボタンの表示／非表示（または操作可／不可）を切り替え
    public void SetConfirmButtonEnabled(bool enabled)
    {
        if (confirmButton == null) return;

        // 表示／非表示にしたいならこちら：
        confirmButton.gameObject.SetActive(enabled);

        // 操作だけ無効化したいならこちらを使う（表示は残す）：
        // confirmButton.interactable = enabled;
    }

    public void ReceivePlayerHand(Hand hand)
    {
        if (playerReceived) return;
        playerHand = hand;
        playerReceived = true;
        TryStartPresentationSequence();
    }

    public void ReceiveOpponentHand(Hand hand)
    {
        if (opponentReceived) return;
        opponentHand = hand;
        opponentReceived = true;
        TryStartPresentationSequence();
    }

    private bool isPresenting = false;
    private void TryStartPresentationSequence()
    {
        if (!playerReceived || !opponentReceived) return;
        if (isPresenting) return; // 既に演出中なら二重起動防止

        isPresenting = true;
        StartCoroutine(FullPresentationSequence());
    }

    private IEnumerator FullPresentationSequence()
    {
        // 微小な間
        yield return new WaitForSeconds(beforeJankenDelay);

        // "Janken ..." 表示\
        jankenMessageText.text = "Janken ...";
        jankenMessageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(jankenDisplayDuration);

        // "Janken ..."非表示
        jankenMessageText.gameObject.SetActive(false);

        // 微小な間
        yield return new WaitForSeconds(beforeOpenDelay);

        // 板をオープンして相手の手を明かす演出
        // "Pon!!" 表示
        jankenMessageText.text = "Pon!!";
        jankenMessageText.gameObject.SetActive(true);
        yield return StartCoroutine(OpenHand());

        // 少し余韻
        yield return new WaitForSeconds(afterPonDelay);

        // 手をテキストで表示
        ShowHandInText();

        // 消しておく
        jankenMessageText.gameObject.SetActive(false);

        // 勝敗判定とシーン遷移
        int result = JankenUtility.Resolve(playerHand, opponentHand);
        yield return StartCoroutine(HandleResult(result));
    }

    private IEnumerator OpenHand()
    {
        Vector3 start = opponentCoverBoard.localPosition;
        Vector3 target = start + Vector3.up * openMoveDistance;
        float elapsed = 0f;

        audioSource.Play();

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);
            opponentCoverBoard.localPosition = Vector3.Lerp(start, target, t);
            yield return null;
        }

        opponentCoverBoard.localPosition = target;
    }

    // 手を TMP テキストで表示する
    private void ShowHandInText()
    {
        if (playerHandText != null)
        {
            playerHandText.text = HandInText(playerHand);
        }

        if (opponentHandText != null)
        {
            opponentHandText.text = HandInText(opponentHand);
        }
    }

    private string HandInText(Hand hand)
    {
        switch (hand)
        {
            case Hand.Rock:
                return "Rock";      // あるいは "グー"
            case Hand.Scissors:
                return "Scissors";  // あるいは "チョキ"
            case Hand.Paper:
                return "Paper";     // あるいは "パー"
            default:
                return "None";
        }
    }

    // じゃんけんの結果を受けて次の処理を決定する関数
    private IEnumerator HandleResult(int result)
    {
        yield return new WaitForSeconds(resultDisplayDuration);

        // グー or チョキでプレイヤー勝利 → 相手の指チョンパ
        if (result == 1)
        {
            opponent.CutFinger(opponent.GetExtendedFingerNum());

            // どっちもパーが出せなくなったら勝負続行不可
            int playerFinger = player.GetAliveFingerNum();
            int opponentFinger = opponent.GetAliveFingerNum();
            if (playerFinger < 5 && opponentFinger < 5)
            {
                if (playerFinger > opponentFinger)
                {
                    FinishSceneState.Set(playerFinger, opponentFinger, FinishOutcome.YouWin);
                    SceneManager.LoadScene("Finish");
                }
                else if (playerFinger < opponentFinger)
                {
                    FinishSceneState.Set(playerFinger, opponentFinger, FinishOutcome.YouLose);
                    SceneManager.LoadScene("Finish");
                }
                else
                {
                    FinishSceneState.Set(playerFinger, opponentFinger, FinishOutcome.Draw);
                    SceneManager.LoadScene("Finish");
                }
            }
            // 相手がグー以外出せなくなったら勝利
            else if (opponentFinger < 2)
            {
                FinishSceneState.Set(playerFinger, opponentFinger, FinishOutcome.YouWin);
                SceneManager.LoadScene("Finish");
            }
            else
            {
                SceneManager.LoadScene(winSceneName);
            }
        }
        // パーでプレイヤー勝利 → プレイヤーに指二本追加
        else if (result == 2)
        {
            player.AddFinger(2);

            // 全部で15本指獲得したら勝利
            if (player.GetAliveFingerNum() >= 15)
            {
                FinishSceneState.Set(player.GetAliveFingerNum(), opponent.GetAliveFingerNum(), FinishOutcome.YouWin);
                SceneManager.LoadScene("Finish");
            }
            else
            {
                SceneManager.LoadScene(winSceneName);
            }
        }
        // グー or チョキで相手勝利 → プレイヤーの指チョンパ
        else if (result == -1)
        {
            player.CutFinger(player.GetExtendedFingerNum());

            // どっちもパーが出せなくなったら勝負続行不可
            int playerFinger = player.GetAliveFingerNum();
            int opponentFinger = opponent.GetAliveFingerNum();
            if (playerFinger < 5 && opponentFinger < 5)
            {
                if (playerFinger > opponentFinger)
                {
                    FinishSceneState.Set(playerFinger, opponentFinger, FinishOutcome.YouWin);
                    SceneManager.LoadScene("Finish");
                }
                else if (playerFinger < opponentFinger)
                {
                    FinishSceneState.Set(playerFinger, opponentFinger, FinishOutcome.YouLose);
                    SceneManager.LoadScene("Finish");
                }
                else
                {
                    FinishSceneState.Set(playerFinger, opponentFinger, FinishOutcome.Draw);
                    SceneManager.LoadScene("Finish");
                }
            }
            //プレイヤーがグー以外出せなくなったら敗北
            else if (playerFinger < 2)
            {
                FinishSceneState.Set(playerFinger, opponentFinger, FinishOutcome.YouLose);
                SceneManager.LoadScene("Finish");
            }
            else
            {
                SceneManager.LoadScene(loseSceneName);
            }
        }
        // パーで相手勝利 → 相手に指二本追加
        else if (result == -2)
        {
            opponent.AddFinger(2);

            // 全部で15本指獲得したら勝利
            if (opponent.GetAliveFingerNum() >= 15)
            {
                FinishSceneState.Set(player.GetAliveFingerNum(), opponent.GetAliveFingerNum(), FinishOutcome.YouLose);
                SceneManager.LoadScene("Finish");
            }
            else
            {
                SceneManager.LoadScene(loseSceneName);
            }
        }
        // あいこ
        else
        {
            // 二倍以上多かったら二本切断，二倍以下で差があったら一本切って渡す，差がなかったら何もなし
            if (player.GetAliveFingerNum() > opponent.GetAliveFingerNum())
            {
                if (player.GetAliveFingerNum() > opponent.GetAliveFingerNum() * 2)
                {
                    player.CutFinger(2);
                }
                else
                {
                    player.CutFinger(1);
                    opponent.AddFinger(1);
                }
            }
            else if (player.GetAliveFingerNum() < opponent.GetAliveFingerNum())
            {
                if (player.GetAliveFingerNum() < opponent.GetAliveFingerNum() * 2)
                {
                    opponent.CutFinger(2);
                }
                else
                {
                    opponent.CutFinger(1);
                    player.AddFinger(1);
                }
            }
            
            int playerFinger = player.GetAliveFingerNum();
            int opponentFinger = opponent.GetAliveFingerNum();
            // どっちもパーが出せなくなったら勝負続行不可
            if (playerFinger < 5 && opponentFinger < 5)
            {
                if (playerFinger > opponentFinger)
                {
                    FinishSceneState.Set(playerFinger, opponentFinger, FinishOutcome.YouWin);
                    SceneManager.LoadScene("Finish");
                }
                else if (playerFinger < opponentFinger)
                {
                    FinishSceneState.Set(playerFinger, opponentFinger, FinishOutcome.YouLose);
                    SceneManager.LoadScene("Finish");
                }
                else
                {
                    FinishSceneState.Set(playerFinger, opponentFinger, FinishOutcome.Draw);
                    SceneManager.LoadScene("Finish");
                }
            }
            else
            {
                SceneManager.LoadScene(drawSceneName);
            }
        }
    }

    public void ResetRound()
    {
        playerHand = Hand.None;
        opponentHand = Hand.None;
        playerReceived = false;
        opponentReceived = false;

        if (playerHandText != null) playerHandText.text = "";
        if (opponentHandText != null) opponentHandText.text = "";
        if (jankenMessageText != null)
        {
            jankenMessageText.text = "";
            jankenMessageText.gameObject.SetActive(false);
        }

        // ボタン表示
        SetConfirmButtonEnabled(true);
        
        // Alive Finger 表示
        if (playerAliveFingerText != null)
        {
            playerAliveFingerText.text = $"Your Alive Fingers\n{player.GetAliveFingerNum()}";
        }

        if (opponentAliveFingerText != null)
        {
            opponentAliveFingerText.text = $"Opponent Alive Fingers\n{opponent.GetAliveFingerNum()}";
        }
    }

    // 外部経路
    public void OnRemotePlayerHandReceived(Hand hand) => ReceivePlayerHand(hand);
    public void OnRemoteOpponentHandReceived(Hand hand) => ReceiveOpponentHand(hand);
}
