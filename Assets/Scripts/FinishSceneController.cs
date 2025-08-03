using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FinishSceneController : MonoBehaviour
{
    [Header("References")]
    public TMP_Text playerAliveFingerText;
    public TMP_Text opponentAliveFingerText;
    public TMP_Text resultText;

    [Header("Optional")]
    public Button retryButton;
    public Button titleButton;

    void Start()
    {
        // 表示をセット
        int playerAlive = FinishSceneState.PlayerAliveFingers;
        int opponentAlive = FinishSceneState.OpponentAliveFingers;
        FinishOutcome outcome = FinishSceneState.Outcome;

        if (playerAliveFingerText != null)
            playerAliveFingerText.text = $"Your Alive Fingers\n{playerAlive}";

        if (opponentAliveFingerText != null)
            opponentAliveFingerText.text = $"Opponent Alive Fingers\n{opponentAlive}";

        if (resultText != null)
        {
            switch (outcome)
            {
                case FinishOutcome.YouWin:
                    resultText.text = "You Win";
                    break;
                case FinishOutcome.YouLose:
                    resultText.text = "You Lose";
                    break;
                case FinishOutcome.Draw:
                    resultText.text = "Draw";
                    break;
            }
        }

        // ボタンがあればフック
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetry);

        if (titleButton != null)
            titleButton.onClick.AddListener(OnTitle);
    }

    private void OnRetry()
    {
        // 指初期化
        for (int i = 0; i < 15; i++)
        {
            if (PlayerFingerStatusStore.Instance != null)
            {
                if (i < 10)
                    PlayerFingerStatusStore.Instance.fingerStatuses[i] = FingerStatus.Bent;
                else
                    PlayerFingerStatusStore.Instance.fingerStatuses[i] = FingerStatus.Disabled;
            }
        }
        for (int i = 0; i < 15; i++)
        {
            if (OpponentFingerStatusStore.Instance != null)
            {
                if (i < 10)
                    OpponentFingerStatusStore.Instance.fingerStatuses[i] = FingerStatus.Bent;
                else
                    OpponentFingerStatusStore.Instance.fingerStatuses[i] = FingerStatus.Disabled;
            }
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene"); 
    }

    private void OnTitle()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
    }
}
