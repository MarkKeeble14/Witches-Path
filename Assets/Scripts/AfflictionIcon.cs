using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AfflictionIcon : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI stacksRemaining;

    private Affliction setTo;

    [Header("Animations")]
    [SerializeField] private float regularScale;
    [SerializeField] private float maxScale;
    private float targetScale;
    [SerializeField] private float changeScaleSpeed = 1f;

    private GameObject spawnedToolTip;

    private void Awake()
    {
        // Set Target Scale Initially
        targetScale = regularScale;
    }

    private void Update()
    {
        // Allow target scale to fall back to regular scale
        if (targetScale != regularScale)
        {
            targetScale = Mathf.MoveTowards(targetScale, regularScale, changeScaleSpeed * Time.deltaTime);
        }

        // Set Transforms Actual Scale Scale
        image.transform.localScale = targetScale * Vector3.one;

        SetStacksRemaining(Utils.RoundTo(setTo.GetStacks(), 0).ToString());
    }

    public void AnimateScale()
    {
        targetScale = maxScale;
    }

    public void SetAffliction(Affliction aff)
    {
        setTo = aff;
        SetTypeText(aff.Name);
    }

    public void SetSprite(Sprite s)
    {
        image.sprite = s;
    }

    public void SetTypeText(string s)
    {
        typeText.text = s;
    }

    public void SetStacksRemaining(string s)
    {
        stacksRemaining.text = s;
    }

    public void SpawnToolTip()
    {
        if (CombatManager._Instance.AllowGameSpaceToolTips)
            spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(setTo, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }
}
