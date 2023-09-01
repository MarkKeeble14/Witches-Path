using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

public abstract class SpellDisplay : MonoBehaviour
{
    protected Spell Spell { get; private set; }

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
    protected bool scaleLocked;
    public void SetTargetScale(float v)
    {
        targetScale = v;
    }

    public void SetScaleLocked(bool b)
    {
        scaleLocked = b;
    }

    [SerializeField] private float changeScaleSpeed = 1f;

    [SerializeField] private GameObject[] disableWhenEmpty;

    protected SpellDisplayState displayState = SpellDisplayState.Normal;
    private Tweener shakeTweener;
    public bool IsEmpty { get; private set; }

    [SerializeField] private Sprite defaultSprite;

    private void Start()
    {
        targetScale = regularScale;
    }

    protected void Update()
    {
        if (displayState == SpellDisplayState.Normal && !scaleLocked)
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
        if (displayState == SpellDisplayState.Selected)
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

    public virtual void SetSpell(Spell spell)
    {
        Spell = spell;
        spellIcon.sprite = spell.GetSpellSprite();
        nameText.text = spell.Name;
        spell.SetEquippedTo(this);
        SetEmpty(false);
    }

    public virtual void Unset()
    {
        nameText.text = "";
        text.text = "";
        spellIcon.sprite = defaultSprite;
        progressBar.fillAmount = 1;
        SetEmpty(true);
        Spell = null;
    }

    public void SetEmpty(bool isEmpty)
    {
        IsEmpty = isEmpty;
        foreach (GameObject obj in disableWhenEmpty)
        {
            obj.SetActive(!isEmpty);
        }
    }

    public virtual void SpawnToolTip()
    {
        // Only spawn ToolTip if spell is set
        if (!IsEmpty)
            spawnedToolTip = UIManager._Instance.SpawnGenericToolTips(Spell, transform);
    }

    public void DestroyToolTip()
    {
        Destroy(spawnedToolTip);
    }
}
