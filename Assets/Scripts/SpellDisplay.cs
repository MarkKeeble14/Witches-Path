using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

public abstract class SpellDisplay : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI text;
    [SerializeField] protected TextMeshProUGUI nameText;
    [SerializeField] protected Image progressBar;
    [SerializeField] protected Image spellIcon;
    [SerializeField] private Image main;

    [SerializeField] private float regularScale;
    [SerializeField] private float maxScale;

    [SerializeField] private Vector2 toolTipOffset;
    private GameObject spawnedToolTip;

    private float targetScale;

    [SerializeField] private float changeScaleSpeed = 1f;

    [SerializeField] private GameObject[] disableWhenEmpty;
    private bool isEmpty;

    protected SpellDisplayState displayState = SpellDisplayState.Normal;
    private Tweener shakeTweener;

    private void Start()
    {
        targetScale = regularScale;
    }

    public bool IsAvailable => isAvailable;

    protected bool isAvailable = true;

    [SerializeField] private Sprite defaultSprite;

    protected void Update()
    {
        if (displayState == SpellDisplayState.Normal)
        {
            // Allow target scale to fall back to regular scale
            if (targetScale != regularScale)
            {
                targetScale = Mathf.MoveTowards(targetScale, regularScale, changeScaleSpeed * Time.deltaTime);
            }

            // Set Scale
            main.transform.localScale = targetScale * Vector3.one;
        }
    }

    public void SetSpellDisplayState(SpellDisplayState displayState)
    {
        this.displayState = displayState;
        if (displayState == SpellDisplayState.SwapSequence)
        {
            shakeTweener = transform.DOShakePosition(1, 3, 10, 90, false, false).SetLoops(-1);
        }
        else if (displayState == SpellDisplayState.Normal)
        {
            shakeTweener.Kill();
        }
    }

    public void AnimateScale()
    {
        targetScale = maxScale;
    }

    public void Unset()
    {
        nameText.text = "";
        text.text = "";
        spellIcon.sprite = defaultSprite;
        progressBar.fillAmount = 1;
        isAvailable = true;
    }

    public void SetEmpty(bool isEmpty)
    {
        this.isEmpty = isEmpty;
        foreach (GameObject obj in disableWhenEmpty)
        {
            obj.SetActive(!isEmpty);
        }
    }

    public abstract Spell GetSpell();

    public void SpawnToolTip()
    {
        // Only spawn ToolTip if spell is set
        if (!isEmpty)
            spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(GetSpell(), transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }
}
