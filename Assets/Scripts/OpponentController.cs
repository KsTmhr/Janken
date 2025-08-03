using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentController : JankenPlayerController
{
    private Hand chosenHand = Hand.None;
    private bool hasSubmitted = false;

    void Awake()
    {
        // 永続ストアを確保／初期化
        if (OpponentFingerStatusStore.Instance == null)
        {
            GameObject storeGO = new GameObject("FingerStatusStore");
            storeGO.AddComponent<OpponentFingerStatusStore>();
        }

        if (fingerControllers != null)
        {
            OpponentFingerStatusStore.Instance.Initialize(15);
            fingerStatuses = OpponentFingerStatusStore.Instance.fingerStatuses;
        }
        else
        {
            // 安全にサイズだけ確保（後から設定される想定）
            if (OpponentFingerStatusStore.Instance != null && fingerStatuses == null)
            {
                fingerStatuses = OpponentFingerStatusStore.Instance.fingerStatuses;
            }
        }

        // 手全部折り曲げ (Additional Finger以外)
        for (int i = 0; i < fingerStatuses.Length; i++)
        {
            if (fingerStatuses[i] != FingerStatus.Disabled)
            {
                if (i < 10)
                    fingerStatuses[i] = FingerStatus.Bent;
                else
                    fingerStatuses[i] = FingerStatus.Extended;
            }
        }

        // 手の表示も更新
        UpdateHand();
    }

    // 通常はランダム選択して送る
    public void DecideAndSubmit()
    {
        if (hasSubmitted) return;

        chosenHand = GetRandomHand();
        SubmitHand();
    }

    // ランダムに指を選ぶ
    private Hand GetRandomHand()
    {
        // 選べる手
        int[] fingers = new int[] { 0, 2, 5 };

        // 生きてる指の数
        int fingerCount = 0;
        for (int i = 0; i < fingerStatuses.Length; i++)
        {
            if (fingerStatuses[i] != FingerStatus.Disabled)
            {
                fingerCount++;
            }
        }

        // 生きてる指の数によってはパーは選べない
        int hand = fingers[Random.Range(0, fingerCount >= 5 ? 3 : 2)];

        // hand 本ランダムに選んで Extended に
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < 10; i++)
        {
            if (fingerStatuses[i] != FingerStatus.Disabled)
            {
                availableIndices.Add(i);
            }
        }

        // シャッフル
        for (int i = 0; i < availableIndices.Count; i++)
        {
            int swap = Random.Range(i, availableIndices.Count);
            int tmp = availableIndices[i];
            availableIndices[i] = availableIndices[swap];
            availableIndices[swap] = tmp;
        }

        // 先頭 hand 個を Extended に
        for (int i = 0; i < Mathf.Min(hand, availableIndices.Count); i++)
        {
            int idx = availableIndices[i];
            fingerStatuses[idx] = FingerStatus.Extended;

            // 共有ストアにも反映（実質同一配列なので不要だが明示的に保険）
            OpponentFingerStatusStore.Instance.fingerStatuses[idx] = fingerStatuses[idx];
        }

        UpdateHand();

        return JankenUtility.DecideHand(hand);
    }

    // 送信
    private void SubmitHand()
    {
        if (gameMaster != null)
        {
            gameMaster.ReceiveOpponentHand(chosenHand);
            hasSubmitted = true;
        }
    }

    // 外部トリガー（例：プレイ開始時）で呼ぶ
    public void StartDecision()
    {
        DecideAndSubmit();
    }
}
