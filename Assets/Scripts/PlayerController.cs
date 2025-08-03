using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class PlayerController : JankenPlayerController
{
    public TMP_Text fingerCountText;
    public AudioClip set;
    public AudioClip choice;
    private bool isSubmitted;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 1f;

        // 永続ストアを確保／初期化
        if (PlayerFingerStatusStore.Instance == null)
        {
            GameObject storeGO = new GameObject("FingerStatusStore");
            storeGO.AddComponent<PlayerFingerStatusStore>();
        }

        if (fingerControllers != null)
        {
            PlayerFingerStatusStore.Instance.Initialize(15);
            fingerStatuses = PlayerFingerStatusStore.Instance.fingerStatuses;
        }
        else
        {
            // 安全にサイズだけ確保（後から設定される想定）
            if (PlayerFingerStatusStore.Instance != null && fingerStatuses == null)
            {
                fingerStatuses = PlayerFingerStatusStore.Instance.fingerStatuses;
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

        // 初回表示更新
        UpdateFingerCountDisplay();

        // 手の表示も更新
        UpdateHand();
    }

	public void UpdateFingerCountDisplay()
    {
        if (fingerStatuses == null) return;

        if (fingerCountText != null)
        {
            fingerCountText.text = "Selected Fingers: " + GetExtendedFingerNum().ToString();
        }
    }

    public void OnFingerClicked(FingerController clickedFinger)
    {
        if (isSubmitted) return;
        if (clickedFinger == null) return;
        if (fingerStatuses == null) return;
        
        audioSource.clip = choice;
        audioSource.Play();

        // 現在の状態に応じてスイッチ（IsBentFinger が true なら今は曲がってる＝Extended にしたい、仕様合わせて反転する）
        // ここでは clickedFinger.Switch() のあとに状態を反映すると仮定
        clickedFinger.Switch();

        int index = clickedFinger.fingerIndex;
        if (index >= 0 && index < fingerStatuses.Length)
        {
            // クリック後の状態を取得して同期（IsBentFinger が true なら bent、false なら extended など調整）
            fingerStatuses[index] = clickedFinger.IsBentFinger ? FingerStatus.Extended : FingerStatus.Bent;

            // 共有ストアにも反映（実質同一配列なので不要だが明示的に保険）
            if (PlayerFingerStatusStore.Instance != null)
            {
                PlayerFingerStatusStore.Instance.fingerStatuses[index] = fingerStatuses[index];
            }
        }

        // 表示を更新
        UpdateFingerCountDisplay();
    }

    // 確定ボタンが押されたとき呼ばれる．
    public void OnDecideHand()
    {
        if (isSubmitted) return;
        isSubmitted = true;

        audioSource.clip = set;
        audioSource.Play();

        // 確定ボタン非表示
        if (gameMaster != null)
        {
            gameMaster.SetConfirmButtonEnabled(false);
        }

        if (fingerStatuses == null) return;

        Hand playerHand = JankenUtility.DecideHand(GetExtendedFingerNum());

        if (gameMaster != null)
        {
            gameMaster.ReceivePlayerHand(playerHand);
        }
    }

}
