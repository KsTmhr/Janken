using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class JankenPlayerController : MonoBehaviour
{
    public GameMaster gameMaster; // GameMaster参照
    public FingerController[] fingerControllers; // 指10本のController（曲げと伸ばしで合計20）
    public FingerStatus[] fingerStatuses; // 実体はストアと共有

    public void UpdateHand()
    {
        foreach (FingerController fc in fingerControllers)
        {
            if (fingerStatuses[fc.fingerIndex] == FingerStatus.Extended)
            {
                if (fc.IsBentFinger)
                    fc.Hide();
                else
                    fc.Show();
            }
            else if (fingerStatuses[fc.fingerIndex] == FingerStatus.Bent)
            {
                if (fc.IsBentFinger)
                    fc.Show();
                else
                    fc.Hide();
            }
            else
            {
                fc.Hide();
            }
        }
    }

    public void CutFinger(int num)
    {
        // 出てる指全カット
        if (num == -1)
        {
            for (int i = fingerStatuses.Length - 1; i >= 0; i--)
            {
                if (fingerStatuses[i] == FingerStatus.Extended)
                    fingerStatuses[i] = FingerStatus.Disabled;
            }
        }
        // 本数指定で生きている指からカット
        else
        {
            int cutNum = 0;
            for (int i = fingerStatuses.Length - 1; i >= 0; i--)
            {
                if (fingerStatuses[i] != FingerStatus.Disabled)
                {
                    fingerStatuses[i] = FingerStatus.Disabled;
                    cutNum++;
                }
                if (cutNum >= num)
                    break;
            }
        }
    }

    public void AddFinger(int num)
    {
        int addedNum = 0;
        for (int i = 0; i < fingerStatuses.Length; i++)
        {
            if (fingerStatuses[i] == FingerStatus.Disabled)
            {
                fingerStatuses[i] = FingerStatus.Bent;
                addedNum++;
            }
            if (addedNum >= num)
            {
                break;
            }
        }
    }

    public int GetAliveFingerNum()
    {
        int fingerCount = 0;
        for (int i = 0; i < fingerStatuses.Length; i++)
        {
            if (fingerStatuses[i] != FingerStatus.Disabled)
            {
                fingerCount++;
            }
        }
        return fingerCount;
    }

    public int GetExtendedFingerNum(bool includeAdditionalFingers=false)
    {
        int extendedCount = 0;
        for (int i = 0; i < (includeAdditionalFingers ? fingerStatuses.Length : 10); i++)
        {
            if (fingerStatuses[i] == FingerStatus.Extended)
            {
                extendedCount++;
            }
        }
        return extendedCount;
    }
}
