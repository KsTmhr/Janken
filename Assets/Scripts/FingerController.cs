using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerController : MonoBehaviour
{
    public bool IsBentFinger; // 曲げた指にアタッチされているSpriteならtrue
    public int fingerIndex;
    public GameObject counterpart; // 対応するもう一方の状態のオブジェクト（曲げ or 伸ばし）

    public void Switch()
    {
        this.gameObject.SetActive(false);
        counterpart.SetActive(true);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }
    public void Hide()

    {
        this.gameObject.SetActive(false);
    }
}
