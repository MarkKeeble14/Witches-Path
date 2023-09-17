using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class DGDoShakeData
{
    public float Duration = 1f;
    public float Strength = 100;
    public int Vibrato = 10;
    public float Randomness = 90;
    public bool Snapping = false;
    public bool FadeOut = true;
    public ShakeRandomnessMode Mode = ShakeRandomnessMode.Full;

    public void DoShakeAnchorPos(RectTransform rect)
    {
        rect.DOShakeAnchorPos(Duration, Strength, Vibrato, Randomness, Snapping, FadeOut, Mode);
    }
}
