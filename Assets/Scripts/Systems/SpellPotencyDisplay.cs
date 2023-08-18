using System;
using UnityEngine;

public class SpellPotencyDisplay : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector2 minMaxPotency;
    [SerializeField] private Vector2 minMaxScale;
    [SerializeField] private Transform toScale;

    [Header("Animate")]
    [SerializeField] private float animateScaleSpeed;
    [SerializeField] private float animateAlphaSpeed;
    private float currentPotency;
    private float goalScale;

    [SerializeField] private CanvasGroup cv;

    public void SetCurrentPotency(float v)
    {
        currentPotency = v;
    }

    private void Update()
    {
        goalScale = MathHelper.Normalize(currentPotency, minMaxPotency.x, minMaxPotency.y, minMaxScale.x, minMaxScale.y);
        toScale.localScale = Vector3.Lerp(toScale.localScale, goalScale * Vector3.one, Time.deltaTime * animateScaleSpeed);
        cv.alpha = Mathf.Lerp(cv.alpha, 1, Time.deltaTime * animateAlphaSpeed);
    }

    public Vector2 GetMinMaxPotency()
    {
        return minMaxPotency;
    }
}
